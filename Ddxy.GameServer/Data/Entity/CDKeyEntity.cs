using System;
using FreeSql.DataAnnotations;

namespace Ddxy.GameServer.Data.Entity
{
    /// <summary>
    /// 类型, 1充值 2积分单冲 3物品
    /// </summary>
    public enum CDKeyType : byte
    {
        /// <summary>
        /// 1充值
        /// </summary>
        Charge = 1,

        /// <summary>
        /// 2积分单冲
        /// </summary>
        BindJadeCharge = 2,

        /// <summary>
        /// 3物品
        /// </summary>
        ItemCharge = 3,
    }

    [Table(Name = "cd_key")]
    public class CDKeyEntity : IEquatable<CDKeyEntity>
    {
        /// <summary>
        /// id
        /// </summary>
        [Column(IsPrimary = true, IsIdentity = true)]
        public uint Id { get; set; }

        /// <summary>
        /// Key值
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 类型, 1充值 2积分单冲 3物品
        /// </summary>
        public byte TypeId { get; set; }

        /// <summary>
        /// 面额，单位元
        /// </summary>
        public uint Price { get; set; }

        /// <summary>
        /// 附加信息
        /// </summary>
        public string Extra { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public uint CreateTime { get; set; }

        /// <summary>
        /// 兑换时间
        /// </summary>
        public uint DoneTime { get; set; }

        public void CopyFrom(CDKeyEntity other)
        {
            Id = other.Id;
            Key = other.Key;
            TypeId = other.TypeId;
            Price = other.Price;
            Extra = other.Extra;
            CreateTime = other.CreateTime;
            DoneTime = other.DoneTime;
        }

        public bool Equals(CDKeyEntity other)
        {
            if (other == null) return false;
            return Id == other.Id &&
            Key.Equals(other.Key) &&
            TypeId == other.TypeId &&
            Price == other.Price &&
            Extra.Equals(other.Extra) &&
            CreateTime == other.CreateTime &&
            DoneTime == other.DoneTime;
        }
    }
}