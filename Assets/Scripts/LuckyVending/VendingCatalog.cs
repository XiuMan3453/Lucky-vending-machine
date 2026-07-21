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
        if (items.Count > 0)
        {
            return;
        }

        items = new List<VendingItemDefinition>
        {
            new VendingItemDefinition("water", "矿泉水", "水", "普通", 8, new[] { "饮料", "健康" }, new Color32(78, 171, 235, 255)),
            new VendingItemDefinition("soda", "汽水", "汽", "普通", 9, new[] { "饮料", "零食" }, new Color32(229, 62, 72, 255)),
            new VendingItemDefinition("chips", "薯片", "薯", "普通", 7, new[] { "零食" }, new Color32(245, 190, 58, 255)),
            new VendingItemDefinition("gum", "口香糖", "糖", "普通", 5, new[] { "零食", "甜食" }, new Color32(117, 219, 118, 255)),
            new VendingItemDefinition("coffee", "咖啡", "咖", "优质", 14, new[] { "饮料", "早餐", "咖啡因" }, new Color32(132, 82, 46, 255)),
            new VendingItemDefinition("donut", "甜甜圈", "甜", "优质", 12, new[] { "早餐", "零食", "甜食" }, new Color32(238, 106, 174, 255)),
            new VendingItemDefinition("sandwich", "三明治", "餐", "优质", 16, new[] { "早餐" }, new Color32(232, 168, 85, 255)),
            new VendingItemDefinition("energy_bar", "能量棒", "棒", "优质", 13, new[] { "健康", "零食" }, new Color32(103, 197, 122, 255)),
            new VendingItemDefinition("chocolate", "巧克力", "巧", "稀有", 22, new[] { "零食", "甜食" }, new Color32(95, 54, 40, 255)),
            new VendingItemDefinition("energy_drink", "能量饮料", "能", "稀有", 20, new[] { "饮料", "健康", "咖啡因" }, new Color32(52, 201, 181, 255)),
            new VendingItemDefinition("coupon", "优惠券", "券", "特殊", 3, new[] { "促销" }, new Color32(154, 93, 222, 255)),
            new VendingItemDefinition("double_tag", "双倍标签", "双", "特殊", 2, new[] { "促销" }, new Color32(241, 102, 128, 255))
        };
    }

    public VendingItemDefinition GetById(string id)
    {
        EnsureDefaults();
        return items.First(item => item.id == id);
    }

    public List<VendingItemDefinition> BuildWeightedPool()
    {
        EnsureDefaults();
        var weighted = new List<VendingItemDefinition>();
        foreach (var item in items)
        {
            int weight = item.rarity == "普通" ? 5 : item.rarity == "优质" ? 3 : item.rarity == "稀有" ? 2 : 1;
            for (int i = 0; i < weight; i++)
            {
                weighted.Add(item);
            }
        }

        return weighted;
    }
}
