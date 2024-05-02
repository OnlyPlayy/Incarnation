using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonImageTinter : MonoBehaviour
{
    public Image imageToTint;
    public Color selectedColor = Color.white;
    private Color originalColor;

    void Start()
    {
        if (imageToTint != null)
        {
            originalColor = imageToTint.color;

            EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
            AddEventTrigger(trigger, EventTriggerType.Select, OnButtonSelect);
            AddEventTrigger(trigger, EventTriggerType.Deselect, OnButtonDeselect);
        }
        else
        {
            Debug.LogWarning("Image to tint not assigned: " + gameObject.name);
        }
    }

    private void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }

    private void OnButtonSelect(BaseEventData eventData)
    {
        if (imageToTint != null)
        {
            imageToTint.color = selectedColor;
        }
    }

    private void OnButtonDeselect(BaseEventData eventData)
    {
        if (imageToTint != null)
        {
            imageToTint.color = originalColor;
        }
    }
}
