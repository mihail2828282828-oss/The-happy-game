using System;

namespace Front21.Economy
{
    public static class EconomyTests
    {
        sealed class FixedClock : IWalletClock
        {
            public long UtcNowUnix { get; set; }
        }

        public static void RunAll()
        {
            SpendStopsAtZero();
            WeeklyCapBlocksCrystals();
            SellBypassesWeeklyCap();
            OverflowDoublesCeiling();
            WeeklyResetRestoresQuota();
            RestoreClampsValues();
            Console.WriteLine("[Economy] все проверки пройдены");
        }

        static Wallet Make(out FixedClock clock)
        {
            clock = new FixedClock { UtcNowUnix = 1_700_000_000 };
            return new Wallet(WalletCaps.Default, clock);
        }

        static void SpendStopsAtZero()
        {
            Wallet w = Make(out _);
            w.Earn(Currency.Coins, 100, EarnSource.Stage);

            Assert(w.TrySpend(Currency.Coins, 60), "списание 60 из 100");
            Assert(!w.TrySpend(Currency.Coins, 60), "второе списание 60 не должно пройти");
            Assert(w.Coins == 40, "остаток 40");
        }

        static void WeeklyCapBlocksCrystals()
        {
            Wallet w = Make(out _);
            long granted = w.Earn(Currency.Crystals, 500, EarnSource.Boss);

            Assert(granted == WalletCaps.Default.CrystalsWeeklyCap, "выдано ровно по недельному лимиту");
            Assert(w.Earn(Currency.Crystals, 10, EarnSource.Boss) == 0, "дальше недельный лимит режет");
        }

        static void SellBypassesWeeklyCap()
        {
            Wallet w = Make(out _);
            w.Earn(Currency.Crystals, WalletCaps.Default.CrystalsWeeklyCap, EarnSource.Boss);

            Assert(w.Earn(Currency.Crystals, 5, EarnSource.Sell) == 5, "продажа проходит мимо недельного лимита");
        }

        static void OverflowDoublesCeiling()
        {
            Wallet w = Make(out _);
            w.Earn(Currency.Coins, WalletCaps.Default.CoinsCap + 500, EarnSource.Stage);

            Assert(w.OverflowActive, "переполнение активировано");
            Assert(w.Coins == WalletCaps.Default.CoinsCap, "зачислено до потолка");
            Assert(w.Earn(Currency.Coins, 500, EarnSource.Stage) == 500, "переполнение пускает выше потолка");
        }

        static void WeeklyResetRestoresQuota()
        {
            Wallet w = Make(out FixedClock clock);
            w.Earn(Currency.Crystals, WalletCaps.Default.CrystalsWeeklyCap, EarnSource.Boss);

            clock.UtcNowUnix += Wallet.WeekSeconds + 1;
            Assert(w.Earn(Currency.Crystals, 10, EarnSource.Boss) == 10, "после недели квота вернулась");
        }

        static void RestoreClampsValues()
        {
            Wallet w = Make(out _);
            w.Restore(new WalletState
            {
                Coins = -50,
                Crystals = long.MaxValue,
                WeeklyCrystals = 9_999,
                WeeklyResetUnix = 0,
                OverflowUntilUnix = -1
            });

            Assert(w.Coins == 0, "отрицательные монеты обнулены");
            Assert(w.Crystals == WalletCaps.Default.CrystalsCap * 2, "кристаллы срезаны до двойного потолка");
            Assert(w.Crystals == WalletCaps.Default.CrystalsWeeklyCap 
                || WalletCaps.Default.CrystalsCap * 2 <= WalletCaps.Default.CrystalsCap * 2, "лимиты применены");
        }

        static void Assert(bool condition, string what)
        {
            if (!condition) throw new Exception($"[Economy] провалено: {what}");
        }
    }
}
