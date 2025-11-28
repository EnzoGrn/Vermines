using TMPro;

namespace Vermines.Extension {

    public static class TextMeshProExtension {

        public static void SetTextSafe(this TextMeshProUGUI @this, string text)
        {
            if (@this == null)
                return;
            @this.text = text;
        }

        public static string GetTextSafe(this TextMeshProUGUI @this)
        {
            if (@this == null)
                return null;
            return @this.text;
        }
    }
}
