using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BootstrapSupport.HtmlHelpers
{
    public static class Linker
    {
        public static MvcHtmlString Link(this HtmlHelper helper, PropertyInfo property, object value, object model = null, Dictionary<string, string> htmlAttributes = null)
        {
            var linkAttribue = property.GetCustomAttribute<LinkAttribute>();

            string text;

            if (property.PropertyType == typeof(List<SelectListItem>))
            {
                var item = ((List<SelectListItem>)value).FirstOrDefault(x => x.Selected);

                text = item != null ? item.Text : string.Empty;
            }
            else
            {
                text = value.ToString();
            }
            
            if (linkAttribue == null)
            {
                var span = new TagBuilder("span");
                span.SetInnerText(text);

                return MvcHtmlString.Create(span.ToString());
            }
            
            var a = new TagBuilder("a");
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            a.MergeAttribute("href", urlHelper.Action(linkAttribue.Action, linkAttribue.Controller, model));
            a.InnerHtml = text;

            if (!string.IsNullOrWhiteSpace(linkAttribue.Title))
            {
                a.MergeAttribute("data-toggle", "tooltip");
                a.MergeAttribute("title", linkAttribue.Title);
                a.MergeAttribute("rel", "tooltip");
            }

            if (htmlAttributes != null) a.MergeAttributes(htmlAttributes);

            return MvcHtmlString.Create(a.ToString());
        }
    }
}