using System;
using System.Collections;
using System.Linq;
using LinqToEPiServer.Implementation;

namespace LinqToEpiServer.PageTypeBuilder
{
    public class FilterByType<T> : IResultTransformer
    {
        public object Transform(object input)
        {
            if (input == null) throw new ArgumentNullException("input");

            var enumerable = input as IEnumerable;
            if (enumerable == null) throw new InvalidOperationException(string.Format("input must be IEnumerable, was {0}", input.GetType()));
            return enumerable.OfType<T>();
        }
    }
}