using System;
using System.Collections.Generic;
using System.Linq;

public static class VendingStockingService
{
    public static VendingPlacementResult PlaceOneItem(VendingItemDefinition[] shelf, VendingItemDefinition item, Random rng)
    {
        var emptySlots = Enumerable.Range(0, shelf.Length).Where(index => shelf[index] == null).ToList();
        if (emptySlots.Count > 0)
        {
            int slotIndex = emptySlots[rng.Next(emptySlots.Count)];
            shelf[slotIndex] = item;
            return new VendingPlacementResult(slotIndex, false);
        }

        int replaceIndex = rng.Next(shelf.Length);
        shelf[replaceIndex] = item;
        return new VendingPlacementResult(replaceIndex, true);
    }
}
