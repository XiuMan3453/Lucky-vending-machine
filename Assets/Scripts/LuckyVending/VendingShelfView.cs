using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class VendingShelfView : MonoBehaviour
{
    public VendingSlotView[] slots;

    public void Bind(System.Action<int> onSlotClicked)
    {
        foreach (var slot in slots)
        {
            slot.Bind(onSlotClicked);
        }
    }

    public void Render(VendingItemDefinition[] shelf, HashSet<int> highlightedSlots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Render(shelf[i], highlightedSlots);
        }
    }

    public IEnumerator PlaySpinAnimation(VendingItemDefinition[] finalShelf)
    {
        var ownedItems = finalShelf.Where(item => item != null).ToList();
        if (ownedItems.Count == 0)
        {
            yield break;
        }

        const int frames = 10;
        for (int frame = 0; frame < frames; frame++)
        {
            var preview = BuildRandomPreview(ownedItems, finalShelf.Length);
            Render(preview, null);
            for (int i = 0; i < slots.Length; i++)
            {
                StartCoroutine(slots[i].PlaySpinPulse(i * 0.01f));
            }
            yield return new WaitForSeconds(0.08f);
        }

        Render(finalShelf, null);
        yield return new WaitForSeconds(0.16f);
    }

    public IEnumerator PlayComboAnimation(HashSet<int> highlightedSlots)
    {
        if (highlightedSlots == null || highlightedSlots.Count == 0)
        {
            yield return new WaitForSeconds(0.18f);
            yield break;
        }

        for (int blink = 0; blink < 3; blink++)
        {
            SetComboSlots(highlightedSlots, true);
            yield return new WaitForSeconds(0.16f);
            SetComboSlots(highlightedSlots, false);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator PlayEarningAnimation(Dictionary<int, int> slotEarnings)
    {
        if (slotEarnings == null || slotEarnings.Count == 0)
        {
            yield return new WaitForSeconds(0.18f);
            yield break;
        }

        foreach (var group in slotEarnings.Where(pair => pair.Value > 0).GroupBy(pair => pair.Value).OrderBy(group => group.Key))
        {
            foreach (var pair in group)
            {
                StartCoroutine(slots[pair.Key].PlayEarningPopup(pair.Value));
            }

            yield return new WaitForSeconds(0.34f);
        }

        yield return new WaitForSeconds(0.28f);
    }

    private void SetComboSlots(HashSet<int> highlightedSlots, bool emphasized)
    {
        foreach (int slotIndex in highlightedSlots)
        {
            if (slotIndex >= 0 && slotIndex < slots.Length)
            {
                slots[slotIndex].SetComboEmphasis(emphasized);
            }
        }
    }

    private static VendingItemDefinition[] BuildRandomPreview(List<VendingItemDefinition> ownedItems, int slotCount)
    {
        var preview = new VendingItemDefinition[slotCount];
        var slots = Enumerable.Range(0, slotCount).OrderBy(_ => Random.value).Take(ownedItems.Count).ToList();
        for (int i = 0; i < ownedItems.Count; i++)
        {
            preview[slots[i]] = ownedItems[i];
        }

        return preview;
    }
}
