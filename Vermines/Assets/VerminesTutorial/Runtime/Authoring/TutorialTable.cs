using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Vermines.Tutorial.Authoring
{
    /// Une ligne du tableau de scénario, convertie en valeurs typées. Copiée ensuite
    /// dans un TutorialStepData par l'outil d'import de l'éditeur.
    public class ParsedStep
    {
        public int sourceLine;
        public string lessonId;
        public string stepId;
        public PopupStyle popupStyle = PopupStyle.Bubble;
        public string title;
        public string body;
        public string textKey;
        public List<string> targetIds = new List<string>();
        public SpotlightShape shape = SpotlightShape.RoundedRect;
        public float padding = 16f;
        public bool dimScreen = true;
        public bool blockClicksOutsideTargets = true;
        public PointerMode pointer = PointerMode.None;
        public string pointerDropTargetId;
        public PopupPlacement placement = PopupPlacement.Auto;
        public AdvanceCondition advanceCondition = AdvanceCondition.WaitForButton;
        public string eventId;
        public string eventArg;
        public int eventCount = 1;
        public float delaySeconds = 2f;
        public bool restrictGameActions = true;
        public List<AllowedAction> allowedActions = new List<AllowedAction>();
        public List<TutorialCommand> onEnter = new List<TutorialCommand>();
        public List<TutorialCommand> onExit = new List<TutorialCommand>();

        /// Copie la ligne dans un asset d'étape (import CSV dans l'éditeur, ou
        /// chargement du tableau à l'exécution). Le portrait n'est pas touché.
        public void ApplyTo(TutorialStepData dst)
        {
            dst.stepId = stepId;
            dst.popupStyle = popupStyle;
            dst.title = new TutorialText(title, string.IsNullOrEmpty(textKey) ? null : textKey + ".title");
            dst.body = new TutorialText(body, string.IsNullOrEmpty(textKey) ? null : textKey);
            dst.targetIds = new List<string>(targetIds);
            dst.shape = shape;
            dst.padding = padding;
            dst.dimScreen = dimScreen;
            dst.blockClicksOutsideTargets = blockClicksOutsideTargets;
            dst.pointer = pointer;
            dst.pointerDropTargetId = pointerDropTargetId;
            dst.placement = placement;
            dst.advanceCondition = advanceCondition;
            dst.eventId = eventId;
            dst.eventArg = eventArg;
            dst.eventCount = eventCount < 1 ? 1 : eventCount;
            dst.delaySeconds = delaySeconds;
            dst.restrictGameActions = restrictGameActions;
            dst.allowedActions = new List<AllowedAction>(allowedActions);
            dst.onEnter = new List<TutorialCommand>(onEnter);
            dst.onExit = new List<TutorialCommand>(onExit);
        }
    }

    /// Lecture et écriture du tableau de scénario au format CSV (export Excel /
    /// Google Sheets). Les en-têtes et les valeurs acceptent le français.
    ///
    /// Colonnes reconnues (ordre libre, colonnes manquantes = valeur par défaut) :
    ///   lecon, id_etape, style, titre, texte, cle_texte, cibles, forme, marge, voile,
    ///   bloquer_clics, pointeur, cible_depot, placement, condition, evenement,
    ///   argument, nombre, delai, actions_autorisees, commandes_debut, commandes_fin, notes
    ///
    /// Listes (cibles, actions, commandes) : éléments séparés par "|".
    public static class TutorialTable
    {
        public static readonly string[] Columns =
        {
            "lecon", "id_etape", "style", "titre", "texte", "cle_texte", "cibles", "forme", "marge", "voile",
            "bloquer_clics", "pointeur", "cible_depot", "placement", "condition", "evenement", "argument",
            "nombre", "delai", "actions_autorisees", "commandes_debut", "commandes_fin", "notes"
        };

        // ------------------------------------------------------------------ CSV brut

        /// Découpe un texte CSV (guillemets, retours à la ligne dans une cellule,
        /// séparateur ; , ou tabulation détecté sur la première ligne).
        public static List<string[]> ParseCsv(string text, char? delimiter = null)
        {
            var rows = new List<string[]>();
            if (string.IsNullOrEmpty(text))
                return rows;
            if (text[0] == '﻿')
                text = text.Substring(1);

            char sep = delimiter ?? DetectDelimiter(text);
            var row = new List<string>();
            var cell = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"')
                        {
                            cell.Append('"');
                            i++;
                        }
                        else
                            inQuotes = false;
                    }
                    else
                        cell.Append(c);
                    continue;
                }

                if (c == '"')
                    inQuotes = true;
                else if (c == sep)
                {
                    row.Add(cell.ToString());
                    cell.Clear();
                }
                else if (c == '\r' || c == '\n')
                {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                        i++;
                    row.Add(cell.ToString());
                    cell.Clear();
                    rows.Add(row.ToArray());
                    row.Clear();
                }
                else
                    cell.Append(c);
            }
            if (cell.Length > 0 || row.Count > 0)
            {
                row.Add(cell.ToString());
                rows.Add(row.ToArray());
            }
            rows.RemoveAll(r => Array.TrueForAll(r, string.IsNullOrWhiteSpace));
            return rows;
        }

        private static char DetectDelimiter(string text)
        {
            int end = text.IndexOfAny(new[] { '\r', '\n' });
            string header = end < 0 ? text : text.Substring(0, end);
            int semi = Count(header, ';'), comma = Count(header, ','), tab = Count(header, '\t');
            if (tab > semi && tab > comma) return '\t';
            return semi >= comma ? ';' : ',';
        }

        private static int Count(string s, char c)
        {
            int n = 0;
            foreach (char ch in s)
                if (ch == c) n++;
            return n;
        }

        // ------------------------------------------------------------- Tableau → étapes

        public static List<ParsedStep> Parse(string csvText, List<string> warnings)
        {
            warnings = warnings ?? new List<string>();
            var result = new List<ParsedStep>();
            var rows = ParseCsv(csvText);
            if (rows.Count == 0)
            {
                warnings.Add("Le fichier est vide.");
                return result;
            }

            var header = new Dictionary<string, int>();
            for (int c = 0; c < rows[0].Length; c++)
                header[Normalize(rows[0][c])] = c;
            if (!header.ContainsKey("id_etape"))
                warnings.Add("Colonne \"id_etape\" absente : les étapes seront numérotées automatiquement et ne pourront pas être mises à jour proprement.");

            string lastLesson = null;
            for (int r = 1; r < rows.Count; r++)
            {
                var cells = rows[r];
                int line = r + 1;
                string Get(string col) => header.TryGetValue(col, out int i) && i < cells.Length ? cells[i].Trim() : string.Empty;

                var step = new ParsedStep { sourceLine = line };
                step.lessonId = Get("lecon");
                if (string.IsNullOrEmpty(step.lessonId))
                    step.lessonId = lastLesson;
                lastLesson = step.lessonId;
                step.stepId = Get("id_etape");
                if (string.IsNullOrEmpty(step.stepId))
                    step.stepId = $"{step.lessonId ?? "etape"}_{r:00}";

                void Warn(string msg) => warnings.Add($"Ligne {line} ({step.stepId}) : {msg}");

                step.popupStyle = ParseEnum(Get("style"), PopupStyle.Bubble, StyleAliases, Warn);
                step.title = Get("titre");
                step.body = Get("texte").Replace("\\n", "\n");
                step.textKey = Get("cle_texte");
                step.targetIds = SplitList(Get("cibles"));
                step.shape = ParseEnum(Get("forme"), SpotlightShape.RoundedRect, ShapeAliases, Warn);
                step.padding = ParseFloat(Get("marge"), 16f, Warn);
                step.dimScreen = ParseBool(Get("voile"), true, Warn);
                step.blockClicksOutsideTargets = ParseBool(Get("bloquer_clics"), true, Warn);
                step.pointer = ParseEnum(Get("pointeur"), PointerMode.None, PointerAliases, Warn);
                step.pointerDropTargetId = NullIfEmpty(Get("cible_depot"));
                step.placement = ParseEnum(Get("placement"), PopupPlacement.Auto, PlacementAliases, Warn);
                step.advanceCondition = ParseEnum(Get("condition"), AdvanceCondition.WaitForButton, ConditionAliases, Warn);
                step.eventId = NullIfEmpty(Get("evenement"));
                step.eventArg = NullIfEmpty(Get("argument"));
                step.eventCount = (int)ParseFloat(Get("nombre"), 1f, Warn);
                step.delaySeconds = ParseFloat(Get("delai"), 2f, Warn);
                step.allowedActions = ParseActions(Get("actions_autorisees"), out step.restrictGameActions, Warn);
                step.onEnter = ParseCommands(Get("commandes_debut"), Warn);
                step.onExit = ParseCommands(Get("commandes_fin"), Warn);

                if (!string.IsNullOrEmpty(step.eventId) && string.IsNullOrEmpty(Get("condition")))
                    step.advanceCondition = AdvanceCondition.WaitForEvent;

                result.Add(step);
            }
            return result;
        }

        // ------------------------------------------------------------- Étapes → tableau

        public static string ToCsv(IEnumerable<ParsedStep> steps, char delimiter = ';')
        {
            var sb = new StringBuilder();
            AppendRow(sb, Columns, delimiter);
            foreach (var s in steps)
            {
                AppendRow(sb, new[]
                {
                    s.lessonId, s.stepId, s.popupStyle.ToString(), s.title, (s.body ?? string.Empty).Replace("\n", "\\n"), s.textKey,
                    string.Join("|", s.targetIds), s.shape.ToString(), Fmt(s.padding), s.dimScreen ? "oui" : "non",
                    s.blockClicksOutsideTargets ? "oui" : "non", s.pointer.ToString(), s.pointerDropTargetId, s.placement.ToString(),
                    s.advanceCondition.ToString(), s.eventId, s.eventArg, s.eventCount.ToString(CultureInfo.InvariantCulture),
                    Fmt(s.delaySeconds), FormatActions(s.restrictGameActions, s.allowedActions),
                    string.Join(" | ", s.onEnter.ConvertAll(c => c.ToString())),
                    string.Join(" | ", s.onExit.ConvertAll(c => c.ToString())), string.Empty
                }, delimiter);
            }
            return sb.ToString();
        }

        private static string Fmt(float f) => f.ToString(CultureInfo.InvariantCulture);

        private static void AppendRow(StringBuilder sb, IList<string> cells, char delimiter)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (i > 0)
                    sb.Append(delimiter);
                string v = cells[i] ?? string.Empty;
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

        // ------------------------------------------------------------ Valeurs simples

        private static readonly Dictionary<string, string> StyleAliases = new Dictionary<string, string>
        {
            { "bulle", "Bubble" }, { "fenetre", "Modal" }, { "modal", "Modal" }, { "centre", "Modal" },
            { "narrateur", "Narrator" }, { "aucun", "None" }, { "rien", "None" }
        };
        private static readonly Dictionary<string, string> ShapeAliases = new Dictionary<string, string>
        {
            { "rectangle", "RoundedRect" }, { "rect", "RoundedRect" }, { "cercle", "Circle" }, { "rond", "Circle" }
        };
        private static readonly Dictionary<string, string> PointerAliases = new Dictionary<string, string>
        {
            { "aucun", "None" }, { "non", "None" }, { "tapoter", "Tap" }, { "clic", "Tap" }, { "glisser", "Drag" }
        };
        private static readonly Dictionary<string, string> PlacementAliases = new Dictionary<string, string>
        {
            { "auto", "Auto" }, { "dessus", "Above" }, { "au_dessus", "Above" }, { "haut", "Above" },
            { "dessous", "Below" }, { "en_dessous", "Below" }, { "bas", "Below" }, { "gauche", "Left" }, { "droite", "Right" }
        };
        private static readonly Dictionary<string, string> ConditionAliases = new Dictionary<string, string>
        {
            { "immediat", "Immediate" }, { "delai", "WaitForDelay" }, { "bouton", "WaitForButton" }, { "suivant", "WaitForButton" },
            { "evenement", "WaitForEvent" }, { "action", "WaitForEvent" }, { "clic_cible", "WaitForTargetClick" }, { "clic", "WaitForTargetClick" }
        };
        private static readonly Dictionary<string, GameActionType> ActionAliases = new Dictionary<string, GameActionType>(StringComparer.OrdinalIgnoreCase)
        {
            { "jouer", GameActionType.PlayCard }, { "acheter", GameActionType.BuyCard }, { "recycler", GameActionType.RecycleCard },
            { "defausser", GameActionType.DiscardCard }, { "garder", GameActionType.KeepCard },
            { "activer", GameActionType.ActivatePartisanEffect }, { "sacrifier", GameActionType.SacrificePartisan },
            { "fin_tour", GameActionType.EndTurn }, { "fin_de_tour", GameActionType.EndTurn }, { "menu", GameActionType.OpenMenu },
            { "autre", GameActionType.Other },
            { "phase_suivante", GameActionType.NextPhase }, { "phase", GameActionType.NextPhase },
            { "choisir", GameActionType.ChooseEffect }, { "choisir_effet", GameActionType.ChooseEffect }
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
            warn($"valeur \"{raw}\" inconnue pour {typeof(T).Name}, \"{fallback}\" utilisé.");
            return fallback;
        }

        private static bool ParseBool(string raw, bool fallback, Action<string> warn)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return fallback;
            switch (Normalize(raw))
            {
                case "oui": case "o": case "true": case "vrai": case "1": case "x": case "yes": return true;
                case "non": case "n": case "false": case "faux": case "0": case "no": return false;
            }
            warn($"\"{raw}\" n'est pas oui/non, \"{(fallback ? "oui" : "non")}\" utilisé.");
            return fallback;
        }

        private static float ParseFloat(string raw, float fallback, Action<string> warn)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return fallback;
            if (float.TryParse(raw.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float f))
                return f;
            warn($"\"{raw}\" n'est pas un nombre, {fallback} utilisé.");
            return fallback;
        }

        private static string NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s;

        public static List<string> SplitList(string raw)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(raw))
                return list;
            foreach (var part in raw.Split('|'))
            {
                string p = part.Trim();
                if (p.Length > 0)
                    list.Add(p);
            }
            return list;
        }

        /// Minuscules, sans accents, espaces et tirets remplacés par "_".
        public static string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            string decomposed = s.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(decomposed.Length);
            foreach (char c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;
                sb.Append(c == ' ' || c == '-' || c == '\'' ? '_' : c);
            }
            return sb.ToString();
        }

        // ------------------------------------------------------- Actions et commandes

        /// "acheter:ouvriere | fin_tour" → liste d'actions. "toutes" ou "*" = pas de
        /// restriction ; vide ou "aucune" = rien n'est autorisé.
        public static List<AllowedAction> ParseActions(string raw, out bool restrict, Action<string> warn = null)
        {
            var list = new List<AllowedAction>();
            restrict = true;
            string norm = Normalize(raw);
            if (norm == "toutes" || norm == "tout" || norm == "*" || norm == "all")
            {
                restrict = false;
                return list;
            }
            if (norm.Length == 0 || norm == "aucune" || norm == "rien" || norm == "none")
                return list;

            foreach (var item in SplitList(raw))
            {
                int colon = item.IndexOf(':');
                string actionName = (colon < 0 ? item : item.Substring(0, colon)).Trim();
                string cardId = colon < 0 ? null : NullIfEmpty(item.Substring(colon + 1).Trim());

                if (!ActionAliases.TryGetValue(Normalize(actionName), out GameActionType type)
                    && !Enum.TryParse(actionName, true, out type))
                {
                    warn?.Invoke($"action \"{actionName}\" inconnue (attendu : jouer, acheter, recycler, defausser, garder, activer, sacrifier, fin_tour, phase_suivante, choisir...).");
                    continue;
                }
                list.Add(new AllowedAction(type, cardId));
            }
            return list;
        }

        private static readonly Regex CommandRegex = new Regex(@"^\s*(\w+)\s*(?:\((.*)\))?\s*$", RegexOptions.Compiled);

        public static List<TutorialCommand> ParseCommands(string raw, Action<string> warn = null)
        {
            var list = new List<TutorialCommand>();
            foreach (var item in SplitList(raw))
            {
                var cmd = ParseCommand(item, warn);
                if (cmd != null)
                    list.Add(cmd);
            }
            return list;
        }

        /// "GiveCardToHand(card=ouvriere, player=0)" → TutorialCommand.
        public static TutorialCommand ParseCommand(string raw, Action<string> warn = null)
        {
            var m = CommandRegex.Match(raw ?? string.Empty);
            if (!m.Success)
            {
                warn?.Invoke($"commande illisible \"{raw}\" (format attendu : Type(cle=valeur, ...)).");
                return null;
            }
            if (!Enum.TryParse(m.Groups[1].Value, true, out TutorialCommandType type))
            {
                warn?.Invoke($"commande \"{m.Groups[1].Value}\" inconnue.");
                return null;
            }

            var cmd = new TutorialCommand { type = type };
            string args = m.Groups[2].Value;
            if (string.IsNullOrWhiteSpace(args))
                return cmd;

            foreach (var pair in args.Split(','))
            {
                int eq = pair.IndexOf('=');
                if (eq < 0)
                {
                    warn?.Invoke($"paramètre \"{pair.Trim()}\" sans \"=\" dans \"{raw}\".");
                    continue;
                }
                string key = Normalize(pair.Substring(0, eq));
                string value = pair.Substring(eq + 1).Trim();
                switch (key)
                {
                    case "player": case "joueur": cmd.player = ParseInt(value, warn, raw); break;
                    case "card": case "carte": cmd.card = value; break;
                    case "target": case "cible": cmd.target = value; break;
                    case "amount": case "nombre": case "quantite": cmd.amount = ParseInt(value, warn, raw); break;
                    case "seconds": case "secondes": case "duree":
                        cmd.seconds = float.TryParse(value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float f) ? f : 0f;
                        break;
                    case "arg": case "texte": case "event": case "evenement": cmd.arg = value; break;
                    default: warn?.Invoke($"paramètre \"{key}\" inconnu dans \"{raw}\"."); break;
                }
            }
            return cmd;
        }

        private static int ParseInt(string value, Action<string> warn, string raw)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i))
                return i;
            warn?.Invoke($"\"{value}\" n'est pas un entier dans \"{raw}\".");
            return 0;
        }
    }
}
