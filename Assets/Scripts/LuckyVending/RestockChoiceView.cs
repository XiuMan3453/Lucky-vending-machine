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
        label.text = item.displayName + "\n" + item.rarity + "  " + item.baseValue + "\n" + string.Join(" / ", item.tags) + GetSpecialRuleText(item);
        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onSelected?.Invoke(currentItem));
    }

    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
    }

    private static string GetSpecialRuleText(VendingItemDefinition item)
    {
        if (string.IsNullOrEmpty(item.specialRule))
        {
            return string.Empty;
        }

        if (item.specialRule == "double_adjacent")
        {
            return "\n相邻商品收益翻倍";
        }

        if (item.specialRule == "boost_row")
        {
            return "\n强化所在整排";
        }

        if (item.specialRule == "boost_tag_group")
        {
            return "\n强化相邻标签组";
        }

        return "\n特殊规则";
    }
}
