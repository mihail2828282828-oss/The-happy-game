using System;

namespace Front21.Economy
{
    [Serializable]
    public sealed class WalletState
    {
        public long Coins;
        public long Crystals;
        public long WeeklyCrystals;
        public long WeeklyResetUnix;
        public long OverflowUntilUnix;
    }
}
