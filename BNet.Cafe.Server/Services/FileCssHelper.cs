using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace BNet.Cafe.Server.Services
{
    public class FileCssHelper
    {
        public static string StyleSheetVersion(string file)
        {
            var page = HttpContext.Current;
            string href = file + "?v=" + System.IO.File.GetLastWriteTime(page.Server.MapPath("~/" + file)).Ticks.ToString();
            return $"<link href='{href}' rel='stylesheet' />";
        }
        public static void BundleCss()
        {
            string cssOutput = @"\bundle.min.css";
            string rootDirectory = HttpContext.Current.Server.MapPath("~/Assets");
            string outputFile = rootDirectory + cssOutput;

            var cssFiles = Directory.GetFiles(rootDirectory, "*.css", SearchOption.AllDirectories);

            StringBuilder finalOutput = new StringBuilder();

            foreach (var file in cssFiles)
            {
                if (Stylesheets(file))
                {
                    string content = File.ReadAllText(file);
                    string minified = MinifyCss(content);

                    finalOutput.AppendLine($"/* {Path.GetFileName(file)} */");
                    finalOutput.AppendLine(minified);
                }
            }

            string newContent = finalOutput.ToString();

            // Only write if content differs
            if (!File.Exists(outputFile) || File.ReadAllText(outputFile) != newContent)
            {
                File.WriteAllText(outputFile, newContent);
            }
        }


        static bool Stylesheets(string file)
        {
            file = file.Replace("\\", "/");

            return
                   file.Contains("Assets/Pages/Style.css")
                || file.Contains("Assets/BNetChart/Style.css")
                || file.Contains("Assets/BNetSelect/Style.css")
                || file.Contains("Assets/BNetModal/Style.css")
                || file.Contains("Assets/BNetAlert/Style.css")
                || file.Contains("Assets/BNetPageLoader/Style.css")
                || file.Contains("Assets/BNetTable/Style.css")
                || file.Contains("Assets/BNetDropdown/Style.css")
                || file.Contains("Assets/BNetBadge/Style.css")
                || file.Contains("Assets/BNetDatePicker/Style.css");
        }

        static string MinifyCss(string css)
        {
            css = Regex.Replace(css, @"/\*.*?\*/", "", RegexOptions.Singleline);
            css = css.Replace("\r", "").Replace("\n", "").Replace("\t", "");
            css = Regex.Replace(css, @"\s+", " ");
            css = Regex.Replace(css, @"\s*{\s*", "{");
            css = Regex.Replace(css, @"\s*}\s*", "}");
            css = Regex.Replace(css, @"\s*;\s*", ";");
            css = Regex.Replace(css, @"\s*:\s*", ":");
            css = Regex.Replace(css, @"\s*,\s*", ",");
            css = Regex.Replace(css, @";}", "}");
            return css.Trim();
        }
    }
}