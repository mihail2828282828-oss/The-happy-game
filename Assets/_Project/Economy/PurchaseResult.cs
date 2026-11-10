namespace Front21.Economy
{
    public enum PurchaseFailure
    {
        None = 0,
        UnknownItem = 1,
        CategoryClosed = 2,
        LockedByLevel = 3,
        NotEnoughCurrency = 4,
        StackLimit = 5,
        NotOwned = 6,
        LevelCapReached = 7,
        InvalidQuantity = 8
    }

    public readonly struct PurchaseResult
    {
        public readonly bool Success;
        public readonly PurchaseFailure Failure;
        public readonly long PricePaid;

        PurchaseResult(bool success, PurchaseFailure failure, long pricePaid)
        {
            Success = success;
            Failure = failure;
            PricePaid = pricePaid;
        }

        public static PurchaseResult Ok(long price) => new(true, PurchaseFailure.None, price);
        public static PurchaseResult Fail(PurchaseFailure failure) => new(false, failure, 0);
    }
}
