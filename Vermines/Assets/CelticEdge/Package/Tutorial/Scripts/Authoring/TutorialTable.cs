using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System;

namespace CelticEdge.Tutorial.Authoring {

    /// <summary>
    /// A row from the scenario table, converted to typed values.
    /// It is then copied into a TutorialStepData by the editor's import tool.
    /// </summary>
    public class ParsedStep {

        public int    SourceLine;
        public string LessonId;
        public string StepId;

        public PopupStyle PopupStyle = PopupStyle.Bubble;

        public string Title;
        public string Body;
        public string TextKey;

        public List<string> TargetIds = new();

        public SpotlightShape Shape = SpotlightShape.RoundedRect;

        public float Padding                   = 16f;
        public bool  DimScreen                 = true;
        public bool  BlockClicksOutsideTargets = true;

        public PointerMode Pointer = PointerMode.None;

        public string PointerDropTargetId;

        public PopupPlacement   Placement        = PopupPlacement.Auto;
        public AdvanceCondition AdvanceCondition = AdvanceCondition.WaitForButton;

        public string EventId;
        public string EventArg;

        public int           EventCount = 1;
        public float       DelaySeconds = 2f;
        public bool RestrictGameActions = true;

        public List<AllowedAction> AllowedActions = new();
        public List<TutorialCommand>      OnEnter = new();
        public List<TutorialCommand>       OnExit = new();

        /// <summary>
        /// Copies the row to a stage asset (CSV import into the editor, or loading the array at runtime).
        /// The portrait remains unchanged.
        /// </summary>
        public void ApplyTo(TutorialStepData dst)
        {
            dst.StepId     = StepId;
            dst.PopupStyle = PopupStyle;

            dst.Title     = new TutorialText(Title, string.IsNullOrEmpty(TextKey) ? null : TextKey + ".title");
            dst.Body      = new TutorialText(Body , string.IsNullOrEmpty(TextKey) ? null : TextKey);
            dst.TargetIds = new List<string>(TargetIds);

            dst.Shape                     = Shape;
            dst.Padding                   = Padding;
            dst.DimScreen                 = DimScreen;
            dst.BlockClicksOutsideTargets = BlockClicksOutsideTargets;
            dst.Pointer                   = Pointer;
            dst.PointerDropTargetId       = PointerDropTargetId;
            dst.Placement                 = Placement;
            dst.AdvanceCondition          = AdvanceCondition;
            dst.EventId                   = EventId;
            dst.EventArg                  = EventArg;
            dst.EventCount                = EventCount < 1 ? 1 : EventCount;
            dst.DelaySeconds              = DelaySeconds;
            dst.RestrictGameActions       = RestrictGameActions;

            dst.AllowedActions = new List<AllowedAction>(AllowedActions);
            dst.OnEnter        = new List<TutorialCommand>(OnEnter);
            dst.OnExit         = new List<TutorialCommand>(OnExit);
        }
    }

    /// <summary>
    /// Read and write the scenario table in CSV format (export to Excel / Google Sheets).
    /// Headers and values can be entered in French or English.
    ///
    /// Recognized columns (order does not matter; missing columns = default value):
    ///   lecon, id_etape, style, titre, texte, cle_texte, cibles, forme, marge, voile,
    ///   bloquer_clics, pointeur, cible_depot, placement, condition, evenement,
    ///   argument, nombre, delai, actions_autorisees, commandes_debut, commandes_fin, notes
    ///
    /// (in English: lesson, step_id, style, title, text, text_key, targets, shape,
    ///              padding, dim, block_clicks, pointer, drop_target, placement, condition, event,
    ///              argument, count, delay, allowed_actions, on_enter, on_exit, notes
    /// )
    ///
    /// Lists (targets, actions, commands): elements separated by "|".
    /// A line whose lesson is "lesson_id/sequence_id" belongs to the sub-sequence sequence_id of that lesson (see TutorialTableLoader).
    /// </summary>
    public static class TutorialTable {

