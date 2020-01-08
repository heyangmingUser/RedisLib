using System;
using System.Collections.Generic;
using System.Text;

namespace RedisServer
{
    public class RedisConfig
    {
        /// <summary>
        /// 读写服务器地址
        /// </summary>
        public string ReadWriteHosts { get; set; }

        /// <summary>
        /// 是否打开缓存1是0否
        /// </summary>
        public int IsOpenCache{ get; set; }

        /// <summary>
        /// 指定的数据库索引，从0开始计算
        /// </summary>
        public int Datebase { get; set; }
    }
}
