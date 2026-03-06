using EyuBot.Abstractions;

namespace EyuBot.Storage
{
    public static class StorageFactory
    {
        public static IStorage CreateStorage(string storageType, string connectionString)
        {
            switch (storageType.ToLower())
            {
                case "sqlite":
                    return new SqliteStorage(connectionString);
                case "redis":
                    // 实现Redis存储
                    throw new System.NotImplementedException("Redis storage is not implemented yet");
                case "cloud":
                    // 实现云存储
                    throw new System.NotImplementedException("Cloud storage is not implemented yet");
                default:
                    throw new System.ArgumentException($"Unknown storage type: {storageType}");
            }
        }
    }
}
