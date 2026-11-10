namespace Front21.Economy
{
    public enum EarnSource
    {
        Stage = 0,
        Boss = 1,
        Daily = 2,
        Achievement = 3,
        Endless = 4,
        Sell = 5
    }

    public static class EarnSourceRules
    {
        /// <summary>Продажа предметов не расходует недельный лимит кристаллов.</summary>
        public static bool CountsTowardWeeklyCap(this EarnSource source) => source != EarnSource.Sell;
    }
}
