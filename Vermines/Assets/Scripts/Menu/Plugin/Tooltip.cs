using OMGG.Menu.Screen;
using UnityEngine.UI;
using UnityEngine;
using Fusion;

namespace Vermines.Menu.Pluggin {

    [RequireComponent(typeof(Button))]
    public class Tooltip : MenuScreenPlugin {

        [InlineHelp, SerializeField]
        protected string _Header;

        [InlineHelp, SerializeField, TextArea]
        protected string _Tooltip;

        [InlineHelp, SerializeField]
        protected Button _Button;

        public void OnEnable()
        {
            Popup popup = FindAnyObjectByType<Popup>(FindObjectsInactive.Include);

            if (popup != null)
                _Button.onClick.AddListener(() => popup.OpenPopup(_Tooltip, _Header));
        }

        public void OnDisable()
        {
            if (_Button != null)
                _Button.onClick.RemoveAllListeners();
        }
    }
}
