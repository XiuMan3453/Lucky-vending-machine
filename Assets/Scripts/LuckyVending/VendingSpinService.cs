using System;
using System.Collections.Generic;
using System.Linq;

public static class VendingSpinService
{
    public static void RandomizeOccupiedSymbols(VendingItemDefinition[] shelf, Random rng)
    {
        var ownedItems = shelf.Where(item => item != null).ToList();
        if (ownedItems.Count <= 1)
        {
            return;
        }

        var targetSlots = Enumerable.Range(0, shelf.Length).ToList();
        Shuffle(targetSlots, rng);

        Array.Clear(shelf, 0, shelf.Length);
        for (int i = 0; i < ownedItems.Count; i++)
        {
            shelf[targetSlots[i]] = ownedItems[i];
        }
    }

    private static void Shuffle<T>(IList<T> values, Random rng)
    {
        for (int i = values.Count - 1; i > 0; i--)
        {
            int swapIndex = rng.Next(i + 1);
            var temp = values[i];
            values[i] = values[swapIndex];
            values[swapIndex] = temp;
        }
    }
}
