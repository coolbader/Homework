namespace Homework
{
    public static class StaticExtentions
    {
        /// <summary>
        /// 已有的获取元素索引的扩展方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetIndex<T>(this SortedSet<T> set, T value)
        {
            return set.GetViewBetween(set.Min, value).Count - 1;
        }

        /// <summary>
        /// 根据序号获取对象的扩展方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static T GetByIndex<T>(this SortedSet<T> set, int index)
        {
            if (set == null)
            {
                throw new ArgumentNullException(nameof(set), "SortedSet 不能为 null");
            }

            if (index < 0 || index >= set.Count)
            {
                throw new IndexOutOfRangeException("索引超出了 SortedSet 的范围");
            }

            // 使用 Skip 和 Take 方法获取指定索引的元素
            return set.Skip(index).Take(1).First();
        }
    }
}
