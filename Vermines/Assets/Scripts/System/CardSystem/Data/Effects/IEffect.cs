using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.CardSystem.Data.Effect {

    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;

    public interface IEffect {

        /// <summary>
        /// The description of the effect.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The type of effect.
        /// </summary>
        EffectType Type { get; }

        AEffect SubEffect { get; set; }

        #region Methods

        /// <summary>
        /// Initialize the effect with the card.
        /// </summary>
        /// <param name="card">The card to link to the effect</param>
        void Initialize(ICard card);

        /// <summary>
        /// Function for played the effect.
        /// </summary>
        void Play(PlayerRef player);

        /// <summary>
        /// Function that stop an effect, for example stoping a reduction in market.
        /// </summary>
        void Stop(PlayerRef player);

        /// <summary>
        /// Function called after an RPC, if needed.
        /// </summary>
        void NetworkEventFunction(PlayerRef player);

        /// <summary>
        /// Function for draw the effect.
        /// </summary>
        List<(string, Sprite)> Draw();

        #endregion
    }

    [System.Serializable]
    public abstract class AEffect : ScriptableObject, IEffect {

        /// <summary>
        /// The reference of the card.
        /// </summary>
        public ICard Card { get; set; }

        /// <summary>
        /// The description of the effect.
        /// </summary>
        virtual public string Description { get; set; }

        /// <summary>
        /// The type of effect.
        /// </summary>
        virtual public EffectType Type { get; set; } = EffectType.None;

        /// <summary>
        /// The sub effect of the effect.
        /// </summary>
        virtual public AEffect SubEffect { get; set; }

        #region Methods

        public void Initialize(ICard card)
        {
            Card = card;
        
            UpdateDescription();
        }

        public virtual void Play(PlayerRef player)
        {
            if (SubEffect != null)
                SubEffect.Play(player);
        }

        public virtual void Stop(PlayerRef player)
        {
            if (SubEffect != null)
                SubEffect.Stop(player);
        }

        public virtual void NetworkEventFunction(PlayerRef player)
        {
            if (SubEffect != null)
                SubEffect.NetworkEventFunction(player);
        }

        public virtual List<(string, Sprite)> Draw()
        {
            return new List<(string, Sprite)>();
        }

        #endregion

        #region Editor

        protected virtual void UpdateDescription()
        {
            if (SubEffect != null)
                Description = SubEffect.Description;
        }

        virtual public void OnValidate() {}

        #endregion
    }
}
