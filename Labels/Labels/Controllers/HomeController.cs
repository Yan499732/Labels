using Labels.Models;
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

namespace Labels.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public LabelsLogicModel la = new LabelsLogicModel();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public string Index(string URL, string labels)
        {
            WebClient webClient = new WebClient();
            Dictionary<string, string> TextLabels = new Dictionary<string, string>();
            Dictionary<string, string> PictureLabels = new Dictionary<string, string>();
            Dictionary<string, string[]> TableLabels = new Dictionary<string, string[]>();
            string[] TableMas = new string[100];
            string Path = new KnownFolder(KnownFolderType.Downloads).Path;

            var re = new Regex(" = ");
            var re2 = new Regex(@"""");
            var re3 = new Regex(@"\[");
            var re4 = new Regex(@"\]");
            string[] labels1 = labels.Split('\n');

            for (int i = 0; i < labels1.Length; i++)
            {
                labels1[i] = re.Replace(labels1[i], ";");
                labels1[i] = labels1[i].Substring(0, labels1[i].Length - 2);

                //Текст
                if (labels1[i].IndexOf(@"""") != -1 && labels1[i].IndexOf(@"[") == -1)
                {
                    labels1[i] = re2.Replace(labels1[i], "");
                    TextLabels.Add(
                        labels1[i].Substring(0, labels1[i].IndexOf(";")), //Ключ
                        labels1[i].Substring(labels1[i].IndexOf(";") + 1) //Значение
                        );
                }

                //Изображения
                else if (labels1[i].IndexOf(@"""") == -1 && labels1[i].IndexOf(@"[") == -1 && !string.IsNullOrWhiteSpace(labels1[i]))
                {
                    PictureLabels.Add(
                        labels1[i].Substring(0, labels1[i].IndexOf(";")), //Ключ
                        labels1[i].Substring(labels1[i].IndexOf(";") + 1) //Значение
                        );
                }

                //Таблицы
                else if (labels1[i].IndexOf(@"""") != -1 && labels1[i].IndexOf(@"[") != -1)
                {
                    labels1[i] = re2.Replace(labels1[i], "");
                    labels1[i] = re3.Replace(labels1[i], "");
                    labels1[i] = re4.Replace(labels1[i], "");
                    TableMas = Regex.Split(labels1[i].Substring(labels1[i].IndexOf(";") + 1), "\\, ");
                    TableLabels.Add(
                         labels1[i].Substring(0, labels1[i].IndexOf(";")), //Ключ
                         Regex.Split(labels1[i].Substring(labels1[i].IndexOf(";") + 1), "\\, ") //Значение
                        );
                    TableMas = null;
                }
            }

            webClient.DownloadFile(URL, Path + "\\Документ.docx");

            Document document = new Document();
            document.LoadFromFile(Path + "\\Документ.docx");

            //Текст
            if (TextLabels.Any()) foreach (var Data in TextLabels) document.Replace("<label>" + Data.Key + "</label>", Data.Value, false, true);

            //Изображения
            if (PictureLabels.Any())
            {
                foreach (var Data in PictureLabels) webClient.DownloadFile(Data.Value, Path + "\\" + Data.Key + ".jpg");
                Image[] image = new Image[100];
                int i = 0;
                foreach (var Data in PictureLabels)
                {
                    image[i] = Image.FromFile(Path + "\\" + Data.Key + ".jpg");
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
                foreach (var Data in PictureLabels) System.IO.File.Delete(Path + "\\" + Data.Key + ".jpg");
            }

            //Таблицы
            if (TableLabels.Any())
            {
                Table table = document.Sections[0].Tables[1] as Spire.Doc.Table;
                TableRow row;
                int length = 0;

                foreach (var Data in TableLabels)
                {
                    if (Data.Value.Length > length) length = Data.Value.Length;
                }

                for (int i = 0; i < length; i++)
                {
                    row = table.AddRow();
                }

                //table.Rows.Insert(1, row);

                //table.AddRow(true, 2);
                //table.AddRow(false, 2);

                foreach (var Data in TableLabels)
                {

                }
            }

            document.SaveToFile(Path + "\\Документ.docx");

            return URL + ", " + labels;
        }
    }
}
