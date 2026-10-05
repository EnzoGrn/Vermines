using UnityEngine;
using Vermines.CardSystem.Enumerations;

namespace Vermines.UI.Utils
{
    /// <summary>
    /// Provides methods to load card-related sprites from the Resources folder.
    /// </summary>
    /// <remarks>
    /// The sprites must be organized in the Resources/Sprites/Card/[Type or Family]/[spriteName].png format.
    /// </remarks>
    public static class UISpriteLoader
    {
        /// <summary>
        /// Loads the default sprite for a card based on its type and family.
        /// The sprite must be located under Resources/Sprites/Card/[Type or Family]/[spriteName].png
        /// </summary>
        /// <param name="type">The card type (e.g., Equipment, Tools, Partisan).</param>
        /// <param name="family">The card family (used only if type is Partisan).</param>
        /// <param name="spriteName">The name of the sprite file, without extension.</param>
        /// <returns>The loaded sprite, or null if not found or invalid input.</returns>
        public static Sprite GetDefaultSprite(CardType type, CardFamily family, string spriteName)
        {
            string path = "Sprites/Card/";

            if (type == CardType.Equipment || type == CardType.Tools)
            {
                path += $"{type}/";
            }
            else if (type == CardType.Partisan)
            {
                if (family == CardFamily.None)
                    return null;
                path += $"{family}/";
            }
            else
            {
                return null;
            }

            path += spriteName;

            return Resources.Load<Sprite>(path);
        }

        /// <summary>
        /// Loads the sprite icon associated with a given card type.
        /// The sprite must be located under Resources/Sprites/Card/Icons/[Type].png
        /// </summary>
        /// <param name="type">The card type.</param>
        /// <returns>The loaded sprite, or null if not found.</returns>
        public static Sprite GetTypeIconSprite(CardType type)
        {
            string path = $"Sprites/Card/Icons/{type}";
            return Resources.Load<Sprite>(path);
        }

        /// <summary>
        /// Loads the sprite icon associated with a given card family.
        /// The sprite must be located under Resources/Sprites/Card/Icons/Family/[Family].png
        /// </summary>
        /// <param name="family">The card family.</param>
        /// <returns>The loaded sprite, or null if not found or family is None.</returns>
        public static Sprite GetFamilyIconSprite(CardFamily family)
        {
            if (family == CardFamily.None)
                return null;

            string path = $"Sprites/Card/Icons/Family/{family}";
            return Resources.Load<Sprite>(path);
        }
    }
}