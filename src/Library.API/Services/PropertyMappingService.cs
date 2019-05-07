using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Entities;
using Library.API.Models;

namespace Library.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private Dictionary<string, PropertyMappingValue> _authorPropertyMapping
            = new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                {"Id", new PropertyMappingValue(new List<string>() {"Id"})},
                {"Genre", new PropertyMappingValue(new List<string>() {"Genre"})},
                {"Age", new PropertyMappingValue(new List<string>() {"DateOfBirth"}, true)},
                {"Name", new PropertyMappingValue(new List<string>() {"FirstName", "LastName"})}
            };

        // PropertyMapping<TSource,TDestination> is unresolved, to fix this I added marker interface
        private IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            propertyMappings.Add(new PropertyMapping<AuthorDto,Author>(_authorPropertyMapping));
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            // get matching mapping
            var matchingMapping = propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

            var enumerable = matchingMapping as PropertyMapping<TSource, TDestination>[] ?? matchingMapping.ToArray();
            if (enumerable.Count() == 1)
            {
                return enumerable.First()._mappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping for <{typeof(TSource)}");
        }

        // invalid order by statements (eg: order by ece, where property called "ece" does not exist)
        // are caused by server or developers, and not by the consumer of the API
        // so we need to return a proper invalid property error
        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
                return true;

            //the string is separated by ",", so we split it
            var fieldsAfterSplit = fields.Split(",");

            foreach (var field in fieldsAfterSplit)
            {
                var trimmedField = field.Trim();

                // remove everything after the first " " if the fields 
                // are coming from an orderBy string this part must be ignored
                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? trimmedField : trimmedField.Remove(indexOfFirstSpace);

                //find the matching property
                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }

            return true;
        }

    }
}
