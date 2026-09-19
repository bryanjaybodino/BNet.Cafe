using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace BNet.Cafe.Server.Services
{
    public class FileJsHelpler
    {
        public static string ScriptVersion(string file)
        {
            var page = HttpContext.Current;
            string src = file + "?v=" + System.IO.File.GetLastWriteTime(page.Server.MapPath("~/" + file)).Ticks.ToString();
            return $"<script src='{src}' defer='defer'></script>";
        }

        public static void BundleBNetPageScripts(ScriptManager ScriptManager1)
        {
            ScriptManager1.CompositeScript.Scripts.Add(new ScriptReference("~/Assets/Pages/Script.js"));
            ScriptManager1.CompositeScript.Scripts.Add(new ScriptReference("~/Assets/BNetSelect/Script.js"));
            ScriptManager1.CompositeScript.Scripts.Add(new ScriptReference("~/Assets/BNetModal/Script.js"));
            ScriptManager1.CompositeScript.Scripts.Add(new ScriptReference("~/Assets/BNetAlert/Script.js"));
            ScriptManager1.CompositeScript.Scripts.Add(new ScriptReference("~/Assets/BNetPageLoader/Script.js"));
            ScriptManager1.CompositeScript.Scripts.Add(new ScriptReference("~/Assets/BNetDatePicker/Script.js"));
        }
    }
}