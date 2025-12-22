using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Vermines.Extension {

    public static partial class ListExtension {

        // Complexity: O(1)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveBySwap<T>(this IList<T> list, int index)
        {
            list[index] = list[list.Count - 1];

            list.RemoveAt(list.Count - 1);

            return true;
        }

        public static T Find<T>(this IList<T> list, System.Predicate<T> condition)
        {
            for (int i = 0; i < list.Count; i++)
                if (condition.Invoke(list[i]))
                    return list[i];
            return default;
        }
    }
}
