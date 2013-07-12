using System;
using System.Web.Mvc;
using TestControlTool.Web.Models;

namespace TestControlTool.Web
{
    public class MachineModelBinder : DefaultModelBinder
    {
        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, System.Type modelType)
        {
            if (modelType == typeof(MachineModel))
            {
                var values = (ValueProviderCollection)bindingContext.ValueProvider;

                var type = values.GetValue("DestinationType").AttemptedValue == "VCenter" ? typeof(VCenterMachineModel) : typeof(HyperVMachineModel);
                
                var instance = bindingContext.Model ?? base.CreateModel(controllerContext, bindingContext, type);

                bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => instance, type);

                return instance;
            }

            return base.CreateModel(controllerContext, bindingContext, modelType);
        }
    }
}