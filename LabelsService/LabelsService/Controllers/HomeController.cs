using LabelsService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Doc.Fields;
using System.Drawing;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Npgsql;

namespace LabelsService.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string URL)
        {
            DateTime start = DateTime.Now;
            string label = Request.Form["val1[]"];
            string value = Request.Form["val2[]"];
            string[] type = Request.Form["undefined[]"];
            WebClient webClient = new WebClient();
            Dictionary<string, string> TextLabels = new Dictionary<string, string>();
            Dictionary<string, string> PictureLabels = new Dictionary<string, string>();
            Dictionary<string, string[]> TableLabels = new Dictionary<string, string[]>();
            Image[] image = new Image[100];
            int index = 0;
            TextRange range = null;
            TextSelection[] selection = new TextSelection[100];
            string path = new KnownFolder(KnownFolderType.Downloads).Path;
            string errormessage = "Создать документ не удалось из-за следующих ошибок:\n";
            bool error = false;
            NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;Port=5432;User Id=postgres;Password=123;Database=LabelsServiceDB;");
            string patholdword = path + @"\Документ(Шаблон).docx";
            string pathnewword = path + @"\Документ(Результат).docx";
            string imagespaths = "";
            //Удаление лишних пропусков
            label = label.Replace("⁣", "");
            value = value.Replace("⁣", "");
            //Разделение переменной на массив
            string[] labels = label.Split(',');
            string[] values = value.Split(',');

            try
            {
                for (int i = 0; i < type.Length; i++)
                {
                    //Текст
                    if (type[i] == "Текстовая метка") TextLabels.Add(labels[i], values[i]);
                    //Изображения
                    if (type[i] == "Метка изображения") PictureLabels.Add(labels[i], values[i]);
                    //Таблицы
                    if (type[i] == "Метка таблицы") TableLabels.Add(labels[i], Regex.Split(values[i], "\\, "));
                }
            }
            catch { errormessage += "- Ошибка при сортировке меток\n"; error = true; }
            //Скачивание документа
            try { webClient.DownloadFile(URL, patholdword); }
            catch { errormessage += "- Ошибка при скачивании документа\n"; }
            //Открытие документа для работы
            Document document = new Document();
            try { document.LoadFromFile(patholdword); }
            catch { errormessage += "- Ошибка при открытии документа\n"; }
            //Текст
            try
            {
                if (TextLabels.Any())
                {
                    foreach (var Data in TextLabels) document.Replace("<label>" + Data.Key + "</label>", Data.Value, false, true);
                    TextLabels.Clear();
                }
            }
            catch { errormessage += "- Ошибка при замене текстовых меток\n"; error = true; }
            //Изображения
            try
            {
                if (PictureLabels.Any())
                {
                    foreach (var Data in PictureLabels) webClient.DownloadFile(Data.Value, path + "\\" + Data.Key + ".jpg");

                    int i = 0;

                    foreach (var Data in PictureLabels)
                    {
                        using (var bmpTemp = new Bitmap(path + "\\" + Data.Key + ".jpg"))
                            image[i] = new Bitmap(bmpTemp);
                        i++;
                    }

                    i = 0;

                    foreach (var Data in PictureLabels)
                    {
                        selection[i] = document.FindString("<label>" + Data.Key + "</label>", true, true);
                        DocPicture pic = new DocPicture(document);
                        pic.LoadImage(image[i]);
                        range = selection[i].GetAsOneRange();
                        index = range.OwnerParagraph.ChildObjects.IndexOf(range);
                        range.OwnerParagraph.ChildObjects.Insert(index, pic);
                        range.OwnerParagraph.ChildObjects.Remove(range);
                        i++;
                    }

                    foreach (var Data in PictureLabels)
                    {
                        imagespaths += path + "\\" + Data.Key + ".jpg" + " ";
                    }
                }
            }
            catch { errormessage += "- Ошибка при замене меток изображений\n"; error = true; }
            //Таблицы
            try
            {
                if (TableLabels.Any())
                {
                    Table table = document.Sections[0].Tables[1] as Spire.Doc.Table;
                    TableRow row = table.Rows[1];
                    int length = 0;

                    foreach (var Data in TableLabels)
                    {
                        if (Data.Value.Length > length) length = Data.Value.Length;
                    }

                    for (int i = 0; i < length - 1; i++)
                    {
                        row = table.AddRow();
                    }

                    foreach (var Data in TableLabels)
                    {
                        document.Replace("<label>" + Data.Key + "</label>", Data.Value[0], false, true);

                        for (int s = 2, d = 1; d < Data.Value.Length; s++, d++)
                        {
                            if (Data.Key == "val1") table[s, 0].AddParagraph().AppendText(Data.Value[d]);
                            else if (Data.Key == "val2") table[s, 1].AddParagraph().AppendText(Data.Value[d]);
                            else if (Data.Key == "val3") table[s, 2].AddParagraph().AppendText(Data.Value[d]);
                        }
                    }
                }
            }
            catch { errormessage += "- Ошибка при замене меток таблицы\n"; error = true; }
            //Сохранение документа
            try { document.SaveToFile(pathnewword); }
            catch { errormessage += "- Ошибка при сохранении документа\n"; error = true; }
            //Время работы сервиса
            DateTime stop = DateTime.Now;
            var result = stop - start;
            //Работа с БД
            try
            {
                //Подготовка переменных к созданию запроса
                patholdword = patholdword.Replace("\\", "\\\\");
                pathnewword = pathnewword.Replace("\\", "\\\\");
                imagespaths = imagespaths.Replace("\\", "\\\\");
                string labelsnames = "'{ " + label + " }'";
                string labelsvalues = "'{ " + value + " }'";
                //Добавление записи в БД
                string command1 = $@"INSERT INTO public.""ServiceResults""
                (""FileOld"", ""FileNew"", ""LabelsNames"", ""LabelsValues"", ""WorkingTime"")
                VALUES (bytea('{patholdword}'), bytea('{pathnewword}'), {labelsnames}, {labelsvalues}, interval '{result}');";

                string command2 = $@"INSERT INTO public.""ServiceResults""
                (""FileOld"", ""FileNew"", ""LabelsNames"", ""LabelsValues"", ""Images"", ""WorkingTime"")
                VALUES (bytea('{patholdword}'), bytea('{pathnewword}'), {labelsnames}, {labelsvalues}, bytea('{imagespaths}'), interval '{result}');";
                NpgsqlCommand com;
                if (imagespaths == "") com = new NpgsqlCommand(command1, conn);
                else com = new NpgsqlCommand(command2, conn);
                conn.Open();
                com.ExecuteNonQuery();
                conn.Close();
            }
            catch { errormessage += "- Ошибка при отправке запроса в таблицу базы данных\n"; error = true; }
            //Удаление документа и изображений
            try { System.IO.File.Delete(patholdword); }
            catch { errormessage += "- Ошибка при удалении документа\n"; error = true; }
            try { foreach (var Data in PictureLabels) System.IO.File.Delete(path + "\\" + Data.Key + ".jpg"); }
            catch { errormessage += "- Ошибка при удалении изображений\n"; error = true; }
            //Очищение словарей
            TextLabels.Clear();
            PictureLabels.Clear();
            TableLabels.Clear();
            //Подготовка сообщения
            if (error == true)
            {
                ViewBag.Title = "Error";
                ViewBag.Message = errormessage;
            }
            else
            {
                ViewBag.Title = "Not Error";
                ViewBag.Message = "Документ был успешно создан. Полный путь к документу: " + pathnewword + "\nВремя, затраченное на создание документа: " + result.ToString("hh':'mm':'ss");
            }

            return View();
        }
    }
}
