using System;

namespace Front21.Economy
{
    /// <summary>
    /// Хранилище двух валют с жёсткими лимитами. Все изменения проходят
    /// только через Earn и TrySpend, прямых сеттеров нет.
    /// </summary>
    public interface IWallet
    {
        long Coins { get; }
        long Crystals { get; }
        bool OverflowActive { get; }

        bool CanAfford(Currency currency, long amount);

        bool TrySpend(Currency currency, long amount);

        /// <returns>Сколько единиц реально зачислено: остаток может срезать лимит.</returns>
        long Earn(Currency currency, long amount, EarnSource source);

        event Action<Currency, long> Changed;
        event Action<Currency, long> CapHit;
    }
}