        private static readonly Dictionary<string, string> _HeaderAliases = new() {
            { "lesson", "lecon" }, { "step_id", "id_etape" }, { "step", "id_etape" }, { "title", "titre" }, { "text", "texte" },
            { "body", "texte" }, { "text_key", "cle_texte" }, { "targets", "cibles" }, { "shape", "forme" }, { "padding", "marge" },
            { "dim", "voile" }, { "dim_screen", "voile" }, { "block_clicks", "bloquer_clics" }, { "pointer", "pointeur" },
            { "drop_target", "cible_depot" }, { "event", "evenement" }, { "arg", "argument" }, { "count", "nombre" },
            { "delay", "delai" }, { "allowed_actions", "actions_autorisees" }, { "actions", "actions_autorisees" },
            { "on_enter", "commandes_debut" }, { "on_exit", "commandes_fin" }
        };

        /// <summary>
        /// Game-specific action names, usable in the "actions_autorisees" column instead of the identifier (e.g. "acheter" → "buy").
        /// Without an alias, the name written in the table is the identifier. Aliases ignore case and accents.
        /// </summary>
        public static void RegisterActionAlias(string alias, string actionId)
        {
            if (!string.IsNullOrEmpty(alias) && !string.IsNullOrEmpty(actionId))
                _ActionAliases[Normalize(alias)] = actionId;
        }

        /// <summary>
        /// Declares the commands the game bridge can execute: the table then reports any other command as a typo.
        /// Without a declaration, any command is accepted.
        /// </summary>
        public static void RegisterCommands(params string[] names)
        {
            foreach (var name in names) {
                if (!string.IsNullOrEmpty(name))
                    _KnownCommands.Add(name);
            }
        }

        private static readonly Dictionary<string, string> _ActionAliases = new();
        private static readonly HashSet<string>            _KnownCommands = new(StringComparer.OrdinalIgnoreCase);

        public static readonly string[] Columns = {
            "lecon", "id_etape", "style", "titre", "texte", "cle_texte", "cibles", "forme", "marge", "voile",
            "bloquer_clics", "pointeur", "cible_depot", "placement", "condition", "evenement", "argument",
            "nombre", "delai", "actions_autorisees", "commandes_debut", "commandes_fin", "notes"
        };

        #region Raw CSV

        /// <summary>
        /// Splits a CSV text (quotes, line breaks inside a cell, delimiter ';', ',' or tab detected on the first line).
        /// </summary>
        public static List<string[]> ParseCsv(string text, char? delimiter = null)
        {
            var rows = new List<string[]>();

            if (string.IsNullOrEmpty(text))
                return rows;
            if (text[0] == '﻿')
                text = text.Substring(1);
            char      sep = delimiter ?? DetectDelimiter(text);
            var       row = new List<string>();
            var      cell = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < text.Length; i++) {
                char c = text[i];

                if (inQuotes) {
                    if (c == '"') {
                        if (i + 1 < text.Length && text[i + 1] == '"') {
                            cell.Append('"');

                            i++;
                        } else {
                            inQuotes = false;
                        }
                    } else {
                        cell.Append(c);
                    }

                    continue;
                }

                if (c == '"') {
                    inQuotes = true;
                } else if (c == sep) {
                    row.Add(cell.ToString());
                    cell.Clear();
                } else if (c == '\r' || c == '\n') {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                        i++;
                    row.Add(cell.ToString());
                    cell.Clear();
                    rows.Add(row.ToArray());
                    row.Clear();
                } else {
                    cell.Append(c);
                }
            }

            if (cell.Length > 0 || row.Count > 0) {
                row.Add(cell.ToString());
                rows.Add(row.ToArray());
            }

            rows.RemoveAll(r => Array.TrueForAll(r, string.IsNullOrWhiteSpace));

            return rows;
        }

        private static char DetectDelimiter(string text)
        {
            int       end = text.IndexOfAny(new[] { '\r', '\n' });
            string header = end < 0 ? text : text.Substring(0, end);
            int      semi = Count(header, ';');
            int     comma = Count(header, ',');
            int       tab = Count(header, '\t');

            if (tab > semi && tab > comma)
                return '\t';
            return semi >= comma ? ';' : ',';
        }

        private static int Count(string s, char c)
        {
            int n = 0;

            foreach (char ch in s) {
                if (ch == c)
                    n++;
            }

            return n;
        }

