using Fusion;
using OMGG.DesignPattern;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {
    using Newtonsoft.Json;
    using OMGG.Chronicle;
    using System;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Chronicle;
    using Vermines.Gameplay.Commands.Cards.Effects;
    using Vermines.Gameplay.Errors;
    using Vermines.Player;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Spend/Spend data and earn more.")]
    public class SpendAndEarnMultiplicatorEffect : AEffect {

        #region Constants

        private static readonly string eloquenceSpendTemplate = "Spend <b><color=purple>xE</color></b>";
        private static readonly string soulSpendTemplate = "Spend <b><color=red>xA</color></b>";

        private static readonly string linkerTemplate = " to ";

        private static readonly string eloquenceEarnTemplate = "earn <b><color=purple>{0}xE</color></b>";
        private static readonly string soulEarnTemplate = "earn <b><color=red>{0}xA</color></b>";

        private static readonly string linkerSubEffectTemplate = " then ";

        #endregion

        #region Properties

        [SerializeField]
        private string _Description;

        public override string Description
        {
            get => _Description;
            set
            {
                _Description = value;
            }
        }

        [SerializeField]
        private int _Multiplicator = 2;

        public int Multiplicator
        {
            get => _Multiplicator;
            set
            {
                _Multiplicator = value;

                UpdateDescription();
            }
        }

        [SerializeField]
        private DataType _DataToSpend = DataType.Eloquence;

        public DataType DataToSpend
        {
            get => _DataToSpend;
            set
            {
                _DataToSpend = value;

                UpdateDescription();
            }
        }

        [SerializeField]
        private DataType _DataToEarn = DataType.Soul;

        public DataType DataToEarn
        {
            get => _DataToEarn;
            set
            {
                _DataToEarn = value;

                UpdateDescription();
            }
        }

        [SerializeField]
        private AEffect _SubEffect = null;

        public override AEffect SubEffect
        {
            get => _SubEffect;
            set
            {
                _SubEffect = value;

                UpdateDescription();
            }
        }

        #endregion

        #region UI Elements

        public Sprite EloquenceIcon = null;
        public Sprite SoulIcon = null;
        public Sprite ThenIcon = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            if (player != Context.Runner.LocalPlayer)
                return;

            GameEvents.OnEffectPromptRequested.Invoke(EffectPrompt.Spend(Card, _DataToSpend, _DataToEarn, _Multiplicator));
            GameEvents.OnEffectSpendSubmitted.AddListener(Spend);
        }

        private void Spend(int amount)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(Context.Runner.LocalPlayer);

            // NOTE: kept as it was, but the second test reads _DataToEarn where
            // _DataToSpend looks intended (spending eloquence to earn souls is
            // blocked when souls < amount). To confirm.
            bool notEnough = (_DataToSpend == DataType.Eloquence && player.Statistics.Eloquence < amount)
                          || (_DataToEarn == DataType.Soul && player.Statistics.Souls < amount);
            bool overLimit = (_DataToSpend == DataType.Eloquence && amount > Context.GameplayMode.MaxEloquence)
                          || (_DataToSpend == DataType.Soul && amount > Context.GameplayMode.SoulsLimit);

            // Invalid amount: keep the prompt open and tell the player.
            if (amount > 0 && (notEnough || overLimit))
            {
                GameActionError error = new GameActionError
                {
                    Scope = ErrorScope.Local,
                    Target = Context.Runner.LocalPlayer,
                    Severity = ErrorSeverity.Minor,
                    Location = ErrorLocation.Effect,
                    MessageKey = "Effect_SpendInvalid"
                };

                GameEvents.OnActionRefused.Invoke(error, GameActionError.Localize(error));

                return;
            }

            GameEvents.OnEffectSpendSubmitted.RemoveListener(Spend);
            GameEvents.OnEffectPromptClosed.Invoke(EffectPromptKind.Spend);

            // Spending nothing is the way to decline the effect.
            if (amount <= 0)
                return;
            player.NetworkEventCardEffect(Card == null ? -1 : Card.ID, amount.ToString());
        }


        public override void NetworkEventFunction(PlayerRef playerRef, string data)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerRef);

            int amount = int.Parse(data);

            ICommand spendCommand = new SpendCommand(player, amount, DataToSpend);

            CommandInvoker.ExecuteCommand(spendCommand);

            int amountEarned = amount * Multiplicator;

            ICommand earnCommand = new EarnCommand(player, amountEarned, DataToEarn);

            CommandInvoker.ExecuteCommand(earnCommand);

            ChronicleEntry entry = new() {
                Id = $"assassin-{Card.ID}-{amount}",
                TimestampUtc = DateTime.UtcNow.Ticks,
                EventType = new VerminesLogEventType(VerminesLogsType.BountyHunterEffect),
                TitleKey = $"T_ContractSigned",
                MessageKey = $"D_ContractSigned",
                IconKey = $"Partisan_Card_Effect",
                DescriptionArgs = new string[] {
                    "ST_ContractSigned",
                    player.Nickname,
                    DataToSpend == DataType.Eloquence ? "ST_Eloquence" : "ST_Soul",
                    amount.ToString(),
                    DataToEarn == DataType.Eloquence ? "ST_Eloquence" : "ST_Soul",
                    amountEarned.ToString()
                }
            };

            var payloadObject = new {
                DescriptionArgs = entry.DescriptionArgs,
                CardId = Card.ID,
                AmountSpent = amount,
                AmountEarned = amountEarned,
                // ...
            };

            string payloadJson = JsonConvert.SerializeObject(payloadObject);

            ChroniclePayloadStorage.Add(entry.Id, payloadJson);

            entry.PayloadJson = payloadJson;

            player.AddChronicle(entry);

            base.Play(playerRef);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new() {
                { ("-X", null) }
            };

            if (DataToSpend == DataType.Eloquence) {
                elements.Add((null, EloquenceIcon));
            } else if (DataToSpend == DataType.Soul) {
                elements.Add((null, SoulIcon));
            }

            elements.Add((" : ", null));

            if (Multiplicator != 1)
                elements.Add(($"+{Multiplicator}X", null));
            else
                elements.Add(($"+X", null));

            if (DataToEarn == DataType.Eloquence) {
                elements.Add((null, EloquenceIcon));
            } else if (DataToEarn == DataType.Soul) {
                elements.Add((null, SoulIcon));
            }

            if (SubEffect != null) {
                elements.Add((null, ThenIcon));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
            string descriptionTemplate = "";

            if (DataToSpend == DataType.Eloquence)
                descriptionTemplate += $"{eloquenceSpendTemplate}{linkerTemplate}";
            else if (DataToSpend == DataType.Soul)
                descriptionTemplate += $"{soulSpendTemplate}{linkerTemplate}";

            if (DataToEarn == DataType.Eloquence)
                descriptionTemplate += string.Format(eloquenceEarnTemplate, Multiplicator);
            else if (DataToEarn == DataType.Soul)
                descriptionTemplate += string.Format(soulEarnTemplate, Multiplicator);
            Description = descriptionTemplate;

            if (SubEffect != null) {
                string subDescription = SubEffect.Description;

                if (!string.IsNullOrEmpty(subDescription))
                    subDescription = char.ToLower(subDescription[0]) + subDescription[1..];
                Description += $"{linkerSubEffectTemplate}{subDescription}";
            }
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (EloquenceIcon == null)
                EloquenceIcon = Resources.Load<Sprite>("Sprites/UI/Icons/Eloquence");
            if (SoulIcon == null)
                SoulIcon = Resources.Load<Sprite>("Sprites/UI/Icons/Souls");
            if (ThenIcon == null)
                ThenIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Then");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
