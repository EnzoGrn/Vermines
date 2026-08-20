using System.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine;
using Fusion;
using TMPro;

namespace Vermines.Menu.Matchmaking {

    using Vermines.Core.Services;
    using Vermines.Core.Network;
    using Vermines.Core.UI;
    using Vermines.Core;
    using Vermines.UI.Dialog;
    using Vermines.UI;
    using Vermines.Extension;

    public class MatchmakingView : UIView {

        #region Attributes

        [SerializeField]
        private UIButton _LeaveButton;

        [SerializeField]
        private TextMeshProUGUI _StatusText;

        [SerializeField]
        private TextMeshProUGUI _StatusDescriptionText;

        [SerializeField]
        private TextMeshProUGUI _TimerText;

        [SerializeField]
        private TextMeshProUGUI _PlayerCountText;

        private CancellationTokenSource _SearchCancellation;
        private string _ActiveTicketId;
        private bool _IsSearching;
        private bool _IsJoiningSession;
        private float _SearchStartTime;
        private int _CurrentPlayers = 1;
        private int _MaxPlayers = MatchmakerTicketClient.DefaultMaxPlayers;

        #endregion

        #region Events

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (_LeaveButton != null)
                _LeaveButton.onClick.AddListener(OnLeaveButton);
            BindStatusTextsIfNeeded();
        }

        protected override void OnDeinitialize()
        {
            if (_LeaveButton != null)
                _LeaveButton.onClick.RemoveListener(OnLeaveButton);
            UnsubscribeNetworkMatchmaking();
            CancelSearchInternal();

            base.OnDeinitialize();
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            _MaxPlayers      = MatchmakerTicketClient.DefaultMaxPlayers;
            _CurrentPlayers  = 1;
            _SearchStartTime = Time.realtimeSinceStartup;

            RefreshUi();

            if (IsNetworkSessionReady()) {
                EnterWaitingForPlayersState();

                return;
            }

            _ = StartSearchAsync();
        }

        protected override void OnClose()
        {
            UnsubscribeNetworkMatchmaking();
            CancelSearchInternal();

            base.OnClose();
        }

        private void Update()
        {
            if (!IsOpen)
                return;
            if (_IsSearching || _IsJoiningSession || IsNetworkSessionReady())
                RefreshUi();
        }

        #endregion

        #region Search

        private async Task StartSearchAsync()
        {
            if (_IsSearching || _IsJoiningSession)
                return;
            _IsSearching         = true;
            _SearchCancellation  = new CancellationTokenSource();
            _SearchStartTime     = Time.realtimeSinceStartup;
            _CurrentPlayers      = 1;

            SetStatus("SEARCHING", "Looking for players...");
            RefreshUi();

            try {
                MatchmakerMatchResult match = await MatchmakerTicketClient.FindMatchAsync(Context.PlayerData, _SearchCancellation.Token);

                if (string.IsNullOrEmpty(match.MatchId))
                    return;
                _ActiveTicketId   = match.TicketId;
                _MaxPlayers       = match.MaxPlayers;
                _IsSearching      = false;
                _IsJoiningSession = true;

                SetStatus("MATCH FOUND", "Connecting to session...");
                RefreshUi();

                var request = new SessionRequest {
                    UserID       = Context.PlayerData.UserID,
                    GameMode     = GameMode.AutoHostOrClient,
                    GameplayType = GameplayType.Standart,
                    SessionName  = match.MatchId,
                    ScenePath    = Context.MatchmakingScenePath,
                    MaxPlayers   = match.MaxPlayers,
                    IsCustom     = false
                };

                Global.Networking.StartGame(request);
            } catch (OperationCanceledException) {
                SetStatus("CANCELLED", "Matchmaking cancelled.");
            } catch (Exception exception) {
                Debug.LogException(exception);

                SetStatus("FAILED", exception.Message);
                OpenErrorDialog(exception.Message);
            } finally {
                _IsSearching      = false;
                _IsJoiningSession = false;
                _ActiveTicketId   = null;

                _SearchCancellation?.Dispose();

                _SearchCancellation = null;
            }
        }

        private void EnterWaitingForPlayersState()
        {
            _IsSearching      = false;
            _IsJoiningSession = false;
            _SearchStartTime  = Time.realtimeSinceStartup;

            SubscribeNetworkMatchmaking();

            if (Context.NetworkMatchmaking != null) {
                _CurrentPlayers = Mathf.Max(1, Context.NetworkMatchmaking.PlayerCount);
                _MaxPlayers     = Context.NetworkMatchmaking.MaxPlayers;
            }
            SetStatus("WAITING", "Waiting for players...");
            RefreshUi();
        }

