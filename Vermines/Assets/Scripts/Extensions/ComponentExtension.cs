using UnityEngine;

namespace Vermines.Extension {

    public static partial class ComponentExtension {

        public static void SetActive(this Component component, bool value)
        {
            if (component == null)
                return;
            if (component.gameObject.activeSelf == value)
                return;
            component.gameObject.SetActive(value);
        }
    }
}
