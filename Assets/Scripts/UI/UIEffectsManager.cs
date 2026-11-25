using TMPro;
using UnityEngine;

/// <summary>Scene-wide singleton that owns the shared button effects: hover/click sounds, the custom cursor and tooltips.</summary>
public sealed class UIEffectsManager : MonoBehaviour
{
    public static UIEffectsManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip hoverSound, clickSound;

    [Header("Cursor")]
    [SerializeField] RectTransform cursorParent;
    [SerializeField] GameObject cursorArrow, cursorHand;

    [Header("Tooltip")]
    [SerializeField] Camera cam;
    [SerializeField] Canvas canvas;
    [SerializeField] RectTransform tooltipBackground;
    [SerializeField] TextMeshProUGUI tooltipDisplay;

    void Awake()
    {
        Instance = this;
        Cursor.visible = false; // The game draws its own cursor
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update() => cursorParent.localPosition = ScreenToCanvas(Input.mousePosition);

    Vector2 ScreenToCanvas(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, screenPos, cam, out Vector2 local);
        return local;
    }

    public void OnButtonEnter(string tooltipText)
    {
        cursorArrow.SetActive(false);
        cursorHand.SetActive(true);
        audioSource.PlayOneShot(hoverSound);

        if (string.IsNullOrEmpty(tooltipText)) return;

        // Place the tooltip just above or below the cursor, anchored on the side facing the screen center
        Vector2 mouse = Input.mousePosition;
        Vector2 position = ScreenToCanvas(mouse);
        position.y += position.y > 0 ? -60 : 60;
        tooltipBackground.localPosition = position;
        tooltipBackground.pivot = new Vector2(mouse.x / Screen.width, mouse.y / Screen.height);

        tooltipDisplay.text = tooltipText;
        tooltipBackground.gameObject.SetActive(true);
    }

    public void OnButtonExit()
    {
        cursorArrow.SetActive(true);
        cursorHand.SetActive(false);
        tooltipBackground.gameObject.SetActive(false);
    }

    public void OnButtonClick() => audioSource.PlayOneShot(clickSound);
}
