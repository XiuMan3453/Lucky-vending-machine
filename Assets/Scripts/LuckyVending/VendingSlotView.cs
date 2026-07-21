using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class VendingSlotView : MonoBehaviour
{
    public int slotIndex;
    public Button button;
    public Image background;
    public Text label;

    private Action<int> onClicked;

    public void Bind(Action<int> clickHandler)
    {
        onClicked = clickHandler;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClicked?.Invoke(slotIndex));
    }

    public void Render(VendingItemDefinition item, HashSet<int> highlightedSlots)
    {
        bool highlighted = highlightedSlots != null && highlightedSlots.Contains(slotIndex);
        if (item == null)
        {
            background.color = new Color32(226, 238, 238, 255);
            label.text = "空";
            label.color = new Color32(68, 91, 96, 255);
            return;
        }

        background.color = highlighted ? Lighten(item.color) : item.color;
        label.color = Color.white;
        label.text = item.shortCode + "\n" + item.baseValue + "\n" + item.tags[0];
    }

    private static Color32 Lighten(Color32 color)
    {
        return new Color32((byte)Mathf.Min(255, color.r + 50), (byte)Mathf.Min(255, color.g + 50), (byte)Mathf.Min(255, color.b + 50), 255);
    }
}
