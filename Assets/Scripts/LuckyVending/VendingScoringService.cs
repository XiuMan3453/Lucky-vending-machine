using System.Collections.Generic;
using System.Linq;

public static class VendingScoringService
{
    public const int GridSize = 4;
    public const int SlotCount = GridSize * GridSize;

    public static VendingScoreResult Score(VendingItemDefinition[] shelf, string preferredTag)
    {
        int total = 0;
        var log = new List<string>();
        var highlightSlots = new HashSet<int>();
        var slotEarnings = new Dictionary<int, int>();
        var occupied = shelf.Where(item => item != null).ToList();

        for (int i = 0; i < shelf.Length; i++)
        {
            var item = shelf[i];
            if (item == null)
            {
                continue;
            }

            total += item.baseValue;
            AddSlotEarning(slotEarnings, i, item.baseValue);
        }
        log.Add("基础售价：+" + total);

        var tags = occupied.SelectMany(item => item.tags).Distinct().Where(tag => tag != "促销").ToList();
        foreach (var tag in tags)
        {
            bool[] visited = new bool[SlotCount];
            for (int i = 0; i < SlotCount; i++)
            {
                if (visited[i] || shelf[i] == null || !shelf[i].HasTag(tag))
                {
                    continue;
                }

                var group = CollectConnectedTagGroup(shelf, i, tag, visited);
                if (group.Count <= 1)
                {
                    continue;
                }

                int bonus = group.Count * (group.Count - 1) * 3;
                total += bonus;
                AddSharedBonus(slotEarnings, group, bonus);
                foreach (int slot in group)
                {
                    highlightSlots.Add(slot);
                }
                log.Add(tag + "相连 x" + group.Count + "：+" + bonus);
            }
        }

        for (int row = 0; row < GridSize; row++)
        {
            foreach (var tag in tags)
            {
                int count = 0;
                for (int col = 0; col < GridSize; col++)
                {
                    var item = shelf[row * GridSize + col];
                    if (item != null && item.HasTag(tag))
                    {
                        count++;
                    }
                }

                if (count >= 3)
                {
                    int bonus = count * 4;
                    total += bonus;
                    var rowSlots = new List<int>();
                    log.Add("第" + (row + 1) + "排" + tag + "：+" + bonus);
                    for (int col = 0; col < GridSize; col++)
                    {
                        int slot = row * GridSize + col;
                        if (shelf[slot] != null && shelf[slot].HasTag(tag))
                        {
                            highlightSlots.Add(slot);
                            rowSlots.Add(slot);
                        }
                    }
                    AddSharedBonus(slotEarnings, rowSlots, bonus);
                }
            }
        }

        var customerSlots = Enumerable.Range(0, shelf.Length).Where(index => shelf[index] != null && shelf[index].HasTag(preferredTag)).ToList();
        int customerCount = customerSlots.Count;
        if (customerCount > 0)
        {
            int customerBonus = customerCount * 2;
            total += customerBonus;
            foreach (int slot in customerSlots)
            {
                AddSlotEarning(slotEarnings, slot, 2);
            }
            log.Add("顾客偏好" + preferredTag + " x" + customerCount + "：+" + customerBonus);
        }

        int promoBonus = ScorePromoAdjacency(shelf, highlightSlots, slotEarnings);
        if (promoBonus > 0)
        {
            total += promoBonus;
            log.Add("促销相邻：+" + promoBonus);
        }

        int specialBonus = ScoreSpecialRules(shelf, highlightSlots, slotEarnings, log);
        total += specialBonus;

        log.Add("本轮合计：" + total);
        return new VendingScoreResult(total, log, highlightSlots, slotEarnings);
    }

    private static List<int> CollectConnectedTagGroup(VendingItemDefinition[] shelf, int start, string tag, bool[] visited)
    {
        var group = new List<int>();
        var queue = new Queue<int>();
        queue.Enqueue(start);
        visited[start] = true;

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            group.Add(current);
            foreach (int next in GetNeighbors(current))
            {
                if (!visited[next] && shelf[next] != null && shelf[next].HasTag(tag))
                {
                    visited[next] = true;
                    queue.Enqueue(next);
                }
            }
        }

