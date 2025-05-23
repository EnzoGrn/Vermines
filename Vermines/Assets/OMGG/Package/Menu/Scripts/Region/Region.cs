using OMGG.Menu.Region;
using System.Collections.Generic;

namespace OMGG.Menu.Region {

    public class Region {

        /// <summary>
        /// Find the region with the lowest ping.
        /// </summary>
        /// <param name="regions">List of all available region</param>
        /// <returns>The index of the best available region</returns>
        public static int FindBestAvailableOnlineRegionIndex(List<OnlineRegion> regions)
        {
            int lowestPing = int.MaxValue;
            int index      = -1;

            for (int i = 0; regions != null && i < regions.Count; i++) {
                if (regions[i].Ping < lowestPing) {
                    lowestPing = regions[i].Ping;
                    index      = i;
                }
            }

            return index;
        }
    }
}
