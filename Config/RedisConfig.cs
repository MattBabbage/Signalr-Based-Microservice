using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DaprSignalrBeta.Config;

namespace DaprSignalrBeta.Config
{
    public class RedisConfig
    {
        public const string RedisConfigKey = "RedisConfig";
        public bool UseAsBackplane { get; set; }
        public Settings Settings { get; set; }
    }
}
