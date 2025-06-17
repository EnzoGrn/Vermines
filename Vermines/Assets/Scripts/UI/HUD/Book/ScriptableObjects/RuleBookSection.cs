using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine;

namespace Vermines.UI.Book
{
    [CreateAssetMenu(menuName = "Book/Rule Book Section")]
    public class RuleBookSection : ScriptableObject
    {
        public RuleBookSectionType type;
        public LocalizedString sectionName;
        public List<LocalizedRulePage> pages;
    }
}