using System;

namespace TestControlTool.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HelpAttribute : Attribute
    {
        public string Title { get; set; }

        public string Message { get; set; }
    }
}