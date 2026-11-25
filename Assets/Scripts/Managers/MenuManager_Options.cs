using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Constants;
using static Utils;

// Options menu: game speed, sound, FPS counter, bird spawning and flap key
public sealed partial class MenuManager
{
    [SerializeField] TextMeshProUGUI[] gameSpeedTexts;
    [SerializeField] TextMeshProUGUI fpsText, flapKeyText;
    [SerializeField] Image soundsCheckmark, birdsCheckmark, fpsCheckmark;
    [SerializeField] AudioClip keySelectSound;
    string flapKey;
    int gameSpeed;
    bool spawnBirds, showFps;

    enum MenuOption : byte { EasyGameSpeed = 0, MediumGameSpeed = 1, FastGameSpeed = 2, ToggleSound = 3, ToggleBirds = 4, ToggleFps = 5, SelectFlapKey = 6 } // Values are wired to the buttons in the scene

    public void OptionsSelection(int index)
    {
        switch ((MenuOption)index)
        {
            case MenuOption.EasyGameSpeed:
            case MenuOption.MediumGameSpeed:
            case MenuOption.FastGameSpeed:
                gameSpeed = index;
                UpdateGameSpeedTextColors();
                break;
            case MenuOption.ToggleSound:
                AudioListener.volume = AudioListener.volume == 1 ? 0 : 1;
                SetCheckmarkSprite(soundsCheckmark, AudioListener.volume == 1);
                break;
            case MenuOption.ToggleBirds:
                spawnBirds = !spawnBirds;
                SetCheckmarkSprite(birdsCheckmark, spawnBirds);
                break;
            case MenuOption.ToggleFps:
                showFps = !showFps;
                fpsText.gameObject.SetActive(showFps);
                if (showFps) InvokeRepeating(nameof(ShowFps), 0, FPSHudUpdateInterval);
                else CancelInvoke(nameof(ShowFps));
                SetCheckmarkSprite(fpsCheckmark, showFps);
                break;
            case MenuOption.SelectFlapKey:
                StartCoroutine(nameof(DetectFlapKey));
                break;
        }
    }

    void UpdateGameSpeedTextColors()
    {
        foreach (TextMeshProUGUI text in gameSpeedTexts) text.color = Color.white;
        gameSpeedTexts[gameSpeed].color = Color.yellow;
    }

    void SetCheckmarkSprite(Image image, bool isEnabled) => image.sprite = spriteAtlas.GetSprite(isEnabled ? "Checkmark_Enabled" : "Checkmark_Disabled");

    void ShowFps() => fpsText.text = FpsLabel();

    // Waits for a single key press and stores it as the flap key
    IEnumerator DetectFlapKey()
    {
        flapKeyText.text = "...";
        while (true)
        {
            while (!Input.anyKeyDown) yield return null;
            foreach (KeyCode key in AllowedFlapKeys)
            {
                if (!Input.GetKeyDown(key)) continue;

                audioSource.PlayOneShot(keySelectSound);
                flapKey = key.ToString();
                flapKeyText.text = FormatKeyName(flapKey);
                yield break;
            }
            yield return null;
        }
    }

    // Every key except mouse buttons and keys that are reserved or unreliable
    static readonly KeyCode[] AllowedFlapKeys = System.Enum.GetValues(typeof(KeyCode))
        .Cast<KeyCode>()
        .Where(key => (key < KeyCode.Mouse0 || key > KeyCode.Mouse6) &&
                      key != KeyCode.Escape && key != KeyCode.Pause && key != KeyCode.Print &&
                      key != KeyCode.SysReq && key != KeyCode.Break &&
                      key != KeyCode.Numlock && key != KeyCode.CapsLock && key != KeyCode.ScrollLock)
        .ToArray();
}
