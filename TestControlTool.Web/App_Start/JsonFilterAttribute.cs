using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Web;
using System.Web.Mvc;

namespace TestControlTool.Web.App_Start
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class JsonFilterAttribute : ActionFilterAttribute
    {
        public string Param { get; set; }
        public Type DataType { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var reader = new StreamReader(filterContext.HttpContext.Request.InputStream);
            var val = reader.ReadToEnd();

            reader.BaseStream.Seek(0,SeekOrigin.Begin);

            if (/*filterContext.HttpContext.Request.ContentType.Contains("application/json")*/ true)
            {
                var ser = new DataContractJsonSerializer(DataType);
                var data = ser.ReadObject(filterContext.HttpContext.Request.InputStream);
                filterContext.ActionParameters[Param] = data;

            }
        }
    }

    public class JsonFiltersAttribute : ActionFilterAttribute
    {
        public IEnumerable<string> Params { get; set; }
        public IEnumerable<Type> DataTypes { get; set; }

        public JsonFiltersAttribute(IEnumerable<string> parameters, IEnumerable<Type> dataTypes)
        {
            Params = parameters;
            DataTypes = dataTypes;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Params.Count() != DataTypes.Count()) throw new ArgumentException("Params and DataTypes count doesn't match");

            if (filterContext.HttpContext.Request.ContentType.Contains("application/json"))
            {
                for (var i = 0; i < Params.Count(); i++)
                {
                    var ser = new DataContractJsonSerializer(DataTypes.ElementAt(i));
                    var data = ser.ReadObject(filterContext.HttpContext.Request.InputStream);
                    filterContext.ActionParameters[Params.ElementAt(i)] = data;
                }
            }
        }
    }
}