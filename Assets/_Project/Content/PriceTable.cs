using System;
using UnityEngine;

namespace Front21.Content
{
    [CreateAssetMenu(menuName = "Front21/Прайс-таблица", fileName = "PriceTable")]
    public sealed class PriceTable : ScriptableObject
    {
        [SerializeField, Min(1f)] float _growthPerLevel = 1.18f;
        [SerializeField, Min(0.01f)] float _growthCap = 4.5f;
        [SerializeField, Min(1)] int _roundTo = 5;

        /// <summary>Множитель уровня. Растёт до потолка, чтобы высокие уровни не улетали в космос.</summary>
        public float LevelMultiplier(int level)
        {
            if (level <= 1) return 1f;
            float raw = Mathf.Pow(_growthPerLevel, level - 1);
            return Mathf.Min(raw, _growthCap);
        }

        public long Round(long value)
        {
            if (value <= 0) return 0;
            long step = Math.Max(1, _roundTo);
            return (value + step - 1) / step * step;
        }

        /// <summary>Итоговая цена товара на уровне, без скидок.</summary>
        public long Compute(long basePrice, int level) => Round((long)Math.Round(basePrice * LevelMultiplier(level)));
    }
}
