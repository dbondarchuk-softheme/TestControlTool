using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BootstrapSupport
{
    /// <summary>
    /// Link for the model property
    /// </summary>
    public class LinkAttribute : Attribute
    {
        /// <summary>
        /// Action name
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Controller name
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// Title for the link
        /// </summary>
        public string Title { get; set; }
    }
}