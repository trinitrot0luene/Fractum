using System;
using System.Collections.Generic;
using System.Linq;
using Fractum.Entities;

namespace Fractum.Utilities
{
    public static class ListExtensions
    {
        public static void AddOrUpdate<T>(this IList<T> source, Func<T, T, bool> propSelector, T newValue, Action<T> updateAction)
        {
            var existingValue = source.FirstOrDefault(t => propSelector(t, newValue));
            if (existingValue != null)
                updateAction(existingValue);
            else
                source.Add(newValue);
        }
    }
}