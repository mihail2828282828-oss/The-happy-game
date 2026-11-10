using Front21.Economy;
using UnityEngine;

namespace Front21.Content
{
    [CreateAssetMenu(menuName = "Front21/Товар", fileName = "Item_")]
    public sealed class ShopItemDef : ScriptableObject
    {
        [Header("Идентификация")]
        [SerializeField] string _id;
        [SerializeField] string _displayName;
        [SerializeField] ShopCategory _category;
        [SerializeField] ItemTag _tag;

        [Header("Цена")]
        [SerializeField] Currency _currency = Currency.Coins;
        [SerializeField, Min(1)] long _basePrice = 10;
        [SerializeField, Min(1)] int _maxLevel = 1;

        [Header("Правила")]
        [SerializeField, Min(0)] int _unlockLevel;
        [SerializeField, Min(0)] int _maxStack;
        [SerializeField] bool _featuredEligible = true;
        [SerializeField] bool _stackable;

        public string Id => _id;
        public string DisplayName => _displayName;
        public ShopCategory Category => _category;
        public ItemTag Tag => _tag;
        public Currency Currency => _currency;
        public long BasePrice => _basePrice;
        public int MaxLevel => Mathf.Max(1, _maxLevel);
        public int UnlockLevel => _unlockLevel;
        public int MaxStack => _stackable ? Mathf.Max(1, _maxStack) : 1;
        public bool Stackable => _stackable;
        public bool FeaturedEligible => _featuredEligible;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(_id)) Debug.LogError($"[Shop] у товара {name} пустой id");
            if (_basePrice <= 0) Debug.LogError($"[Shop] у товара {_id} нулевая цена");
            if (_stackable && _maxStack <= 0) Debug.LogError($"[Shop] у товара {_id} стек без лимита");
        }
    }
}
