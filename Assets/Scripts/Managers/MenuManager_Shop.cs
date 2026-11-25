using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static Constants;
using static Utils;

// Shop menu: buying and equipping cosmetics, upgrading skills
public sealed partial class MenuManager
{
    [SerializeField] SpriteAtlas spriteAtlas;
    [SerializeField] GameObject shopCosmetics, shopSkills;
    [SerializeField] Image cosmeticPreviewImage;
    [SerializeField] GameObject cosmeticBuyButton;
    [SerializeField] TextMeshProUGUI[] cosmeticStyleTexts;
    [SerializeField] TextMeshProUGUI cosmeticPriceText;
    [SerializeField] TextMeshProUGUI[] skillLevelTexts;
    [SerializeField] TextMeshProUGUI[] skillPriceTexts;
    [SerializeField] TextMeshProUGUI coinText;
    [SerializeField] GameObject[] skillBuyButtons;
    [SerializeField] AudioClip[] buySounds;

    enum ShopType : byte { Bird = 1, Background = 2, Obstacle = 3 } // Values are passed from the shop buttons in the scene

    // One purchasable cosmetic category. Name doubles as the sprite prefix ("Bird0") and the save-key prefix ("BirdSelected", "BirdsBought0").
    sealed class CosmeticCategory
    {
        public readonly string Name;
        public readonly int Cost;
        public readonly bool[] Bought;
        public int Selected;

        public CosmeticCategory(string name, int count, int cost)
        {
            Name = name;
            Cost = cost;
            Bought = new bool[count];
        }

        public string SelectedKey => Name + "Selected";
        public string BoughtKey(int index) => $"{Name}sBought{index}";
    }

    readonly CosmeticCategory[] categories =
    {
        new("Bird", MaxBirdTypes, BirdUnlockCost),
        new("Background", MaxBackgroundTypes, BackgroundUnlockCost),
        new("Obstacle", MaxObstacleTypes, ObstacleUnlockCost),
    };

    // Skills, indexed like the buttons in the scene: 0 = Greed, 1 = Shield
    static readonly string[] SkillKeys = { PrefKeys.Skill1GreedLevel, PrefKeys.Skill2ShieldLevel };
    readonly int[] skillLevels = new int[2];

    int coin;
    ShopType shopType;
    int selectedItemIndex;

    CosmeticCategory Category => categories[(int)shopType - 1];

    void LoadShopStats()
    {
        foreach (var category in categories)
        {
            category.Selected = PlayerPrefs.GetInt(category.SelectedKey);
            category.Bought[0] = true; // The first style is always unlocked
            for (int i = 1; i < category.Bought.Length; i++) category.Bought[i] = GetPrefBool(category.BoughtKey(i));
        }
        for (int i = 0; i < skillLevels.Length; i++) skillLevels[i] = Mathf.Clamp(PlayerPrefs.GetInt(SkillKeys[i]), 0, MaxSkillLevel);
    }

    void SaveShopStats()
    {
        foreach (var category in categories)
        {
            PlayerPrefs.SetInt(category.SelectedKey, category.Selected);
            for (int i = 1; i < category.Bought.Length; i++) SetPrefBool(category.BoughtKey(i), category.Bought[i]);
        }
        for (int i = 0; i < skillLevels.Length; i++) PlayerPrefs.SetInt(SkillKeys[i], skillLevels[i]);
    }

    // Opens the cosmetics shop for birds, backgrounds or obstacles
    public void OpenCosmeticShop(int index)
    {
        shopMenu.SetActive(false);
        shopCosmetics.SetActive(true);
        cosmeticBuyButton.SetActive(false);
        shopType = (ShopType)index;

        for (int i = 0; i < cosmeticStyleTexts.Length; i++) cosmeticStyleTexts[i].gameObject.SetActive(i < Category.Bought.Length);

        selectedItemIndex = Category.Selected;
        cosmeticPreviewImage.rectTransform.sizeDelta = shopType == ShopType.Obstacle ? new Vector2(141, 512) : new Vector2(512, 512); // Obstacles are tall and narrow

        UpdateCosmeticItemUI();
    }

    // Colors the style labels (yellow = equipped, white = owned, grey = locked) and shows the previewed style
    void UpdateCosmeticItemUI()
    {
        var category = Category;
        for (int i = 0; i < cosmeticStyleTexts.Length; i++)
        {
            if (!cosmeticStyleTexts[i].gameObject.activeSelf) continue;
            bool owned = category.Bought[i];
            cosmeticStyleTexts[i].color = !owned ? Color.grey : i == category.Selected ? Color.yellow : Color.white;
        }

        cosmeticPreviewImage.sprite = spriteAtlas.GetSprite(category.Name + selectedItemIndex);
    }

    // Equips the style if it's owned, otherwise offers to buy it
    public void SelectCosmeticItem(int index)
    {
        selectedItemIndex = index;
        var category = Category;

        if (category.Bought[index])
        {
            cosmeticBuyButton.SetActive(false);
            category.Selected = index;
        }
        else
        {
            cosmeticBuyButton.SetActive(true);
            cosmeticPriceText.text = $"Buy: {category.Cost} coin";
        }

        UpdateCosmeticItemUI();
    }

    public void BuySelectedItem()
    {
        var category = Category;
        if (!TrySpend(category.Cost)) return;

        cosmeticBuyButton.SetActive(false);
        category.Bought[selectedItemIndex] = true;
        category.Selected = selectedItemIndex;
        UpdateCosmeticItemUI();
    }

    public void OpenSkillShop()
    {
        shopMenu.SetActive(false);
        shopSkills.SetActive(true);
        UpdateSkillUI();
    }

    public void BuySkill(int index)
    {
        if (skillLevels[index] >= MaxSkillLevel || !TrySpend(SkillUpgradeCost)) return;

        skillLevels[index]++;
        UpdateSkillUI();
    }

    void UpdateSkillUI()
    {
        for (int i = 0; i < skillLevels.Length; i++)
        {
            skillLevelTexts[i].text = $"{skillLevels[i]} / {MaxSkillLevel}";
            skillBuyButtons[i].SetActive(skillLevels[i] < MaxSkillLevel);
        }
        foreach (TextMeshProUGUI text in skillPriceTexts) text.text = $"Buy: {SkillUpgradeCost} coin";
    }

    // Deducts the cost and plays the purchase sound, or plays the error sound if the player can't afford it
    bool TrySpend(int cost)
    {
        if (coin < cost)
        {
            audioSource.PlayOneShot(cantDoThatSound);
            return false;
        }

        audioSource.PlayOneShot(GetRandomElement(buySounds));
        coin -= cost;
        coinText.text = coin.ToString();
        return true;
    }
}
