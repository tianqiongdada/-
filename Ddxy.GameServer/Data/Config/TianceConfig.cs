using Ddxy.Protocol;
using System.Collections.Generic;
namespace Ddxy.GameServer.Data.Config
{
    public class TianceConfig
    {
        public uint id { get; set; }
        public SkillId skillId { get; set; }
        public string name { get; set; }
        public TianceFuType type { get; set; }
        public uint tier { get; set; }
        public List<Race> race { get; set; }
    }
}