        #endregion

        #region Table to steps

        public static List<ParsedStep> Parse(string csvText, List<string> warnings)
        {
            warnings ??= new List<string>();

            var result = new List<ParsedStep>();
            var rows   = ParseCsv(csvText);

            if (rows.Count == 0) {
                warnings.Add("The file is empty.");

                return result;
            }

            var header = new Dictionary<string, int>();

            for (int c = 0; c < rows[0].Length; c++) {
                string name = Normalize(rows[0][c]);

                header[_HeaderAliases.TryGetValue(name, out string canonical) ? canonical : name] = c;
            }

            if (!header.ContainsKey("id_etape"))
                warnings.Add("Column \"id_etape\" is missing: steps will be numbered automatically and cannot be updated cleanly.");
            string lastLesson = null;

            for (int r = 1; r < rows.Count; r++) {
                var cells = rows[r];
                int  line = r + 1;

                string Get(string col) => header.TryGetValue(col, out int i) && i < cells.Length ? cells[i].Trim() : string.Empty;

                var step = new ParsedStep { SourceLine = line };

                step.LessonId = Get("lecon");

                if (string.IsNullOrEmpty(step.LessonId))
                    step.LessonId = lastLesson;
                lastLesson  = step.LessonId;
                step.StepId = Get("id_etape");

                if (string.IsNullOrEmpty(step.StepId))
                    step.StepId = $"{step.LessonId ?? "etape"}_{r:00}";

                void Warn(string msg) => warnings.Add($"Line {line} ({step.StepId}): {msg}");

                step.PopupStyle                = ParseEnum(Get("style"), PopupStyle.Bubble, _StyleAliases, Warn);
                step.Title                     = Get("titre");
                step.Body                      = Get("texte").Replace("\\n", "\n");
                step.TextKey                   = Get("cle_texte");
                step.TargetIds                 = SplitList(Get("cibles"));
                step.Shape                     = ParseEnum(Get("forme"), SpotlightShape.RoundedRect, _ShapeAliases, Warn);
                step.Padding                   = ParseFloat(Get("marge"), 16f, Warn);
                step.DimScreen                 = ParseBool(Get("voile"), true, Warn);
                step.BlockClicksOutsideTargets = ParseBool(Get("bloquer_clics"), true, Warn);
                step.Pointer                   = ParseEnum(Get("pointeur"), PointerMode.None, _PointerAliases, Warn);
                step.PointerDropTargetId       = NullIfEmpty(Get("cible_depot"));
                step.Placement                 = ParseEnum(Get("placement"), PopupPlacement.Auto, _PlacementAliases, Warn);
                step.AdvanceCondition          = ParseEnum(Get("condition"), AdvanceCondition.WaitForButton, _ConditionAliases, Warn);
                step.EventId                   = NullIfEmpty(Get("evenement"));
                step.EventArg                  = NullIfEmpty(Get("argument"));
                step.EventCount                = (int)ParseFloat(Get("nombre"), 1f, Warn);
                step.DelaySeconds              = ParseFloat(Get("delai"), 2f, Warn);
                step.AllowedActions            = ParseActions(Get("actions_autorisees"), out step.RestrictGameActions, Warn);
                step.OnEnter                   = ParseCommands(Get("commandes_debut"), Warn);
                step.OnExit                    = ParseCommands(Get("commandes_fin"), Warn);

                if (!string.IsNullOrEmpty(step.EventId) && string.IsNullOrEmpty(Get("condition")))
                    step.AdvanceCondition = AdvanceCondition.WaitForEvent;
                result.Add(step);
            }

            return result;
        }

        #endregion

        #region Steps to table

