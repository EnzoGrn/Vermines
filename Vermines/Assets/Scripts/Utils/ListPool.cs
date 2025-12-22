using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace Vermines.Utils {

    using Vermines.Extension;

    public partial class ListPool<T> {

        private const int POOL_CAPACITY = 4;
        private const int LIST_CAPACITY = 16;

        public static readonly ListPool<T> Shared = new();

        private List<List<T>> _Pool = new(POOL_CAPACITY);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> Get(int capacity)
        {
            lock (_Pool) {
                int poolCount = _Pool.Count;

                if (poolCount == 0)
                    return new List<T>(capacity > 0 ? capacity : LIST_CAPACITY);
                for (int i = 0; i < poolCount; i++) {
                    List<T> list = _Pool[i];

                    if (list.Capacity < capacity)
                        continue;
                    _Pool.RemoveBySwap(i);

                    return list;
                }
                int lastListIndex = poolCount - 1;

                List<T> lastList = _Pool[lastListIndex];

                lastList.Capacity = capacity;

                _Pool.RemoveAt(lastListIndex);

                return lastList;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(List<T> list)
        {
            if (list == null)
                return;
            list.Clear();

            lock (_Pool) {
                _Pool.Add(list);
            }
        }
    }

    public static class ListPool {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> Get<T>(int capacity)
        {
            return ListPool<T>.Shared.Get(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return<T>(List<T> list)
        {
            ListPool<T>.Shared.Return(list);
        }

    }
}