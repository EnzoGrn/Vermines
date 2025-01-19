using UnityEditor;
using UnityEngine;
using System.IO;

namespace Vermines.CardSystem.Data {

    using Vermines.CardSystem.Enumerations;

    [CustomEditor(typeof(CardData))]
    public class CardDataEditor : Editor {

        private bool _ShowProperties = true;
        private bool _ShowStats      = true;

        public override void OnInspectorGUI()
        {
            if (target == null || target is not CardData)
                return;
            CardData cardData = (CardData)target;

            // -- Title
            GUILayout.Label("Card Configuration", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // -- [Header("Card Information")]
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Card Information", EditorStyles.boldLabel);

            // [Tooltip("The name of the card.")]
            cardData.Name = EditorGUILayout.TextField(new GUIContent("Card Name", "The name of the card."), cardData.Name);

            // [Tooltip("The description of every action that the card can perform.")]
            cardData.Description = EditorGUILayout.TextField(new GUIContent("Card Description", "The description of every action that the card can perform."), cardData.Description);

            GUILayout.Space(5);
            DrawDescriptionPreview(cardData.Description);
            GUILayout.Space(5);

            // [Tooltip("Number of exemplars of the card. (Use in Editing mode and when the cards are loading)")]
            cardData.Exemplars = EditorGUILayout.IntField(new GUIContent("Exemplars", "Number of exemplars of the card. (Use in Editing mode and when the cards are loading).\nThis value must be superior to 0."), cardData.Exemplars);

            // [Tooltip("Is the card in the default deck of players?")]
            cardData.IsStartingCard = EditorGUILayout.Toggle(new GUIContent("Is Starting Card", "Is the card in the default deck of players?"), cardData.IsStartingCard);

            GUILayout.EndVertical();
            GUILayout.Space(10);
            // -- EOF --

            // -- [Header("Card Properties")]
            _ShowProperties = EditorGUILayout.Foldout(_ShowProperties, "Card Properties", true);

            if (_ShowProperties) {
                GUILayout.BeginVertical(EditorStyles.helpBox);

                // [Tooltip("The type of the card.")]
                cardData.Type = (CardType)EditorGUILayout.EnumPopup(new GUIContent("Type", "The type of the card."), cardData.Type);


                if (cardData.IsStartingCard == false && cardData.Type == CardType.Partisan) {
                    // [Tooltip("Did the card belongs to a family of a player?")]
                    cardData.IsFamilyCard = EditorGUILayout.Toggle(new GUIContent("Is Family Card", "Did the card belongs to a family of a player?"), cardData.IsFamilyCard);
                } else {
                    cardData.IsFamilyCard = false;
                }

                if (cardData.Type == CardType.Partisan) {
                    if (cardData.IsFamilyCard == false) {
                        // [Tooltip("Family of the card.")]
                        cardData.Family = (CardFamily)EditorGUILayout.EnumPopup(new GUIContent("Family", "Family of the card."), cardData.Family);
                    } else {
                        cardData.Family = CardFamily.None;
                    }

                    if (cardData.IsStartingCard == false) {
                        // [Tooltip("Level of the card, only available for partisan cards.")]
                        // [Range(1, 2)]
                        cardData.Level = EditorGUILayout.IntSlider(new GUIContent("Level", "Level of the card, only available for partisan cards."), cardData.Level, 1, 2);
                    } else
                        cardData.Level = 0;
                } else {
                    cardData.Family = CardFamily.None;
                    cardData.Level  = 0;
                }

                GUILayout.EndVertical();
            }
            // -- EOF --

            // -- [Header("Card Stats")]
            if (cardData.IsStartingCard == false || cardData.Type == CardType.Partisan) {
                _ShowStats = EditorGUILayout.Foldout(_ShowStats, "Card Stats", true);

                if (_ShowStats) {
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    if (cardData.IsStartingCard == false) {
                        // [Tooltip("The cost of the card (with Eloquence as the currency).")]
                        cardData.Eloquence = EditorGUILayout.IntField(new GUIContent("Eloquence (cost)", "The cost of the card (with Eloquence as the currency)."), cardData.Eloquence);
                    } else {
                        cardData.Eloquence = 0;
                    }

                    if (cardData.Type == CardType.Partisan) {
                        // [Tooltip("The souls of the card (souls represent the points system of the game).")]
                        cardData.Souls = EditorGUILayout.IntField(new GUIContent("Souls", "The souls of the card (souls represent the points system of the game)."), cardData.Souls);
                    } else {
                        cardData.Souls = 0;
                    }

                    GUILayout.EndVertical();
                }
            }
            // -- EOF --

            // -- [Header("UI Elements")]
            GUILayout.Space(10);
            GUILayout.BeginVertical(EditorStyles.helpBox);

            if (cardData.IsFamilyCard == false) {
                DrawSpritePreview(cardData.Sprite, cardData.Type, cardData.Family);

                // [Tooltip("The visual representation of the card. (Character, object, etc.)")]
                cardData.Sprite     = (Sprite)EditorGUILayout.ObjectField(new GUIContent("Card Visual", "The visual representation of the card. (Character, object, etc.)"), cardData.Sprite, typeof(Sprite), false);
                cardData.SpriteName = string.Empty;
            } else {
                // [Tooltip("The sprite name of the visual, an item, or a character that represents the card.")]
                cardData.SpriteName = EditorGUILayout.TextField(new GUIContent("Sprite Name", "The sprite name of the visual, an item, or a character that represents the card."), cardData.SpriteName);
                cardData.Sprite     = null;

                DrawAllSpritePreview(cardData.SpriteName);
            }

            GUILayout.EndVertical();
            // -- EOF --

            // -- Refresh the UI, if it has changed
            if (GUI.changed)
                EditorUtility.SetDirty(cardData);
        }

        private void DrawDescriptionPreview(string description)
        {
            if (description.Equals(string.Empty))
                return;
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Description Preview", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Transform the description into a rich text preview
            string preview = FormatDescription(description);

            // Display the formatted description
            GUIStyle richTextStyle = new(EditorStyles.label) {
                richText = true,
                wordWrap = true
            };
            GUILayout.Label(preview, richTextStyle);

            GUILayout.EndVertical();
        }

        private string FormatDescription(string description)
        {
            // Red color for '{number}A'
            description = System.Text.RegularExpressions.Regex.Replace(description, @"\{(\d+)A\}", "<color=red>{$1A}</color>");

            // Purple color for '{number}E' 
            description = System.Text.RegularExpressions.Regex.Replace(description, @"\{(\d+)E\}", "<color=purple>{$1E}</color>");

            description = description.Replace("<br>", "<b><br></b>");

            return description;
        }

        private void DrawSpritePreview(Sprite sprite, CardType type, CardFamily family)
        {
            if (sprite == null) {
                EditorGUILayout.HelpBox("No preview available\nDrag and drop a sprite on the bottom-right box", MessageType.Info);
            } else {
                Texture2D background = AssetPreview.GetAssetPreview(GetDefaultSprite(type, family, "Background.png"));
                Texture2D icon = AssetPreview.GetAssetPreview(GetDefaultSprite(type, family, "Icon.png"));
                Texture2D texture    = AssetPreview.GetAssetPreview(sprite);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("Card Visual Preview", EditorStyles.boldLabel);

                float previewWidth  = 102.40f;
                float previewHeight = 177.15f;

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                Rect previewRect = GUILayoutUtility.GetRect(previewWidth, previewHeight);

                if (background != null)
                    GUI.DrawTexture(previewRect, background, ScaleMode.ScaleToFit);
                if (texture != null) {
                    float textureWidth  = previewWidth  * 0.6f; // 60% scale 
                    float textureHeight = previewHeight * 0.6f; // 60% scale

                    Rect textureRect = new(previewRect.x + (previewRect.width - textureWidth) / 2, previewRect.y + (previewRect.height - textureHeight) / 2, textureWidth, textureHeight);

                    GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleToFit, true, 0, Color.white, 0, 0);
                }
                if (icon != null) {
                    float iconWidth = previewWidth * 0.2f; // 20% scale
                    float iconHeight = previewHeight * 0.2f; // 20% scale

                    Rect iconRect = new(previewRect.x + 5, previewRect.y + 35, iconWidth, iconHeight);

                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true, 0, Color.white, 0, 0);
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }

        private void DrawAllSpritePreview(string spriteName)
        {
            if (spriteName.Equals(string.Empty)) {
                return;
            } else {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("Family Sprite Previews", EditorStyles.boldLabel);

                foreach (CardFamily family in System.Enum.GetValues(typeof(CardFamily))) {
                    if (family == CardFamily.None || family == CardFamily.Count)
                        continue;
                    string   folderPath  = $"Assets/Resources/Sprites/Card/{family}";
                    string[] spritePaths = Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories);

                    foreach (string path in spritePaths) {
                        if (!path.Contains(spriteName))
                            continue;
                        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                        if (sprite != null) {
                            GUILayout.Space(10);
                            GUILayout.Label($"Family: {family}", EditorStyles.boldLabel);

                            DrawSpritePreview(sprite, CardType.Partisan, family);

                            break;
                        }
                    }
                }

                GUILayout.EndVertical();
            }
        }

        private Sprite GetDefaultSprite(CardType type, CardFamily family, string spriteWanted)
        {
            string folderPath = $"Assets/Resources/Sprites/Card/";
            Sprite background;

            if (type == CardType.Equipment || type == CardType.Tools) {
                folderPath += $"{type}";
            } else if (type == CardType.Partisan) {
                if (family == CardFamily.None)
                    return null;
                folderPath += $"{family}";
            } else {
                return null;
            }
            string[] sprites = Directory.GetFiles(folderPath, spriteWanted, SearchOption.AllDirectories);

            if (sprites == null)
                return null;
            background = AssetDatabase.LoadAssetAtPath<Sprite>(sprites[0]);

            return background;
        }
    }
}
