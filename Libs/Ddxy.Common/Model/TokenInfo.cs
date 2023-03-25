using Ddxy.Common.Model.Admin;

namespace Ddxy.Common.Model
{
    public readonly struct TokenInfo
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// Admin Category, 如果是游戏前端的用户则为0
        /// </summary>
        public AdminCategory Category { get; }

        /// <summary>
        /// Token特殊检查，只有后台用户有效
        /// </summary>
        public string Sec { get; }

        public TokenInfo(uint id, AdminCategory category, string sec = null)
        {
            Id = id;
            Category = category;
            Sec = sec;
        }
    }
}