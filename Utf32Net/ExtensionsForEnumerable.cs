using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utf32Net
{
    static class ExtensionsForEnumerable
    {
        //public static IEnumerable<T[]> Split<T>(this IEnumerable<T> input, T delimiter, IEqualityComparer<T>? comparer = null)
        //{
        //    comparer ??= EqualityComparer<T>.Default;
        //    var buffer = new List<T>();
        //    foreach(T element in input)
        //    {
        //        if (comparer.Equals(element, delimiter))
        //        {
        //            yield return buffer.ToArray();
        //            buffer.Clear();
        //        }
        //        else
        //        {
        //            buffer.Add(element);
        //        }
        //    }
        //    yield return buffer.ToArray();
        //}

        //public static IEnumerable<T[]> Split<T>(this IEnumerable<T> input, T[] delimiters, IEqualityComparer<T>? comparer = null)
        //{
        //    comparer ??= EqualityComparer<T>.Default;
        //    var buffer = new List<T>();
        //    foreach (T element in input)
        //    {
        //        if (delimiters.Any(delim => comparer.Equals(element, delim)))
        //        {
        //            yield return buffer.ToArray();
        //            buffer.Clear();
        //        }
        //        else
        //        {
        //            buffer.Add(element);
        //        }
        //    }
        //    yield return buffer.ToArray();
        //}

        public static IEnumerable<T[]> Split<T>(this IEnumerable<T> input, ISet<T> delimiters)
        {
            var buffer = new List<T>();
            foreach (T element in input)
            {
                if (delimiters.Contains(element))
                {
                    yield return buffer.ToArray();
                    buffer.Clear();
                }
                else
                {
                    buffer.Add(element);
                }
            }
            yield return buffer.ToArray();
        }

        public static IEnumerable<T[]> SplitInclusive<T>(this IEnumerable<T> input, ISet<T> delimiters)
        {
            var buffer = new List<T>();
            foreach (T element in input)
            {
                if (delimiters.Contains(element))
                {
                    yield return buffer.ToArray();
                    yield return new[] { element };
                    buffer.Clear();
                }
                else
                {
                    buffer.Add(element);
                }
            }
            yield return buffer.ToArray();
        }
    }
}
