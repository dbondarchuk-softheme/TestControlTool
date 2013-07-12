using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace TestControlTool.Web.BootstrapSupport.HtmlHelpers
{
    public static class EditorWithHelp
    {
        public static MvcHtmlString Editor(this HtmlHelper helper, PropertyInfo property, object value = null, object htmlAttributes = null)
        {
            var additionalInfo = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes) ?? new RouteValueDictionary();

            dynamic helpAttribute = property.GetCustomAttributes().FirstOrDefault(x => x.GetType().Name.Contains("HelpAttribute"));
            
            if (helpAttribute != null)
            {
                additionalInfo.Add("data-toogle", "popover");
                additionalInfo.Add("data-content", helpAttribute.Message);
                additionalInfo.Add("data-title", helpAttribute.Title);
                additionalInfo.Add("data-trigger", "focus");
                additionalInfo.Add("rel", "popover");
            }

            var dataTypeAttribute = property.GetCustomAttribute<DataTypeAttribute>();

            if (dataTypeAttribute == null || dataTypeAttribute.DataType != DataType.Password)
            {
                return helper.TextBox(property.Name, value, additionalInfo);
            }
            else if (dataTypeAttribute.DataType == DataType.Password)
            {
                return helper.Password(property.Name, value, additionalInfo);
            }

            return helper.Editor(property.Name);
        }
    }
}