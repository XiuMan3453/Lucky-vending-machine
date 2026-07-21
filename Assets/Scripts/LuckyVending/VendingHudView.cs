using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public sealed class VendingHudView : MonoBehaviour
{
    public Text moneyText;
    public Text targetText;
    public Text roundText;
    public Text customerText;
    public Text statusText;
    public Text logText;
    public Text swapText;
    public Button swapButton;
    public Button restartButton;

    public void Bind(System.Action onSwap, System.Action onRestart)
    {
        swapButton.onClick.RemoveAllListeners();
        swapButton.onClick.AddListener(() => onSwap?.Invoke());
        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(() => onRestart?.Invoke());
    }

    public void RenderSummary(int totalMoney, int stageIndex, int currentTarget, int roundIndex, int totalRounds, string preferredTag, int swapUses, bool swapMode, bool canSwap, bool runEnded)
    {
        moneyText.text = totalMoney.ToString();
        targetText.text = ChineseTextConfig.StageTarget(stageIndex, currentTarget);
        roundText.text = ChineseTextConfig.RoundCounter(Mathf.Min(roundIndex + 1, totalRounds), totalRounds);
        customerText.text = ChineseTextConfig.CustomerPreference(preferredTag);
        swapText.text = ChineseTextConfig.SwapUses(swapUses, swapMode);
        swapButton.interactable = canSwap;
        restartButton.gameObject.SetActive(runEnded);
    }

    public void SetStatus(string message)
    {
        statusText.text = message;
    }

    public void SetLog(IEnumerable<string> lines)
    {
        logText.text = string.Join("\n", lines.Take(7));
    }
}
