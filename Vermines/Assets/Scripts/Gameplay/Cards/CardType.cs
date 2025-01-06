using System;
using UnityEngine;

namespace Vermines {

    [Serializable]
    public enum CardType {
        None = -1,

        Bee       = 0,
        Cricket   = 1,
        Equipment = 2,
        Firefly   = 3,
        Fly       = 4,
        Ladybug   = 5,
        Mosquito  = 6,
        Scarab    = 7,
        Tools     = 8,

        Count // This is just a count of the number of card types
    }
}
