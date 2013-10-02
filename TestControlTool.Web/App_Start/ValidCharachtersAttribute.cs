using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace TestControlTool.Web.App_Start
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ValidCharachtersAttribute : RegularExpressionAttribute
    {
        static ValidCharachtersAttribute()
        {
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(ValidCharachtersAttribute), typeof(RegularExpressionAttributeAdapter));
        }

        public ValidCharachtersAttribute() : base(@"^[^<>#%\?]+$")
        {
            ErrorMessage = @"Sorry, but you can't use this characters: < > # % \ ?";
        }
    }
}