using Fusion;
using UnityEngine;
using System.Collections.Generic;
using Vermines.UI.Plugin;
using Vermines.UI;
using Vermines.CardSystem.Enumerations;
using Vermines.Player;
using Vermines.UI.Screen;
using Vermines;
using Vermines.CardSystem.Elements;

public class CopyEffectPlugin : GameplayScreenPlugin, IGameplayScreenPluginParam<ICard>
{
    #region Attributes

    [Header("Card Type Triggering This Plugin")]

    /// <summary>
    /// The type of card that will trigger this plugin.
    /// </summary>
    [InlineHelp]
    public CardType CardTypeTrigger = CardType.Partisan;

    /// <summary>
    /// The list of entries currently displayed in the plugin.
    /// </summary>
    protected List<Vermines.UI.Screen.ShopCardEntry> currentEntries = new();

    /// <summary>
    /// The card that is currently activated in the plugin.
    /// It can't be get itself when copy effect is activated.
    /// </summary>
    protected ICard activatedCard;

    #endregion

    #region Override Methods

    /// <summary>
    /// Show the plugin.
    /// </summary>
    /// <param name="screen">
    /// The parent screen that this plugin is attached to.
    /// </param>
    public override void Show(GameplayUIScreen screen)
    {
        base.Show(screen);
    }

    /// <summary>
    /// Hide the plugin.
    /// </summary>
    public override void Hide()
    {
        base.Hide();
    }

    /// <summary>
    /// Sets the plugin's parameter with a value of type <see cref="ICard"/>.
    /// </summary>
    public void SetParam(ICard param)
    {
        activatedCard = param;
        // Clear current entries before fetching new ones
        currentEntries.Clear();
    }

    #endregion

    #region Unity Methods

    #endregion

    #region Methods

    public virtual List<Vermines.UI.Screen.ShopCardEntry> GetEntries()
    {
        PlayerController player = PlayerController.Local;

        foreach (var card in player.Deck.Hand) {
            if (card.Data.Type == CardTypeTrigger)
                currentEntries.Add(new ShopCardEntry(card));
        }

        foreach (var card in player.Deck.Equipments) {
            if (card.Data.Type == CardTypeTrigger)
                currentEntries.Add(new ShopCardEntry(card));
        }

        foreach (var card in player.Deck.PlayedCards) {
            if (card.Data.Type == CardTypeTrigger)
                currentEntries.Add(new ShopCardEntry(card));
        }

        foreach (var card in player.Deck.Discard) {
            if (card.Data.Type == CardTypeTrigger)
                currentEntries.Add(new ShopCardEntry(card));
        }

        return currentEntries;
    }

    #endregion
}
