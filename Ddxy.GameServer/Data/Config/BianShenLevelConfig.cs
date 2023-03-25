using System.Text.Json;
namespace Ddxy.GameServer.Data.Config
{
    public class BianShenLevelConfig
    {
        public int level { get; set; }
        public string desc { get; set; }
        public JsonElement? attr { get; set; }
        // 本级所需经验
        public int exp { get; set; }
    }
}