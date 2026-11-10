namespace Front21.Core
{
    /// <summary>
    /// Абстракция хранилища прогресса. Реализация подменяется на локальную
    /// или серверную без правок в игровом коде.
    /// </summary>
    public interface ISaveSystem
    {
        bool TryLoad<T>(string key, out T value) where T : class;
        void Store<T>(string key, T value) where T : class;
        void Flush();
        void Wipe();
    }
}
