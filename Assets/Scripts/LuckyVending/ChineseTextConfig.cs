public static class ChineseTextConfig
{
    public const string InitialStatus = "先选择 4 个普通符号，完成后正式开始。";
    public const string InitialLogMatchingTags = "相同标签上下左右相连会加钱。";
    public const string InitialLogNoRandomFill = "未拥有的符号不会凭空出现。";
    public const string InitialLogSpinAfterPick = "补货弹窗会在结算动画结束后出现。";
    public const string ShelfFullReplacement = "货架已满，新商品替换后完成旋转。";
    public const string SpinStarting = "售货机正在旋转...";
    public const string ComboChecking = "正在检查标签组合...";
    public const string EarningsPlaying = "正在结算每个格子的收入...";
    public const string ChooseRestock = "选择一个补货符号，选择后会自动旋转。";
    public const string SwapSelecting = "交换模式：请选择两个已有商品的格子。";
    public const string SwapCancelled = "已取消交换。";
    public const string SelectSecondSlot = "。请选择第二个格子。";
    public const string SelectionCancelled = "已取消当前选择，请重新选择第一个格子。";

    public static string RoundIncome(int roundIndex, int score)
    {
        return "第 " + roundIndex + " 回合旋转后收入 " + score + "。需要时可用交换道具。";
    }

    public static string InitialPickStatus(int remaining)
    {
        return "请选择初始普通符号，还需要 " + remaining + " 个。";
    }

    public static string InitialPickAdded(string itemName, int remaining)
    {
        return "已加入：" + itemName + "。还需要 " + remaining + " 个初始符号。";
    }

    public static string InitialPicksFinished()
    {
        return "初始符号选择完成。请选择第一轮补货符号。";
    }

    public static string RunCompleted(int totalMoney)
    {
        return "本局完成！总销售额：" + totalMoney;
    }

    public static string StageTargetReached(int currentTarget)
    {
        return "阶段达成！新目标：" + currentTarget;
    }

    public static string StageFailed(int totalMoney)
    {
        return "阶段目标未达成。本局销售额：" + totalMoney;
    }

    public static string SlotSelected(string itemName)
    {
        return "已选择：" + itemName + SelectSecondSlot;
    }

    public static string SwapDelta(int delta)
    {
        return "交换完成，本轮收入变化：" + delta;
    }

    public static string StageTarget(int stageIndex, int currentTarget)
    {
        return "阶段 " + (stageIndex + 1) + " 目标：" + currentTarget;
    }

    public static string RoundCounter(int roundIndex, int totalRounds)
    {
        return "回合 " + roundIndex + " / " + totalRounds;
    }

    public static string CustomerPreference(string preferredTag)
    {
        return "顾客想要\n" + preferredTag + "标签\n每个 +2";
    }

    public static string SwapUses(int swapUses, bool swapMode)
    {
        return "交换道具：" + swapUses + (swapMode ? "  选择中" : string.Empty);
    }
}
