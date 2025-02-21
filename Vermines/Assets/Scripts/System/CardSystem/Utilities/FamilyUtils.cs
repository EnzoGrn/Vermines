using System.Collections.Generic;
using System.Linq;

namespace Vermines.CardSystem.Utilities {

    using Vermines.CardSystem.Enumerations;

    static public class FamilyUtils {

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
            List<CardFamily> availableFamilies = new() {
                CardFamily.Ladybug,
                CardFamily.Scarab,
                CardFamily.Fly,
                CardFamily.Cricket
            };
            List<CardFamily> families = new();

            for (int i = 0; i < numberOfPlayer; i++) {
                int index = random.Next(availableFamilies.Count);

                families.Add(availableFamilies[index]);
                availableFamilies.RemoveAt(index);
            }

            return families;
        }
    }
}
