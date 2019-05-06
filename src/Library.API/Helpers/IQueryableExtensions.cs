using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Services;

namespace Library.API.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source,
            string orderBy, Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            throw  new ArgumentNullException();
        }
    }
}
