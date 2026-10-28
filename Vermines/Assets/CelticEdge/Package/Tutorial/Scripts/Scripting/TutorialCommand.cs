using System.Collections.Generic;
using System.Globalization;
using System;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// Commands executed by the tutorial itself.
    /// All others are passed to the game (ITutorialGameBridge.ExecuteCommand), which assigns their names:
    /// "GiveItem", 'AddGold', "SpawnEnemy"...
    /// </summary>
    public static class TutorialCommands {

        /// <summary>Wait(seconds=1.5)</summary>
        public const string Wait = "Wait";

        /// <summary>HideElement(target=market): hides a target without deactivating it.</summary>
        public const string HideElement = "HideElement";

        /// <summary>ShowElement(target=market)</summary>
        public const string ShowElement = "ShowElement";

        /// <summary>PublishEvent(event=door_opened, arg=red, value=1)</summary>
        public const string PublishEvent = "PublishEvent";

        /// <summary>PlayInterlude(id=trop_cher): play a supplementary segment of the lesson.</summary>
        public const string PlayInterlude = "PlayInterlude";

        /// <summary>PauseAgents() / ResumeAgents(): Pauses scripted agents (TutorialSession.AgentsPaused).</summary>
        public const string PauseAgents  = "PauseAgents";

        /// <summary>PauseAgents() / ResumeAgents(): Resumes scripted agents (TutorialSession.AgentsPaused).</summary>
        public const string ResumeAgents = "ResumeAgents";

        private static readonly HashSet<string> _BuiltIn = new(StringComparer.OrdinalIgnoreCase) {
            Wait, HideElement, ShowElement, PublishEvent, PlayInterlude, PauseAgents, ResumeAgents
        };

        public static bool IsBuiltIn(string name) => !string.IsNullOrEmpty(name) && _BuiltIn.Contains(name);
    }

    [Serializable]
    public struct TutorialCommandArg {

        public string Key;
        public string Value;

        public TutorialCommandArg(string key, string value)
        {
            Key   = key;
            Value = value;
        }
    }

    /// <summary>
    /// A command and its parameters. In the CSV table, it is written as Name(key=value, ...), for example:
    ///   AddGold(amount=5)
    ///   GiveItem(item=sword, player=0)
    ///   HideElement(target=market)
    ///   Wait(seconds=1.5)
    ///
    /// Keys can be anything; the get methods (Get, GetInt...) are case-insensitive and ignore accents,
    /// and accept multiple names for the same key (GetInt("amount", "number")).
    /// </summary>
    [Serializable]
    public class TutorialCommand {

        public string Name;

        public List<TutorialCommandArg> Args = new();

        public TutorialCommand() {}

        public TutorialCommand(string name, params (string key, object value)[] arguments)
        {
            Name = name;

            foreach (var (key, value) in arguments)
                Set(key, value);
        }

        public bool Is(string commandName) => string.Equals(Name, commandName, StringComparison.OrdinalIgnoreCase);

        public bool IsBuiltIn => TutorialCommands.IsBuiltIn(Name);

        public TutorialCommand Set(string key, object value)
        {
            string text = value is IFormattable f ? f.ToString(null, CultureInfo.InvariantCulture) : value?.ToString();

            for (int i = 0; i < Args.Count; i++) {
                if (SameKey(Args[i].Key, key)) {
                    Args[i] = new TutorialCommandArg(Args[i].Key, text);

                    return this;
                }
            }

            Args.Add(new TutorialCommandArg(key, text));

            return this;
        }

        public bool Has(params string[] keys) => TryGet(out _, keys);

        /// <summary>
        /// The value of the first key found in `keys`, or `null`.
        /// </summary>
        public string Get(params string[] keys) => TryGet(out string v, keys) ? v : null;

        public int GetInt(int fallback, params string[] keys) => TryGet(out string v, keys) && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i) ? i : fallback;

        public int GetInt(params string[] keys) => GetInt(0, keys);

        public float GetFloat(float fallback, params string[] keys) => TryGet(out string v, keys) && float.TryParse(v.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float f) ? f : fallback;

        public float GetFloat(params string[] keys) => GetFloat(0f, keys);

        public bool GetBool(bool fallback, params string[] keys)
        {
            if (!TryGet(out string v, keys))
                return fallback;
            switch (Authoring.TutorialTable.Normalize(v)) {
                case "oui": case "vrai": case "true": case "yes": case "1":
                    return true;
                case "non": case "faux": case "false": case "no": case "0":
                    return false;
                default:
                    return fallback;
            }
        }

        private bool TryGet(out string value, string[] keys)
        {
            foreach (string key in keys) {
                foreach (var arg in Args) {
                    if (SameKey(arg.Key, key)) {
                        value = arg.Value;

                        return true;
                    }
                }
            }

            value = null;

            return false;
        }

        private static bool SameKey(string a, string b) => Authoring.TutorialTable.Normalize(a) == Authoring.TutorialTable.Normalize(b);

        public override string ToString()
        {
            var parts = new List<string>();

            foreach (var arg in Args)
                parts.Add($"{arg.Key}={arg.Value}");
            return $"{Name}({string.Join(", ", parts)})";
        }
    }
}