        return group;
    }

    private static int ScorePromoAdjacency(VendingItemDefinition[] shelf, HashSet<int> highlightSlots, Dictionary<int, int> slotEarnings)
    {
        int bonus = 0;
        for (int i = 0; i < shelf.Length; i++)
        {
            if (shelf[i] == null || !shelf[i].HasTag("促销"))
            {
                continue;
            }

            foreach (int neighbor in GetNeighbors(i))
            {
                if (shelf[neighbor] != null && !shelf[neighbor].HasTag("促销"))
                {
                    bonus += 5;
                    AddSharedBonus(slotEarnings, new[] { i, neighbor }, 5);
                    highlightSlots.Add(i);
                    highlightSlots.Add(neighbor);
                }
            }
        }

        return bonus;
    }

    private static int ScoreSpecialRules(VendingItemDefinition[] shelf, HashSet<int> highlightSlots, Dictionary<int, int> slotEarnings, List<string> log)
    {
        int totalBonus = 0;
        for (int i = 0; i < shelf.Length; i++)
        {
            var item = shelf[i];
            if (item == null || string.IsNullOrEmpty(item.specialRule))
            {
                continue;
            }

            if (item.specialRule == "double_adjacent")
            {
                int bonus = ScoreDoubleAdjacent(shelf, i, highlightSlots, slotEarnings);
                totalBonus += bonus;
                if (bonus > 0)
                {
                    log.Add(item.displayName + "相邻翻倍：+" + bonus);
                }
                continue;
            }

            if (item.specialRule == "boost_row")
            {
                int bonus = ScoreBoostRow(shelf, i, highlightSlots, slotEarnings);
                totalBonus += bonus;
                if (bonus > 0)
                {
                    log.Add(item.displayName + "强化整排：+" + bonus);
                }
                continue;
            }

            if (item.specialRule == "boost_tag_group")
            {
                int bonus = ScoreBoostTagGroup(shelf, i, highlightSlots, slotEarnings);
                totalBonus += bonus;
                if (bonus > 0)
                {
                    log.Add(item.displayName + "强化标签组：+" + bonus);
                }
            }
        }

        return totalBonus;
    }

    private static int ScoreDoubleAdjacent(VendingItemDefinition[] shelf, int specialSlot, HashSet<int> highlightSlots, Dictionary<int, int> slotEarnings)
    {
        int bonus = 0;
        foreach (int neighbor in GetNeighbors(specialSlot))
        {
            var neighborItem = shelf[neighbor];
            if (neighborItem == null || neighborItem.HasTag("促销"))
            {
                continue;
            }

            int amount = GetCurrentSlotEarning(slotEarnings, neighbor);
            if (amount <= 0)
            {
                amount = neighborItem.baseValue;
            }

            bonus += amount;
            AddSlotEarning(slotEarnings, neighbor, amount);
            highlightSlots.Add(specialSlot);
            highlightSlots.Add(neighbor);
        }

        return bonus;
    }

    private static int ScoreBoostRow(VendingItemDefinition[] shelf, int specialSlot, HashSet<int> highlightSlots, Dictionary<int, int> slotEarnings)
    {
        int row = specialSlot / GridSize;
        int bonus = 0;
        for (int col = 0; col < GridSize; col++)
        {
            int slot = row * GridSize + col;
            var item = shelf[slot];
            if (item == null || slot == specialSlot)
            {
                continue;
            }

            int amount = item.HasTag("促销") ? 3 : 6;
            bonus += amount;
            AddSlotEarning(slotEarnings, slot, amount);
            highlightSlots.Add(specialSlot);
            highlightSlots.Add(slot);
        }

        return bonus;
    }

    private static int ScoreBoostTagGroup(VendingItemDefinition[] shelf, int specialSlot, HashSet<int> highlightSlots, Dictionary<int, int> slotEarnings)
    {
        var neighborTags = GetNeighbors(specialSlot)
            .Where(slot => shelf[slot] != null && !shelf[slot].HasTag("促销"))
            .SelectMany(slot => shelf[slot].tags)
            .Where(tag => tag != "促销")
            .GroupBy(tag => tag)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Select(group => group.Key)
            .ToList();

        if (neighborTags.Count == 0)
        {
            return 0;
        }

        string targetTag = neighborTags[0];
        var affectedSlots = Enumerable.Range(0, shelf.Length)
            .Where(slot => shelf[slot] != null && shelf[slot].HasTag(targetTag))
            .ToList();

        int bonus = affectedSlots.Count * 4;
        foreach (int slot in affectedSlots)
        {
            AddSlotEarning(slotEarnings, slot, 4);
            highlightSlots.Add(slot);
        }
        highlightSlots.Add(specialSlot);
        return bonus;
    }

    private static IEnumerable<int> GetNeighbors(int index)
    {
        int row = index / GridSize;
        int col = index % GridSize;
        if (row > 0) yield return index - GridSize;
        if (row < GridSize - 1) yield return index + GridSize;
        if (col > 0) yield return index - 1;
        if (col < GridSize - 1) yield return index + 1;
    }

    private static void AddSlotEarning(Dictionary<int, int> slotEarnings, int slot, int amount)
    {
        if (!slotEarnings.ContainsKey(slot))
        {
            slotEarnings[slot] = 0;
        }

        slotEarnings[slot] += amount;
    }

    private static int GetCurrentSlotEarning(Dictionary<int, int> slotEarnings, int slot)
    {
        return slotEarnings.ContainsKey(slot) ? slotEarnings[slot] : 0;
    }

    private static void AddSharedBonus(Dictionary<int, int> slotEarnings, IEnumerable<int> slots, int amount)
    {
        var slotList = slots.Distinct().ToList();
        if (slotList.Count == 0)
        {
            return;
        }

        int baseShare = amount / slotList.Count;
        int remainder = amount % slotList.Count;
        for (int i = 0; i < slotList.Count; i++)
        {
            AddSlotEarning(slotEarnings, slotList[i], baseShare + (i < remainder ? 1 : 0));
        }
    }
}
