using System;

namespace Front21.Economy
{
    /// <summary>
    /// Уровень игрока и опыт. Опыт идёт только вперёд, уровень зажат в пределы.
    /// </summary>
    public sealed class PlayerProgress : IProgressProvider
    {
        public const int MaxLevel = 60;
        const long BaseXpNeeded = 100;
        const long XpPerLevelStep = 60;

        int _level = 1;
        long _xp;

        public int PlayerLevel => _level;
        public long Xp => _xp;

        public event Action<int> LevelChanged;

        public static long XpToNext(int level) =>
            level >= MaxLevel ? long.MaxValue : BaseXpNeeded + (level - 1) * XpPerLevelStep;

        public int AddXp(long amount)
        {
            if (amount <= 0 || _level >= MaxLevel) return 0;

            int gained = 0;
            _xp += amount;

            while (_level < MaxLevel)
            {
                long need = XpToNext(_level);
                if (_xp < need) break;

                _xp -= need;
                _level++;
                gained++;
                LevelChanged?.Invoke(_level);
            }

            return gained;
        }

        public void SetLevel(int level)
        {
            _level = Math.Clamp(level, 1, MaxLevel);
            _xp = 0;
            LevelChanged?.Invoke(_level);
        }

        public PlayerProgressState Snapshot() => new PlayerProgressState { Level = _level, Xp = _xp };

        public void Restore(PlayerProgressState state)
        {
            if (state == null) return;

            _level = Math.Clamp(state.Level, 1, MaxLevel);
            _xp = Math.Max(0, state.Xp);
            LevelChanged?.Invoke(_level);
        }
    }
}
