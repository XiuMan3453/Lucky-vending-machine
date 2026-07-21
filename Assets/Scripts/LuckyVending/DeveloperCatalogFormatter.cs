using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class DeveloperCatalogFormatter
{
    private const string Uncategorized = "未分类";

    public static string BuildGroupedCatalogText(IEnumerable<VendingItemDefinition> items)
    {
        if (items == null)
        {
            return "暂无商品。";
        }

        var itemList = items.Where(item => item != null).ToList();
        if (itemList.Count == 0)
        {
            return "暂无商品。";
        }

        var builder = new StringBuilder();
        var groups = itemList
            .GroupBy(GetPrimaryCategory)
            .OrderBy(group => group.Key);

        foreach (var group in groups)
        {
            builder.AppendLine("【" + group.Key + "】");
            foreach (var item in group.OrderBy(item => item.rarity).ThenBy(item => item.displayName))
            {
                builder.AppendLine(FormatItemLine(item));
            }

            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static string GetPrimaryCategory(VendingItemDefinition item)
    {
        if (item.tags == null || item.tags.Length == 0 || string.IsNullOrEmpty(item.tags[0]))
        {
            return Uncategorized;
        }

        return item.tags[0];
    }

    private static string FormatItemLine(VendingItemDefinition item)
    {
        string tags = item.tags == null || item.tags.Length == 0 ? Uncategorized : string.Join(" / ", item.tags);
        string specialRule = GetSpecialRuleText(item.specialRule);
        string line = "- " + item.displayName + "（" + item.rarity + "，价值 " + item.baseValue + "，标签：" + tags + "）";
        return string.IsNullOrEmpty(specialRule) ? line : line + " 规则：" + specialRule;
    }

    private static string GetSpecialRuleText(string specialRule)
    {
        if (string.IsNullOrEmpty(specialRule))
        {
            return string.Empty;
        }

        if (specialRule == "double_adjacent")
        {
            return "相邻商品收益翻倍";
        }

        if (specialRule == "boost_row")
        {
            return "强化所在整排";
        }

        if (specialRule == "boost_tag_group")
        {
            return "强化相邻标签组";
        }

        return "特殊规则";
    }
}
