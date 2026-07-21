public readonly struct VendingPlacementResult
{
    public readonly int SlotIndex;
    public readonly bool ReplacedExistingItem;

    public VendingPlacementResult(int slotIndex, bool replacedExistingItem)
    {
        SlotIndex = slotIndex;
        ReplacedExistingItem = replacedExistingItem;
    }
}
