using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static Constants;
using static Utils;

/// <summary>Runs a round: spawning, scoring, day/night cycle, death and restart.</summary>
[DefaultExecutionOrder(-100)] // Other scripts read Instance in Awake
public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] Player player;
    [SerializeField] SpriteAtlas spriteAtlas;

    // Canvas
    [SerializeField] Image loadingBar;
    [SerializeField] GameObject menu, loadingMenu;
    [SerializeField] TextMeshProUGUI loadingText, coinText, scoreText, deathText, fpsText;
    [SerializeField] Image background;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip uiSelectSound;
    enum MenuAction { Play = 0, Menu = 1, Quit = 2 } // Values are wired to the menu buttons in the scene

    // Pooled objects
    [SerializeField] Movable[] birdsToPool, obstaclesToPool, coinsToPool;

    // Day/night cycle
    [SerializeField] Transform sunMoonIcon;
    [SerializeField] TextMeshProUGUI timerText;
    int minutes, hours = 12;

    // Round state
    int currentScore, coinsCollectedThisRound;

    // Saved data
    int gameSpeed, highScore, coin, totalDeaths, skill1GreedLevel;
    bool showFpsOption, spawnBirdsOption;

    Coroutine flapRespawnCoroutine;
    Coroutine birdLoop, obstacleCoinLoop, dayNightLoop, scoreLoop; // Started with Repeat, stopped when the player dies

    public Player Player => player;
    public SpriteAtlas SpriteAtlas => spriteAtlas;
    public int GameSpeed => gameSpeed;

    void Awake() => Instance = this;

    void Start()
    {
        LoadStats();
        player.OnCoinTake += TakeCoin;
        player.OnDeath += StopAllObjects;
        player.OnRespawn += Restart;
        StartGameplayLoops();
        if (showFpsOption) this.Repeat(0, FPSHudUpdateInterval, UpdateFpsHud);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        player.OnCoinTake -= TakeCoin;
        player.OnDeath -= StopAllObjects;
        player.OnRespawn -= Restart;
    }

    #region Gameplay loops
    void StartGameplayLoops()
    {
        StopGameplayLoops(); // Never run two copies of a loop
        if (spawnBirdsOption) birdLoop = this.Repeat(BirdSpawnDelay, BirdSpawnDelay, SpawnBird);
        float spawnDelay = ObstacleAndCoinSpawnDelay[gameSpeed];
        obstacleCoinLoop = this.Repeat(spawnDelay, spawnDelay, SpawnObstacleAndCoin);
        dayNightLoop = this.Repeat(DayNightCycleInterval, DayNightCycleInterval, DayNightCycle);
        scoreLoop = this.Repeat(ScoreGainInterval, ScoreGainInterval, GainScore);
    }

    void StopGameplayLoops()
    {
        this.StopRepeat(ref birdLoop);
        this.StopRepeat(ref obstacleCoinLoop);
        this.StopRepeat(ref dayNightLoop);
        this.StopRepeat(ref scoreLoop);
    }

    void UpdateFpsHud() => fpsText.text = FpsLabel();

    void SpawnBird() { if (PercentChance(BirdSpawnChance)) PoolComponent(birdsToPool)?.Activate(); }

    void SpawnObstacleAndCoin()
    {
        PoolComponent(obstaclesToPool)?.Activate();
        PoolComponent(coinsToPool)?.Activate();
    }

    // Advances the in-game clock and updates the background brightness and sun/moon icon
    void DayNightCycle()
    {
        minutes = (minutes + 1) % 60;
        if (minutes == 0) hours = (hours + 1) % 24;
        timerText.text = $"{hours:00}:{minutes:00}";

        float timeOfDay = (hours + minutes / 60f) / 24f; // 0 and 1 are midnight, 0.5 is noon
        float shifted = (timeOfDay + 0.5f) % 1f;         // Shifted so that noon is the brightest point
        background.color = shifted < 0.5f ? Color.Lerp(Color.white, Color.black, shifted * 2) : Color.Lerp(Color.black, Color.white, (shifted - 0.5f) * 2);

        sunMoonIcon.localRotation = Quaternion.Euler(0, 0, (timeOfDay * 360f + 180f) % 360f);
    }

    void GainScore()
    {
        currentScore += ScoreIncrement[gameSpeed];
        UpdateScore();
    }

    void UpdateScore()
    {
        if (currentScore > highScore) highScore = currentScore;
        scoreText.text = $"{currentScore} / {highScore}";
    }
    #endregion


    #region Player events
    void TakeCoin()
    {
        int coinsEarned = PercentChance(GreedDuplicationChance[skill1GreedLevel]) ? 2 : 1;
        coin += coinsEarned;
        coinsCollectedThisRound += coinsEarned;
        coinText.text = coin.ToString();
        currentScore += CoinScoreIncrement;
        UpdateScore();
    }

    void StopAllObjects()
    {
        StopPool(coinsToPool);
        StopPool(obstaclesToPool);
        if (spawnBirdsOption) StopPool(birdsToPool);
        StopGameplayLoops();

        totalDeaths++;
        SaveStats();
        deathText.text = $"Total Deaths: {totalDeaths}\nHighest Score: {highScore}\nCoins Collected This Round: {coinsCollectedThisRound}";
        coinsCollectedThisRound = 0;
        this.Wait(0.5f, () =>
        {
            menu.SetActive(true);
            flapRespawnCoroutine = StartCoroutine(FlapRespawnRoutine());
        });
    }

    static void StopPool(Movable[] pool) { foreach (var movable in pool) movable.Stop(); }

    // Pressing the flap key on the death menu starts a new round
    IEnumerator FlapRespawnRoutine()
    {
        yield return new WaitUntil(() => Input.GetKeyDown(player.FlapKey));
        MenuSelection((int)MenuAction.Play);
    }

    void Restart()
    {
        foreach (var movable in coinsToPool) movable.gameObject.SetActive(false);
        foreach (var movable in obstaclesToPool) movable.gameObject.SetActive(false);
        foreach (var movable in birdsToPool) movable.gameObject.SetActive(false);

        StartGameplayLoops();

        currentScore = 0;
        UpdateScore();
    }

    // Death menu buttons (Play, Menu, Quit)
    public void MenuSelection(int index)
    {
        audioSource.PlayOneShot(uiSelectSound);

        switch ((MenuAction)index)
        {
            case MenuAction.Play:
                if (flapRespawnCoroutine != null) StopCoroutine(flapRespawnCoroutine);
                player.Respawn();
                menu.SetActive(false);
                break;
            case MenuAction.Menu:
                SaveStats();
                foreach (Transform child in menu.transform) child.gameObject.SetActive(false);
                loadingMenu.SetActive(true);
                StartCoroutine(LoadSceneAsync("Menu", loadingBar, loadingText));
                break;
            case MenuAction.Quit:
                QuitApplication();
                break;
        }
    }
    #endregion


    #region Load & save
    void LoadStats()
    {
        highScore = PlayerPrefs.GetInt(PrefKeys.HighestScore, 100);
        totalDeaths = PlayerPrefs.GetInt(PrefKeys.TotalDeaths);
        coin = PlayerPrefs.GetInt(PrefKeys.Coin);
        skill1GreedLevel = Mathf.Clamp(PlayerPrefs.GetInt(PrefKeys.Skill1GreedLevel), 0, MaxSkillLevel);
        background.sprite = spriteAtlas.GetSprite("Background" + PlayerPrefs.GetInt(PrefKeys.BackgroundSelected, 0));
        UpdateScore();
        coinText.text = coin.ToString();

        AudioListener.volume = PlayerPrefs.GetFloat(PrefKeys.GlobalVolume, 1);
        gameSpeed = Mathf.Clamp(PlayerPrefs.GetInt(PrefKeys.GameSpeed), 0, MaxGameSpeed);
        showFpsOption = GetPrefBool(PrefKeys.ShowFps);
        fpsText.gameObject.SetActive(showFpsOption);
        spawnBirdsOption = GetPrefBool(PrefKeys.SpawnBirds, true); // Same default as the menu toggle

        Obstacle.SetSpeed(gameSpeed);
        Coin.SetSpeed(gameSpeed);
    }

    void OnApplicationQuit() => SaveStats();

    void SaveStats()
    {
        PlayerPrefs.SetInt(PrefKeys.Coin, coin);
        PlayerPrefs.SetInt(PrefKeys.HighestScore, highScore);
        PlayerPrefs.SetInt(PrefKeys.TotalDeaths, totalDeaths);
        PlayerPrefs.Save();
    }
    #endregion
}
