using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LuckyVendingPrototype : MonoBehaviour
{
    private const int GridSize = 4;
    private const int SlotCount = GridSize * GridSize;
    private const int TotalRounds = 9;

    private readonly List<ItemDef> catalog = new List<ItemDef>();
    private readonly List<ItemDef> itemPool = new List<ItemDef>();
    private readonly ItemDef[] shelf = new ItemDef[SlotCount];
    private readonly List<Button> slotButtons = new List<Button>();
    private readonly List<Text> slotLabels = new List<Text>();
    private readonly List<Button> choiceButtons = new List<Button>();
    private readonly List<Text> choiceLabels = new List<Text>();
    private readonly System.Random rng = new System.Random();

    private Font uiFont;
    private RectTransform runtimeRoot;
    private Text moneyText;
    private Text targetText;
    private Text roundText;
    private Text poolText;
    private Text customerText;
    private Text statusText;
    private Text logText;
    private Text swapText;
    private Button swapButton;
    private Button restartButton;

    private int roundIndex;
    private int totalMoney;
    private int stageIndex;
    private int currentTarget;
    private int swapUses;
    private int currentRoundScore;
    private int selectedSwapIndex = -1;
    private bool swapMode;
    private bool runEnded;
    private string preferredTag = "Breakfast";
    private readonly int[] stageTargets = { 1200, 2600, 4300 };

    private void Start()
    {
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        EnsureEventSystem();
        BuildCatalog();
        BuildInterface();
        StartNewRun();
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystem.transform.SetParent(transform.parent, false);
    }

    private void BuildCatalog()
    {
        catalog.Clear();
        catalog.Add(new ItemDef("Water", "WA", "Common", 8, new[] { "Drink", "Healthy" }, new Color32(78, 171, 235, 255)));
        catalog.Add(new ItemDef("Soda", "SO", "Common", 9, new[] { "Drink", "Snack" }, new Color32(229, 62, 72, 255)));
        catalog.Add(new ItemDef("Chips", "CH", "Common", 7, new[] { "Snack" }, new Color32(245, 190, 58, 255)));
        catalog.Add(new ItemDef("Gum", "GU", "Common", 5, new[] { "Snack", "Sweet" }, new Color32(117, 219, 118, 255)));
        catalog.Add(new ItemDef("Coffee", "CF", "Premium", 14, new[] { "Drink", "Breakfast", "Caffeine" }, new Color32(132, 82, 46, 255)));
        catalog.Add(new ItemDef("Donut", "DO", "Premium", 12, new[] { "Breakfast", "Snack", "Sweet" }, new Color32(238, 106, 174, 255)));
        catalog.Add(new ItemDef("Sandwich", "SW", "Premium", 16, new[] { "Breakfast" }, new Color32(232, 168, 85, 255)));
        catalog.Add(new ItemDef("Energy Bar", "EB", "Premium", 13, new[] { "Healthy", "Snack" }, new Color32(103, 197, 122, 255)));
        catalog.Add(new ItemDef("Chocolate", "CO", "Rare", 22, new[] { "Snack", "Sweet" }, new Color32(95, 54, 40, 255)));
        catalog.Add(new ItemDef("Energy Drink", "ED", "Rare", 20, new[] { "Drink", "Healthy", "Caffeine" }, new Color32(52, 201, 181, 255)));
        catalog.Add(new ItemDef("Coupon", "CP", "Special", 3, new[] { "Promo" }, new Color32(154, 93, 222, 255)));
        catalog.Add(new ItemDef("Double Tag", "DT", "Special", 2, new[] { "Promo" }, new Color32(241, 102, 128, 255)));
    }

    private void BuildInterface()
    {
        if (runtimeRoot != null)
        {
            Destroy(runtimeRoot.gameObject);
        }

        var canvasObject = new GameObject("Lucky Vending Runtime UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(480, 640);
        scaler.matchWidthOrHeight = 0.5f;

        runtimeRoot = canvasObject.GetComponent<RectTransform>();
        Stretch(runtimeRoot);

        var background = CreateImage("Warm Shop Background", runtimeRoot, new Color32(250, 230, 202, 255));
        Stretch(background.rectTransform);

        var leftPanel = CreatePanel("Sales Panel", runtimeRoot, new Color32(255, 248, 232, 245));
        Anchor(leftPanel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-118f, -72f), new Vector2(224f, 132f));
        CreateText("Left Title", leftPanel, "TODAY'S SALES", 12, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(40, 91, 94, 255), new Vector2(0f, 48f), new Vector2(200f, 20f));
        moneyText = CreateText("Money Text", leftPanel, string.Empty, 21, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(226, 139, 23, 255), new Vector2(0f, 25f), new Vector2(200f, 28f));
        targetText = CreateText("Target Text", leftPanel, string.Empty, 10, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(74, 100, 104, 255), new Vector2(0f, 4f), new Vector2(200f, 18f));
        roundText = CreateText("Round Text", leftPanel, string.Empty, 10, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(50, 83, 88, 255), new Vector2(0f, -15f), new Vector2(200f, 18f));
        swapText = CreateText("Swap Text", leftPanel, string.Empty, 10, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(102, 72, 129, 255), new Vector2(0f, -34f), new Vector2(200f, 18f));
        swapButton = CreateButton("Swap Button", leftPanel, "SWAP 2 SLOTS", new Color32(111, 82, 177, 255), new Vector2(0f, -55f), new Vector2(160f, 26f), ToggleSwapMode);
        poolText = CreateText("Pool Text", leftPanel, string.Empty, 8, FontStyle.Normal, TextAnchor.UpperLeft, new Color32(75, 88, 92, 255), new Vector2(0f, -82f), new Vector2(190f, 28f));
        poolText.gameObject.SetActive(false);

        var machine = CreatePanel("Vending Machine", runtimeRoot, new Color32(30, 152, 153, 255));
        Anchor(machine, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 18f), new Vector2(400f, 350f));
        CreateText("Machine Title", machine, "FRESH PICKS", 22, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0f, 150f), new Vector2(330f, 36f));

        var gridPanel = CreatePanel("Shelf Grid", machine, new Color32(34, 78, 93, 255));
        Anchor(gridPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 12f), new Vector2(300f, 300f));
        var grid = gridPanel.gameObject.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = GridSize;
        grid.cellSize = new Vector2(62f, 62f);
        grid.spacing = new Vector2(6f, 6f);
        grid.padding = new RectOffset(13, 13, 13, 13);

        for (int i = 0; i < SlotCount; i++)
        {
            int slotIndex = i;
            var button = CreateButton("Shelf Slot " + i, gridPanel, "", new Color32(245, 245, 245, 255), Vector2.zero, Vector2.zero, () => SelectShelfSlot(slotIndex));
            button.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            button.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 0f);
            slotButtons.Add(button);
            slotLabels.Add(button.GetComponentInChildren<Text>());
        }

        statusText = CreateText("Status Text", machine, string.Empty, 11, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, new Vector2(0f, -154f), new Vector2(350f, 34f));

        var rightPanel = CreatePanel("Customer Panel", runtimeRoot, new Color32(255, 248, 232, 245));
        Anchor(rightPanel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(118f, -72f), new Vector2(224f, 132f));
        CreateText("Order Title", rightPanel, "CUSTOMER ORDER", 12, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(179, 82, 25, 255), new Vector2(0f, 48f), new Vector2(200f, 20f));
        customerText = CreateText("Customer Text", rightPanel, string.Empty, 12, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(73, 94, 99, 255), new Vector2(0f, 18f), new Vector2(200f, 52f));
        logText = CreateText("Log Text", rightPanel, string.Empty, 8, FontStyle.Normal, TextAnchor.UpperLeft, new Color32(68, 81, 86, 255), new Vector2(0f, -34f), new Vector2(190f, 48f));
        restartButton = CreateButton("Restart Button", rightPanel, "NEW RUN", new Color32(235, 143, 35, 255), new Vector2(0f, -55f), new Vector2(160f, 26f), StartNewRun);

        var choicesPanel = CreatePanel("Restock Panel", runtimeRoot, new Color32(255, 251, 241, 248));
        Anchor(choicesPanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 72f), new Vector2(460f, 124f));
        CreateText("Restock Title", choicesPanel, "PICK 1 RESTOCK ITEM", 11, FontStyle.Bold, TextAnchor.MiddleCenter, new Color32(48, 88, 91, 255), new Vector2(0f, 43f), new Vector2(420f, 22f));

        for (int i = 0; i < 3; i++)
        {
            int choiceIndex = i;
            var button = CreateButton("Choice " + i, choicesPanel, string.Empty, new Color32(248, 178, 66, 255), new Vector2(-150f + 150f * i, -17f), new Vector2(135f, 74f), () => ChooseOffer(choiceIndex));
            choiceButtons.Add(button);
            choiceLabels.Add(button.GetComponentInChildren<Text>());
        }
    }

    private void StartNewRun()
    {
        itemPool.Clear();
        itemPool.Add(GetItem("Water"));
        itemPool.Add(GetItem("Soda"));
        itemPool.Add(GetItem("Chips"));
        itemPool.Add(GetItem("Coffee"));

        Array.Clear(shelf, 0, shelf.Length);
        roundIndex = 0;
        totalMoney = 0;
        stageIndex = 0;
        currentTarget = stageTargets[stageIndex];
        swapUses = 2;
        currentRoundScore = 0;
        selectedSwapIndex = -1;
        swapMode = false;
        runEnded = false;
        PickCustomerTag();
        statusText.text = "Choose a restock item to start the first round.";
        logText.text = "Build around matching tags. Connected tags pay more.";
        SetChoicesInteractable(true);
        PrepareOffers();
        RenderShelf();
        UpdateSummaryTexts();
    }

    private ItemDef GetItem(string itemName)
    {
        return catalog.First(item => item.Name == itemName);
    }

    private void PrepareOffers()
    {
        if (runEnded)
        {
            SetChoicesInteractable(false);
            return;
        }

        var offers = new List<ItemDef>();
        var weightedCatalog = BuildWeightedCatalog();
        while (offers.Count < 3)
        {
            var candidate = weightedCatalog[rng.Next(weightedCatalog.Count)];
            if (!offers.Contains(candidate))
            {
                offers.Add(candidate);
            }
        }

        for (int i = 0; i < choiceButtons.Count; i++)
        {
            var item = offers[i];
            choiceButtons[i].gameObject.SetActive(true);
            choiceButtons[i].interactable = true;
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => ChooseItem(item));
            choiceLabels[i].text = item.Name + "\n" + item.Rarity + "  $" + item.BaseValue + "\n" + string.Join(" / ", item.Tags);
            choiceButtons[i].GetComponent<Image>().color = item.Color;
        }
    }

    private List<ItemDef> BuildWeightedCatalog()
    {
        var weighted = new List<ItemDef>();
        foreach (var item in catalog)
        {
            int weight = item.Rarity == "Common" ? 5 : item.Rarity == "Premium" ? 3 : item.Rarity == "Rare" ? 2 : 1;
            for (int i = 0; i < weight; i++)
            {
                weighted.Add(item);
            }
        }

        return weighted;
    }

    private void ChooseOffer(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= choiceButtons.Count)
        {
            return;
        }
    }

    private void ChooseItem(ItemDef item)
    {
        if (runEnded)
        {
            return;
        }

        itemPool.Add(item);
        roundIndex++;
        PickCustomerTag();
        FillShelfFromPool();
        var result = ScoreShelf();
        currentRoundScore = result.Total;
        totalMoney += currentRoundScore;
        statusText.text = "Round " + roundIndex + " earned $" + currentRoundScore + ". Use Swap if a tag is one slot away.";
        logText.text = string.Join("\n", result.LogLines.Take(9));
        RenderShelf(result.HighlightSlots);
        CheckStageProgress();
        UpdateSummaryTexts();
        PrepareOffers();
    }

    private void FillShelfFromPool()
    {
        for (int i = 0; i < shelf.Length; i++)
        {
            shelf[i] = itemPool[rng.Next(itemPool.Count)];
        }
    }

    private ScoreResult ScoreShelf()
    {
        int total = 0;
        var log = new List<string>();
        var highlightSlots = new HashSet<int>();

        foreach (var item in shelf)
        {
            total += item.BaseValue;
        }
        log.Add("Base shelf value: $" + total);

        var tags = shelf.SelectMany(item => item.Tags).Distinct().Where(tag => tag != "Promo").ToList();
        foreach (var tag in tags)
        {
            bool[] visited = new bool[SlotCount];
            for (int i = 0; i < SlotCount; i++)
            {
                if (visited[i] || !shelf[i].HasTag(tag))
                {
                    continue;
                }

                var group = CollectConnectedTagGroup(i, tag, visited);
                if (group.Count <= 1)
                {
                    continue;
                }

                int bonus = group.Count * (group.Count - 1) * 3;
                total += bonus;
                foreach (var slot in group)
                {
                    highlightSlots.Add(slot);
                }
                log.Add(tag + " connected x" + group.Count + ": +$" + bonus);
            }
        }

        for (int row = 0; row < GridSize; row++)
        {
            foreach (var tag in tags)
            {
                int count = 0;
                for (int col = 0; col < GridSize; col++)
                {
                    if (shelf[row * GridSize + col].HasTag(tag))
                    {
                        count++;
                    }
                }

                if (count >= 3)
                {
                    int bonus = count * 4;
                    total += bonus;
                    log.Add("Row " + (row + 1) + " " + tag + " focus: +$" + bonus);
                    for (int col = 0; col < GridSize; col++)
                    {
                        int slot = row * GridSize + col;
                        if (shelf[slot].HasTag(tag))
                        {
                            highlightSlots.Add(slot);
                        }
                    }
                }
            }
        }

        int customerCount = shelf.Count(item => item.HasTag(preferredTag));
        if (customerCount > 0)
        {
            int customerBonus = customerCount * 2;
            total += customerBonus;
            log.Add("Customer likes " + preferredTag + " x" + customerCount + ": +$" + customerBonus);
        }

        int promoBonus = ScorePromoAdjacency(highlightSlots);
        if (promoBonus > 0)
        {
            total += promoBonus;
            log.Add("Promo adjacency: +$" + promoBonus);
        }

        log.Add("Total this round: $" + total);
        return new ScoreResult(total, log, highlightSlots);
    }

    private List<int> CollectConnectedTagGroup(int start, string tag, bool[] visited)
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
                if (!visited[next] && shelf[next].HasTag(tag))
                {
                    visited[next] = true;
                    queue.Enqueue(next);
                }
            }
        }

        return group;
    }

    private IEnumerable<int> GetNeighbors(int index)
    {
        int row = index / GridSize;
        int col = index % GridSize;
        if (row > 0) yield return index - GridSize;
        if (row < GridSize - 1) yield return index + GridSize;
        if (col > 0) yield return index - 1;
        if (col < GridSize - 1) yield return index + 1;
    }

    private int ScorePromoAdjacency(HashSet<int> highlightSlots)
    {
        int bonus = 0;
        for (int i = 0; i < shelf.Length; i++)
        {
            if (!shelf[i].HasTag("Promo"))
            {
                continue;
            }

            foreach (int neighbor in GetNeighbors(i))
            {
                if (!shelf[neighbor].HasTag("Promo"))
                {
                    bonus += 5;
                    highlightSlots.Add(i);
                    highlightSlots.Add(neighbor);
                }
            }
        }

        return bonus;
    }

    private void CheckStageProgress()
    {
        bool checkStage = roundIndex == 3 || roundIndex == 6 || roundIndex == TotalRounds;
        if (!checkStage)
        {
            return;
        }

        if (totalMoney >= currentTarget)
        {
            stageIndex++;
            if (stageIndex >= stageTargets.Length || roundIndex >= TotalRounds)
            {
                EndRun("Run complete. Final sales: $" + totalMoney + ".");
                return;
            }

            currentTarget = stageTargets[stageIndex];
            statusText.text = "Stage cleared. New sales target: $" + currentTarget + ".";
            return;
        }

        EndRun("Target missed at round " + roundIndex + ". Final sales: $" + totalMoney + ".");
    }

    private void EndRun(string message)
    {
        runEnded = true;
        swapMode = false;
        statusText.text = message;
        SetChoicesInteractable(false);
        restartButton.gameObject.SetActive(true);
    }

    private void PickCustomerTag()
    {
        string[] tags = { "Breakfast", "Snack", "Drink", "Healthy" };
        preferredTag = tags[rng.Next(tags.Length)];
    }

    private void ToggleSwapMode()
    {
        if (runEnded || swapUses <= 0 || shelf[0] == null)
        {
            return;
        }

        swapMode = !swapMode;
        selectedSwapIndex = -1;
        statusText.text = swapMode ? "Swap mode: select two shelf slots." : "Swap cancelled.";
        UpdateSummaryTexts();
        RenderShelf();
    }

    private void SelectShelfSlot(int index)
    {
        if (!swapMode || runEnded || shelf[index] == null)
        {
            return;
        }

        if (selectedSwapIndex < 0)
        {
            selectedSwapIndex = index;
            RenderShelf(new HashSet<int> { index });
            statusText.text = "Selected " + shelf[index].Name + ". Pick a second slot.";
            return;
        }

        if (selectedSwapIndex == index)
        {
            selectedSwapIndex = -1;
            RenderShelf();
            statusText.text = "Selection cleared. Pick the first slot again.";
            return;
        }

        var temp = shelf[selectedSwapIndex];
        shelf[selectedSwapIndex] = shelf[index];
        shelf[index] = temp;
        swapUses--;
        swapMode = false;
        selectedSwapIndex = -1;

        var oldRoundScore = currentRoundScore;
        var result = ScoreShelf();
        currentRoundScore = result.Total;
        totalMoney += currentRoundScore - oldRoundScore;
        statusText.text = "Swap resolved. Round score changed by $" + (currentRoundScore - oldRoundScore) + ".";
        logText.text = string.Join("\n", result.LogLines.Take(9));
        RenderShelf(result.HighlightSlots);
        UpdateSummaryTexts();
    }

    private void RenderShelf()
    {
        RenderShelf(new HashSet<int>());
    }

    private void RenderShelf(HashSet<int> highlighted)
    {
        for (int i = 0; i < slotButtons.Count; i++)
        {
            var image = slotButtons[i].GetComponent<Image>();
            var item = shelf[i];
            if (item == null)
            {
                image.color = new Color32(218, 232, 231, 255);
                slotLabels[i].text = "EMPTY";
                slotLabels[i].color = new Color32(72, 94, 97, 255);
                continue;
            }

            image.color = highlighted != null && highlighted.Contains(i) ? Lighten(item.Color) : item.Color;
            slotLabels[i].text = item.Code + "\n$" + item.BaseValue + "\n" + item.Tags[0];
            slotLabels[i].color = Color.white;
        }
    }

    private Color32 Lighten(Color32 color)
    {
        return new Color32((byte)Mathf.Min(255, color.r + 50), (byte)Mathf.Min(255, color.g + 50), (byte)Mathf.Min(255, color.b + 50), 255);
    }

    private void UpdateSummaryTexts()
    {
        moneyText.text = "$" + totalMoney;
        targetText.text = "Stage " + (stageIndex + 1) + " target: $" + currentTarget;
        roundText.text = "Round " + Mathf.Min(roundIndex + 1, TotalRounds) + " / " + TotalRounds;
        customerText.text = "Customer wants\n" + preferredTag + " tags\n+ $2 each";
        swapText.text = "Swap tools: " + swapUses + (swapMode ? "  ACTIVE" : string.Empty);
        swapButton.interactable = !runEnded && swapUses > 0 && shelf[0] != null;
        restartButton.gameObject.SetActive(runEnded);

        var groupedPool = itemPool.GroupBy(item => item.Name).Select(group => group.Key + " x" + group.Count()).Take(11);
        poolText.text = "Current item pool\n" + string.Join("\n", groupedPool);
    }

    private void SetChoicesInteractable(bool interactable)
    {
        foreach (var button in choiceButtons)
        {
            button.interactable = interactable;
        }
    }

    private Image CreatePanel(string name, Component parent, Color color)
    {
        var panel = CreateImage(name, parent, color);
        panel.gameObject.AddComponent<Outline>().effectColor = new Color32(0, 0, 0, 35);
        return panel;
    }

    private Image CreateImage(string name, Component parent, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent.transform, false);
        var image = obj.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private Text CreateText(string name, Component parent, string value, int size, FontStyle style, TextAnchor anchor, Color color, Vector2 position, Vector2 dimensions)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent.transform, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;

        var text = obj.GetComponent<Text>();
        text.font = uiFont;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = anchor;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    private Button CreateButton(string name, Component parent, string label, Color color, Vector2 position, Vector2 dimensions, UnityEngine.Events.UnityAction action)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent.transform, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;

        var image = obj.GetComponent<Image>();
        image.color = color;

        var button = obj.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        int labelSize = dimensions.x <= 80f || dimensions.y <= 40f ? 9 : dimensions.x <= 150f ? 10 : 15;
        var text = CreateText("Label", obj.transform, label, labelSize, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, Vector2.zero, dimensions - new Vector2(10f, 8f));
        text.raycastTarget = false;
        return button;
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void Anchor(Component component, Vector2 min, Vector2 max, Vector2 position, Vector2 dimensions)
    {
        var rect = component.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
    }

    private sealed class ItemDef
    {
        public readonly string Name;
        public readonly string Code;
        public readonly string Rarity;
        public readonly int BaseValue;
        public readonly string[] Tags;
        public readonly Color32 Color;

        public ItemDef(string name, string code, string rarity, int baseValue, string[] tags, Color32 color)
        {
            Name = name;
            Code = code;
            Rarity = rarity;
            BaseValue = baseValue;
            Tags = tags;
            Color = color;
        }

        public bool HasTag(string tag)
        {
            return Tags.Contains(tag);
        }
    }

    private sealed class ScoreResult
    {
        public readonly int Total;
        public readonly List<string> LogLines;
        public readonly HashSet<int> HighlightSlots;

        public ScoreResult(int total, List<string> logLines, HashSet<int> highlightSlots)
        {
            Total = total;
            LogLines = logLines;
            HighlightSlots = highlightSlots;
        }
    }
}
