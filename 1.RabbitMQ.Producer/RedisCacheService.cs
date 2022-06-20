using StackExchange.Redis;
using System.Reflection;

namespace _1.RabbitMQ.Producer
{
    public class RedisCacheService : IRedisCacheService, IGlobalCacheService, IDisposable
    {
        private readonly Lazy<ConnectionMultiplexer> _lazyConnection;

        public ConnectionMultiplexer Connection() => _lazyConnection.Value;
        private IDatabase Database(int id) => Connection().GetDatabase(id);

        public RedisCacheService(string connection)
        {
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connection));
        }

        public async Task<T> GetAsync<T>(RedisKey key)
        {
            var database = Database(key.Database);
            string redisValue = await database.StringGetAsync(key.KeyName);
            return redisValue.Deserialize<T>();
        }

        public async Task<string> GetAsync(RedisKey key)
        {
            var database = Database(key.Database);
            return await database.StringGetAsync(key.KeyName);
        }

        public async Task<bool> SetAsync<T>(int databaseId, object key, T value, bool overrideKey)
        {
            var redisKey = key.ToString();
            if (overrideKey) redisKey = Key<T>(key);
            var serialize = value.Serialize();
            if (serialize is not { Length: > 1 })
                return false;
            var database = Database(databaseId);
            await database.StringSetAsync(redisKey, serialize);
            return true;
        }

        public async Task<bool> SetAsync<T>(RedisKey key, T value)
        {
            var database = Database(key.Database);
            await database.StringSetAsync(key.KeyName, value.Serialize());
            return true;
        }

        public async Task<bool> SetAsync(RedisKey key, string value)
        {
            if (value is not { Length: > 1 })
                return false;
            var database = Database(key.Database);
            await database.StringSetAsync(key.KeyName, value);
            return true;
        }

        public async Task<long> QueuePushAsync(RedisKey key, byte[] value)
        {
            var database = Database(key.Database);
            return await database.ListLeftPushAsync(key.KeyName, value);
        }

        public async Task<long> QueuePushAsync(RedisKey key, string value)
        {
            var database = Database(key.Database);
            return await database.ListLeftPushAsync(key.KeyName, value);
        }

        public async Task<bool> SetPushAsync(RedisKey key, byte[] value)
        {
            var database = Database(key.Database);
            return await database.SetAddAsync(key.KeyName, value);
        }

        public async Task<bool> SetPushAsync(RedisKey key, string value)
        {
            var database = Database(key.Database);
            return await database.SetAddAsync(key.KeyName, value);
        }

        public async Task<RedisValue[]> ListAsync(RedisKey key)
        {
            var database = Database(key.Database);
            return await database.ListRangeAsync(key.KeyName);
        }

        public async Task<RedisValue[]> ListAsync(RedisKey key, int start, int end)
        {
            var database = Database(key.Database);
            return await database.ListRangeAsync(key.KeyName, start, end);
        }

        public async Task<List<T>> ListByPrefixAsync<T>(RedisKey prefix)
        {
            var records = new List<T>();
            var server = Server();
            var keys = server.Keys(pattern: $"{prefix.KeyName}*", database: prefix.Database);
            foreach (var key in keys)
            {
                var record = await GetAsync<T>(new RedisKey
                {
                    Database = prefix.Database,
                    KeyName = key
                });
                records.Add(record);
            }

            return records;
        }

        public async Task<List<string>> ListKeyByPrefixAsync(RedisKey prefix)
        {
            var server = Server();
            var keys = server.KeysAsync(pattern: $"{prefix.KeyName}*", database: prefix.Database);
            var list = new List<string>();
            await foreach (var key in keys)
            {
                list.Add(key.ToString());
            }
            return list;
        }

        public async Task<bool> ExistsAsync(RedisKey key)
        {
            var database = Database(key.Database);
            return await database.KeyExistsAsync(key.KeyName);
        }

        public async Task<string> ExecuteLuaScript(int databaseId, string script)
        {
            var database = Database(databaseId);
            var json = await database.ScriptEvaluateAsync(script);
            return json.ToString();
        }

        private static string Key<T>(object key) => typeof(T).GetTypeInfo().Assembly.GetName().Name + ":" + key;

        public static string Key(Type type, object key) => type.GetTypeInfo().Assembly.GetName().Name + ":" + key;


        public async Task HashIncrementAsync(RedisKey key, string field, long value)
        {
            var database = Database(key.Database);
            await database.HashIncrementAsync(key.KeyName, field, value);
        }

        public async Task ListLeftPushAsync(RedisKey key, string value)
        {
            var database = Database(key.Database);
            await database.ListLeftPushAsync(key.KeyName, value);
        }

        public async Task KeyDeleteAsync(RedisKey key)
        {
            var database = Database(key.Database);
            await database.KeyDeleteAsync(key.KeyName);
        }

        private IServer Server()
        {
            foreach (var endpoint in _lazyConnection.Value.GetEndPoints())
            {
                var server = _lazyConnection.Value.GetServer(endpoint);
                if (!server.IsReplica) return server;
            }

            throw new InvalidOperationException("Requires a master endpoint (found none)");
        }


        public void Dispose()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
