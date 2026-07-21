using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class LuckyVendingGameController : MonoBehaviour
{
    private const int TotalRounds = 9;

    public VendingCatalog catalog;
    public VendingShelfView shelfView;
    public VendingHudView hudView;
    public RestockChoiceView[] restockChoices;

    private readonly VendingItemDefinition[] shelf = new VendingItemDefinition[VendingScoringService.SlotCount];
    private readonly System.Random rng = new System.Random();
    private readonly int[] stageTargets = { 260, 620, 1050 };

    private int roundIndex;
    private int totalMoney;
    private int stageIndex;
    private int currentTarget;
    private int swapUses;
    private int currentRoundScore;
    private int selectedSwapIndex = -1;
    private bool swapMode;
    private bool runEnded;
    private string preferredTag = "早餐";

    private void Awake()
    {
        catalog.EnsureDefaults();
        shelfView.Bind(SelectShelfSlot);
        hudView.Bind(ToggleSwapMode, StartNewRun);
    }

    private void Start()
    {
        StartNewRun();
    }

    public void StartNewRun()
    {
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

        AddStartingItem("water", 0);
        AddStartingItem("soda", 1);
        AddStartingItem("chips", 4);
        AddStartingItem("coffee", 5);

        PickCustomerTag();
        shelfView.Render(shelf, null);
        hudView.SetStatus(ChineseTextConfig.InitialStatus);
        hudView.SetLog(new[] { ChineseTextConfig.InitialLogMatchingTags, ChineseTextConfig.InitialLogNoRandomFill, ChineseTextConfig.InitialLogSpinAfterPick });
        RenderHud();
        PrepareOffers();
    }

    private void AddStartingItem(string id, int slot)
    {
        var item = catalog.GetById(id);
        shelf[slot] = item;
    }

    private void PrepareOffers()
    {
        if (runEnded)
        {
            SetOffersInteractable(false);
            return;
        }

        var offers = new List<VendingItemDefinition>();
        var weighted = catalog.BuildWeightedPool();
        while (offers.Count < restockChoices.Length)
        {
            var candidate = weighted[rng.Next(weighted.Count)];
            if (!offers.Contains(candidate))
            {
                offers.Add(candidate);
            }
        }

        for (int i = 0; i < restockChoices.Length; i++)
        {
            restockChoices[i].SetChoice(offers[i], ChooseRestockItem);
        }
    }

    private void ChooseRestockItem(VendingItemDefinition item)
    {
        if (runEnded)
        {
            return;
        }

        roundIndex++;
        PickCustomerTag();
        var placement = VendingStockingService.PlaceOneItem(shelf, item, rng);
        VendingSpinService.RandomizeOccupiedSymbols(shelf, rng);
        var result = VendingScoringService.Score(shelf, preferredTag);
        currentRoundScore = result.Total;
        totalMoney += currentRoundScore;
        shelfView.Render(shelf, result.HighlightSlots);
        hudView.SetStatus(placement.ReplacedExistingItem ? ChineseTextConfig.ShelfFullReplacement : ChineseTextConfig.RoundIncome(roundIndex, currentRoundScore));
        hudView.SetLog(result.LogLines);
        CheckStageProgress();
        RenderHud();
        PrepareOffers();
    }

    private void CheckStageProgress()
    {
        bool shouldCheck = roundIndex == 3 || roundIndex == 6 || roundIndex == TotalRounds;
        if (!shouldCheck)
        {
            return;
        }

        if (totalMoney >= currentTarget)
        {
            stageIndex++;
            if (stageIndex >= stageTargets.Length || roundIndex >= TotalRounds)
            {
                EndRun(ChineseTextConfig.RunCompleted(totalMoney));
                return;
            }

            currentTarget = stageTargets[stageIndex];
            hudView.SetStatus(ChineseTextConfig.StageTargetReached(currentTarget));
            return;
        }

        EndRun(ChineseTextConfig.StageFailed(totalMoney));
    }

    private void EndRun(string message)
    {
        runEnded = true;
        swapMode = false;
        SetOffersInteractable(false);
        hudView.SetStatus(message);
        RenderHud();
    }

    private void ToggleSwapMode()
    {
        if (runEnded || swapUses <= 0 || shelf.All(item => item == null))
        {
            return;
        }

        swapMode = !swapMode;
        selectedSwapIndex = -1;
        hudView.SetStatus(swapMode ? ChineseTextConfig.SwapSelecting : ChineseTextConfig.SwapCancelled);
        shelfView.Render(shelf, null);
        RenderHud();
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
            shelfView.Render(shelf, new HashSet<int> { index });
            hudView.SetStatus(ChineseTextConfig.SlotSelected(shelf[index].displayName));
            return;
        }

        if (selectedSwapIndex == index)
        {
            selectedSwapIndex = -1;
            shelfView.Render(shelf, null);
            hudView.SetStatus(ChineseTextConfig.SelectionCancelled);
            return;
        }

        var temp = shelf[selectedSwapIndex];
        shelf[selectedSwapIndex] = shelf[index];
        shelf[index] = temp;
        swapUses--;
        swapMode = false;
        selectedSwapIndex = -1;

        var oldRoundScore = currentRoundScore;
        var result = VendingScoringService.Score(shelf, preferredTag);
        currentRoundScore = result.Total;
        totalMoney += currentRoundScore - oldRoundScore;
        shelfView.Render(shelf, result.HighlightSlots);
        hudView.SetStatus(ChineseTextConfig.SwapDelta(currentRoundScore - oldRoundScore));
        hudView.SetLog(result.LogLines);
        RenderHud();
    }

    private void PickCustomerTag()
    {
        string[] tags = { "早餐", "零食", "饮料", "健康" };
        preferredTag = tags[rng.Next(tags.Length)];
    }

    private void RenderHud()
    {
        bool canSwap = !runEnded && swapUses > 0 && shelf.Any(item => item != null);
        hudView.RenderSummary(totalMoney, stageIndex, currentTarget, roundIndex, TotalRounds, preferredTag, swapUses, swapMode, canSwap, runEnded);
    }

    private void SetOffersInteractable(bool interactable)
    {
        foreach (var choice in restockChoices)
        {
            choice.SetInteractable(interactable);
        }
    }
}
