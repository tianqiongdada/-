using System.Collections.Generic;
namespace Ddxy.GameServer.Data.Config
{
    // 转盘奖励配置
    public class Draw1stConfig
    {
        // 转盘奖项
        public List<DrawItem> drawItems { get; set; }
        // 白名单
        public List<uint> whiteList { get; set; }
    }
}