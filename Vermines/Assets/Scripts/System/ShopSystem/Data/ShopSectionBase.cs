using Newtonsoft.Json;

namespace Vermines.ShopSystem.Data {

    using Vermines.CardSystem.Elements;

    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ShopSectionBase {

        #region Getters & Setters

        public abstract bool HasCard(int cardId);
        public abstract void SetFree(bool free);

        #endregion

        #region Copy

        public abstract ShopSectionBase DeepCopy();

        #endregion

        #region Methods

        public abstract ICard BuyCard(int cardId);

        public abstract void ApplyReduction(int amount);
        public abstract void RemoveReduction(int amount);

        public virtual ICard ChangeCard(ICard card)
        {
            return null;
        }

        public virtual void Refill() {}

        public virtual void ReturnCard(ICard card) {}

        #endregion
    }
}
