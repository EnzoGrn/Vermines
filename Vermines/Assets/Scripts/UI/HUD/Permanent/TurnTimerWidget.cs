using UnityEngine;

namespace Vermines.UI
{
    using Text = TMPro.TMP_Text;

    /// <summary>
    /// Permanent HUD widget: shows the time left in the current turn.
    /// Put this script on an ALWAYS ACTIVE object (it subscribes in Awake) and
    /// assign the child to show/hide as _Root.
    /// </summary>
    public class TurnTimerWidget : MonoBehaviour
    {
        [SerializeField] private GameObject _Root;
        [SerializeField] private Text _Label;

        [Header("Warning")]
        [SerializeField] private int _WarningSeconds = 10;
        [SerializeField] private Color _NormalColor = Color.white;
        [SerializeField] private Color _WarningColor = Color.red;

        private void Awake()
        {
            if (_Root != null)
                _Root.SetActive(false);

            GameEvents.OnTurnTimerChanged.AddListener(OnTimerChanged);
        }

        private void OnDestroy()
        {
            GameEvents.OnTurnTimerChanged.RemoveListener(OnTimerChanged);
        }

        private void OnTimerChanged(int secondsLeft)
        {
            if (_Root != null)
                _Root.SetActive(secondsLeft >= 0);

            if (secondsLeft < 0 || _Label == null)
                return;

            _Label.text = $"{secondsLeft / 60:00}:{secondsLeft % 60:00}";
            _Label.color = secondsLeft <= _WarningSeconds ? _WarningColor : _NormalColor;
        }
    }
}
