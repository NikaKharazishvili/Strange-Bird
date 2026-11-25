using System;
using UnityEngine;
using UnityEngine.UI;
using static Utils;
using static Constants;

/// <summary>Handles player movement, the passive shield skill and collision detection.</summary>
public sealed class Player : MonoBehaviour
{
    public event Action OnCoinTake, OnDeath, OnRespawn;

    [SerializeField] Image skill2;
    [SerializeField] RuntimeAnimatorController[] animatorControllers;
    [SerializeField] Animator animator, skill1Animator, skill2Animator;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] sounds; // Indexed by the Sound enum

    [SerializeField] Whoosh[] whooshVFXPool;

    enum Sound { Flap, Coin, ShieldOn, ShieldOff, Death }

    public KeyCode FlapKey { get; private set; }
    public bool IsAlive { get; private set; } = true;

    float shieldTimer, shieldCooldown;
    bool isInvulnerable;

    // Applies the saved bird skin, shield cooldown and flap key
    void Awake()
    {
        int birdIndex = Mathf.Clamp(PlayerPrefs.GetInt(PrefKeys.BirdSelected, 0), 0, animatorControllers.Length - 1);
        animator.runtimeAnimatorController = animatorControllers[birdIndex];

        int shieldLevel = Mathf.Clamp(PlayerPrefs.GetInt(PrefKeys.Skill2ShieldLevel, 0), 0, MaxSkillLevel);
        shieldCooldown = ShieldCooldown[shieldLevel];

        string savedKey = PlayerPrefs.GetString(PrefKeys.FlapKey, PrefKeys.DefaultFlapKey);
        FlapKey = Enum.TryParse(savedKey, out KeyCode key) ? key : KeyCode.Space;
    }

    void Update()
    {
        if (!IsAlive) return;
        ApplyFlapAndGravity();
        UpdateShield();
    }

    void ApplyFlapAndGravity()
    {
        rb.linearVelocityY -= PlayerGravity * Time.deltaTime;

        if (!Input.GetKeyDown(FlapKey) && !Input.GetMouseButtonDown(0)) return;

        PlaySound(Sound.Flap);
        rb.linearVelocity = Vector2.zero; // Cancel the fall before applying the impulse so every flap is equally strong
        rb.AddForce(new Vector2(0, PlayerJumpForce), ForceMode2D.Impulse);

        var whoosh = PoolComponent(whooshVFXPool);
        if (whoosh == null) return;
        whoosh.transform.position = transform.position - Vector3.up * 0.15f;
        whoosh.Activate();
    }

    // The shield activates by itself whenever its cooldown is over
    void UpdateShield()
    {
        if (shieldTimer < shieldCooldown) { shieldTimer += Time.deltaTime; return; }

        PlaySound(Sound.ShieldOn);
        shieldTimer = 0;
        isInvulnerable = true;
        skill2.color = spriteRenderer.color = new Color(1, 1, 1, 0.5f);

        this.Wait(ShieldDuration, () =>
        {
            PlaySound(Sound.ShieldOff);
            isInvulnerable = false;
            skill2Animator.Play("AnimateSkill");
            skill2.color = spriteRenderer.color = Color.white;
        });
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAlive) return;
        if (other.CompareTag("Obstacle") && !isInvulnerable) this.Wait(0.03f, Die);
        else if (other.CompareTag("Coin")) TakeCoin(other.gameObject);
    }

    void Die()
    {
        if (!IsAlive) return; // An obstacle has several colliders, so this can be reached more than once
        PlaySound(Sound.Death);
        IsAlive = false;
        animator.enabled = false;
        rb.linearVelocity = Vector2.zero;
        foreach (var whoosh in whooshVFXPool) whoosh.gameObject.SetActive(false);
        OnDeath?.Invoke();
    }

    void TakeCoin(GameObject coin)
    {
        coin.SetActive(false);
        PlaySound(Sound.Coin);
        skill1Animator.Play("AnimateSkill");
        OnCoinTake?.Invoke();
    }

    public void Respawn()
    {
        shieldTimer = 0;
        IsAlive = true;
        animator.enabled = true;
        transform.position = new Vector3(PlayerRespawnX, 0);
        OnRespawn?.Invoke();
    }

    void PlaySound(Sound sound) => audioSource.PlayOneShot(sounds[(int)sound]);
}
