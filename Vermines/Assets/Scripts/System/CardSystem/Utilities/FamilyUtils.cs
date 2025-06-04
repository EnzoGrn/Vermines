using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace Vermines.CardSystem.Utilities {

    using Vermines.CardSystem.Enumerations;

    static public class FamilyUtils {

        static public List<CardFamily> AvailableFamilies = new() {
            CardFamily.Ladybug,
            CardFamily.Scarab,
            CardFamily.Fly,
            CardFamily.Cricket
        };

        static public int[] FamiliesListToIds(List<CardFamily> families)
        {
            int[] familiesIds = families.Select(family => (int)family).ToArray();

            return familiesIds;
        }

        static public List<CardFamily> FamiliesIdsToList(int[] familiesIds)
        {
            List<CardFamily> families = familiesIds.Select(familyId => (CardFamily)familyId).ToList();

            return families;
        }

        static public List<CardFamily> GenerateFamilies(int seed, int numberOfPlayer)
        {
            System.Random random = new(seed);
            List<CardFamily> availableFamilies = new(AvailableFamilies);
            List<CardFamily> families          = new();

            for (int i = 0; i < numberOfPlayer; i++) {
                int index = random.Next(availableFamilies.Count);

                families.Add(availableFamilies[index]);
                availableFamilies.RemoveAt(index);
            }

            return families;
        }

        static public List<CardFamily> GenerateFamilies(int seed, int numberOfPlayer, List<CardFamily> playersFamily)
        {
            int alreadyHaveFamily = playersFamily.Count;

            if (alreadyHaveFamily == numberOfPlayer)
                return playersFamily;
            System.Random random = new(seed);

            List<CardFamily> availableFamilies = new(AvailableFamilies);

            foreach (CardFamily family in playersFamily) {
                if (availableFamilies.Contains(family))
                    availableFamilies.Remove(family);
            }

            List<CardFamily> families = new(playersFamily);

            for (int i = alreadyHaveFamily; i < numberOfPlayer; i++) {
                int index = random.Next(availableFamilies.Count);

                families.Add(availableFamilies[index]);
                availableFamilies.RemoveAt(index);
            }

            return families;
        }

        static public Sprite GetSpriteByFamily(CardType type, CardFamily family, string spriteWanted)
        {
            string folderPath = $"Sprites/Card/";

            if (type == CardType.Equipment || type == CardType.Tools)
                folderPath += $"{type}";
            else if (type == CardType.Partisan) {
                if (family == CardFamily.None)
                    return null;
                folderPath += $"{family}";
            } else
                return null;
            Sprite[] sprites = Resources.LoadAll<Sprite>(folderPath);

            if (sprites == null || sprites.Length == 0) {
                Debug.LogWarning($"No sprites found in Resources/{folderPath}");

                return null;
            }
            string targetName = Path.GetFileNameWithoutExtension(spriteWanted);
            Sprite sprite     = sprites.FirstOrDefault(s => s.name == targetName);

            if (sprite == null)
                Debug.LogWarning($"Sprite named {targetName} not found in Resources/{folderPath}");
            return sprite;
        }
    }
}
