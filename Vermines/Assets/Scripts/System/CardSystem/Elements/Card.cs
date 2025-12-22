namespace Vermines.CardSystem.Elements {

    using Fusion;
    using Vermines.CardSystem.Data;
    using Vermines.Core.Scene;

    /// <summary>
    /// Interface for a card.
    /// </summary>
    /// <note>
    /// Do not hesitate to add more properties, methods or events to this interface.
    /// I currently involved this interface when I have to manipulate a card.
    /// </note>
    public interface ICard {

        int ID { get; set; }

        bool IsAnonyme { get; set; }

        bool HasBeenActivatedThisTurn { get; set; }

        public CardData Data { get; set; }

        public PlayerRef Owner { get; set; }

        public SceneContext Context { get; set; }
    }

    /// <summary>
    /// Implementation of the interface <see cref="ICard"/>.
    /// It's an abstract class, so you can't instantiate it.
    /// </summary>
    public abstract class Card : ICard {

        public int ID { get; set; }

        public bool IsAnonyme { get; set; }

        private CardData _Data;

        public CardData Data
        {
            get => IsAnonyme ? null : _Data; // Security check
            set => _Data = value;
        }

        public bool HasBeenActivatedThisTurn { get; set; }

        public PlayerRef Owner { get; set; }

        public SceneContext Context { get; set; }
    }
}
