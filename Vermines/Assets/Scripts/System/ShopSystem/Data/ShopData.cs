using System.Collections.Generic;
using Defective.JSON;
using UnityEngine;

namespace Vermines.ShopSystem.Data {

    using Vermines.ShopSystem.Enumerations;
    using Vermines.CardSystem.Elements;
    using System.Linq;

    public class ShopData : ScriptableObject {

        public void Initialize(int slots)
        {
            Sections = new Dictionary<ShopType, ShopSection>();

            for (int i = 0; i < (int)ShopType.Count; i++)
                Sections.Add((ShopType)i, new ShopSection(slots));
        }

        public Dictionary<ShopType, ShopSection> Sections;

        /// <summary>
        /// Serialize the shop.
        /// </summary>
        /// <example>
        /// [
        ///    {
        ///        type: 0,
        ///        slots: [
        ///           { 0: null },
        ///           { 1:    5 },
        ///           { 2: null },
        ///           { 3:   45 },
        ///           { 4:  155 },
        ///           { 5:    1 }
        ///        ],
        ///        deck: [10,11,12,16,18]
        ///        discard: [2,3,1,7,8]
        ///    },
        ///    {
        ///        type: 1,
        ///        slots: [
        ///           { 0: null },
        ///           { 1:    6 },
        ///           { 2:   19 },
        ///           { 3:   15 },
        ///           { 4:  145 },
        ///           { 5:  111 }
        ///        ],
        ///        deck: [30,31,32,36,38]
        ///        discard: [22,23,21,27,28]
        ///    }
        /// ]
        /// </example>
        /// <returns>The shop's data</returns>
        public string Serialize()
        {
            JSONObject json = new(JSONObject.Type.Array);

            foreach (var section in Sections)
            {
                JSONObject sectionJson = new(JSONObject.Type.Object);

                sectionJson.AddField("type", (int)section.Key);

                json.Add(section.Value.Serialize(sectionJson));
            }

            return json.ToString();
        }

        /// <summary>
        /// Deserialize the shop, please use Initialize before calling this method.
        /// </summary>
        public void Deserialize(string json)
        {
            JSONObject data = new(json);

            for (int i = 0; i < data.count; i++) {
                JSONObject typeJSON = data[i].GetField("type");

                if (typeJSON != null) {
                    ShopType type = (ShopType)typeJSON.intValue;

                    Sections[type].Deserialize(data[i]);
                }
            }
        }

        public bool HasCard(ShopType type, int cardID)
        {
            ShopSection section = Sections[type];

            if (section == null)
                return false;
            return section.HasCard(cardID);
        }

        /// <summary>
        /// Call this methods only if the card can be buy, because it's remove it from the shop
        /// </summary>
        /// <param name="cardID">The card wanted</param>
        /// <returns>The card bought</returns>
        public ICard BuyCard(ShopType type, int cardID)
        {
            ShopSection section = Sections[type];

            if (section == null)
                return null;
            return section.BuyCard(cardID);
        }

        public void FillShop(ShopType type, List<ICard> cards)
        {
            ShopSection section = Sections[type];

            if (section == null)
                return;
            section.Deck = cards;
        }

        public ShopData DeepCopy()
        {
            ShopData shop = ScriptableObject.CreateInstance<ShopData>();

            shop.Sections = this.Sections.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.DeepCopy()
            );

            return shop;
        }
    }
}
