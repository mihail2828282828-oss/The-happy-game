using System;

namespace Front21.Economy
{
    public sealed class Wallet : IWallet
    {
        public const int SellPricePercent = 35;
        public const long OverflowDurationSeconds = 172_800; // 48 часов
        public const long WeekSeconds = 604_800;

        readonly WalletCaps _caps;
        readonly IWalletClock _clock;

        long _coins;
        long _crystals;
        long _weeklyCrystals;
        long _weeklyResetUnix;
        long _overflowUntilUnix;

        public long Coins => _coins;
        public long Crystals => _crystals;
        public bool OverflowActive => _overflowUntilUnix > _clock.UtcNowUnix;

        public event Action<Currency, long> Changed;
        public event Action<Currency, long> CapHit;

        public Wallet() : this(WalletCaps.Default, new SystemWalletClock()) { }

        public Wallet(WalletCaps caps, IWalletClock clock)
        {
            if (!caps.IsValid) throw new ArgumentException("Некорректные лимиты кошелька.", nameof(caps));

            _caps = caps;
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _weeklyResetUnix = _clock.UtcNowUnix + WeekSeconds;
        }

        public bool CanAfford(Currency currency, long amount)
        {
            if (amount <= 0) return false;
            return Balance(currency) >= amount;
        }

        public bool TrySpend(Currency currency, long amount)
        {
            if (amount <= 0) return false;
            if (Balance(currency) < amount) return false;

            if (currency == Currency.Coins) _coins -= amount;
            else _crystals -= amount;

            Changed?.Invoke(currency, Balance(currency));
            return true;
        }

        public long Earn(Currency currency, long amount, EarnSource source)
        {
            if (amount <= 0) return 0;

            RefreshWeeklyWindow();

            long capacity = capacity(currency, source);
            if (capacity <= 0)
            {
                CapHit?.Invoke(currency, amount);
                return 0;
            }

            long granted = Math.Min(amount, capacity);
            long cap = currency == Currency.Coins ? _caps.CoinsCap : _caps.CrystalsCap;

            if (currency == Currency.Coins) _coins += granted;
            else
            {
                _crystals += granted;
                if (source.CountsTowardWeeklyCap()) _weeklyCrystals += granted;
            }

            if (granted < amount || Balance(currency) >= cap)
            {
                TouchOverflow();
                CapHit?.Invoke(currency, amount - granted);
            }

            Changed?.Invoke(currency, Balance(currency));
            return granted;
        }

        long capacity(Currency currency, EarnSource source)
        {
            long balance = Balance(currency);
            long cap = currency == Currency.Coins ? _caps.CoinsCap : _caps.CrystalsCap;

            // Переполнение уже идёт — разрешаем дойти до двойного потолка.
            if (OverflowActive) cap *= 2;

            long room = cap - balance;
            if (room <= 0) return 0;

            if (currency == Currency.Crystals && source.CountsTowardWeeklyCap())
            {
                long weeklyRoom = _caps.CrystalsWeeklyCap - _weeklyCrystals;
                room = Math.Min(room, Math.Max(0, weeklyRoom));
            }

            return room;
        }

        void TouchOverflow()
        {
            long now = _clock.UtcNowUnix;
            if (_overflowUntilUnix < now) _overflowUntilUnix = now + OverflowDurationSeconds;
        }

        void RefreshWeeklyWindow()
        {
            long now = _clock.UtcNowUnix;
            if (now < _weeklyResetUnix) return;

            _weeklyCrystals = 0;
            _weeklyResetUnix = now + WeekSeconds;
        }

        long Balance(Currency currency) => currency == Currency.Coins ? _coins : _crystals;

        public static long SellPrice(long buyPrice) => Math.Max(0, buyPrice) * SellPricePercent / 100;

        public WalletState Snapshot() => new WalletState
        {
            Coins = _coins,
            Crystals = _crystals,
            WeeklyCrystals = _weeklyCrystals,
            WeeklyResetUnix = _weeklyResetUnix,
            OverflowUntilUnix = _overflowUntilUnix
        };

        public void Restore(WalletState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            long cap = Math.Min(_caps.CoinsCap * 2, Math.Max(0, state.Coins));
            long crystalCap = Math.Min(_caps.CrystalsCap * 2, Math.Max(0, state.Crystals));

            _coins = cap;
            _crystals = crystalCap;
            _weeklyCrystals = Math.Clamp(state.WeeklyCrystals, 0, _caps.CrystalsWeeklyCap);
            _weeklyResetUnix = state.WeeklyResetUnix > 0 ? state.WeeklyResetUnix : _clock.UtcNowUnix + WeekSeconds;
            _overflowUntilUnix = Math.Max(0, state.OverflowUntilUnix);

            RefreshWeeklyWindow();
        }
    }
}
