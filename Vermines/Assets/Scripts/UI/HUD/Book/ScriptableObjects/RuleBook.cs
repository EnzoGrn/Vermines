using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Vermines.UI.Book
{
    [CreateAssetMenu(menuName = "Book/Rule Book")]
    public class RuleBook : ScriptableObject
    {
        public List<RuleBookSection> sections;

        public RuleBookSection GetSectionByType(RuleBookSectionType type)
        {
            return sections.FirstOrDefault(s => s.type == type);
        }
    }
}