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
        public static MvcHtmlString Editor(this HtmlHelper helper, Type type, string name, object value = null, IDictionary<string, object> htmlAttributes = null, bool isPassword = false)
        {
            var additionalInfo = new RouteValueDictionary(htmlAttributes ?? new Dictionary<string, object>());
            
            if (type == typeof(string))
            {
                return EditorForString(helper, name, isPassword, value, additionalInfo);
            }

            if (type.IsEnum)
            {
                return helper.EnumDropDownList(name, type);
            }

            if (type == typeof(bool))
            {
                bool isChecked;

                bool.TryParse((value ?? string.Empty).ToString(), out isChecked);

                additionalInfo["data-trigger"] = "hover";

                return helper.CheckBox(name, isChecked, additionalInfo);
            }

            #region Numbers

            if (type == typeof(int))
            {
                return EditorForNumber(name, int.MinValue.ToString(), int.MaxValue.ToString(), value, additionalInfo);
            }

            if (type == typeof(uint))
            {
                return EditorForNumber(name, uint.MinValue.ToString(), uint.MaxValue.ToString(), value, additionalInfo);
            }

            if (type == typeof(short))
            {
                return EditorForNumber(name, short.MinValue.ToString(), short.MaxValue.ToString(), value, additionalInfo);
            }

            if (type == typeof(ushort))
            {
                return EditorForNumber(name, ushort.MinValue.ToString(), ushort.MaxValue.ToString(), value,
                                       additionalInfo);
            }

            if (type == typeof(byte))
            {
                return EditorForNumber(name, byte.MinValue.ToString(), byte.MaxValue.ToString(), value, additionalInfo);
            }

            if (type == typeof(sbyte))
            {
                return EditorForNumber(name, sbyte.MinValue.ToString(), sbyte.MaxValue.ToString(), value, additionalInfo);
            }

            if (type == typeof(long))
            {
                return EditorForNumber(name, long.MinValue.ToString(), long.MaxValue.ToString(), value, additionalInfo);
            }

            if (type == typeof(ulong))
            {
                return EditorForNumber(name, ulong.MinValue.ToString(), ulong.MaxValue.ToString(), value, additionalInfo);
            }

            #endregion


            return helper.Editor(name);
        }

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

            /*var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();

            if (desireName == null && displayAttribute != null && !string.IsNullOrWhiteSpace(displayAttribute.Name))
            {
                desireName = displayAttribute.Name;
            }*/

            var name = desireName ?? property.Name;

            var dataTypeAttribute = property.GetCustomAttribute<DataTypeAttribute>();

            var isPassword = dataTypeAttribute != null && dataTypeAttribute.DataType == DataType.Password;

            return Editor(helper, property.PropertyType, name, value, additionalInfo, isPassword);
        }

        public static IDictionary<string, object> GetDisableInformation(PropertyInfo property, string parentProperty = null)
        {
            var dictionary = new Dictionary<string, object>();

            IEnumerable<dynamic> disableAttributes = property.GetCustomAttributes().Where(x => x.GetType().Name == "DisableAttribute").ToList();

            if (disableAttributes.Any())
            {
                dictionary.Add("data-disable-enabled", "true");

                var values = "";
                var properties = "";

                foreach (var disableAttribute in disableAttributes)
                {
                    object[] disablingValues = disableAttribute.Values;
                    string[] disablingProperties = disableAttribute.Properties;

                    var parentPropertyPrefix = string.IsNullOrWhiteSpace(parentProperty) ? "" : parentProperty + "-";

                    if (disablingValues.Length != disablingProperties.Length)
                    {
                        return new Dictionary<string, object>();
                    }

                    for (var i = 0; i < disablingProperties.Length; i++)
                    {
                        properties += parentPropertyPrefix + disablingProperties[i] + ',';
                        values += disablingValues[i].ToString() + ',';
                    }

                    properties = properties.TrimEnd(',') + ';';
                    values = values.TrimEnd(',') + ';';
                }

                properties = properties.TrimEnd(';');
                values = values.TrimEnd(';');
                
                dictionary.Add("data-disabling-values", values);
                dictionary.Add("data-disabling-properties", properties);
            }

            return dictionary;
        }

        public static bool IsSupportedType(Type type)
        {
            return type.IsEnum || type.IsPrimitive || type == typeof (string);
        }

        private static MvcHtmlString EditorForString(HtmlHelper helper, string desireName, bool isPassword = true, object value = null, RouteValueDictionary additionalInfo = null)
        {
            return isPassword ? helper.Password(desireName, value, additionalInfo) : helper.TextBox(desireName, value, additionalInfo);
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