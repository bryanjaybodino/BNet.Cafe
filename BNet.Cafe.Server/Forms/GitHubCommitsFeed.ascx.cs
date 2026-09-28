using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Xml.Linq;

namespace BNet.Cafe.Server.Forms.Controls
{
    public partial class GitHubCommitsFeed : System.Web.UI.UserControl
    {
        private const string ATOM_FEED_URL = "https://github.com/bryanjaybodino/BNet.Cafe.Timer/commits/main.atom";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Page.RegisterAsyncTask(new PageAsyncTask(LoadCommitsFeedAsync));
            }
        }

        protected void LinkButton_Refresh_Click(object sender, EventArgs e)
        {
            Page.RegisterAsyncTask(new PageAsyncTask(LoadCommitsFeedAsync));
        }

        private async Task LoadCommitsFeedAsync()
        {
            try
            {
                Panel_Error.Visible = false;

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "BNet-Cafe-Server-App");

                    string xmlContent = await client.GetStringAsync(ATOM_FEED_URL);
                    XDocument doc = XDocument.Parse(xmlContent);

                    // XML Namespaces in the Atom Feed
                    XNamespace ns = "http://www.w3.org/2005/Atom";
                    XNamespace mediaNs = "http://search.yahoo.com/mrss/";

                    var commits = doc.Root.Elements(ns + "entry")
                        .Take(10) // Strictly top 10 latest commits
                        .Select(entry =>
                        {
                            string title = entry.Element(ns + "title")?.Value?.Trim() ?? "No commit title";
                            string updatedStr = entry.Element(ns + "updated")?.Value ?? "";
                            string authorName = entry.Element(ns + "author")?.Element(ns + "name")?.Value ?? "Contributor";
                            string commitUrl = entry.Element(ns + "link")?.Attribute("href")?.Value ?? "#";
                            string avatarUrl = entry.Element(mediaNs + "thumbnail")?.Attribute("url")?.Value ?? "";

                            // Extract and decode full commit description/info content
                            string rawContent = entry.Element(ns + "content")?.Value ?? "";
                            string infoText = ExtractCommitContent(rawContent);

                            DateTime.TryParse(updatedStr, out DateTime updatedDate);

                            return new
                            {
                                Title = title.Split('\n')[0], // First line title
                                Info = infoText,             // Full commit message details
                                Author = authorName,
                                AvatarUrl = avatarUrl,
                                UpdatedDate = updatedDate != DateTime.MinValue ? updatedDate.ToString("MMM dd, yyyy HH:mm") : updatedStr,
                                Link = commitUrl
                            };
                        }).ToList();

                    Repeater_Commits.DataSource = commits;
                    Repeater_Commits.DataBind();
                }
            }
            catch (Exception)
            {
                Panel_Error.Visible = true;
                Repeater_Commits.DataSource = null;
                Repeater_Commits.DataBind();
            }
        }

        /// <summary>
        /// Cleans HTML pre-tags and unescapes the HTML content string from Atom feed.
        /// </summary>
        private string ExtractCommitContent(string rawHtml)
        {
            if (string.IsNullOrWhiteSpace(rawHtml)) return string.Empty;

            // Decode HTML entities (e.g. &lt; to <)
            string decoded = HttpUtility.HtmlDecode(rawHtml);

            // Strip HTML tags (<pre>, </pre>, etc.)
            string plainText = Regex.Replace(decoded, "<.*?>", string.Empty).Trim();

            return plainText;
        }
    }
}