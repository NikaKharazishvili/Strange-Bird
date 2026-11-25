using UnityEngine;
using static Constants;

/// <summary>Short-lived wind effect spawned from a pool on every flap; drifts backwards for its lifespan.</summary>
public sealed class Whoosh : MonoBehaviour
{
    [SerializeField] Animator animator;

    public void Activate()
    {
        gameObject.SetActive(true);
        animator.Play(0); // The controller has a single animation
        this.Wait(WhooshLifespan, () => gameObject.SetActive(false));
    }

    void Update() => transform.position += Vector3.left * (WhooshSpeed * Time.deltaTime);
}
