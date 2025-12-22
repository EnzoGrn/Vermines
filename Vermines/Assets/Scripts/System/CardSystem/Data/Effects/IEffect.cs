using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.CardSystem.Data.Effect {
    using System;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Core.Scene;

    public interface IEffect {

        /// <summary>
        /// The reference of the card.
        /// </summary>
        ICard Card { get; set; }

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
        void Initialize(SceneContext context, ICard card);

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
        void NetworkEventFunction(PlayerRef player, string data);

        void OnAction(string ActionMessage, PlayerRef player, ICard card);

        /// <summary>
        /// Function for draw the effect.
        /// </summary>
        List<(string, Sprite)> Draw();

        #endregion
    }

    [System.Serializable]
    public abstract class AEffect : ScriptableObject, IEffect {

        public SceneContext Context;

        /// <summary>
        /// The reference of the card.
        /// </summary>
        virtual public ICard Card { get; set; }

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

        public void Initialize(SceneContext context, ICard card)
        {
            Context = context;
            Card    = card;

            if (SubEffect != null)
                SubEffect.Initialize(context, card);
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

        public virtual void NetworkEventFunction(PlayerRef player, string data)
        {
            if (SubEffect != null)
                SubEffect.NetworkEventFunction(player, data);
        }

        public virtual void OnAction(string ActionMessage, PlayerRef player, ICard card)
        {
            if (SubEffect != null)
                SubEffect.OnAction(ActionMessage, player, card);
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

        #region Comparator

        public override bool Equals(object obj)
        {
            if (obj is not AEffect other)
                return false;
            bool cardEqual = (Card == null && other.Card == null) || (Card != null && other.Card != null && Card.ID == other.Card.ID);

            return Type == other.Type && Description == other.Description && cardEqual;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Description, Card?.ID ?? 0);
        }

        #endregion
    }
}
