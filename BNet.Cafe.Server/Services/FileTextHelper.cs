using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.Services
{
    public class FileTextHelper
    {
        /// <summary>
        /// Creates a new text file with a single line of content.
        /// </summary>
        static void CreateText(string content, string filePath, string fileName)
        {
            string fullPath = Path.Combine(filePath, $"{fileName}.txt");
            File.WriteAllText(fullPath, content);
        }

        /// <summary>
        /// Updates the content of the existing text file with new content.
        /// </summary>
        public static void UpdateText(string content, string filePath, string fileName)
        {
            string fullPath = Path.Combine(filePath, $"{fileName}.txt");

            if (File.Exists(fullPath))
            {
                File.WriteAllText(fullPath, content);
            }
            else
            {
                throw new FileNotFoundException("Text file not found.", fullPath);
            }
        }

        /// <summary>
        /// Reads and returns the single line of text from the file.
        /// </summary>
        public static string GetText(string filePath, string fileName)
        {
            string fullPath = Path.Combine(filePath, $"{fileName}.txt");
            if (File.Exists(fullPath))
            {
                return File.ReadAllText(fullPath);
            }
            else
            {
                CreateText("", filePath, fileName);
                return string.Empty;
            }
        }

    }
}