using System;
using System.Collections.Generic;
using Front21.Content;

namespace Front21.Economy
{
    public sealed class FeaturedRotation
    {
        public const int Slots = 3;
        public const int DiscountPercent = 30;
        public const long RotationSeconds = 86_400;

        readonly IWalletClock _clock;
        readonly List<string> _ids = new();
        long _nextRotationUnix;

        public IReadOnlyList<string> Ids => _ids;
        public long NextRotationUnix => _nextRotationUnix;

        public FeaturedRotation(IWalletClock clock) =>
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));

        public void Refresh(IReadOnlyList<ShopItemDef> pool)
        {
            if (pool == null) return;

            long now = _clock.UtcNowUnix;
            if (now < _nextRotationUnix && _ids.Count == Slots) return;

            _ids.Clear();
            _nextRotationUnix = now + RotationSeconds;

            var candidates = new List<ShopItemDef>();
            for (int i = 0; i < pool.Count; i++)
            {
                ShopItemDef item = pool[i];
                if (item != null && item.FeaturedEligible) candidates.Add(item);
            }

            var rng = new Random(candidates.Count ^ (int)now);
            int take = Math.Min(Slots, candidates.Count);
            for (int i = 0; i < take; i++)
            {
                int pick = rng.Next(candidates.Count);
                _ids.Add(candidates[pick].Id);
                candidates.RemoveAt(pick);
            }
        }

        public bool IsFeatured(string itemId) => _ids.Contains(itemId);

        public static long ApplyDiscount(long price) =>
            Math.Max(1, price * (100 - DiscountPercent) / 100);
    }
}
