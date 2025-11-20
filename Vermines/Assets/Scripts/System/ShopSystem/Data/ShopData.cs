using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Vermines.ShopSystem.Data {

    using Vermines.ShopSystem.Enumerations;
    using Vermines.CardSystem.Elements;

    [JsonObject(MemberSerialization.OptIn)]
    public class ShopData : ScriptableObject {

        #region Properties

        [JsonProperty]
        public Dictionary<ShopType, ShopSectionBase> Sections = new();

        #endregion

        #region Initialization

        public void Initialize()
        {
            Sections ??= new();
        }

        public void AddSection(ShopType type, ShopSectionBase section)
        {
            Initialize();

            if (Sections.ContainsKey(type))
                Sections[type] = section;
            else
                Sections.Add(type, section);
        }

        #endregion

        #region Serialization

        public string Serialize()
        {
            JsonSerializerSettings settings = new() {
                TypeNameHandling = TypeNameHandling.Auto, // Keep sub-classes (CourtyardSection / MarketSection).
                Converters       = new List<JsonConverter> {
                    new CardJsonConverter()
                }
            };

            return JsonConvert.SerializeObject(this, settings);
        }

        public string SerializeSection(ShopType type)
        {
            if (!Sections.TryGetValue(type, out var section))
                return "{}";
            JsonSerializerSettings settings = new() {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters       = new List<JsonConverter> {
                    new CardJsonConverter()
                }
            };

            return JsonConvert.SerializeObject(section, settings);
        }

        public void DeserializeSection(ShopType type, string data)
        {
            JsonSerializerSettings settings = new() {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters       = new List<JsonConverter> {
                    new CardJsonConverter()
                }
            };

            if (type == ShopType.Courtyard) {
                var section = JsonConvert.DeserializeObject<CourtyardSection>(data, settings);

                AddSection(type, section);
            } else if (type == ShopType.Market) {
                var section = JsonConvert.DeserializeObject<MarketSection>(data, settings);

                AddSection(type, section);
            }
        }

        public static ShopData Deserialize(string json)
        {
            JsonSerializerSettings settings = new() {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters       = new List<JsonConverter> {
                    new CardJsonConverter()
                }
            };

            ShopData data = JsonConvert.DeserializeObject<ShopData>(json, settings);

            return data;
        }

        #endregion

        #region Getters & Setters

        public bool HasCard(ShopType type, int cardId)
        {
            if (!Sections.TryGetValue(type, out ShopSectionBase section))
                return false;
            return section.HasCard(cardId);
        }

        public void SetFree(ShopType type, bool free)
        {
            if (Sections.TryGetValue(type, out var section))
                section.SetFree(free);
        }

        public Dictionary<int, ICard> GetDisplayCards(ShopType type)
        {
            if (!Sections.TryGetValue(type, out var section))
                return new Dictionary<int, ICard>();
            if (section is CourtyardSection courtyard)
                return new Dictionary<int, ICard>(courtyard.AvailableCards);
            if (section is MarketSection market) {
                Dictionary<int, ICard> result = new();

                foreach (var kvp in market.CardPiles)
                    result[kvp.Key] = kvp.Value.Count > 0 ? kvp.Value[^1] : null;
                return result;
            }

            Debug.LogWarning($"[ShopData.GetDisplayCards({type})]: type not handle.");

            return new Dictionary<int, ICard>();
        }

        #endregion

        #region Methods

        public ICard BuyCard(ShopType type, int cardID)
        {
            if (!Sections.TryGetValue(type, out var section))
                return null;
            return section.BuyCard(cardID);
        }

        public void ApplyReduction(ShopType type, int amount)
        {
            if (Sections.TryGetValue(type, out var section))
                section.ApplyReduction(amount);
        }

        public void RemoveReduction(ShopType type, int amount)
        {
            if (Sections.TryGetValue(type, out var section))
                section.RemoveReduction(amount);
        }

        #endregion

        #region Utilities

        public ShopData DeepCopy()
        {
            ShopData shop = ScriptableObject.CreateInstance<ShopData>();

            if (Sections == null)
                return shop;
            shop.Sections = Sections.ToDictionary(entry => entry.Key, entry => entry.Value.DeepCopy());

            return shop;
        }

        public void CopyFrom(ShopData other)
        {
            if (other == null)
                return;
            Sections = other.Sections;
        }

        public void Clear()
        {
            Sections?.Clear();
        }

        #endregion
    }
}
