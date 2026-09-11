using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Services
{
    public static class GridViewTemplateService
    {
        private const string PageButtonClass = "page-link pointer";
        private const int CacheExpiryMinutes = 20;

        // ── Cache State Persistence ────────────────────────────────────────────────

        private static string GetCacheKey(GridView gridView)
        {
            try
            {
                string sessionID = HttpContext.Current.Session?.SessionID ?? "StaticSession";
                string pageUrl = HttpContext.Current.Request.RawUrl;

                string rawKey = $"GV_PageIndex_{pageUrl}_{gridView.UniqueID}_{sessionID}";

                return $"GV_PageIndex_{HashString(rawKey)}";
            }
            catch
            {
                return $"GV_PageIndex_{gridView.ID}";
            }
        }

        private static string HashString(string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));

                return Convert.ToBase64String(bytes).ToLowerInvariant();
            }
        }


        public static int GetPaginationIndex(GridView gridView)
        {
            string cacheKey = GetCacheKey(gridView);
            object cachedValue = HttpContext.Current.Cache[cacheKey];

            if (cachedValue == null)
            {
                SetPaginationIndex(gridView, 0);
                return 0;
            }

            string eventTarget = HttpContext.Current.Request["__EVENTTARGET"] ?? "";
            string eventArgument = HttpContext.Current.Request["__EVENTARGUMENT"] ?? "";

            if (eventTarget.Equals(gridView.UniqueID, StringComparison.OrdinalIgnoreCase) ||
                eventTarget.Equals(gridView.ClientID, StringComparison.OrdinalIgnoreCase))
            {
                if (eventArgument.StartsWith("Page$", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(eventArgument.Replace("Page$", ""), out int targetPageOneBased))
                    {
                        int newPageIndex = targetPageOneBased - 1;
                        SetPaginationIndex(gridView, newPageIndex);
                        return newPageIndex;
                    }
                }
            }

            return (int)cachedValue;
        }

        public static void SetPaginationIndex(GridView gridView, int index)
        {
            HttpContext.Current.Cache.Insert(
                GetCacheKey(gridView),
                index,
                null,
                DateTime.Now.AddMinutes(CacheExpiryMinutes),
                System.Web.Caching.Cache.NoSlidingExpiration
            );
        }

        // ── Main Entry Method ───────────────────────────────────────────────────

        public static void SetGridView(GridView gridView, object data, Panel paginationPanel, bool showHeader = true)
        {
            int currentPageIndex = GetPaginationIndex(gridView);
            gridView.PageIndex = currentPageIndex;

            ApplyGridViewStyles(gridView, showHeader);

            gridView.DataSource = data;
            gridView.DataBind();

            if (paginationPanel == null) return;

            int currentBatchCount = GetObjectCount(data);

            if (currentBatchCount == 0 && currentPageIndex == 0)
            {
                RenderEmptyState(paginationPanel);
            }
            else
            {
                RenderPaginationPanel(gridView, paginationPanel, currentPageIndex, currentBatchCount);
            }
        }

        private static void ApplyGridViewStyles(GridView gridView, bool showHeader)
        {
            gridView.CssClass = "bnet-table";
            gridView.PagerStyle.CssClass = "d-none";
            gridView.GridLines = GridLines.None;
            gridView.AllowPaging = true;
            gridView.PageSize = GridviewPaginationService.PageSize;
            gridView.ShowHeaderWhenEmpty = showHeader;
            gridView.ShowHeader = showHeader;
            gridView.AutoGenerateColumns = false;
        }

        private static void RenderEmptyState(Panel paginationPanel)
        {
            paginationPanel.Controls.Clear();
            paginationPanel.CssClass = "bnet-table-empty-container text-center p-4";

            var label = new Label
            {
                Text = "No Data Available",
                CssClass = "text-muted font-weight-bold"
            };
            paginationPanel.Controls.Add(label);
        }

        // ── Custom UI Rendering ─────────────────────────────────────────────────

        private static void RenderPaginationPanel(GridView gridView, Panel paginationPanel, int currentPage, int currentBatchCount)
        {
            paginationPanel.Controls.Clear();
            paginationPanel.CssClass = "bnet-table-pagination-container";

            int totalPages = (int)Math.Ceiling((double)currentBatchCount / GridviewPaginationService.PageSize);

            var ul = new HtmlGenericControl("ul");
            ul.Attributes["class"] = "bnet-pagination mb-0 justify-content-center";

            // Previous Button (<)
            RenderPageButton(ul, gridView, currentPage - 1, "&lsaquo;", currentPage == 0);

            // Single Active Page Number Button ([1], [2], etc.)
            var liActive = new HtmlGenericControl("li");
            liActive.Attributes["class"] = "page-item active";

            var spanActive = new HtmlGenericControl("span");
            spanActive.Attributes["class"] = PageButtonClass;
            spanActive.InnerText = (currentPage + 1).ToString();

            liActive.Controls.Add(spanActive);
            ul.Controls.Add(liActive);

            // Next Button (>)
            RenderPageButton(ul, gridView, currentPage + 1, "&rsaquo;", currentPage >= totalPages - 1);

            paginationPanel.Controls.Add(ul);
        }

        private static void RenderPageButton(HtmlGenericControl ul, GridView gridView, int targetPageIndex, string buttonText, bool isDisabled)
        {
            var li = new HtmlGenericControl("li");
            li.Attributes["class"] = "page-item" + (isDisabled ? " disabled" : "");

            if (isDisabled)
            {
                var span = new HtmlGenericControl("span");
                span.Attributes["class"] = PageButtonClass + " disabled";
                span.InnerHtml = buttonText;
                li.Controls.Add(span);
            }
            else
            {
                var anchor = new HtmlGenericControl("a");
                anchor.Attributes["class"] = PageButtonClass;
                anchor.InnerHtml = buttonText;

                int oneBasedPage = targetPageIndex + 1;

                anchor.Attributes["onclick"] = BuildPageClickScript(gridView, oneBasedPage);

                li.Controls.Add(anchor);
            }

            ul.Controls.Add(li);
        }

        private static string BuildPageClickScript(GridView gridView, int oneBased)
        {
            string disableScript = "document.querySelectorAll('.page-link').forEach(el=>{el.style.pointerEvents='none';el.style.opacity='0.6';});";

            string postBack = gridView.Page.ClientScript
                                       .GetPostBackClientHyperlink(gridView, $"Page${oneBased}")
                                       .Replace("javascript:", "");

            return $"{disableScript} {postBack}; return false;";
        }

        private static int GetObjectCount(object data)
        {
            if (data == null) return 0;

            if (data is DataTable dt) return dt.Rows.Count;
            if (data is DataView dv) return dv.Count;
            if (data is DataSet ds && ds.Tables.Count > 0) return ds.Tables[0].Rows.Count;
            if (data is ICollection collection) return collection.Count;
            if (data is IEnumerable enumerable) return enumerable.Cast<object>().Count();

            return 0;
        }
    }
}