using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class CardUIView : MonoBehaviour {

    /**
     * Descriptor field images.
     * 
     * Index 0: Partisan fields
     * Index 1: Tools & Equipment fields
     */
    public Sprite[] DescriptorFieldImages;

    /**
     * Icons of the specific type of card.
     * 
     * Index 0: Left name field icon
     * Index 1: Right name field icon
     */
    public Image[] Icons;

    public Image Background; // Background image from Sprites/Card/{Type}/Background.jpg
    public Image SplashArt;  // Character art image from Sprites/Card/{Type}/{Character}.png

    // Using Text Mesh Pro
    public TMP_Text Name;
    public TMP_Text Description;

    // Top Right Banner
    // -- Souls
    public GameObject SoulsBanner; // Active if the card is a Partisan card, disabled otherwise
    public TMP_Text   Souls;

    // -- Eloquence
    public GameObject EloquenceBanner; // Active if the card has a cost, disabled otherwise
    public TMP_Text   Eloquence;

    private ICard _Card;

    public void Start()
    {
        UpdateUI();
    }

    public void SetCard(ICard card)
    {
        _Card = card;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_Card == null) {
            Debug.LogWarning("Card is null.");

            return;
        }

        Debug.Assert(DescriptorFieldImages.Length == 2, "DescriptorFieldImages must have 2 elements.");
        Debug.Assert(Icons.Length == 2, "Icons must have 2 elements.");

        CardData data = _Card.Data;

        // -- Changing the background image depending on the card type
        Background.sprite = Resources.Load<Sprite>("Sprites/Card/" + data.Type.ToString() + "/Background");
        //        Background.sprite = Resources.Load<Sprite>("Assets/Resources/Sprites/Card/" + data.Type.ToString() + "/Background.jpg");

        // -- Changing the icons depending on the card type
        for (int i = 0; i < Icons.Length; i++) {
            Icons[i].sprite = Resources.Load<Sprite>("Sprites/Card/" + data.Type.ToString() + "/Icon");

            Icons[i].gameObject.SetActive(Icons[i].sprite != null);
        }

        // -- Changing the descriptor field image depending on the card type
        if (data.Type == CardType.Tools || data.Type == CardType.Equipment)
            DescriptorFieldImages[1] = Resources.Load<Sprite>("Sprites/UI/Card/Item_Card_Descriptor");
        else
            DescriptorFieldImages[0] = Resources.Load<Sprite>("Sprites/UI/Card/Partisan_Card_Descriptor");

        // -- Update Character Art
        if (data.Sprite != null) {
            SplashArt.gameObject.SetActive(true);

            SplashArt.sprite = data.Sprite;
        } else {
            SplashArt.gameObject.SetActive(false);
        }
        // -- Update Name and Description
        Name.text        = data.Name;
        Description.text = data.Description;

        // -- Update Eloquence Banner
        if (_Card.HasCost())
            UpdateBanner(EloquenceBanner, Eloquence, data.Eloquence.ToString());
        else
            UpdateBanner(EloquenceBanner, Eloquence, "-1");

        // -- Update Souls Banner
        if (_Card is PartisanCard)
            UpdateBanner(SoulsBanner, Souls, data.Souls.ToString());
        else
            UpdateBanner(SoulsBanner, Souls, "-1");
    }

    private void UpdateBanner(GameObject banner, TMP_Text text, string value)
    {
        banner.SetActive(value != "-1");

        text.text = value;
    }
}
