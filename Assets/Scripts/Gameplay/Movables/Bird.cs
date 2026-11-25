using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Utils;
using static Constants;

/// <summary>Decorative background bird: random look, flight direction and occasional chat bubble.</summary>
public sealed class Bird : Movable
{
    [SerializeField] TextMeshPro chatText;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] birdChatSounds;
    [SerializeField] Animator animator;
    [SerializeField] RuntimeAnimatorController[] birdVariationAnimators;
    static float moveSpeed;

    static readonly string[] BirdChatMessages =
    {
        "What's quackin', good lookin'?", "Hey, wingman!", "Tweet dreams!", "Fly high, don't be shy!",
        "Keep calm and chirp on!", "You're eggcellent!", "Winging it today?", "What the flock!",
        "Peck on, peck off.", "Just winging by!", "Spread your wings and fly!", "You're tweet-tastic!",
        "Flying solo today?", "Keep your beak up!", "Chirp happens!", "A little birdie told me...",
        "Flap it out!", "Life's a chirp!", "Eggciting times ahead!", "What's up, beak face?",
        "You're so fly!", "Bird is the word!", "Let's get this bread!", "Shake your tail feathers!",
        "Wing it like a boss!", "Feathered and fabulous!", "Stay chirpy!", "You're the tweetest!",
        "Flap till you drop!", "Peckish, aren't we?", "Squawk to the walk!", "Catch you on the fly!",
        "Don't ruffle my feathers!", "High-flying fun!", "Quirky chirp!", "Feather in your cap!",
        "Keep flapping!", "Feathered friends forever!", "Tweet it out!", "Birds of paradise!",
        "Squawk and awe!", "Up, up, and away!", "Beak sneak!", "Chirp-tastic!",
        "Flap happy!", "Bird-brained fun!", "Fluff and stuff!"
    };

    // Depletes as birds chat and refills when empty, so no message repeats until all have been used
    static readonly List<string> ChatPool = new();

    static string TakeRandomChat()
    {
        if (ChatPool.Count == 0) ChatPool.AddRange(BirdChatMessages);
        string message = GetRandomElement(ChatPool);
        ChatPool.Remove(message);
        return message;
    }

    void Awake() => moveSpeed = BirdSpeed[GameManager.Instance.GameSpeed];

    void OnEnable()
    {
        TeleportToStartingPosition();
        animator.runtimeAnimatorController = GetRandomElement(birdVariationAnimators);
        Move();
    }

    protected override void DeactivateOnLeavingScreen()
    {
        float x = transform.position.x;
        if (x <= OffscreenLeftX || x >= OffscreenRightX) gameObject.SetActive(false);
    }

    public override void TeleportToStartingPosition() => transform.position = new Vector2(BirdSpawnX, Random.Range(-BirdSpawnY, BirdSpawnY));

    // Either drifts along with the player (and may chat) or flies against them at full speed
    public override void Move()
    {
        chatText.gameObject.SetActive(false);
        if (PercentChance(BirdMoveRightChance))
        {
            rb.linearVelocityX = BirdDriftSpeed;
            transform.rotation = Quaternion.Euler(0, 180, 0);

            if (PercentChance(BirdChatChance))
            {
                this.Wait(BirdChatDelay, () =>
                {
                    if (!GameManager.Instance.Player.IsAlive) return;
                    audioSource.PlayOneShot(GetRandomElement(birdChatSounds));
                    chatText.gameObject.SetActive(true);
                    chatText.text = TakeRandomChat();
                });
            }
        }
        else
        {
            rb.linearVelocityX = moveSpeed;
            transform.rotation = Quaternion.identity;
        }
    }

    // After the player dies, birds that were drifting along fly away with the rest of the world
    public override void Stop()
    {
        if (Mathf.Approximately(rb.linearVelocityX, BirdDriftSpeed)) rb.linearVelocity = new Vector2(-moveSpeed, 0);
    }
}
