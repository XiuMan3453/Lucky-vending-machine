using NUnit.Framework;
using UnityEngine;

public sealed class DeveloperCatalogFormatterTests
{
    [Test]
    public void BuildGroupedCatalogText_GroupsItemsByFirstTag()
    {
        var items = new[]
        {
            new VendingItemDefinition("water", "矿泉水", "水", "普通", 8, new[] { "饮料", "健康" }, new Color32(78, 171, 235, 255)),
            new VendingItemDefinition("chips", "薯片", "薯", "普通", 7, new[] { "零食" }, new Color32(245, 190, 58, 255)),
            new VendingItemDefinition("coupon", "优惠券", "券", "特殊", 3, new[] { "促销" }, new Color32(154, 93, 222, 255), "double_adjacent")
        };

        string text = DeveloperCatalogFormatter.BuildGroupedCatalogText(items);

        StringAssert.Contains("【饮料】", text);
        StringAssert.Contains("【零食】", text);
        StringAssert.Contains("【促销】", text);
        StringAssert.Contains("- 矿泉水（普通，价值 8，标签：饮料 / 健康）", text);
        StringAssert.Contains("优惠券", text);
        StringAssert.Contains("规则：相邻商品收益翻倍", text);
    }
}
