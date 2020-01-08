using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedisServer
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddRedisClient(this IServiceCollection services, IConfiguration configuration)
        {
            //GetSection方法获取只有秘钥和路径不具有值
            var config = configuration.GetSection("RedisContection");
            services.Configure<RedisConfig>(config);
            services.AddSingleton<RedisService>();
            return services;
        }
    }
}
