using UnityEngine;
using static Constants;

/// <summary>Base class for pooled objects that scroll across the screen (coins, obstacles, birds).</summary>
public abstract class Movable : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D rb;

    /// <summary>Called by GameManager to take this object out of the pool.</summary>
    public void Activate() => gameObject.SetActive(true);

    protected virtual void Update() => DeactivateOnLeavingScreen();

    protected virtual void DeactivateOnLeavingScreen()
    {
        if (transform.position.x <= OffscreenLeftX) gameObject.SetActive(false);
    }

    public abstract void TeleportToStartingPosition();

    public abstract void Move();

    /// <summary>Called when the player dies.</summary>
    public abstract void Stop();
}
