using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using Fusion;

namespace Vermines.Core.Services {

    using Vermines.Core.Settings;
    using Vermines.Core.Player;

    /// <summary>
    /// Handles Unity Matchmaker ticket create / poll / cancel. Networking (Fusion) stays separate.
    /// </summary>
    public static class MatchmakerTicketClient {

        public const string DefaultQueueName = "default-queue";

        public const int DefaultMaxPlayers = 4;

        private const int TicketPollDelayMs = 1000;

        public static string GetQueueName()
        {
            NetworkSettings networkSettings = Global.Settings?.Network;
            string          queueName       = networkSettings?.QueueName;

            return string.IsNullOrWhiteSpace(queueName) ? DefaultQueueName : queueName;
        }

        public static async Task<MatchmakerMatchResult> FindMatchAsync(PlayerData playerData, CancellationToken cancellationToken)
        {
            if (playerData == null || !Global.Settings.Cultists.IsValidCultistID(playerData.CultistID))
                throw new InvalidOperationException("A cultist must be selected before matchmaking.");
            if (!await Global.PlayerService.EnsureAuthenticatedAsync())
                throw new InvalidOperationException("Unity Services authentication failed.");
            string queueName = GetQueueName();
            string ticketId  = null;

            try {
                var customData = new Dictionary<string, object> {
                    { "CultistID", playerData.CultistID }
                };

                var player = new Unity.Services.Matchmaker.Models.Player(AuthenticationService.Instance.PlayerId, customData);

                CreateTicketResponse ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(new List<Unity.Services.Matchmaker.Models.Player> {
                    player
                }, new CreateTicketOptions(queueName));

                ticketId = ticketResponse.Id;

                cancellationToken.ThrowIfCancellationRequested();

                string matchId = await PollForMatchAsync(ticketId, cancellationToken);

                return await GetMatchResultAsync(matchId, ticketId);
            } catch (OperationCanceledException) {
                await CancelTicketAsync(ticketId);

                throw;
            }
        }

        public static async Task CancelTicketAsync(string ticketId)
        {
            if (string.IsNullOrEmpty(ticketId))
                return;
            try {
                await MatchmakerService.Instance.DeleteTicketAsync(ticketId);
            } catch (Exception exception) {
                Debug.LogException(exception);
            }
        }

        private static async Task<string> PollForMatchAsync(string ticketId, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) {
                TicketStatusResponse ticketStatus = await MatchmakerService.Instance.GetTicketAsync(ticketId);

                if (ticketStatus.Type == typeof(MatchIdAssignment) && ticketStatus.Value is MatchIdAssignment matchIdAssignment) {
                    switch (matchIdAssignment.Status) {
                        case MatchIdAssignment.StatusOptions.Found:
                            return matchIdAssignment.MatchId;

                        case MatchIdAssignment.StatusOptions.InProgress:
                            break;

                        case MatchIdAssignment.StatusOptions.Failed:
                            throw new InvalidOperationException(string.IsNullOrEmpty(matchIdAssignment.Message) ? "Matchmaking failed." : matchIdAssignment.Message);

                        case MatchIdAssignment.StatusOptions.Timeout:
                            throw new TimeoutException("Matchmaking timed out.");
                    }
                }

                await Task.Delay(TicketPollDelayMs, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            return null;
        }

        private static async Task<MatchmakerMatchResult> GetMatchResultAsync(string matchId, string ticketId)
        {
            List<string> playerIds = new();
            int        playerCount = 0;

            for (int attempt = 0; attempt < 5; attempt++) {
                try {
                    StoredMatchmakingResults results = await MatchmakerService.Instance.GetMatchmakingResultsAsync(matchId);

                    if (results?.MatchProperties?.Players != null) {
                        playerIds = results.MatchProperties.Players.Select(player => player.Id).Where(id => !string.IsNullOrEmpty(id)).OrderBy(id => id, StringComparer.Ordinal).ToList();

                        playerCount = playerIds.Count;
                    }

                    if (playerCount <= 0 && results?.MatchProperties?.MaxPlayers > 0)
                        playerCount = results.MatchProperties.MaxPlayers;
                    if (playerCount >= 2)
                        break;
                } catch (Exception exception) {
                    Debug.LogException(exception);
                }

                await Task.Delay(TicketPollDelayMs);
            }

            if (playerCount < 2)
                playerCount = Mathf.Max(2, DefaultMaxPlayers);
            return new MatchmakerMatchResult(matchId, playerCount, ticketId, playerIds);
        }
    }

    public readonly struct MatchmakerMatchResult {

        public string MatchId { get; }
        public int MaxPlayers { get; }
        public string TicketId { get; }
        public IReadOnlyList<string> PlayerIds { get; }

        public MatchmakerMatchResult(string matchId, int maxPlayers, string ticketId, List<string> playerIds)
        {
            MatchId    = matchId;
            MaxPlayers = maxPlayers;
            TicketId   = ticketId;
            PlayerIds  = playerIds ?? new List<string>();
        }

        public GameMode ResolveLocalGameMode(string localPlayerId)
        {
            if (PlayerIds == null || PlayerIds.Count == 0 || string.IsNullOrEmpty(localPlayerId)) {
                Debug.Log($"[Matchmaker] PlayerIds unavailable, falling back to AutoHostOrClient. LocalPlayerId: {localPlayerId}");

                return GameMode.AutoHostOrClient;
            }
            string hostId = PlayerIds[0];

            GameMode resolved = hostId == localPlayerId ? GameMode.Host : GameMode.Client;

            Debug.Log($"[Matchmaker] Resolved GameMode: {resolved} (HostId: {hostId}, LocalId: {localPlayerId}, TotalPlayers: {PlayerIds.Count})");

            return resolved;
        }
    }
}
