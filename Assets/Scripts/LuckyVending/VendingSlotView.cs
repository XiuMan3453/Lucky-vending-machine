using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class VendingSlotView : MonoBehaviour
{
    public int slotIndex;
    public Button button;
    public Image background;
    public Text label;
    public Text earningText;

    private Action<int> onClicked;
    private VendingItemDefinition currentItem;
    private bool currentHighlighted;
    private Vector3 baseScale = Vector3.one;

    public void Bind(Action<int> clickHandler)
    {
        onClicked = clickHandler;
        DisableButtonVisualFeedback();
        EnsureEarningText();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClicked?.Invoke(slotIndex));
    }

    public void Render(VendingItemDefinition item, HashSet<int> highlightedSlots)
    {
        currentItem = item;
        currentHighlighted = highlightedSlots != null && highlightedSlots.Contains(slotIndex);
        transform.localScale = baseScale;
        EnsureEarningText();
        earningText.gameObject.SetActive(false);

        if (item == null)
        {
            background.color = new Color32(226, 238, 238, 255);
            label.text = "空";
            label.color = new Color32(68, 91, 96, 255);
            return;
        }

        background.color = currentHighlighted ? Lighten(item.color) : item.color;
        label.color = Color.white;
        label.text = item.shortCode + "\n" + item.baseValue + "\n" + item.tags[0];
    }

    public void SetComboEmphasis(bool emphasized)
    {
        if (currentItem == null)
        {
            return;
        }

        background.color = emphasized ? Lighten(currentItem.color, 90) : currentItem.color;
        transform.localScale = emphasized ? baseScale * 1.08f : baseScale;
    }

    public IEnumerator PlaySpinPulse(float delay)
    {
        yield return new WaitForSeconds(delay);
        float elapsed = 0f;
        const float duration = 0.18f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Sin((elapsed / duration) * Mathf.PI);
            transform.localScale = baseScale * (1f + 0.12f * t);
            yield return null;
        }

        transform.localScale = baseScale;
    }

    public IEnumerator PlayEarningPopup(int amount)
    {
        if (amount <= 0)
        {
            yield break;
        }

        EnsureEarningText();
        earningText.text = "+" + amount;
        earningText.color = new Color32(255, 236, 118, 255);
        earningText.rectTransform.anchoredPosition = new Vector2(0f, 22f);
        earningText.gameObject.SetActive(true);

        float elapsed = 0f;
        const float duration = 0.55f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float jump = Mathf.Sin(progress * Mathf.PI) * 34f;
            earningText.rectTransform.anchoredPosition = new Vector2(0f, 22f + jump);
            earningText.color = new Color(1f, 0.92f, 0.34f, 1f - progress * 0.35f);
            transform.localScale = baseScale * (1f + 0.08f * Mathf.Sin(progress * Mathf.PI));
            yield return null;
        }

        transform.localScale = baseScale;
        earningText.gameObject.SetActive(false);
    }

    private static Color32 Lighten(Color32 color)
    {
        return Lighten(color, 50);
    }

    private static Color32 Lighten(Color32 color, byte amount)
    {
        return new Color32((byte)Mathf.Min(255, color.r + amount), (byte)Mathf.Min(255, color.g + amount), (byte)Mathf.Min(255, color.b + amount), 255);
    }

    private void DisableButtonVisualFeedback()
    {
        if (button == null)
        {
            return;
        }

        button.transition = Selectable.Transition.None;
    }

    private void EnsureEarningText()
    {
        if (earningText == null)
        {
            return;
        }

        if (earningText.font == null)
        {
            earningText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        earningText.fontSize = 24;
        earningText.fontStyle = FontStyle.Bold;
        earningText.alignment = TextAnchor.MiddleCenter;
        earningText.raycastTarget = false;
        earningText.gameObject.SetActive(false);

        var rect = earningText.rectTransform;
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }
}
