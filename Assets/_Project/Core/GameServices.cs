using System;
using Front21.Economy;
using Front21.Thermal;

namespace Front21.Core
{
    /// <summary>
    /// Единая точка доступа к сервисам без синглтон-разброда по сцене.
    /// Инициализируется один раз из бутстрапа и не ищет объекты на лету.
    /// </summary>
    public static class GameServices
    {
        public static IWallet Wallet { get; private set; }
        public static IThermalGuard Thermal { get; private set; }
        public static ISaveSystem Save { get; private set; }

        public static bool IsReady { get; private set; }

        public static void Bootstrap(IWallet wallet, IThermalGuard thermal, ISaveSystem save)
        {
            if (IsReady) throw new InvalidOperationException("GameServices уже инициализирован.");

            Wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            Thermal = thermal ?? throw new ArgumentNullException(nameof(thermal));
            Save = save ?? throw new ArgumentNullException(nameof(save));
            IsReady = true;
        }

        public static void Shutdown()
        {
            Wallet = null;
            Thermal = null;
            Save = null;
            IsReady = false;
        }
    }
}
