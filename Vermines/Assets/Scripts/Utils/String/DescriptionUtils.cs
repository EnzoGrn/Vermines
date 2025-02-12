using UnityEditor;
using UnityEngine;

namespace Vermines.Utils {

    static public class DescriptionUtils {

        /// <summary>
        /// Replace html tags by rich text tags
        /// </summary>
        public static string FormatDescription(string description)
        {
            // Red color for '{number}A'
            description = System.Text.RegularExpressions.Regex.Replace(description, @"\{(\d+)A\}", "<color=red>{$1A}</color>");

            // Purple color for '{number}E' 
            description = System.Text.RegularExpressions.Regex.Replace(description, @"\{(\d+)E\}", "<color=purple>{$1E}</color>");

            // Bold for '<br>'
            description = description.Replace("<br>", "<b><br></b>");

            return description;
        }
    }
}
