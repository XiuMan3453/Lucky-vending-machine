using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class RestockChoiceView : MonoBehaviour
{
    public Button button;
    public Image background;
    public Text label;

    private VendingItemDefinition currentItem;
    private Action<VendingItemDefinition> onSelected;

    public void SetChoice(VendingItemDefinition item, Action<VendingItemDefinition> selectedHandler)
    {
        currentItem = item;
        onSelected = selectedHandler;
        background.color = item.color;
        label.text = item.displayName + "\n" + item.rarity + "  " + item.baseValue + "\n" + string.Join(" / ", item.tags);
        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onSelected?.Invoke(currentItem));
    }

    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
    }
}