        private bool IsNetworkSessionReady()
        {
            return Context != null && Context.Runner != null && Context.Runner.IsRunning && Context.NetworkMatchmaking != null;
        }

        #endregion

        #region Cancel / Leave

        private void OnLeaveButton()
        {
            var dialog = Open<UIYesNoDialog>();

            if (dialog == null) {
                _ = LeaveMatchmakingAsync();

                return;
            }

            dialog.Title.SetTextSafe("LEAVE MATCHMAKING");
            dialog.Description.SetTextSafe("Are you sure you want to cancel and return to the menu?");

            dialog.HasClosed += async (result) => {
                if (result != true)
                    return;
                await LeaveMatchmakingAsync();
            };
        }

        private async Task LeaveMatchmakingAsync()
        {
            CancelSearchInternal();

            if (!string.IsNullOrEmpty(_ActiveTicketId)) {
                await MatchmakerTicketClient.CancelTicketAsync(_ActiveTicketId);

                _ActiveTicketId = null;
            }

            Global.Networking.StopGame();
        }

        private void CancelSearchInternal()
        {
            if (_SearchCancellation == null)
                return;
            _SearchCancellation.Cancel();
            _SearchCancellation.Dispose();

            _SearchCancellation = null;
            _IsSearching        = false;
        }

        #endregion

        #region Network UI

        private void SubscribeNetworkMatchmaking()
        {
            if (Context?.NetworkMatchmaking == null)
                return;
            Context.NetworkMatchmaking.PlayersChanged -= OnPlayersChanged;
            Context.NetworkMatchmaking.PlayersChanged += OnPlayersChanged;
        }

        private void UnsubscribeNetworkMatchmaking()
        {
            if (Context?.NetworkMatchmaking == null)
                return;
            Context.NetworkMatchmaking.PlayersChanged -= OnPlayersChanged;
        }

        private void OnPlayersChanged(int count, int max)
        {
            _CurrentPlayers = Mathf.Max(1, count);
            _MaxPlayers     = Mathf.Max(1, max);

            SetStatus("WAITING", "Waiting for players...");
            RefreshUi();
        }

        #endregion

        #region UI Helpers

        private void BindStatusTextsIfNeeded()
        {
            if (_StatusText == null || _StatusDescriptionText == null) {
                TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);

                foreach (TextMeshProUGUI text in texts) {
                    if (_StatusText == null && text.gameObject.name == "Status")
                        _StatusText = text;
                    else if (_StatusDescriptionText == null && text.gameObject.name == "StatusDescription")
                        _StatusDescriptionText = text;
                }
            }

            if (_PlayerCountText == null)
                _PlayerCountText = _StatusDescriptionText;
            if (_TimerText == null)
                _TimerText = _StatusText;
        }

        private void SetStatus(string status, string description)
        {
            _StatusText.SetTextSafe(status);
            _StatusDescriptionText.SetTextSafe(description);
        }

        private void RefreshUi()
        {
            TimeSpan elapsed = TimeSpan.FromSeconds(Time.realtimeSinceStartup - _SearchStartTime);

            string timer   = $"{elapsed.Minutes:00}:{elapsed.Seconds:00}";
            string players = $"{_CurrentPlayers}/{_MaxPlayers}";

            if (_TimerText != null && _TimerText != _StatusText)
                _TimerText.SetTextSafe(timer);
            else if (_StatusText != null && _IsSearching)
                _StatusText.SetTextSafe($"SEARCHING  {timer}");

            if (_PlayerCountText != null && _PlayerCountText != _StatusDescriptionText)
                _PlayerCountText.SetTextSafe(players);
            else if (_StatusDescriptionText != null) {
                string description = _IsSearching ? $"Looking for players...  {players}  ·  {timer}" : _IsJoiningSession ? $"Connecting...  {players}" : $"Waiting for players...  {players}  ·  {timer}";

                _StatusDescriptionText.SetTextSafe(description);
            }
        }

        private void OpenErrorDialog(string message)
        {
            var dialog = Open<UIYesNoDialog>();

            if (dialog == null) {
                _ = LeaveMatchmakingAsync();

                return;
            }
            dialog.Title.SetTextSafe("MATCHMAKING FAILED");
            dialog.Description.SetTextSafe(message);
            dialog.YesButtonText.SetTextSafe("OK");
            dialog.NoButtonText.SetTextSafe("BACK");

            dialog.HasClosed += async (_) => {
                await LeaveMatchmakingAsync();
            };
        }

        #endregion
    }
}
