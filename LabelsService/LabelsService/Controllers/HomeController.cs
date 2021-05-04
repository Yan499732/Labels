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
        public string Index(string URL)
        {
            string label = Request.Form["val1[]"];
            string value = Request.Form["val2[]"];
            string[] type = Request.Form["undefined[]"];
            WebClient webClient = new WebClient();
            Dictionary<string, string> TextLabels = new Dictionary<string, string>();
            Dictionary<string, string> PictureLabels = new Dictionary<string, string>();
            Dictionary<string, string[]> TableLabels = new Dictionary<string, string[]>();
            string path = new KnownFolder(KnownFolderType.Downloads).Path;

            label = label.Replace("⁣", "");
            value = value.Replace("⁣", "");

            string[] labels = label.Split(',');
            string[] values = value.Split(',');

            for (int i = 0; i < type.Length; i++)
            {
                //Текст
                if (type[i] == "Текстовая метка") TextLabels.Add(labels[i], values[i]);
                //Изображения
                if (type[i] == "Метка изображения") PictureLabels.Add(labels[i], values[i]);
                //Таблицы
                if (type[i] == "Метка таблицы") TableLabels.Add(labels[i], Regex.Split(values[i], "\\, "));
            }

            webClient.DownloadFile(URL, path + "\\Документ.docx");

            Document document = new Document();
            document.LoadFromFile(path + "\\Документ.docx");

            //Текст
            if (TextLabels.Any())
            {
                foreach (var Data in TextLabels) document.Replace("<label>" + Data.Key + "</label>", Data.Value, false, true);
                TextLabels.Clear();
            }

            //Изображения
            if (PictureLabels.Any())
            {
                foreach (var Data in PictureLabels) webClient.DownloadFile(Data.Value, path + "\\" + Data.Key + ".jpg");
                Image[] image = new Image[100];
                int i = 0;
                foreach (var Data in PictureLabels)
                {
                    image[i] = Image.FromFile(path + "\\" + Data.Key + ".jpg");
                    i++;
                }

                i = 0;
                int index = 0;
                DocPicture pic = new DocPicture(document);
                TextRange range = null;
                TextSelection[] selections = new TextSelection[100];

                foreach (var Data in PictureLabels)
                {
                    pic.LoadImage(image[i]);
                    selections[i] = document.FindString("<label>" + Data.Key + "</label>", true, true);
                    range = selections[i].GetAsOneRange();
                    index = range.OwnerParagraph.ChildObjects.IndexOf(range);
                    range.OwnerParagraph.ChildObjects.Insert(index, pic);
                    range.OwnerParagraph.ChildObjects.Remove(range);
                    i++;
                }
                i = 0;

                //Удаление загруженных изображений
                //foreach (var Data in PictureLabels) System.IO.File.Delete(Path + "\\" + Data.Key + ".jpg");
            }

            //Таблицы
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

            document.SaveToFile(path + "\\Документ.docx");

            return label + " " + value;
        }
    }
}
