using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class VendingStockingServiceTests
{
    [Test]
    public void PlaceOneItem_AddsOnlyOneItemToEmptyShelf()
    {
        var shelf = new VendingItemDefinition[VendingScoringService.SlotCount];
        shelf[0] = CreateItem("water");
        shelf[1] = CreateItem("soda");

        var item = CreateItem("coffee");
        VendingStockingService.PlaceOneItem(shelf, item, new System.Random(1));

        int occupied = 0;
        int coffeeCount = 0;
        foreach (var slot in shelf)
        {
            if (slot == null)
            {
                continue;
            }

            occupied++;
            if (slot.id == "coffee")
            {
                coffeeCount++;
            }
        }

        Assert.AreEqual(3, occupied);
        Assert.AreEqual(1, coffeeCount);
    }

    [Test]
    public void PlaceOneItem_ReplacesOneItemWhenShelfIsFull()
    {
        var shelf = new VendingItemDefinition[VendingScoringService.SlotCount];
        for (int i = 0; i < shelf.Length; i++)
        {
            shelf[i] = CreateItem("water");
        }

        var item = CreateItem("coffee");
        var result = VendingStockingService.PlaceOneItem(shelf, item, new System.Random(1));

        int occupied = 0;
        int coffeeCount = 0;
        foreach (var slot in shelf)
        {
            if (slot == null)
            {
                continue;
            }

            occupied++;
            if (slot.id == "coffee")
            {
                coffeeCount++;
            }
        }

        Assert.IsTrue(result.ReplacedExistingItem);
        Assert.AreEqual(VendingScoringService.SlotCount, occupied);
        Assert.AreEqual(1, coffeeCount);
    }

    [Test]
    public void Score_AddsBonusForConnectedMatchingTags()
    {
        var shelf = new VendingItemDefinition[VendingScoringService.SlotCount];
        shelf[0] = CreateItem("water");
        shelf[1] = CreateItem("soda");

        var result = VendingScoringService.Score(shelf, "饮料");

        Assert.Greater(result.Total, shelf[0].baseValue + shelf[1].baseValue);
        Assert.IsTrue(result.HighlightSlots.Contains(0));
        Assert.IsTrue(result.HighlightSlots.Contains(1));
    }

    [Test]
    public void RandomizeOccupiedSymbols_PreservesOwnedSymbolsAndEmptySlots()
    {
        var shelf = new VendingItemDefinition[VendingScoringService.SlotCount];
        shelf[0] = CreateItem("water");
        shelf[1] = CreateItem("soda");
        shelf[4] = CreateItem("coffee");

        VendingSpinService.RandomizeOccupiedSymbols(shelf, new System.Random(4));

        Assert.AreEqual(3, CountOccupied(shelf));
        CollectionAssert.AreEquivalent(new[] { "coffee", "soda", "water" }, shelf.Where(item => item != null).Select(item => item.id).ToArray());
    }

    [Test]
    public void RandomizeOccupiedSymbols_ChangesPositionsForSeededSpin()
    {
        var shelf = new VendingItemDefinition[VendingScoringService.SlotCount];
        shelf[0] = CreateItem("water");
        shelf[1] = CreateItem("soda");
        shelf[4] = CreateItem("coffee");

        var before = shelf.Select(item => item?.id ?? string.Empty).ToArray();
        VendingSpinService.RandomizeOccupiedSymbols(shelf, new System.Random(4));
        var after = shelf.Select(item => item?.id ?? string.Empty).ToArray();

        CollectionAssert.AreNotEqual(before, after);
    }

    [Test]
    public void BuildWeightedPool_CanLimitOffersToCommonRarity()
    {
        var catalogObject = new GameObject("Catalog");
        try
        {
            var catalog = catalogObject.AddComponent<VendingCatalog>();
            var commonPool = catalog.BuildWeightedPool("普通");

            Assert.IsNotEmpty(commonPool);
            Assert.IsTrue(commonPool.All(item => item.rarity == "普通"));
            Assert.GreaterOrEqual(commonPool.Select(item => item.id).Distinct().Count(), 3);
        }
        finally
        {
            Object.DestroyImmediate(catalogObject);
        }
    }

    [Test]
    public void Score_ReportsSlotEarningsForOccupiedSlots()
    {
        var shelf = new VendingItemDefinition[VendingScoringService.SlotCount];
        shelf[0] = CreateItem("water");
        shelf[1] = CreateItem("soda");

        var result = VendingScoringService.Score(shelf, "饮料");

        Assert.IsTrue(result.SlotEarnings.ContainsKey(0));
        Assert.IsTrue(result.SlotEarnings.ContainsKey(1));
        Assert.Greater(result.SlotEarnings[0], 0);
        Assert.Greater(result.SlotEarnings[1], 0);
        Assert.AreEqual(result.Total, result.SlotEarnings.Values.Sum());
    }

    private static VendingItemDefinition CreateItem(string id)
    {
        if (id == "coffee")
        {
            return new VendingItemDefinition("coffee", "咖啡", "咖", "优质", 14, new[] { "饮料", "早餐" }, new Color32(132, 82, 46, 255));
        }

        if (id == "soda")
        {
            return new VendingItemDefinition("soda", "汽水", "汽", "普通", 9, new[] { "饮料", "零食" }, new Color32(229, 62, 72, 255));
        }

        return new VendingItemDefinition("water", "矿泉水", "水", "普通", 8, new[] { "饮料", "健康" }, new Color32(78, 171, 235, 255));
    }

    private static int CountOccupied(VendingItemDefinition[] shelf)
    {
        int occupied = 0;
        foreach (var item in shelf)
        {
            if (item != null)
            {
                occupied++;
            }
        }

        return occupied;
    }
}
