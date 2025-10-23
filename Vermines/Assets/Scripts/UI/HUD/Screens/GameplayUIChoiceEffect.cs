using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vermines.CardSystem.Data.Effect;
using Vermines.CardSystem.Elements;
using Vermines.Player;

namespace Vermines.UI.Screen
{
    using Text = TMPro.TMP_Text;

    public class GameplayUIChoiceEffect : GameplayUIScreen, IParamReceiver<ICard>
    {
        #region Attributes

        [Header("UI References")]
        [SerializeField] private Transform _buttonContainer;
        [SerializeField] private Button _buttonPrefab;

        private ICard _card;
        private readonly List<Button> _spawnedButtons = new();

        [SerializeField]
        private Image characterImage;

        #endregion

        #region Override Methods

        /// <summary>
        /// The Unity awake method.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// The screen init method.
        /// </summary>
        public override void Init()
        {
            base.Init();
        }

        /// <summary>
        /// The screen show method.
        /// </summary>
        public override void Show()
        {
            base.Show();
        }

        /// <summary>
        /// The screen hide method.
        /// </summary>
        public override void Hide()
        {
            base.Hide();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the callback to be called when the effect is done.
        /// </summary>
        /// <param name="card">The card to set as parameter.</param>
        public void SetParam(ICard card)
        {
            _card = card;

            if (characterImage != null && _card.Data.Sprite != null)
            {
                characterImage.sprite = _card.Data.Sprite;
            }

            GenerateEffectButtons();
        }

        /// <summary>
        /// Dynamically creates buttons for each effect of the card.
        /// </summary>
        private void GenerateEffectButtons()
        {
            ClearButtons();

            if (_card == null || _card.Data == null || _card.Data.Effects == null)
            {
                Debug.LogWarning("[UIChoiceEffect] Card or effects missing.");
                return;
            }

            foreach (AEffect effect in _card.Data.Effects)
            {
                Button newButton = Instantiate(_buttonPrefab, _buttonContainer);
                newButton.gameObject.SetActive(true);

                var label = newButton.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                {
                    Debug.Log("[UIChoiceEffect] Setting button label: " + effect.Description);
                    label.text = effect.Description;
                }
                else
                {
                    Debug.LogWarning("[UIChoiceEffect] No TMP_Text found on button prefab.");
                }

                newButton.onClick.AddListener(() => OnButtonPressed(effect));
                _spawnedButtons.Add(newButton);
            }
        }

        /// <summary>
        /// Clears all dynamically spawned buttons.
        /// </summary>
        private void ClearButtons()
        {
            foreach (Button btn in _spawnedButtons)
                if (btn != null)
                    Destroy(btn.gameObject);

            _spawnedButtons.Clear();
        }

        #endregion

        #region Events

        /// <summary>
        /// Called when a button is pressed.
        /// </summary>
        public void OnButtonPressed(AEffect effect)
        {
            if (_card == null || effect == null)
            {
                Debug.LogWarning("[UIChoiceEffect] Invalid button press - missing card or effect.");
                return;
            }

            PlayerController.Local.OnEffectChoice(_card, effect);
        }

        #endregion
    }
}