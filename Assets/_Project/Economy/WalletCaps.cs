using System;

namespace Front21.Economy
{
    [Serializable]
    public struct WalletCaps
    {
        public long CoinsCap;
        public long CrystalsCap;
        public long CrystalsWeeklyCap;

        public static WalletCaps Default => new WalletCaps
        {
            CoinsCap = 20_000,
            CrystalsCap = 600,
            CrystalsWeeklyCap = 120
        };

        public bool IsValid =>
            CoinsCap > 0 &&
            CrystalsCap > 0 &&
            CrystalsWeeklyCap > 0 &&
            CrystalsWeeklyCap <= CrystalsCap;
    }
}
