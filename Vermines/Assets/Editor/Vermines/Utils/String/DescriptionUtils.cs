using UnityEditor;
using UnityEngine;

namespace Vermines.Editor.Utils {

    static public class DescriptionUtils {

        static public void DrawDescriptionPreview(string description)
        {
            if (description == null)
                return;
            if (description.Equals(string.Empty))
                return;
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Description Preview", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Transform the description into a rich text preview
            string preview = Vermines.Utils.DescriptionUtils.FormatDescription(description);

            // Display the formatted description
            GUIStyle richTextStyle = new(EditorStyles.label) {
                richText = true,
                wordWrap = true
            };
            GUILayout.Label(preview, richTextStyle);

            GUILayout.EndVertical();
        }
    }
}
