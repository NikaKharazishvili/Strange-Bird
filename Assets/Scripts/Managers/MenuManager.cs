using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Utils;

// Main menu: navigation, scene loading and persistence. Shop and options live in the other partial files.
public sealed partial class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject menu, loadingMenu, shopMenu, optionMenu;
    [SerializeField] Image loadingBar, background;
    [SerializeField] TextMeshProUGUI loadingText;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip cantDoThatSound;

    enum MenuType : byte { Play = 0, Shop = 1, Options = 2, Quit = 3, BackToMain = 4, BackToShop = 5 } // Values are wired to the buttons in the scene

    void Awake() => LoadStats();
    void OnApplicationQuit() => SaveStats();

    // Main menu button presses
    public void MenuSelection(int index)
    {
        switch ((MenuType)index)
        {
            case MenuType.Play: Play(); break;
            case MenuType.Shop: ShowSubMenu(shopMenu); break;
            case MenuType.Options: ShowSubMenu(optionMenu); UpdateGameSpeedTextColors(); break;
            case MenuType.Quit: QuitApplication(); break;
            case MenuType.BackToMain: BackToMain(); break;
            case MenuType.BackToShop: BackToShop(); break;
        }
    }

    void Play()
    {
        SaveStats();
        foreach (Transform child in menu.transform) child.gameObject.SetActive(false);
        loadingMenu.SetActive(true);
        StartCoroutine(LoadSceneAsync("Game", loadingBar, loadingText));
    }

    // Dims the background and shows the requested submenu
    void ShowSubMenu(GameObject submenu)
    {
        background.color = new Color(0.2f, 0.2f, 0.2f);
        menu.SetActive(false);
        submenu.SetActive(true);
    }

    void BackToMain()
    {
        background.color = Color.white;
        menu.SetActive(true);
        shopMenu.SetActive(false);
        optionMenu.SetActive(false);

        // Cancel an unfinished key selection and restore the current key name
        StopCoroutine(nameof(DetectFlapKey));
        flapKeyText.text = FormatKeyName(flapKey);
    }

    // Back from a shop page (cosmetics or skills) to the shop menu
    void BackToShop()
    {
        shopMenu.SetActive(true);
        shopCosmetics.SetActive(false);
        cosmeticBuyButton.SetActive(false);
        shopSkills.SetActive(false);
    }

    void LoadStats()
    {
        // Options
        gameSpeed = Mathf.Clamp(PlayerPrefs.GetInt(PrefKeys.GameSpeed), 0, Constants.MaxGameSpeed);
        gameSpeedTexts[gameSpeed].color = Color.yellow;

        AudioListener.volume = PlayerPrefs.GetFloat(PrefKeys.GlobalVolume, 1);
        SetCheckmarkSprite(soundsCheckmark, AudioListener.volume == 1);

        spawnBirds = GetPrefBool(PrefKeys.SpawnBirds, true);
        SetCheckmarkSprite(birdsCheckmark, spawnBirds);

        showFps = GetPrefBool(PrefKeys.ShowFps);
        fpsText.gameObject.SetActive(showFps);
        if (showFps && !IsInvoking(nameof(ShowFps))) InvokeRepeating(nameof(ShowFps), 0, Constants.FPSHudUpdateInterval); // Guard against double invoke after ResetGameProgress
        SetCheckmarkSprite(fpsCheckmark, showFps);

        flapKey = PlayerPrefs.GetString(PrefKeys.FlapKey, PrefKeys.DefaultFlapKey);
        flapKeyText.text = FormatKeyName(flapKey);

        // Shop
        LoadShopStats();
        coin = PlayerPrefs.GetInt(PrefKeys.Coin);
        coinText.text = coin.ToString();
    }

    void SaveStats()
    {
        PlayerPrefs.SetInt(PrefKeys.GameSpeed, gameSpeed);
        PlayerPrefs.SetFloat(PrefKeys.GlobalVolume, AudioListener.volume);
        SetPrefBool(PrefKeys.SpawnBirds, spawnBirds);
        SetPrefBool(PrefKeys.ShowFps, showFps);
        PlayerPrefs.SetString(PrefKeys.FlapKey, flapKey);

        SaveShopStats();
        PlayerPrefs.SetInt(PrefKeys.Coin, coin);
        PlayerPrefs.Save();
    }

    // Development tools, editor only
#if UNITY_EDITOR
    [ContextMenu("Reset Game Progress")]
    void ResetGameProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        LoadStats();
        Debug.Log("Game progress reset: all settings, coins and unlocks cleared.");
    }

    [ContextMenu("Add 500 Coins")]
    void Add500Coin()
    {
        coin += 500;
        coinText.text = coin.ToString();
        Debug.Log($"Added 500 coins. Total: {coin}");
    }
#endif
}
