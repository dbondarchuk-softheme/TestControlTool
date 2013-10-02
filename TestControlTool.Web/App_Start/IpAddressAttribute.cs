using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace TestControlTool.Web.App_Start
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class IpAddressAttribute : RegularExpressionAttribute
    {
        static IpAddressAttribute()
        {
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(IpAddressAttribute), typeof(RegularExpressionAttributeAdapter));
        }

        public IpAddressAttribute() : base(@"^(([01]?\d\d?|2[0-4]\d|25[0-5])\.){3}([01]?\d\d?|25[0-5]|2[0-4]\d)$")
        {
            ErrorMessage = @"Address doesn't match IPv4 standard";
        }
    }
}