using System;

namespace Vermines.Tutorial
{
    /// Un événement de gameplay publié par le jeu : un identifiant ("card_played"),
    /// un argument optionnel (l'ID de la carte, du bouton...) et une valeur entière
    /// optionnelle (quantité d'éloquence gagnée, index du joueur...).
    public readonly struct TutorialEvent
    {
        public readonly string Id;
        public readonly string Arg;
        public readonly int Value;

        public TutorialEvent(string id, string arg = null, int value = 0)
        {
            Id = id;
            Arg = arg;
            Value = value;
        }

        /// Vrai si l'événement correspond à l'ID attendu, et à l'argument attendu
        /// quand celui-ci est renseigné (vide = n'importe quel argument).
        public bool Matches(string id, string arg)
        {
            if (!string.Equals(Id, id, StringComparison.Ordinal))
                return false;
            return string.IsNullOrEmpty(arg) || string.Equals(Arg, arg, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString() => $"{Id}({Arg}, {Value})";
    }

    /// Bus d'événements découplé : le code du jeu publie ses actions sans jamais
    /// connaître le tutoriel, le tutoriel écoute. Reprend l'idée du prototype
    /// TutorialSystem, avec un argument et une valeur en plus pour pouvoir attendre
    /// "le joueur a joué l'Ouvrière" et pas seulement "le joueur a joué une carte".
    ///
    /// Côté jeu :
    ///   TutorialEventBus.Publish(TutorialEvents.CardPlayed, card.Id);
    ///   TutorialEventBus.Publish(TutorialEvents.EloquenceChanged, null, player.Eloquence);
    public static class TutorialEventBus
    {
        public static event Action<TutorialEvent> OnEvent;

        public static void Publish(string eventId, string arg = null, int value = 0)
        {
            if (string.IsNullOrEmpty(eventId))
                return;
            OnEvent?.Invoke(new TutorialEvent(eventId, arg, value));
        }

        /// Retire tous les abonnés. Utile entre deux tests ou au rechargement du domaine.
        public static void ClearAllListeners() => OnEvent = null;
    }

    /// Identifiants standard des événements que le jeu doit publier. Les étapes du
    /// tutoriel les référencent par leur valeur texte (ex. dans le tableau CSV), donc
    /// ne pas renommer une valeur sans mettre à jour les données.
    public static class TutorialEvents
    {
        // Cartes (Arg = ID de la carte)
        public const string CardDrawn = "card_drawn";
        public const string CardPlayed = "card_played";
        public const string CardBought = "card_bought";
        public const string CardRecycled = "card_recycled";
        public const string CardDiscarded = "card_discarded";
        public const string CardKept = "card_kept";
        public const string PartisanPlaced = "partisan_placed";
        public const string PartisanEffectActivated = "partisan_effect_activated";
        public const string PartisanSacrificed = "partisan_sacrificed";
        public const string CourtCardAdded = "court_card_added";

        // Tour (Arg = index du joueur en texte, "0" = joueur humain ; Value = numéro du tour)
        public const string TurnStarted = "turn_started";
        public const string TurnEnded = "turn_ended";
        public const string HandRefilled = "hand_refilled";
        public const string DeckReshuffled = "deck_reshuffled";

        // Ressources (Value = nouveau total)
        public const string EloquenceChanged = "eloquence_changed";
        public const string SoulsChanged = "souls_changed";

        // Partie
        public const string MatchStarted = "match_started";
        public const string MatchEnded = "match_ended";

        // Interface (Arg = ID du bouton ou de la cible)
        public const string ButtonClicked = "button_clicked";
        public const string TargetClicked = "target_clicked";
        public const string PopupClosed = "popup_closed";
    }
}
