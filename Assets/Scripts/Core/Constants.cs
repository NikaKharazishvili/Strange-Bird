/// <summary>Gameplay tuning values. Arrays are indexed by game speed (0 = Slow, 1 = Medium, 2 = Fast) or by skill level.</summary>
public static class Constants
{
    // ---------- Player & Skills ----------
    public const float PlayerJumpForce = 4.5f;
    public const float PlayerGravity = 12;
    public const float PlayerRespawnX = -1.5f;
    public const float WhooshLifespan = 0.3f;
    public const float WhooshSpeed = 3;

    public const int MaxSkillLevel = 3;
    public static readonly int[] GreedDuplicationChance = { 0, 30, 45, 60 }; // Percent chance to double a collected coin
    public static readonly float[] ShieldCooldown = { 12, 10, 8, 6 };
    public const float ShieldDuration = 2;

    // ---------- Shop ----------
    public const int BirdUnlockCost = 100;
    public const int BackgroundUnlockCost = 100;
    public const int ObstacleUnlockCost = 100;
    public const int SkillUpgradeCost = 150;

    public const int MaxBirdTypes = 8;
    public const int MaxBackgroundTypes = 8;
    public const int MaxObstacleTypes = 6;

    // ---------- Game speed ----------
    public const int MaxGameSpeed = 2;
    public static readonly float[] ObstacleAndCoinSpawnDelay = { 1.5f, 1.25f, 1 };
    public static readonly float[] BirdSpeed = { -5, -6, -7 };
    public static readonly float[] CoinSpeed = { -3, -4, -5 };
    public static readonly float[] ObstacleSpeed = { -3, -4, -5 };
    public static readonly float[] ObstaclePingPongSpeed = { 0.3f, 0.45f, 0.6f };
    public static readonly int[] ScoreIncrement = { 2, 3, 4 };

    // ---------- Birds, obstacles and coins ----------
    public const float BirdSpawnDelay = 5;
    public const float BirdChatDelay = 4;
    public const int BirdSpawnChance = 70;
    public const int BirdMoveRightChance = 65; // Chance for a bird to fly along with the player instead of against
    public const int BirdChatChance = 65;
    public const float BirdDriftSpeed = -1;    // Speed of birds flying along with the player (slower than the world scroll)

    public const float BirdSpawnX = 6;
    public const float BirdSpawnY = 1.5f;
    public const float ObstacleSpawnX = 6;
    public const float ObstacleSpawnY = 1;
    public const float CoinSpawnX = 8;
    public const float CoinSpawnY = 2;

    public const float OffscreenLeftX = -6;
    public const float OffscreenRightX = 7;

    // ---------- Score ----------
    public const float ScoreGainInterval = 1;
    public const int CoinScoreIncrement = 2;

    // ---------- Other ----------
    public const float DayNightCycleInterval = 0.2f;
    public const float FPSHudUpdateInterval = 1;
}
