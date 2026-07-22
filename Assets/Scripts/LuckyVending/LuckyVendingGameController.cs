using System;
using System.Collections;
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

    private const int InitialPickCount = 4;
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
    private bool sequenceRunning;
    private int initialPicksRemaining;
    private string preferredTag = "早餐";
    private GameObject offerPopup;

    private void Awake()
    {
        catalog.EnsureDefaults();
        offerPopup = ResolveOfferPopup();
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
        sequenceRunning = false;
        initialPicksRemaining = InitialPickCount;

        PickCustomerTag();
        shelfView.Render(shelf, null);
        hudView.SetStatus(ChineseTextConfig.InitialPickStatus(initialPicksRemaining));
        hudView.SetLog(new[] { ChineseTextConfig.InitialLogMatchingTags, ChineseTextConfig.InitialLogNoRandomFill, ChineseTextConfig.InitialLogSpinAfterPick });
        RenderHud();
        PrepareInitialOffers();
    }

    private void PrepareInitialOffers()
    {
        if (runEnded)
        {
            SetOfferPopupVisible(false);
            return;
        }

        hudView.SetStatus(ChineseTextConfig.InitialPickStatus(initialPicksRemaining));
        SetOffers(BuildOffers("普通"), ChooseInitialItem);
        SetOfferPopupVisible(true);
    }

    private void PrepareRestockOffers()
    {
        if (runEnded)
        {
            SetOfferPopupVisible(false);
            return;
        }

        hudView.SetStatus(ChineseTextConfig.ChooseRestock);
        SetOffers(BuildOffers(null), ChooseRestockItem);
        SetOfferPopupVisible(true);
    }

    private void ChooseInitialItem(VendingItemDefinition item)
    {
        if (runEnded || sequenceRunning || initialPicksRemaining <= 0)
        {
            return;
        }

        StartCoroutine(HandleInitialChoice(item));
    }

    private IEnumerator HandleInitialChoice(VendingItemDefinition item)
    {
        sequenceRunning = true;
        SetOfferPopupVisible(false);
        VendingStockingService.PlaceOneItem(shelf, item, rng);
        initialPicksRemaining--;
        shelfView.Render(shelf, null);
        RenderHud();

        if (initialPicksRemaining > 0)
        {
            hudView.SetStatus(ChineseTextConfig.InitialPickAdded(item.displayName, initialPicksRemaining));
            yield return new WaitForSeconds(0.25f);
            sequenceRunning = false;
            PrepareInitialOffers();
            yield break;
        }

        hudView.SetStatus(ChineseTextConfig.InitialPicksFinished());
        yield return new WaitForSeconds(0.35f);
        sequenceRunning = false;
        PrepareRestockOffers();
    }

    private void ChooseRestockItem(VendingItemDefinition item)
    {
        if (runEnded || sequenceRunning || initialPicksRemaining > 0)
        {
            return;
        }

        StartCoroutine(ResolveRound(item));
    }

    private IEnumerator ResolveRound(VendingItemDefinition item)
    {
        sequenceRunning = true;
        SetOfferPopupVisible(false);
        roundIndex++;
        PickCustomerTag();
        var placement = VendingStockingService.PlaceOneItem(shelf, item, rng);
        hudView.SetStatus(ChineseTextConfig.SpinStarting);
        shelfView.Render(shelf, null);
        yield return new WaitForSeconds(0.12f);

        VendingSpinService.RandomizeOccupiedSymbols(shelf, rng);
        yield return shelfView.PlaySpinAnimation(shelf);

        var result = VendingScoringService.Score(shelf, preferredTag);
        currentRoundScore = result.Total;
        shelfView.Render(shelf, result.HighlightSlots);
        hudView.SetStatus(ChineseTextConfig.ComboChecking);
        yield return shelfView.PlayComboAnimation(result.HighlightSlots);

        hudView.SetStatus(ChineseTextConfig.EarningsPlaying);
        hudView.SetLog(result.LogLines);
        yield return shelfView.PlayEarningAnimation(result.SlotEarnings);

        totalMoney += currentRoundScore;
        hudView.SetStatus(placement.ReplacedExistingItem ? ChineseTextConfig.ShelfFullReplacement : ChineseTextConfig.RoundIncome(roundIndex, currentRoundScore));
        CheckStageProgress();
        sequenceRunning = false;
        RenderHud();

        if (!runEnded)
        {
            PrepareRestockOffers();
        }
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
        SetOfferPopupVisible(false);
        hudView.SetStatus(message);
        RenderHud();
    }

    private void ToggleSwapMode()
    {
        if (runEnded || sequenceRunning || initialPicksRemaining > 0 || swapUses <= 0 || shelf.All(item => item == null))
        {
            return;
        }

        swapMode = !swapMode;
        selectedSwapIndex = -1;
        hudView.SetStatus(swapMode ? ChineseTextConfig.SwapSelecting : ChineseTextConfig.SwapCancelled);
        SetOfferPopupVisible(!swapMode);
        shelfView.Render(shelf, null);
        RenderHud();
    }

    private void SelectShelfSlot(int index)
    {
        if (!swapMode || sequenceRunning || runEnded || shelf[index] == null)
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
        if (!runEnded)
        {
            PrepareRestockOffers();
        }
    }

    private void PickCustomerTag()
    {
        string[] tags = { "早餐", "零食", "饮料", "健康" };
        preferredTag = tags[rng.Next(tags.Length)];
    }

    private void RenderHud()
    {
        bool canSwap = !runEnded && !sequenceRunning && initialPicksRemaining == 0 && swapUses > 0 && shelf.Any(item => item != null);
        hudView.RenderSummary(totalMoney, stageIndex, currentTarget, roundIndex, TotalRounds, preferredTag, swapUses, swapMode, canSwap, runEnded);
    }

    private List<VendingItemDefinition> BuildOffers(string rarityFilter)
    {
        var offers = new List<VendingItemDefinition>();
        var weighted = catalog.BuildWeightedPool(rarityFilter);
        while (offers.Count < restockChoices.Length)
        {
            var candidate = weighted[rng.Next(weighted.Count)];
            if (!offers.Contains(candidate))
            {
                offers.Add(candidate);
            }
        }

        return offers;
    }

    private void SetOffers(List<VendingItemDefinition> offers, Action<VendingItemDefinition> handler)
    {
        for (int i = 0; i < restockChoices.Length; i++)
        {
            restockChoices[i].SetChoice(offers[i], handler);
        }
    }

    private void SetOfferPopupVisible(bool visible)
    {
        foreach (var choice in restockChoices)
        {
            choice.SetInteractable(visible);
        }

        if (offerPopup != null)
        {
            offerPopup.SetActive(visible);
        }
    }

    private GameObject ResolveOfferPopup()
    {
        if (restockChoices == null || restockChoices.Length == 0 || restockChoices[0] == null)
        {
            return null;
        }

        var parent = restockChoices[0].transform.parent;
        return parent != null && parent.parent != null ? parent.parent.gameObject : restockChoices[0].gameObject;
    }
}
