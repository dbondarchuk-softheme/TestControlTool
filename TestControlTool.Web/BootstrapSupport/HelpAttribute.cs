using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestControlTool.Web.BootstrapSupport
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HelpAttribute : Attribute
    {
        public string Title { get; set; }

        public string Message { get; set; }
    }
}