using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Services;
using System.Linq.Dynamic.Core;

namespace Library.API.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source,
            string orderBy, Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if(source == null)
                throw new Exception("source");

            if(mappingDictionary==null)
                throw  new Exception("mappingDictionary");

            if (string.IsNullOrEmpty(orderBy))
                return source;

            // the orderBy string is separated by ",", so we split it
            var orderByAfterSplit = orderBy.Split(',');

            // apply each orderby clause in reverse order - otherwise, the
            // IQueryable will be ordered in the wrong order
            foreach (var orderByClause in orderByAfterSplit.Reverse())
            {
                var trimmedOrderBy = orderByClause.Trim();

                // if the sort option ends with " desc", we order descending
                var orderByDesc = trimmedOrderBy.EndsWith(" desc"); 

                // remove " asc" or " desc" from the orderby clause, so we
                // get the property name to look for in the mapping dictionary
                var indexOfFirstSpace = trimmedOrderBy.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? 
                    trimmedOrderBy : trimmedOrderBy.Remove(indexOfFirstSpace);

                // find the matching property
                if(!mappingDictionary.ContainsKey(propertyName))
                    throw  new Exception($"Key mapping for {propertyName} is missing");

                // get the property mapping value
                var propertyMappingValue = mappingDictionary[propertyName];

                if (propertyMappingValue == null)
                {
                    throw  new Exception("propertyMappingValue");
                }

                // run through the property names in reverse
                // so the orderby clauses are applied in the correct order
                foreach (var destinationProperty in propertyMappingValue.DestinationProperties.Reverse())
                {
                    //revert sort order if necessary
                    if (propertyMappingValue.Revert)
                        orderByDesc = !orderByDesc;

                    source = source.OrderBy(destinationProperty + (orderByDesc ? " descending" : " ascending"));
                }

            }

            return source;
        }
    }
}
