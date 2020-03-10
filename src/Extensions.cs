namespace Corona
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Extensions
    {
        public static IEnumerable<IEnumerable<TVal>> Rows<TVal>(this IEnumerable<IEnumerable<TVal>> multiEnumrable)
        {
            var enumerators = multiEnumrable
                .Select(enumerable => enumerable.GetEnumerator())
                .ToList();

            while (enumerators.All(enumerator => enumerator.MoveNext()))
            {
                yield return enumerators.Select(enumerator => enumerator.Current).ToList();
            }
        }
    }
}