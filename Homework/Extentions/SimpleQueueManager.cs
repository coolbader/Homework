namespace Homework
{
    public class SimpleQueueManager
    {
        /// <summary>
        /// 锁
        /// </summary>
        private static ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public void Add(CustomerData data)
        {
            StaticVariable._queue.Enqueue(data);
        }

    }
}
