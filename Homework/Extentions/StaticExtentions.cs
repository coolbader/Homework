namespace Homework
{
    public static class StaticExtentions
    {
        /// <summary>
        /// 在更新的时候，根据红黑树获取索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetSortedSetIndex<T>(this SortedSet<T> set, T value)
        {
            return set.GetViewBetween(set.Min, value).Count - 1;
        }
    }
}
