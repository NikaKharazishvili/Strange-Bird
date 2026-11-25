using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Forwards pointer events to the scene's UIEffectsManager (sounds, cursor, tooltip).
/// Add to every interactive button; leave tooltipText empty for no tooltip.
/// </summary>
[RequireComponent(typeof(Button))]
public sealed class ButtonInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] string tooltipText;

    static UIEffectsManager Effects => UIEffectsManager.Instance;

    public void OnPointerEnter(PointerEventData _) { if (Effects) Effects.OnButtonEnter(tooltipText); }
    public void OnPointerClick(PointerEventData _) { if (Effects) Effects.OnButtonClick(); }
    public void OnPointerExit(PointerEventData _) { if (Effects) Effects.OnButtonExit(); }

    // A disabled button never receives the exit event, so reset the cursor here
    void OnDisable() { if (Effects) Effects.OnButtonExit(); }
}
