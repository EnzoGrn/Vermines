using UnityEngine.Localization;
using UnityEngine;

namespace Vermines.UI.Book
{
    [CreateAssetMenu(menuName = "Book/Localized Rule Page")]
    public class LocalizedRulePage : ScriptableObject
    {
        public LocalizedString title;
        public LocalizedString content;
    }
}