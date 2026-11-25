using UnityEngine;
using static Utils;
using static Constants;

/// <summary>Obstacle that scrolls left while bouncing vertically between fixed bounds.</summary>
public sealed class Obstacle : Movable
{
    [SerializeField] SpriteRenderer spriteRenderer1, spriteRenderer2;
    static float moveSpeed;
    static float basePingPongSpeed;
    float pingPongSpeed;
    bool isMoving;

    public static void SetSpeed(int gameSpeed)
    {
        moveSpeed = ObstacleSpeed[gameSpeed];
        basePingPongSpeed = ObstaclePingPongSpeed[gameSpeed];
    }

    // Applies the obstacle style selected in the shop
    void Awake()
    {
        var sprite = GameManager.Instance.SpriteAtlas.GetSprite("Obstacle" + PlayerPrefs.GetInt(PrefKeys.ObstacleSelected, 0));
        spriteRenderer1.sprite = sprite;
        spriteRenderer2.sprite = sprite;
    }

    void OnEnable()
    {
        pingPongSpeed = PercentChance(50) ? basePingPongSpeed : -basePingPongSpeed;
        TeleportToStartingPosition();
        Move();
    }

    // Reverse vertical direction at the bounds
    void FixedUpdate()
    {
        if (!isMoving) return;

        bool hitCeiling = transform.position.y >= ObstacleSpawnY && pingPongSpeed > 0;
        bool hitFloor = transform.position.y <= -ObstacleSpawnY && pingPongSpeed < 0;
        if (hitCeiling || hitFloor) pingPongSpeed = -pingPongSpeed;
        rb.linearVelocity = new Vector2(moveSpeed, pingPongSpeed);
    }

    public override void TeleportToStartingPosition() => transform.position = new Vector2(ObstacleSpawnX, Random.Range(-ObstacleSpawnY, ObstacleSpawnY));

    public override void Move()
    {
        isMoving = true;
        rb.linearVelocity = new Vector2(moveSpeed, pingPongSpeed);
    }

    public override void Stop()
    {
        isMoving = false;
        rb.linearVelocity = Vector2.zero;
    }
}
