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
        var occupied = shelf.Where(item => item != null).ToList();

        foreach (var item in occupied)
        {
            total += item.baseValue;
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
                    log.Add("第" + (row + 1) + "排" + tag + "：+" + bonus);
                    for (int col = 0; col < GridSize; col++)
                    {
                        int slot = row * GridSize + col;
                        if (shelf[slot] != null && shelf[slot].HasTag(tag))
                        {
                            highlightSlots.Add(slot);
                        }
                    }
                }
            }
        }

        int customerCount = occupied.Count(item => item.HasTag(preferredTag));
        if (customerCount > 0)
        {
            int customerBonus = customerCount * 2;
            total += customerBonus;
            log.Add("顾客偏好" + preferredTag + " x" + customerCount + "：+" + customerBonus);
        }

        int promoBonus = ScorePromoAdjacency(shelf, highlightSlots);
        if (promoBonus > 0)
        {
            total += promoBonus;
            log.Add("促销相邻：+" + promoBonus);
        }

        log.Add("本轮合计：" + total);
        return new VendingScoreResult(total, log, highlightSlots);
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

    private static int ScorePromoAdjacency(VendingItemDefinition[] shelf, HashSet<int> highlightSlots)
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
                    highlightSlots.Add(i);
                    highlightSlots.Add(neighbor);
                }
            }
        }

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
}
