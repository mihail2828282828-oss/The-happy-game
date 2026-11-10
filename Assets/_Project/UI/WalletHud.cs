using Front21.Core;
using Front21.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace Front21.UI
{
    /// <summary>Индикатор баланса. Обновляется только по событию кошелька.</summary>
    public sealed class WalletHud : MonoBehaviour
    {
        [SerializeField] Text _label;

        IWallet _wallet;

        void Start()
        {
            if (_label == null) _label = UiFactory.CreateText(transform, "Balance", 22);
            if (!GameServices.IsReady) return;

            _wallet = GameServices.Wallet;
            _wallet.Changed += OnChanged;
            OnChanged(Currency.Coins, _wallet.Coins);
        }

        void OnChanged(Currency currency, long balance)
        {
            if (_wallet == null || _label == null) return;
            _label.text = $"{_wallet.Coins} монет · {_wallet.Crystals} крист.";
        }

        void OnDestroy()
        {
            if (_wallet != null) _wallet.Changed -= OnChanged;
        }
    }
}
