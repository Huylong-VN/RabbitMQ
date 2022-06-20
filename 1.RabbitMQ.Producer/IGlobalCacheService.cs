using StackExchange.Redis;

namespace _1.RabbitMQ.Producer
{
    public interface IGlobalCacheService : IRedisCacheService
    {
    }

    public interface IRedisCacheService
    {
        public ConnectionMultiplexer Connection();

        Task<T> GetAsync<T>(RedisKey key);

        Task<string> GetAsync(RedisKey key);

        Task<bool> SetAsync<T>(int databaseId, object key, T value, bool overrideKey);

        Task<bool> SetAsync<T>(RedisKey key, T value);

        Task<bool> SetAsync(RedisKey key, string value);

        Task<long> QueuePushAsync(RedisKey key, byte[] value);

        Task<long> QueuePushAsync(RedisKey key, string value);

        Task<bool> SetPushAsync(RedisKey key, byte[] value);

        Task<bool> SetPushAsync(RedisKey key, string value);

        Task<RedisValue[]> ListAsync(RedisKey key);

        Task<RedisValue[]> ListAsync(RedisKey key, int start, int end);

        Task<List<T>> ListByPrefixAsync<T>(RedisKey prefix);

        Task<List<string>> ListKeyByPrefixAsync(RedisKey prefix);

        Task<bool> ExistsAsync(RedisKey key);

        Task<string> ExecuteLuaScript(int databaseId, string script);

        Task HashIncrementAsync(RedisKey key, string field, long value);

        Task ListLeftPushAsync(RedisKey key, string value);

        Task KeyDeleteAsync(RedisKey key);

    
    }
}
