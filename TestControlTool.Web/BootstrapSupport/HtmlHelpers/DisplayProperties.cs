using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using BootstrapSupport;

namespace TestControlTool.Web.BootstrapSupport.HtmlHelpers
{
    public static class DisplayProperties
    {
        public static IHtmlString Display<TModel>(this HtmlHelper<TModel> helper, string expression, bool encodePassword = false)
        {
            var metadata = ModelMetadata.FromStringExpression(expression, helper.ViewData);

            if (metadata.Model != null)
            {
                if (metadata.ModelType == typeof(List<SelectListItem>))
                {
                    var item = ((List<SelectListItem>) metadata.Model).FirstOrDefault(x => x.Selected);

                    return helper.Raw(item != null ? item.Text : string.Empty);
                }

                if (encodePassword && metadata.DataTypeName == DataType.Password.ToString())
                {
                    return helper.Raw(Regex.Replace(metadata.Model.ToString(), ".", "*"));
                }

                return helper.Raw(metadata.Model.ToString());
            }

            return helper.Raw(string.Empty);
        }
    }
}