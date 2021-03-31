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

            document.Replace("<label>", "123", false, true);

            document.SaveToFile(Path + "\\Документ.docx");

            return URL + ", " + labels;
        }
    }
}
