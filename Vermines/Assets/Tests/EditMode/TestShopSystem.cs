using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

#region Vermines ShopSystem namespace
    using Vermines.ShopSystem.Enumerations;
    using Vermines.ShopSystem.Data;
#endregion

#region Vermines CardSystem namespace
    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Utilities;
    using Vermines.CardSystem.Data;
#endregion

namespace Test.Vermines.ShopSystem {

    public class TestShopSystem {

        [Test]
        public void TestShopDataNetwork()
        {
            // -- Initialize the card
            List<CardFamily> families = FamilyUtils.GenerateFamilies(1234, 2);

            CardSetDatabase.Instance.Initialize(families);

            // -- Initialize the shop
            ShopData shop = ScriptableObject.CreateInstance<ShopData>();

            shop.Initialize(5);

            for (int i = 0; i < 5; i++)
                shop.Sections[ShopType.Market].Deck.Add(CardSetDatabase.Instance.GetCardByID(i));
            shop.Sections[ShopType.Market].Deck.Add(CardSetDatabase.Instance.GetCardByID(5555555)); // -> This card is skip because it doesn't exist!
            for (int i = 5; i < 7; i++)
                shop.Sections[ShopType.Market].DiscardDeck.Add(CardSetDatabase.Instance.GetCardByID(i));
            shop.Sections[ShopType.Market].DiscardDeck.Add(CardSetDatabase.Instance.GetCardByID(5555555)); // -> This card is skip because it doesn't exist!

            for (int i = 7; i < 10; i++)
                shop.Sections[ShopType.Courtyard].Deck.Add(CardSetDatabase.Instance.GetCardByID(i));
            for (int i = 10; i < 20; i++)
                shop.Sections[ShopType.Courtyard].DiscardDeck.Add(CardSetDatabase.Instance.GetCardByID(i));

            shop.Sections[ShopType.Market].AvailableCards[0] = CardSetDatabase.Instance.GetCardByID(21);
            shop.Sections[ShopType.Market].AvailableCards[2] = CardSetDatabase.Instance.GetCardByID(22);
            shop.Sections[ShopType.Market].AvailableCards[3] = CardSetDatabase.Instance.GetCardByID(23);

            // Serialize the shop (first time)
            string data1 = shop.Serialize();
            
            // -- Deserialize the shop
            shop.Deserialize(data1);

            // Serialize the shop (second time)
            string data2 = shop.Serialize();

            // Compare the two serialized data
            Assert.AreEqual(data1, data2);
        }
    }
}
