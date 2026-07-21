using System.Collections.Generic;
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
}
