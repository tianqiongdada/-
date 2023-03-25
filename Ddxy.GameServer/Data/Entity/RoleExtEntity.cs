using System;
using FreeSql.DataAnnotations;

namespace Ddxy.GameServer.Data.Entity
{
    [Table(Name = "role_ext")]
    public class RoleExtEntity : IEquatable<RoleExtEntity>
    {
        /// <summary>
        /// 所属角色id
        /// </summary>
        [Column(IsPrimary = true, Name = "rid")]
        public uint RoleId { get; set; }

        /// <summary>
        /// 拥有的物品及其数量
        /// </summary>
        public string Items { get; set; }

        /// <summary>
        /// 仓库中的物品及其数量
        /// </summary>
        public string Repos { get; set; }

        /// <summary>
        /// 我处理过的全服邮件id集合
        /// </summary>
        public string Mails { get; set; }

        /// <summary>
        /// 天演策信息，等级、天策符信息
        /// </summary>
        public string Tiance { get; set; }

        /// <summary>
        /// 切割等级
        /// </summary>
        [Column(Name = "qiegeLevel")]
        public uint QieGeLevel { get; set; }

        /// <summary>
        /// 切割经验
        /// </summary>
        [Column(Name = "qiegeExp")]
        public uint QieGeExp { get; set; }

        /// <summary>
        /// 是否已经完成新手抽奖？
        /// </summary>
        [Column(Name = "done1stDraw")]
        public bool Done1stDraw { get; set; }

        public void CopyFrom(RoleExtEntity other)
        {
            RoleId = other.RoleId;
            Items = other.Items;
            Repos = other.Repos;
            Mails = other.Mails;
            Tiance = other.Tiance;
            QieGeLevel = other.QieGeLevel;
            QieGeExp = other.QieGeExp;
            Done1stDraw = other.Done1stDraw;
        }

        public bool Equals(RoleExtEntity other)
        {
            if (other == null) return false;
            return RoleId == other.RoleId &&
                   Items.Equals(other.Items) &&
                   Repos.Equals(other.Repos) &&
                   Mails.Equals(other.Mails) &&
                   ((Tiance == null && other.Tiance == null) || (Tiance != null && other.Tiance != null && Tiance.Equals(other.Tiance))) &&
                   QieGeLevel == other.QieGeLevel &&
                   Done1stDraw == other.Done1stDraw &&
                   QieGeExp == other.QieGeExp;
        }
    }
}