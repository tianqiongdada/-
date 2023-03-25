using System.Threading.Tasks;
using Ddxy.GrainInterfaces.Core;
using Orleans;
using Orleans.Concurrency;

namespace Ddxy.GrainInterfaces.Gate
{
    /// <summary>
    /// 执行网关的Http业务, 实现的Grain要求标注[StatelessWorker]
    /// </summary>
    public interface IApiGateGrain : IGrainWithIntegerKey
    {
        /// <summary>
        /// 检测指定userId缓存的token是否和参数中指定的token匹配
        /// <param name="addExpireIfEquals">如果匹配，是否增加过期时间</param>
        /// </summary>
        Task<bool> CheckUserToken(string ip, uint userId, string token, bool addExpireIfEquals = false);

        /// <summary>
        /// 查找指定用户下的所有角色id和上次使用的角色id
        /// </summary>
        Task<Roles> QueryRoles(string ip, uint userId);

        Task ReportError(string ip, Immutable<byte[]> payload);

        /// <summary>
        /// 注册
        /// </summary>
        Task<Immutable<byte[]>> SignUp(string ip, string username, string password, string inviteCode, string platform, string deviceId, bool simulation, bool isRobot);

        /// <summary>
        /// 登入
        /// </summary>
        Task<Immutable<byte[]>> SignIn(string ip, string username, string password, string platform, string deviceId, bool simulation);

        /// <summary>
        /// 获取公告
        /// </summary>
        Task<Immutable<byte[]>> GetNotice();

        /// <summary>
        /// 列举服务器和角色简要信息
        /// </summary>
        Task<Immutable<byte[]>> ListServer(string ip, uint userId);

        /// <summary>
        /// 创建角色
        /// </summary>
        Task<Immutable<byte[]>> CreateRole(string ip, uint userId, uint serverId, uint cfgId, string nickname);

        /// <summary>
        /// 进入区服
        /// </summary>
        Task<Immutable<byte[]>> EnterServer(string ip, uint userId, uint roleId);
        Task<string> MYXinNotify2(string ip, string json);

        Task<string> MYXinNotifyBindJade(string ip, string json);

        Task<string> YunDingNotify(string ip, string json);

        Task<string> YunDingNotifyBindJade(string ip, string json);
    }
}