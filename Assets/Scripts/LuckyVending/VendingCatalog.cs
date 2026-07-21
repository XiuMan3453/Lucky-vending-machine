using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class VendingCatalog : MonoBehaviour
{
    [SerializeField] private List<VendingItemDefinition> items = new List<VendingItemDefinition>();

    public IReadOnlyList<VendingItemDefinition> Items => items;

    private void Awake()
    {
        EnsureDefaults();
    }

    public void EnsureDefaults()
    {
        var defaults = BuildDefaultItems();
        if (items == null)
        {
            items = new List<VendingItemDefinition>();
        }

        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i] == null)
            {
                items.RemoveAt(i);
            }
        }

        foreach (var defaultItem in defaults)
        {
            var existing = items.FirstOrDefault(item => item.id == defaultItem.id);
            if (existing == null)
            {
                items.Add(defaultItem);
                continue;
            }

            CopyDefinition(defaultItem, existing);
        }
    }

    private static List<VendingItemDefinition> BuildDefaultItems()
    {
        return new List<VendingItemDefinition>
        {
            new VendingItemDefinition("water", "矿泉水", "水", "普通", 8, new[] { "饮料", "健康" }, new Color32(78, 171, 235, 255)),
            new VendingItemDefinition("soda", "汽水", "汽", "普通", 9, new[] { "饮料", "零食" }, new Color32(229, 62, 72, 255)),
            new VendingItemDefinition("chips", "薯片", "薯", "普通", 7, new[] { "零食" }, new Color32(245, 190, 58, 255)),
            new VendingItemDefinition("gum", "口香糖", "糖", "普通", 5, new[] { "零食", "甜食" }, new Color32(117, 219, 118, 255)),
            new VendingItemDefinition("tea", "绿茶", "茶", "普通", 8, new[] { "饮料", "健康" }, new Color32(63, 168, 105, 255)),
            new VendingItemDefinition("cookie", "曲奇", "曲", "普通", 6, new[] { "零食", "甜食" }, new Color32(199, 143, 83, 255)),
            new VendingItemDefinition("yogurt", "酸奶", "酸", "普通", 9, new[] { "健康", "早餐" }, new Color32(122, 197, 222, 255)),
            new VendingItemDefinition("instant_noodle", "杯面", "面", "普通", 10, new[] { "热食", "夜宵" }, new Color32(219, 95, 58, 255)),
            new VendingItemDefinition("coffee", "咖啡", "咖", "优质", 14, new[] { "饮料", "早餐", "咖啡因" }, new Color32(132, 82, 46, 255)),
            new VendingItemDefinition("donut", "甜甜圈", "甜", "优质", 12, new[] { "早餐", "零食", "甜食" }, new Color32(238, 106, 174, 255)),
            new VendingItemDefinition("sandwich", "三明治", "餐", "优质", 16, new[] { "早餐" }, new Color32(232, 168, 85, 255)),
            new VendingItemDefinition("energy_bar", "能量棒", "棒", "优质", 13, new[] { "健康", "零食" }, new Color32(103, 197, 122, 255)),
            new VendingItemDefinition("salad", "沙拉杯", "沙", "优质", 15, new[] { "健康", "冷藏" }, new Color32(87, 185, 88, 255)),
            new VendingItemDefinition("bento", "便当", "当", "优质", 18, new[] { "热食", "午餐" }, new Color32(224, 137, 74, 255)),
            new VendingItemDefinition("milk", "牛奶", "奶", "优质", 13, new[] { "饮料", "早餐", "冷藏" }, new Color32(230, 238, 226, 255)),
            new VendingItemDefinition("jerky", "牛肉干", "肉", "优质", 17, new[] { "零食", "夜宵" }, new Color32(132, 71, 52, 255)),
            new VendingItemDefinition("chocolate", "巧克力", "巧", "稀有", 22, new[] { "零食", "甜食" }, new Color32(95, 54, 40, 255)),
            new VendingItemDefinition("energy_drink", "能量饮料", "能", "稀有", 20, new[] { "饮料", "健康", "咖啡因" }, new Color32(52, 201, 181, 255)),
            new VendingItemDefinition("premium_lunch", "高级便当", "高", "稀有", 26, new[] { "热食", "午餐" }, new Color32(186, 84, 54, 255)),
            new VendingItemDefinition("imported_juice", "进口果汁", "汁", "稀有", 21, new[] { "饮料", "冷藏" }, new Color32(240, 128, 62, 255)),
            new VendingItemDefinition("coupon", "优惠券", "券", "特殊", 3, new[] { "促销" }, new Color32(154, 93, 222, 255), "double_adjacent"),
            new VendingItemDefinition("row_ad", "整排广告", "广", "特殊", 2, new[] { "促销" }, new Color32(78, 104, 220, 255), "boost_row"),
            new VendingItemDefinition("double_tag", "双倍标签", "双", "特殊", 2, new[] { "促销" }, new Color32(241, 102, 128, 255), "boost_tag_group"),
            new VendingItemDefinition("mystery_box", "神秘补货箱", "箱", "特殊", 4, new[] { "促销", "惊喜" }, new Color32(129, 88, 45, 255), "boost_row")
        };
    }

    private static void CopyDefinition(VendingItemDefinition source, VendingItemDefinition target)
    {
        target.displayName = source.displayName;
        target.shortCode = source.shortCode;
        target.rarity = source.rarity;
        target.baseValue = source.baseValue;
        target.tags = source.tags;
        target.specialRule = source.specialRule;
        target.color = source.color;
    }

    public VendingItemDefinition GetById(string id)
    {
        EnsureDefaults();
        return items.First(item => item.id == id);
    }

    public List<VendingItemDefinition> BuildWeightedPool(string rarityFilter = null)
    {
        EnsureDefaults();
        var weighted = new List<VendingItemDefinition>();
        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(rarityFilter) && item.rarity != rarityFilter)
            {
                continue;
            }

            int weight = item.rarity == "普通" ? 5 : item.rarity == "优质" ? 3 : item.rarity == "稀有" ? 2 : 1;
            for (int i = 0; i < weight; i++)
            {
                weighted.Add(item);
            }
        }

        if (weighted.Count == 0)
        {
            throw new System.InvalidOperationException("没有可用的商品候选。");
        }

        return weighted;
    }
}
