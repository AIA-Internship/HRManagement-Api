using System.Collections.Generic;
using Microsoft.AspNetCore.Html;

namespace HRManagement.Web.Models.Shared
{
    public class PageHeaderViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? Id { get; set; }
        public List<BreadcrumbItem> Breadcrumbs { get; set; } = new List<BreadcrumbItem>();
        
        /// <summary>
        /// Optional: Render custom HTML into the action slot (right side)
        /// </summary>
        public IHtmlContent? RenderActions { get; set; }
    }

    public class BreadcrumbItem
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = "#";
    }
}
