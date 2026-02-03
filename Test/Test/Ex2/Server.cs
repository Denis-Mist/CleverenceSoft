

namespace Exercise.Ex2
{
    public static class Server
    {
        private static int _count = 0;
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public static int GetCount()
        {
            _lock.EnterReadLock();
            try
            {
                return _count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public static int AddToCount(int value)
        {
            _lock.EnterWriteLock();
            try
            {
                _count += value;
                return _count;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public static void Reset()
        {
            _lock.EnterWriteLock();
            try
            {
                _count = 0;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