        public static string ToCsv(IEnumerable<ParsedStep> steps, char delimiter = ';')
        {
            var sb = new StringBuilder();

            AppendRow(sb, Columns, delimiter);

            foreach (var s in steps) {
                AppendRow(sb, new[] {
                    s.LessonId, s.StepId, s.PopupStyle.ToString(), s.Title, (s.Body ?? string.Empty).Replace("\n", "\\n"), s.TextKey,
                    string.Join("|", s.TargetIds), s.Shape.ToString(), Fmt(s.Padding), s.DimScreen ? "oui" : "non",
                    s.BlockClicksOutsideTargets ? "oui" : "non", s.Pointer.ToString(), s.PointerDropTargetId, s.Placement.ToString(),
                    s.AdvanceCondition.ToString(), s.EventId, s.EventArg, s.EventCount.ToString(CultureInfo.InvariantCulture),
                    Fmt(s.DelaySeconds), FormatActions(s.RestrictGameActions, s.AllowedActions),
                    string.Join(" | ", s.OnEnter.ConvertAll(c => c.ToString())),
                    string.Join(" | ", s.OnExit.ConvertAll(c => c.ToString())), string.Empty
                }, delimiter);
            }

            return sb.ToString();
        }

        private static string Fmt(float f) => f.ToString(CultureInfo.InvariantCulture);

        private static void AppendRow(StringBuilder sb, IList<string> cells, char delimiter)
        {
            for (int i = 0; i < cells.Count; i++) {
                if (i > 0)
                    sb.Append(delimiter);
                string   v = cells[i] ?? string.Empty;
                bool quote = v.IndexOfAny(new[] { delimiter, '"', '\n', '\r', ',' }) >= 0;

                sb.Append(quote ? "\"" + v.Replace("\"", "\"\"") + "\"" : v);
            }

            sb.Append("\r\n");
        }

        public static string FormatActions(bool restrict, IEnumerable<AllowedAction> actions)
        {
            if (!restrict)
                return "toutes";
            var parts = new List<string>();

            foreach (var a in actions)
                parts.Add(a.ToString());
            return parts.Count == 0 ? "aucune" : string.Join(" | ", parts);
        }

        #endregion

        #region Simple values

        private static readonly Dictionary<string, string> _StyleAliases = new() {
            { "bulle", "Bubble" }, { "fenetre", "Modal" }, { "modal", "Modal" }, { "centre", "Modal" },
            { "narrateur", "Narrator" }, { "aucun", "None" }, { "rien", "None" }
        };

        private static readonly Dictionary<string, string> _ShapeAliases = new() {
            { "rectangle", "RoundedRect" }, { "rect", "RoundedRect" }, { "cercle", "Circle" }, { "rond", "Circle" }
        };

        private static readonly Dictionary<string, string> _PointerAliases = new() {
            { "aucun", "None" }, { "non", "None" }, { "tapoter", "Tap" }, { "clic", "Tap" }, { "glisser", "Drag" }
        };

        private static readonly Dictionary<string, string> _PlacementAliases = new() {
            { "auto", "Auto" }, { "dessus", "Above" }, { "au_dessus", "Above" }, { "haut", "Above" },
            { "dessous", "Below" }, { "en_dessous", "Below" }, { "bas", "Below" }, { "gauche", "Left" }, { "droite", "Right" }
        };

        private static readonly Dictionary<string, string> _ConditionAliases = new() {
            { "immediat", "Immediate" }, { "delai", "WaitForDelay" }, { "bouton", "WaitForButton" }, { "suivant", "WaitForButton" },
            { "evenement", "WaitForEvent" }, { "action", "WaitForEvent" }, { "clic_cible", "WaitForTargetClick" }, { "clic", "WaitForTargetClick" }
        };

        private static T ParseEnum<T>(string raw, T fallback, Dictionary<string, string> aliases, Action<string> warn) where T : struct
        {
            if (string.IsNullOrWhiteSpace(raw))
                return fallback;
            string key = Normalize(raw);

            if (aliases != null && aliases.TryGetValue(key, out string mapped))
                raw = mapped;
            if (Enum.TryParse(raw.Replace(" ", string.Empty), true, out T value))
                return value;
            warn($"unknown value \"{raw}\" for {typeof(T).Name}, \"{fallback}\" used.");

            return fallback;
        }

        private static bool ParseBool(string raw, bool fallback, Action<string> warn)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return fallback;
            switch (Normalize(raw)) {
                case "oui": case "o": case "true": case "vrai": case "1": case "x": case "yes":
                    return true;
                case "non": case "n": case "false": case "faux": case "0": case "no":
                    return false;
            }

