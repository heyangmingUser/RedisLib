using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedisServer
{
    public class RedisService
    {
        readonly ILogger<RedisService> Log;//日志记录
        ILoggerFactory _loggerFactory;
        private readonly RedisConfig config;
       
        public RedisService(IOptions<RedisConfig> configs, ILoggerFactory loggerFactory)
        {
            config = configs.Value;
            _loggerFactory = loggerFactory;
            Log = loggerFactory.CreateLogger<RedisService>();
          
        }
        private static ConnectionMultiplexer connection; 
        public ConnectionMultiplexer CacheConnection
        {
            get
            {
                try
                {
                    if (connection == null || !connection.IsConnected)
                    {
                        connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(config.ReadWriteHosts)).Value;
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(ex.ToString());
                    return null;
                }
                return connection;
            }
        }
        /// <summary>
        /// 指定数据库
        /// </summary>
        public IDatabase CacheRedis => CacheConnection.GetDatabase(config.Datebase);

        /// <summary>
        /// 单条存值
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">TheValue</param>
        /// <returns></returns>
        public bool  StringSet(string key, string value)
        {
            return  CacheRedis.StringSet(key, value);
        }

        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        public bool StringSet(string key, string value, TimeSpan? expiry = default(TimeSpan?))
        {
            return CacheRedis.StringSet(key, value, expiry);
        }

        /// <summary>
        /// 保存多个key value
        /// </summary>
        /// <param name="arr">key</param>
        /// <returns></returns>
        public bool StringSet(KeyValuePair<RedisKey, RedisValue>[] arr)
        {
            return CacheRedis.StringSet(arr);
        }

        /// <summary>
        /// 批量存值
        /// </summary>
        /// <param name="keysStr"></param>
        /// <param name="valuesStr"></param>
        /// <returns></returns>
        public bool StringSetMany(string[] keysStr, string[] valuesStr)
        {
            var count = keysStr.Length;
            var keyValuePair = new KeyValuePair<RedisKey, RedisValue>[count];
            for (int i = 0; i < count; i++)
            {
                keyValuePair[i] = new KeyValuePair<RedisKey, RedisValue>(keysStr[i], valuesStr[i]);
            }

            return CacheRedis.StringSet(keyValuePair);
        }

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool SetStringKey<T>(string key, T obj, TimeSpan? expiry = default(TimeSpan?))
        {
            string json = JsonConvert.SerializeObject(obj);
            return CacheRedis.StringSet(key, json, expiry);
        }

        /// <summary>
        /// 追加值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void StringAppend(string key, string value)
        {
            //追加值，返回追加后长度
            long appendlong = CacheRedis.StringAppend(key, value);
        }
        /// <summary>
        /// 获取单个key的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public RedisValue GetStringKey(string key)
        {
            return CacheRedis.StringGet(key);
        }
        /// <summary>
        /// 根据key获取值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string StringGet(string key)
        {
            try
            {
                return CacheRedis.StringGet(key);
            }
            catch (Exception ex)
            {
                Log.LogError(ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 获取多个Key
        /// </summary>
        /// <param name="listKey"></param>
        /// <returns></returns>
        public RedisValue[] GetStringKey(List<RedisKey> listKey)
        {
            return CacheRedis.StringGet(listKey.ToArray());
        }
        /// <summary>
        /// 批量获取值
        /// </summary>
        /// <param name="keyStrs"></param>
        /// <returns></returns>
        public string[] StringGetMany(string[] keyStrs)
        {
            var count = keyStrs.Length;
            var keys = new RedisKey[count];
            var addrs = new string[count];

            for (var i = 0; i < count; i++)
            {
                keys[i] = keyStrs[i];
            }
            try
            {

                var values = CacheRedis.StringGet(keys);
                for (var i = 0; i < values.Length; i++)
                {
                    addrs[i] = values[i];
                }
                return addrs;
            }
            catch (Exception ex)
            {
                Log.LogError(ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetStringKey<T>(string key)
        {
            return JsonConvert.DeserializeObject<T>(CacheRedis.StringGet(key));
        }

        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>是否删除成功</returns>
        public bool KeyDelete(string key)
        {
            return CacheRedis.KeyDelete(key);
        }

        /// <summary>
        /// 删除多个key
        /// </summary>
        /// <param name="keys">rediskey</param>
        /// <returns>成功删除的个数</returns>
        public long KeyDelete(RedisKey[] keys)
        {
            return CacheRedis.KeyDelete(keys);
        }

        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public bool KeyRename(string key, string newKey)
        {
            return CacheRedis.KeyRename(key, newKey);
        }
        /// <summary>
        /// 删除hashkey
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public bool HaseDelete(RedisKey key, RedisValue hashField)
        {
            return CacheRedis.HashDelete(key, hashField);
        }
        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public bool HashRemove(string key, string dataKey)
        {
            return CacheRedis.HashDelete(key, dataKey);
        }

        /// <summary>
        /// 设置缓存过期
        /// </summary>
        /// <param name="key"></param>
        /// <param name="datetime"></param>
        public void SetExpire(string key, DateTime datetime)
        {
            CacheRedis.KeyExpire(key, datetime);
        }
    }
}
