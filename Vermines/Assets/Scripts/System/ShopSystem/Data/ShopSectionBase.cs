using Newtonsoft.Json;
using UnityEngine;

namespace Vermines.ShopSystem.Data {

    using Vermines.CardSystem.Elements;

    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ShopSectionBase : ScriptableObject {

        #region Getters & Setters

        public abstract bool HasCard(int cardId);
        public abstract void SetFree(bool free);

        #endregion

        #region Copy

        public abstract ShopSectionBase DeepCopy();

        #endregion

        #region Methods

        public abstract void Initialize();

        public abstract ICard BuyCard(int cardId);

        public abstract void ApplyReduction(int amount);
        public abstract void RemoveReduction(int amount);

        public virtual ICard ChangeCard(ICard card) => null;

        public virtual void Refill() {}

        public virtual void ReturnCard(ICard card) {}

        public void OnEnable()
        {
            Initialize();
        }

        #endregion
    }
}
