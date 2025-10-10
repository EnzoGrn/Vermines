using System.Collections.Generic;
using OMGG.DesignPattern;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {
    using Newtonsoft.Json;
    using OMGG.Chronicle;
    using System;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Chronicle;
    using Vermines.Gameplay.Commands.Cards.Effects;
    using Vermines.Player;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Spend/Spend data and earn more.")]
    public class SpendAndEarnMultiplicatorEffect : AEffect {

        #region Constants

        private static readonly string eloquenceSpendTemplate = "Spend <b><color=purple>xE</color></b>";
        private static readonly string soulSpendTemplate = "Spend <b><color=red>xA</color></b>";

        private static readonly string linkerTemplate = " to ";

        private static readonly string eloquenceEarnTemplate = "earn <b><color=purple>{0}xE</color></b>";
        private static readonly string soulEarnTemplate = "earn <b><color=red>{0}xA</color></b>";

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

        #endregion

        #region UI Elements

        public Sprite EloquenceIcon = null;
        public Sprite SoulIcon = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            if (player != PlayerController.Local.PlayerRef)
                return;
            if (UIContextManager.Instance) {
                // TODO: Context for Assassin
            }

           // TODO: Bind a GameEvents.... for a void function(int). -> Spend(int amount)
        }

        private void Spend(int amount)
        {
            // Unbind the GameEvents.

            // TODO: Pop the context.
            // if (UIContextManager.Instance)
            //    UIContextManager.Instance.PopContextOfType<...>();
            if (amount <= 0 || (_DataToSpend == DataType.Eloquence && amount > GameManager.Instance.Configuration.MaxEloquence) || (_DataToSpend == DataType.Soul && amount > GameManager.Instance.Configuration.MaxSoul))
                return;
            PlayerController.Local.NetworkEventCardEffect(Card.ID, amount.ToString());
        }

        public override void NetworkEventFunction(PlayerRef player, string data)
        {
            int amount = int.Parse(data);

            ICommand spendCommand = new SpendCommand(player, amount, DataToSpend);

            CommandInvoker.ExecuteCommand(spendCommand);

            int amountEarned = amount * Multiplicator;

            ICommand earnCommand = new EarnCommand(player, amountEarned, DataToEarn);
            CommandInvoker.ExecuteCommand(earnCommand);

            PlayerData pData = GameDataStorage.Instance.PlayerData[player];

            ChronicleEntry entry = new() {
                Id = $"assassin-{Card.ID}-{amount}",
                TimestampUtc = DateTime.UtcNow.Ticks,
                EventType = new VerminesLogEventType(VerminesLogsType.BountyHunterEffect),
                TitleKey = $"T_ContractSigned",
                MessageKey = $"D_ContractSigned",
                IconKey = $"Partisan_Card_Effect",
                DescriptionArgs = new string[] {
                    "ST_ContractSigned",
                    pData.Nickname,
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

            PlayerController.Local.AddChronicle(entry);
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
            elements.Add(($"+{Multiplicator}X", null));

            if (DataToEarn == DataType.Eloquence) {
                elements.Add((null, EloquenceIcon));
            } else if (DataToEarn == DataType.Soul) {
                elements.Add((null, SoulIcon));
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
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (EloquenceIcon == null)
                EloquenceIcon = Resources.Load<Sprite>("Sprites/UI/Icons/Eloquence");
            if (SoulIcon == null)
                SoulIcon = Resources.Load<Sprite>("Sprites/UI/Icons/Souls");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
