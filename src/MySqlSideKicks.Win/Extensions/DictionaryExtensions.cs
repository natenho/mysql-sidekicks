namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        public static void AddOrUpdate<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
                return;
            }

            dictionary.Add(key, value);
        }
    }
}
