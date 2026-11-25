using UnityEngine;
using static Constants;

/// <summary>Collectible coin that scrolls left at a speed set by the game speed.</summary>
public sealed class Coin : Movable
{
    static float moveSpeed;

    public static void SetSpeed(int gameSpeed) => moveSpeed = CoinSpeed[gameSpeed];

    void OnEnable()
    {
        TeleportToStartingPosition();
        Move();
    }

    public override void TeleportToStartingPosition() => transform.position = new Vector2(CoinSpawnX, Random.Range(-CoinSpawnY, CoinSpawnY));

    public override void Move() => rb.linearVelocity = new Vector2(moveSpeed, 0);

    public override void Stop() => rb.linearVelocity = Vector2.zero;
}
