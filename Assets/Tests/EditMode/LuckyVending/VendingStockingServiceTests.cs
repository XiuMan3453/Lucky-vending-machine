using System.Linq;
using System.Collections.Generic;
using System.Reflection;
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
            Assert.GreaterOrEqual(commonPool.Select(item => item.id).Distinct().Count(), 6);
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

    [Test]
    public void Catalog_DefaultsIncludeExpandedTagsAndSpecialRules()
    {
        var catalogObject = new GameObject("Catalog");
        try
        {
            var catalog = catalogObject.AddComponent<VendingCatalog>();
            catalog.EnsureDefaults();

            Assert.GreaterOrEqual(catalog.Items.Count, 20);
            Assert.IsTrue(catalog.Items.Any(item => item.HasTag("热食")));
            Assert.IsTrue(catalog.Items.Any(item => item.HasTag("冷藏")));
            Assert.IsTrue(catalog.Items.Any(item => item.HasTag("午餐")));
            Assert.IsTrue(catalog.Items.Any(item => item.specialRule == "double_adjacent"));
            Assert.IsTrue(catalog.Items.Any(item => item.specialRule == "boost_row"));
            Assert.IsTrue(catalog.Items.Any(item => item.specialRule == "boost_tag_group"));
        }
        finally
        {
            Object.DestroyImmediate(catalogObject);
        }
    }

    [Test]
    public void Catalog_EnsureDefaultsUpgradesSerializedDefaultsAndPreservesCustomItems()
    {
        var catalogObject = new GameObject("Catalog");
        try
        {
            var catalog = catalogObject.AddComponent<VendingCatalog>();
            var customItem = new VendingItemDefinition("custom", "自定义商品", "自", "普通", 11, new[] { "自定义" }, new Color32(10, 20, 30, 255));
            SetCatalogItems(catalog, new List<VendingItemDefinition>
            {
                new VendingItemDefinition("coupon", "旧优惠券", "旧", "特殊", 1, new[] { "促销" }, new Color32(1, 2, 3, 255)),
                customItem
            });

            catalog.EnsureDefaults();

            var coupon = catalog.GetById("coupon");
            Assert.AreEqual("优惠券", coupon.displayName);
            Assert.AreEqual("double_adjacent", coupon.specialRule);
            Assert.IsTrue(catalog.Items.Any(item => item.id == "tea"));
            Assert.IsTrue(catalog.Items.Contains(customItem));
        }
        finally
        {
            Object.DestroyImmediate(catalogObject);
        }
    }

    [Test]
    public void SpecialRule_DoubleAdjacentAddsNeighborCurrentEarnings()
    {
        var shelf = new VendingItemDefinition[VendingScoringService.SlotCount];
        shelf[0] = CreateItem("coupon");
        shelf[1] = CreateItem("water");

        var withoutSpecial = new VendingItemDefinition[VendingScoringService.SlotCount];
        withoutSpecial[1] = CreateItem("water");

        var baseResult = VendingScoringService.Score(withoutSpecial, "早餐");
        var specialResult = VendingScoringService.Score(shelf, "早餐");

        Assert.Greater(specialResult.Total, baseResult.Total + shelf[0].baseValue);
        Assert.IsTrue(specialResult.HighlightSlots.Contains(0));
        Assert.IsTrue(specialResult.HighlightSlots.Contains(1));
        Assert.GreaterOrEqual(specialResult.SlotEarnings[1], baseResult.SlotEarnings[1] * 2);
    }

    [Test]
    public void SpecialRule_BoostRowAddsBonusToRowItems()
    {
        var shelf = new VendingItemDefinition[VendingScoringService.SlotCount];
        shelf[4] = CreateItem("row_ad");
        shelf[5] = CreateItem("water");
        shelf[6] = CreateItem("soda");

        var result = VendingScoringService.Score(shelf, "早餐");

        Assert.IsTrue(result.HighlightSlots.Contains(4));
        Assert.IsTrue(result.HighlightSlots.Contains(5));
        Assert.IsTrue(result.HighlightSlots.Contains(6));
        Assert.GreaterOrEqual(result.SlotEarnings[5], shelf[5].baseValue + 6);
        Assert.GreaterOrEqual(result.SlotEarnings[6], shelf[6].baseValue + 6);
    }

    [Test]
    public void SpecialRule_BoostTagGroupAddsBonusToMatchingTagItems()
    {
        var shelf = new VendingItemDefinition[VendingScoringService.SlotCount];
        shelf[5] = CreateItem("double_tag");
        shelf[4] = CreateItem("water");
        shelf[6] = CreateItem("soda");
        shelf[9] = CreateItem("coffee");

        var result = VendingScoringService.Score(shelf, "早餐");

        Assert.IsTrue(result.HighlightSlots.Contains(5));
        Assert.GreaterOrEqual(result.SlotEarnings[4], shelf[4].baseValue + 4);
        Assert.GreaterOrEqual(result.SlotEarnings[6], shelf[6].baseValue + 4);
        Assert.GreaterOrEqual(result.SlotEarnings[9], shelf[9].baseValue + 4);
    }

    [Test]
    public void SlotComboEmphasis_ReturnsToOriginalItemColor()
    {
        var slotObject = new GameObject("Slot");
        var backgroundObject = new GameObject("Background");
        var labelObject = new GameObject("Label");
        var earningObject = new GameObject("Earning");
        try
        {
            backgroundObject.transform.SetParent(slotObject.transform);
            labelObject.transform.SetParent(slotObject.transform);
            earningObject.transform.SetParent(slotObject.transform);

            var slot = slotObject.AddComponent<VendingSlotView>();
            slot.background = backgroundObject.AddComponent<UnityEngine.UI.Image>();
            slot.label = labelObject.AddComponent<UnityEngine.UI.Text>();
            slot.earningText = earningObject.AddComponent<UnityEngine.UI.Text>();
            var item = CreateItem("water");

            slot.Render(item, new HashSet<int> { 0 });
            slot.SetComboEmphasis(true);
            slot.SetComboEmphasis(false);

            Assert.AreEqual(item.color, slot.background.color);
        }
        finally
        {
            Object.DestroyImmediate(slotObject);
        }
    }

    private static VendingItemDefinition CreateItem(string id)
    {
        if (id == "coffee")
        {
            return new VendingItemDefinition("coffee", "咖啡", "咖", "优质", 14, new[] { "饮料", "早餐" }, new Color32(132, 82, 46, 255));
        }

        if (id == "coupon")
        {
            return new VendingItemDefinition("coupon", "优惠券", "券", "特殊", 3, new[] { "促销" }, new Color32(154, 93, 222, 255), "double_adjacent");
        }

        if (id == "row_ad")
        {
            return new VendingItemDefinition("row_ad", "整排广告", "广", "特殊", 2, new[] { "促销" }, new Color32(78, 104, 220, 255), "boost_row");
        }

        if (id == "double_tag")
        {
            return new VendingItemDefinition("double_tag", "双倍标签", "双", "特殊", 2, new[] { "促销" }, new Color32(241, 102, 128, 255), "boost_tag_group");
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

    private static void SetCatalogItems(VendingCatalog catalog, List<VendingItemDefinition> seededItems)
    {
        var field = typeof(VendingCatalog).GetField("items", BindingFlags.Instance | BindingFlags.NonPublic);
        field.SetValue(catalog, seededItems);
    }
}