            warn($"\"{raw}\" is not yes/no, \"{(fallback ? "yes" : "no")}\" used.");

            return fallback;
        }

        private static float ParseFloat(string raw, float fallback, Action<string> warn)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return fallback;
            if (float.TryParse(raw.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float f))
                return f;
            warn($"\"{raw}\" is not a number, {fallback} used.");

            return fallback;
        }

        private static string NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s;

        public static List<string> SplitList(string raw)
        {
            var list = new List<string>();

            if (string.IsNullOrWhiteSpace(raw))
                return list;
            foreach (var part in raw.Split('|')) {
                string p = part.Trim();

                if (p.Length > 0)
                    list.Add(p);
            }

            return list;
        }

        /// <summary>Lowercase, without accents, with spaces, hyphens and apostrophes replaced by "_".</summary>
        public static string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            string decomposed = s.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var            sb = new StringBuilder(decomposed.Length);

            foreach (char c in decomposed) {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;
                sb.Append(c == ' ' || c == '-' || c == '\'' ? '_' : c);
            }

            return sb.ToString();
        }

        #endregion

        #region Actions and commands

        /// <summary>
        /// "buy:sword | end_turn" → list of actions (action:subject).
        /// "toutes", "all" or "*" = no restriction; empty, "aucune" or "none" = nothing is allowed.
        /// </summary>
        public static List<AllowedAction> ParseActions(string raw, out bool restrict, Action<string> warn = null)
        {
            var    list = new List<AllowedAction>();
            string norm = Normalize(raw);

            restrict = true;

            if (norm == "toutes" || norm == "tout" || norm == "*" || norm == "all") {
                restrict = false;

                return list;
            }

            if (norm.Length == 0 || norm == "aucune" || norm == "rien" || norm == "none")
                return list;
            foreach (var item in SplitList(raw)) {
                int         colon = item.IndexOf(':');
                string actionName = (colon < 0 ? item : item.Substring(0, colon)).Trim();
                string    subject = colon < 0 ? null : NullIfEmpty(item.Substring(colon + 1).Trim());

                if (actionName.Length == 0) {
                    warn?.Invoke($"action without a name in \"{item}\".");

                    continue;
                }

                if (_ActionAliases.TryGetValue(Normalize(actionName), out string actionId))
                    actionName = actionId;
                list.Add(new AllowedAction(actionName, subject));
            }

            return list;
        }

        private static readonly Regex _CommandRegex = new(@"^\s*(\w+)\s*(?:\((.*)\))?\s*$", RegexOptions.Compiled);

        public static List<TutorialCommand> ParseCommands(string raw, Action<string> warn = null)
        {
            var list = new List<TutorialCommand>();

            foreach (var item in SplitList(raw)) {
                var cmd = ParseCommand(item, warn);

                if (cmd != null)
                    list.Add(cmd);
            }

            return list;
        }

        /// <summary>"GiveItem(item=sword, player=0)" → TutorialCommand.</summary>
        public static TutorialCommand ParseCommand(string raw, Action<string> warn = null)
        {
            var m = _CommandRegex.Match(raw ?? string.Empty);

            if (!m.Success) {
                warn?.Invoke($"unreadable command \"{raw}\" (expected format: Name(key=value, ...)).");

                return null;
            }

            var cmd = new TutorialCommand { Name = m.Groups[1].Value };

            if (_KnownCommands.Count > 0 && !cmd.IsBuiltIn && !_KnownCommands.Contains(cmd.Name))
                warn?.Invoke($"unknown command \"{cmd.Name}\" (game commands: {string.Join(", ", _KnownCommands)}).");
            string args = m.Groups[2].Value;

            if (string.IsNullOrWhiteSpace(args))
                return cmd;
            foreach (var pair in args.Split(',')) {
                int eq = pair.IndexOf('=');

                if (eq < 0) {
                    warn?.Invoke($"parameter \"{pair.Trim()}\" without \"=\" in \"{raw}\".");

                    continue;
                }

                cmd.Args.Add(new TutorialCommandArg(pair.Substring(0, eq).Trim(), pair.Substring(eq + 1).Trim()));
            }

            return cmd;
        }

        #endregion
    }
}
