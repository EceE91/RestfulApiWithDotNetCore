using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Library.API.Helpers
{
    public class ArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // binder works only enumerable types
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            // get the inputted value through the value provider
            // in my case the value is list of GUIDs
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();

            // if that value is null or whitespace, return null
            if (string.IsNullOrEmpty(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            // the value isn't null or whitespace
            // and the type of the model is enumerable
            // get the enumerable's type, and a converter
            var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0]; // in my case this should return GUID
            var converter = TypeDescriptor.GetConverter(elementType);

            // convert each item in the value list to the enumerable types
            var values = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => converter.ConvertFromString(x.Trim())).ToArray(); // array of GUIDs are created


            // create an array of that type, and set it as the Model Value
            // element type is GUID, and the lenght is the author id amount we entered
            // create a new empty array with type GUID and with the lenght of IDs
            var typedValues = Array.CreateInstance(elementType, values.Length); // create dynamically-created array, like list<>
            // then copy all ID values to new created array
            values.CopyTo(typedValues, 0);
            bindingContext.Model = typedValues;

            // return a successful result, pasing in the model
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;

        }
    }
}
