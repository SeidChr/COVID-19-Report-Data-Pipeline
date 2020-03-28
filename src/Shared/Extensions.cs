namespace Corona.Shared
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Extensions
    {
        public static IEnumerable<IEnumerable<TVal>> Rows<TVal>(
            this IEnumerable<IEnumerable<TVal>> multiEnumrable)
        {
            var enumerators = multiEnumrable
                .Select(enumerable => enumerable.GetEnumerator())
                .ToList();

            while (enumerators.All(enumerator => enumerator.MoveNext()))
            {
                yield return enumerators.Select(enumerator => enumerator.Current).ToList();
            }
        }

        public static void AddTo<TVal>(
            this IEnumerable<TVal> enumerable,
            List<TVal> list)
            => list.AddRange(enumerable);
        
        public static IEnumerable<KeyValuePair<string, List<T>>> WhereRegionIn<T>(
                this IEnumerable<KeyValuePair<string, List<T>>> plotDataSets, 
                List<string> regions) 
                => plotDataSets.Where(pd => regions.Contains(pd.Key));

        public static IEnumerable<KeyValuePair<string, List<T>>> WhereRegionNotIn<T>(
            this IEnumerable<KeyValuePair<string, List<T>>> plotDataSets, 
            List<string> regions) 
            => plotDataSets.Where(pd => !regions.Contains(pd.Key));

        public static double[] Diff(this double[] values)
        {
            for (int i = values.Length - 1; i > 0; i--) 
            {
                values[i] = values[i] - values[i - 1];
            }

            return values;
        }
    }
}