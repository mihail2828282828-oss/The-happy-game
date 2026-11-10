using System;

namespace Front21.Economy
{
    public interface IWalletClock
    {
        long UtcNowUnix { get; }
    }

    public sealed class SystemWalletClock : IWalletClock
    {
        public long UtcNowUnix => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
