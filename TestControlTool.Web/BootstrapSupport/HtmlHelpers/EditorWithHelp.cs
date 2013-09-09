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
using BootstrapSupport;

namespace TestControlTool.Web.BootstrapSupport.HtmlHelpers
{
    public static class EditorWithHelp
    {
        public static MvcHtmlString Editor(this HtmlHelper helper, PropertyInfo property, object value = null, object htmlAttributes = null, string desireName = null, string container = null, string parentProperty = null)
        {
            var additionalInfo = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes) ?? new RouteValueDictionary();

            dynamic helpAttribute = property.GetCustomAttributes().FirstOrDefault(x => x.GetType().Name.Contains("HelpAttribute"));
            
            if (helpAttribute != null)
            {
                var title = !string.IsNullOrWhiteSpace(helpAttribute.Title)
                                ? helpAttribute.Title
                                : property.Name.ToSeparatedWords();

                additionalInfo.Add("data-toggle", "popover");
                additionalInfo.Add("data-content", helpAttribute.Message);
                additionalInfo.Add("data-title", title);
                additionalInfo.Add("data-trigger", "focus");
                additionalInfo.Add("rel", "popover");

                if (container != null )
                {
                    additionalInfo.Add("data-container", container);
                }
            }

            foreach (var info in GetDisableInformation(property, parentProperty))
            {
                additionalInfo.Add(info.Key, info.Value);
            }

            var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();

            if (desireName == null && displayAttribute != null && !string.IsNullOrWhiteSpace(displayAttribute.Name))
            {
                desireName = displayAttribute.Name;
            }

            var name = desireName ?? property.Name;


            if (property.PropertyType == typeof(string))
            {
                return EditorForString(helper, property, name, value, additionalInfo);
            }

            if (property.PropertyType.IsEnum)
            {
                return helper.EnumDropDownList(name, property.PropertyType);
            }

            if (property.PropertyType == typeof(bool))
            {
                bool isChecked;

                bool.TryParse((value ?? string.Empty).ToString(), out isChecked);

                additionalInfo["data-trigger"] = "hover";

                return helper.CheckBox(name, isChecked, additionalInfo);
            }

            #region Numbers

            if (property.PropertyType == typeof (int))
            {
                return EditorForNumber(name, int.MinValue.ToString(), int.MaxValue.ToString(), value, additionalInfo);
            }

            if (property.PropertyType == typeof (uint))
            {
                return EditorForNumber(name, uint.MinValue.ToString(), uint.MaxValue.ToString(), value, additionalInfo);
            }

            if (property.PropertyType == typeof (short))
            {
                return EditorForNumber(name, short.MinValue.ToString(), short.MaxValue.ToString(), value, additionalInfo);
            }

            if (property.PropertyType == typeof (ushort))
            {
                return EditorForNumber(name, ushort.MinValue.ToString(), ushort.MaxValue.ToString(), value,
                                       additionalInfo);
            }

            if (property.PropertyType == typeof (byte))
            {
                return EditorForNumber(name, byte.MinValue.ToString(), byte.MaxValue.ToString(), value, additionalInfo);
            }

            if (property.PropertyType == typeof (sbyte))
            {
                return EditorForNumber(name, sbyte.MinValue.ToString(), sbyte.MaxValue.ToString(), value, additionalInfo);
            }

            if (property.PropertyType == typeof (long))
            {
                return EditorForNumber(name, long.MinValue.ToString(), long.MaxValue.ToString(), value, additionalInfo);
            }

            if (property.PropertyType == typeof (ulong))
            {
                return EditorForNumber(name, ulong.MinValue.ToString(), ulong.MaxValue.ToString(), value, additionalInfo);
            }

            #endregion


            return helper.Editor(property.Name);
        }

        public static IDictionary<string, object> GetDisableInformation(PropertyInfo property, string parentProperty = null)
        {
            var dictionary = new Dictionary<string, object>();

            dynamic disableAttributes = property.GetCustomAttributes().Where(x => x.GetType().Name == "DisableAttribute");

            foreach (var disableAttribute in disableAttributes)
            {
                var disablingValue = disableAttribute.Value;
                IEnumerable<string> disablingProperties = ((string)disableAttribute.Properties).Split(',');

                var parentPropertyPrefix = string.IsNullOrWhiteSpace(parentProperty) ? "" : parentProperty + "_";

                dictionary.Add("data-disabling-value", disablingValue.ToString());
                dictionary.Add("data-disabling-property", disablingProperties.Aggregate("", (s, s1) => s + parentPropertyPrefix + s1 + ",").TrimEnd(','));
                dictionary.Add("data-disable-enabled", "true");
            }

            return dictionary;
        }

        private static MvcHtmlString EditorForString(HtmlHelper helper, PropertyInfo property, string desireName, object value = null, RouteValueDictionary additionalInfo = null)
        {
            var dataTypeAttribute = property.GetCustomAttribute<DataTypeAttribute>();

            if (dataTypeAttribute != null && dataTypeAttribute.DataType == DataType.Password)
            {
                return helper.Password(desireName ?? property.Name, value, additionalInfo);
            }

            return helper.TextBox(desireName ?? property.Name, value, additionalInfo);
        }

        private static MvcHtmlString EditorForNumber(string desireName, string minValue, string maxValue, object value = null, RouteValueDictionary additionalInfo = null)
        {
            var input = new TagBuilder("input");

            input.MergeAttribute("type", "number");
            input.MergeAttribute("id", desireName);
            input.MergeAttribute("name", desireName);
            input.MergeAttributes(additionalInfo);
            input.MergeAttribute("min", minValue);
            input.MergeAttribute("max", maxValue);
            input.MergeAttribute("value", (value ?? string.Empty).ToString());
            input.MergeAttribute("onchange", "IsValidNumberField(this, " + minValue + ", " + maxValue + ");");

            return new MvcHtmlString(input.ToString());
        }
    }
}