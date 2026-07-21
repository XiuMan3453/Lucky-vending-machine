public static class ChineseTextConfig
{
    public const string InitialStatus = "选择一个补货商品，加入后会旋转货架并结算。";
    public const string InitialLogMatchingTags = "相同标签上下左右相连会加钱。";
    public const string InitialLogNoRandomFill = "未拥有的符号不会凭空出现。";
    public const string InitialLogSpinAfterPick = "每次补货后都会旋转一次。";
    public const string ShelfFullReplacement = "货架已满，新商品替换后完成旋转。";
    public const string SwapSelecting = "交换模式：请选择两个已有商品的格子。";
    public const string SwapCancelled = "已取消交换。";
    public const string SelectSecondSlot = "。请选择第二个格子。";
    public const string SelectionCancelled = "已取消当前选择，请重新选择第一个格子。";

    public static string RoundIncome(int roundIndex, int score)
    {
        return "第 " + roundIndex + " 回合旋转后收入 " + score + "。需要时可用交换道具。";
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
