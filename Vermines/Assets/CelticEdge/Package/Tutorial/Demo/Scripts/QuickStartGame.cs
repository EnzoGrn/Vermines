using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace CelticEdge.Tutorial.Demo {

    /// <summary>
    /// The demo "game": a gold counter, a Harvest button (+1 gold) and a Buy button (a sword for 3 gold).
    /// It shows the three things a game does for the tutorial:
    /// ask for permission (TutorialInputGate), publish what happens (TutorialEventBus) and carry targets (TutorialTarget).
    /// </summary>
    public class QuickStartGame : MonoBehaviour {

        public const int SwordPrice = 3;

        private TMP_Text _GoldText;
        private int      _Gold;

        public int Gold => _Gold;

        public static QuickStartGame Create(Transform parent)
        {
            var root = new GameObject("QuickStartGame", typeof(RectTransform));

            root.transform.SetParent(parent, false);

            var game = root.AddComponent<QuickStartGame>();

            game.Build();

            return game;
        }

        public void ResetGame(int startingGold)
        {
            _Gold = startingGold;

            Refresh();
        }

        public void AddGold(int amount)
        {
            _Gold += amount;

            Refresh();

            TutorialEventBus.Publish("gold_changed", null, _Gold);
        }

        private void Harvest()
        {
            if (!TutorialInputGate.IsAllowed("harvest"))
                return;
            AddGold(1);

            TutorialEventBus.Publish("harvested");
        }

        private void BuySword()
        {
            if (!TutorialInputGate.IsAllowed("buy", "sword"))
                return;
            if (_Gold < SwordPrice) {
                // Refusal decided by the game: a hint of the lesson can explain it.
                if (TutorialManager.Instance != null)
                    TutorialManager.Instance.Hints.Trigger("not_enough_gold");
                return;
            }

            AddGold(-SwordPrice);

            TutorialEventBus.Publish("item_bought", "sword");
        }

        private void Refresh()
        {
            if (_GoldText != null)
                _GoldText.text = $"Gold: {_Gold}";
        }

        private void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = gameObject.AddComponent<CanvasScaler>();

            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            gameObject.AddComponent<GraphicRaycaster>();

            var panel   = QuickStartUI.NewUI("Panel", transform);
            var panelRT = (RectTransform)panel.transform;

            panelRT.anchorMin = panelRT.anchorMax = panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(420, 360);

            var layout = panel.AddComponent<VerticalLayoutGroup>();

            layout.spacing                = 24;
            layout.childAlignment         = TextAnchor.MiddleCenter;
            layout.childControlHeight     = true;
            layout.childControlWidth      = true;
            layout.childForceExpandHeight = false;

            _GoldText           = QuickStartUI.NewText("Gold", panel.transform, 48, true);
            _GoldText.alignment = TextAlignmentOptions.Center;

            _GoldText.gameObject.AddComponent<TutorialTarget>().SetTargetId("gold_counter");

            var harvest = QuickStartUI.NewButton("Harvest", panel.transform, "Harvest", new Color(0.25f, 0.45f, 0.25f), 360);

            harvest.GetComponent<LayoutElement>().minHeight = 80;
            harvest.onClick.AddListener(Harvest);
            harvest.gameObject.AddComponent<TutorialTarget>().SetTargetId("harvest_button");

            var buy = QuickStartUI.NewButton("Buy", panel.transform, $"Buy a sword ({SwordPrice} gold)", QuickStartUI.AccentColor, 360);

            buy.GetComponent<LayoutElement>().minHeight = 80;
            buy.onClick.AddListener(BuySword);
            buy.gameObject.AddComponent<TutorialTarget>().SetTargetId("buy_button");

            Refresh();
        }
    }
}
