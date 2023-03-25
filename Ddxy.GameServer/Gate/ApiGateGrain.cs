using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Ddxy.Common.Jwt;
using Ddxy.Common.Model;
using Ddxy.Common.Model.Api;
using Ddxy.Common.Utils;
using Ddxy.GameServer.Data;
using Ddxy.GameServer.Data.Config;
using Ddxy.GameServer.Data.Entity;
using Ddxy.GameServer.Http;
using Ddxy.GameServer.Option;
using Ddxy.GameServer.Util;
using Ddxy.GrainInterfaces;
using Ddxy.GrainInterfaces.Core;
using Ddxy.GrainInterfaces.Gate;
using Ddxy.Protocol;
using FreeSql;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;

namespace Ddxy.GameServer.Gate
{
    [StatelessWorker(5000)]
    [Reentrant]
    public class ApiGateGrain : Grain, IApiGateGrain
    {
        private readonly ILogger<ApiGateGrain> _logger;
        private readonly JwtOptions _jwtOptions;
        private readonly AppOptions _gameOptions;
        private readonly XinPayOptions _xinPayOptions;

        public ApiGateGrain(
            ILogger<ApiGateGrain> logger,
            IOptions<JwtOptions> jwtOptions,
            IOptions<AppOptions> gameOptions, 
            IOptions<XinPayOptions> xinPayOptions)
        {
            _logger = logger;
            _jwtOptions = jwtOptions.Value;
            _gameOptions = gameOptions.Value;
            _xinPayOptions = xinPayOptions.Value;
        }

        public async Task<bool> CheckUserToken(string ip, uint userId, string token, bool addExpireIfEquals = false)
        {
            var cachedToken = await RedisService.GetUserToken(userId);
            if (string.Equals(token, cachedToken))
            {
                if (addExpireIfEquals)
                    await RedisService.AddUserTokenExpire(userId);
                return true;
            }

            return false;
        }

        public async Task<Roles> QueryRoles(string ip, uint userId)
        {
            var all = await DbService.Sql.Queryable<RoleEntity>()
                .Where(it => it.UserId == userId)
                .ToListAsync(it => new { it.Id, it.ServerId });
            var cur = await DbService.QueryLastUseRoleId(userId);

            var roles = new Roles
            {
                Last = cur,
                All = new Dictionary<uint, uint>(all.Count)
            };
            foreach (var x in all)
            {
                roles.All.TryAdd(x.Id, x.ServerId);
            }

            return roles;
        }

        public async Task ReportError(string ip, Immutable<byte[]> payload)
        {
            if (payload.Value == null) return;
            if (CheckPayload(payload))
            {
                LogOp(ip, "report_error", payload);
                return;
            }
            var req = Json.Deserialize<ReportErrorReq>(payload.Value);
            var entity = new ErrorEntity
            {
                Uid = req.Uid,
                Rid = req.Rid,
                Error = req.Error,
                Remark = "",
                Status = ErrorStatus.Open,
                CreateTime = TimeUtil.TimeStamp
            };
            await DbService.InsertEntity(entity);
        }

        public async Task<Immutable<byte[]>> SignUp(string ip, string username, string password, string inviteCode, string platform, string deviceId, bool simulation, bool isRobot)
        {
            if (CheckPayload(username) || CheckPayload(password) || CheckPayload(inviteCode) || CheckPayload(platform) || CheckPayload(deviceId))
            {
                LogOp(ip, "signup", Json.Serialize(new { username, password, inviteCode }));
                return JsonResp.BadOperation().Serialize();
            }
            if (await RedisService.IsBandDeviceId(deviceId))
            {
                return JsonResp.NoPermission("权限不足，请联系管理员").Serialize();
            }
            // 检查用户名是否已存在
            var ret = await DbService.HasUser(username);
            if (ret)
                return JsonResp.Error(ErrCode.UserNameExists).Serialize();
            // 检查注册码
            if (string.IsNullOrWhiteSpace(inviteCode))
                return JsonResp.Error(ErrCode.InviteCodeNotFound).Serialize();
            inviteCode = inviteCode.Trim();
            var tempAdmin = await DbService.Sql.Queryable<AdminEntity>()
                .Where(it => it.InvitCode == inviteCode)
                .FirstAsync(it => new { it.Id, it.Category, it.Status });
            if (tempAdmin == null)
                return JsonResp.Error(ErrCode.InviteCodeNotFound).Serialize();
            if (tempAdmin.Status == AdminStatus.Frozen)
                return JsonResp.Error(ErrCode.InviteCodeNotFound).Serialize();
            // 平台用户parentId为0
            uint parentId = (((int)tempAdmin.Category > 2) ? tempAdmin.Id : 0);
            // 注册账号
            var entity = new UserEntity
            {
                UserName = username,
                Password = PasswordUtil.Encode(password, out var salt),
                PassSalt = salt,
                Status = UserStatus.Normal,
                Type = isRobot ? UserType.Robot : UserType.Normal,
                CreateTime = TimeUtil.TimeStamp,
                ParentId = parentId,
                Platform = platform,
                DeviceId = deviceId,
                Simulation = simulation,
                LastLoginIp = "",
                LastLoginTime = 0,
                LastUseRoleId = 0
            };
            // 判断是否插入成功
            ret = await DbService.InsertUser(entity);
            if (!ret)
                return JsonResp.DbError().Serialize();
            return JsonResp.Ok().Serialize();
        }

        public async Task<Immutable<byte[]>> SignIn(string ip, string username, string password, string platform, string deviceId, bool simulation)
        {
            if (CheckPayload(username) || CheckPayload(password) || CheckPayload(platform) || CheckPayload(deviceId))
            {
                LogOp(ip, "signin", Json.Serialize(new { username, password}));
                return JsonResp.BadOperation().Serialize();
            }
            if (await RedisService.IsBandDeviceId(deviceId))
            {
                return JsonResp.NoPermission("权限不足，请联系管理员").Serialize();
            }
            // 先通过用户名查找记录
            var entity = await DbService.QueryUser(username);
            if (entity == null)
                return JsonResp.Error(ErrCode.UserNotExists).Serialize();
            // 判断密码是否匹配
            var pass = PasswordUtil.Encode(password, entity.PassSalt);
            if (!string.Equals(pass, entity.Password))
                return JsonResp.Error(ErrCode.UserPassError).Serialize();
            // 判断是否被封禁
            if (entity.Status != UserStatus.Normal)
                return JsonResp.Error(ErrCode.UserFrozed).Serialize();
            // 更新登录时间和IP
            await DbService.Sql.Update<UserEntity>()
                .Where(it => it.Id == entity.Id)
                .Set(it => it.LastLoginTime, TimeUtil.TimeStamp)
                .SetIf(!string.IsNullOrWhiteSpace(ip), it => it.LastLoginIp, ip)
                .SetIf(!entity.Platform.Equals(platform), it => it.Platform, platform)
                .SetIf(!entity.DeviceId.Equals(deviceId), it => it.DeviceId, deviceId)
                .SetIf(entity.Simulation != simulation, it => it.Simulation, simulation)
                .ExecuteAffrowsAsync();
            // 构建token
            var token = TokenUtil.GenToken(_jwtOptions, new[]
            {
                new Claim(ClaimTypes.Sid, entity.Id.ToString())
            });
            // 存储到Redis, 后续ws连接时需要校验
            var ret = await RedisService.SetUserToken(entity.Id, token);
            if (!ret)
                return JsonResp.CacheError().Serialize();
            return JsonResp.Ok(entity, token).Serialize();
        }

        public async Task<Immutable<byte[]>> GetNotice()
        {
            var entity = await RedisService.GetNotice();
            return JsonResp.Ok(entity).Serialize();
        }

        public async Task<Immutable<byte[]>> ListServer(string ip, uint userId)
        {
            var servers = await DbService.Sql.Queryable<ServerEntity>()
                .Where(it => it.Status != ServerStatus.Dead)
                .ToListAsync();

            var roles = await DbService.Sql.Queryable<RoleEntity>()
                .Where(it => it.UserId == userId)
                .ToListAsync(it => new
                {
                    it.Id,
                    uid = it.UserId,
                    sid = it.ServerId,
                    it.Status,
                    it.Type,
                    nickname = it.NickName,
                    it.CfgId,
                    it.Sex,
                    it.Race,
                    it.Relive,
                    it.Level
                });

            var dic = new Dictionary<string, object>
            {
                ["servers"] = servers,
                ["roles"] = roles
            };
            return JsonResp.Ok(dic).Serialize();
        }

        public async Task<Immutable<byte[]>> CreateRole(string ip, uint userId, uint serverId, uint cfgId, string nickname)
        {
            if (CheckPayload(nickname))
            {
                LogOp(ip, "create_role", Json.Serialize(new { nickname}));
                return JsonResp.BadOperation().Serialize();
            }
            nickname = nickname.Trim();
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return JsonResp.Error(ErrCode.RoleNickNameExists).Serialize();
            }

            // 角色名
            if (!TextFilter.CheckLimitWord(nickname))
            {
                return JsonResp.Error(ErrCode.RoleNickNameExists).Serialize();
            }

            // 检测是否为脏词
            if (TextFilter.HasDirty(nickname))
            {
                return JsonResp.Error(ErrCode.RoleNickNameExists).Serialize();
            }

            // 检查区服是否存在
            var server = await DbService.QueryServer(serverId);
            if (server == null)
                return JsonResp.Error(ErrCode.ServerNotExists).Serialize();
            if (server.Status != ServerStatus.Normal)
                return JsonResp.Error(ErrCode.ServerNotValid).Serialize();
            // 检验当前区服是否已经有角色了
            var ret = await DbService.HasRole(userId, serverId);
            if (ret)
                return JsonResp.Error(ErrCode.UserHasRoleInServer).Serialize();
            // 检查角色名是否已存在
            ret = await DbService.HasRole(nickname);
            if (ret)
                return JsonResp.Error(ErrCode.RoleNickNameExists).Serialize();
            // 创角的时候，只能使用0转的角色
            var roles = ConfigService.GetRolesCanBeUsed(0);
            if (!roles.Contains(cfgId)) return JsonResp.Error(ErrCode.BadRequest).Serialize();
            if (!ConfigService.Roles.TryGetValue(cfgId, out var roleCfg))
                return JsonResp.Error(ErrCode.BadRequest).Serialize();
            // 获取用户信息
            var userEntity = await DbService.Sql.Queryable<UserEntity>().Where(it => it.Id == userId).FirstAsync(it =>
                new
                {
                    it.Id, it.Status, it.Type, it.ParentId
                });
            if (userEntity == null)
                return JsonResp.Error(ErrCode.UserNotExists).Serialize();
            if (userEntity.Status != UserStatus.Normal)
                return JsonResp.Error(ErrCode.UserFrozed).Serialize();

            // 创建角色
            var roleEntity = new RoleEntity
            {
                UserId = userId,
                ServerId = serverId,
                ParentId = userEntity.ParentId,
                Status = RoleStatus.Normal,
                Type = userEntity.Type,
                NickName = nickname,
                CfgId = cfgId,
                Sex = roleCfg.Sex,
                Race = roleCfg.Race,
                Relive = 0,
                Level = 1,
                Exp = 0,
                Silver = 0,
                Jade = 0,
                BindJade = 0,
                Contrib = 0,
                SldhGongJi = 0,
                GuoShi = 0,
                XlLevel = 0,

                Skins = "{\"has\":[],\"use\":[]}",
                Bianshen = "{\"cards\":{},\"wuxing\":{},\"current\":{\"id\":0,\"timestamp\":\"0\"}}",
                Xingzhen = "{\"unlocked\":{},\"used\":0}",
                Child = "",
                ExpExchangeTimes = 0,

                // 默认在东海渔村
                MapId = 1010,
                MapX = 155,
                MapY = 70,

                SectId = 0,
                SectContrib = 0,
                SectJob = 0,
                SectJoinTime = 0,

                Skills = "",

                Color1 = 0,
                Color2 = 0,

                Star = 0,
                Shane = 0,
                Relives = "",
                Rewards = "",
                Sldh = "",
                SinglePk = "",
                Flags = 0,
                AutoSkill = 0,
                AutoSyncSkill = true,

                TotalPay = 0,
                TotalPayRewards = "",
                TotalPayBS = 0,
                DailyPay = 0,
                DailyPayTime = 0,
                DailyPayRewards = "",
                SafeCode = "",
                SafeLocked = false,
                Spread = 0,
                SpreadTime = 0,

                Online = false,
                OnlineTime = 0,
                CreateTime = TimeUtil.TimeStamp
            };

            // 角色扩展信息
            var extEntity = new RoleExtEntity
            {
                RoleId = 0,
                Items = "",
                Repos = "",
                Mails = "",
                Tiance = "",
                QieGeLevel = 0,
                QieGeExp = 0,
                Done1stDraw = false,
            };

            // 默认给所有的道具
            if (_gameOptions.AllItems)
            {
                var items = new Dictionary<uint, uint>();
                foreach (var (k, v) in ConfigService.Items)
                {
                    var type = (ItemType)v.Type;
                    if (type != ItemType.ShenBing && type != ItemType.XianQi && type != ItemType.Pet)
                        items.Add(k, 500);
                }

                extEntity.Items = JsonConvert.SerializeObject(items);
            }

            // 属性方案, 为了方便测试，创建4套套装
            var schemes = new List<SchemeEntity>
            {
                new()
                {
                    Name = "属性方案1",
                    Equips = "",
                    Ornaments = "",
                    ApAttrs = "",
                    XlAttrs = "",
                    Relives = "",
                    Active = true,
                    CreateTime = TimeUtil.TimeStamp
                },
                new()
                {
                    Name = "属性方案2",
                    Equips = "",
                    Ornaments = "",
                    ApAttrs = "",
                    XlAttrs = "",
                    Relives = "",
                    Active = false,
                    CreateTime = TimeUtil.TimeStamp
                },
                new()
                {
                    Name = "属性方案3",
                    Equips = "",
                    Ornaments = "",
                    ApAttrs = "",
                    XlAttrs = "",
                    Relives = "",
                    Active = false,
                    CreateTime = TimeUtil.TimeStamp
                },
                new()
                {
                    Name = "属性方案4",
                    Equips = "",
                    Ornaments = "",
                    ApAttrs = "",
                    XlAttrs = "",
                    Relives = "",
                    Active = false,
                    CreateTime = TimeUtil.TimeStamp
                }
            };

            var taskEntity = new TaskEntity
            {
                Complets = "",
                States = "",
                DailyStart = "",
                DailyCnt = "",
                InstanceCnt = "",
                ActiveScore = "",
                BeenTake = "",
                StarNum = 0,
                MonkeyNum = 0,
                JinChanSongBaoNum = 0,
                EagleNum = 0,
                UpdateTime = 0,
                CreateTime = TimeUtil.TimeStamp
            };

            ret = await DbService.CreateRole(roleEntity, extEntity, schemes, taskEntity);
            if (!ret) return JsonResp.DbError().Serialize();

            // 返回角色简要信息
            return JsonResp.Ok(new
            {
                roleEntity.Id,
                roleEntity.UserId,
                roleEntity.ServerId,
                roleEntity.Status,
                roleEntity.CfgId,
                roleEntity.Sex,
                roleEntity.Race,
                roleEntity.NickName,
                roleEntity.Relive,
                roleEntity.Level
            }).Serialize();
        }

        public async Task<Immutable<byte[]>> EnterServer(string ip, uint userId, uint roleId)
        {
            var user = await DbService.Sql.Queryable<UserEntity>()
                .Where(it => it.Id == userId)
                .FirstAsync(it => new
                {
                    it.Status
                });
            if (user == null)
                return JsonResp.Error(ErrCode.UserNotExists).Serialize();
            if (user.Status != UserStatus.Normal)
                return JsonResp.Error(ErrCode.UserFrozed).Serialize();

            // 校验请求的角色id是否存在, 是否属于当前token中的用户
            var role = await DbService.QueryRole(roleId);
            if (role == null || role.UserId != userId)
                return JsonResp.Error(ErrCode.BadRequest).Serialize();
            if (role.Status == RoleStatus.Frozen)
                return JsonResp.Error(ErrCode.RoleFrozed).Serialize();

            // 检查区服是否存在
            var server = await DbService.QueryServer(role.ServerId);
            if (server == null)
                return JsonResp.Error(ErrCode.ServerNotExists).Serialize();
            if (server.Status != ServerStatus.Normal)
                return JsonResp.Error(ErrCode.ServerNotValid).Serialize();

            // 检查区服是否已激活
            var isActive = await GrainFactory.GetGrain<IServerGrain>(role.ServerId).CheckActive();
            if (!isActive) JsonResp.Error(ErrCode.ServerNotValid).Serialize();

            // 用户表记录使用的角色id, websocket连接的时候可以获取到
            await DbService.Sql.Update<UserEntity>()
                .Where(it => it.Id == userId)
                .Set(it => it.LastUseRoleId, roleId)
                .ExecuteAffrowsAsync();
            // 更新角色的在线时间
            await DbService.Sql.Update<RoleEntity>()
                .Where(it => it.Id == roleId)
                .Set(it => it.OnlineTime, TimeUtil.TimeStamp)
                .ExecuteAffrowsAsync();
            return JsonResp.Ok().Serialize();
        }

        public async Task<string> MYXinNotify2(string ip, string json)
        {
            if (CheckPayload(json))
            {
                LogOp(ip, "MYXinNotify2", Json.Serialize(new { json }));
                return "bad_op";
            }
            if (string.IsNullOrEmpty(json))
            {
                _logger.Info("json数据为空");
                return "json_error";
            }
            PayNotify req = Json.Deserialize<PayNotify>(json);
            _logger.Info("收到平台订单:{Json}", req.trade_no);
            _logger.Info("支付金额" + req.money);
            if (string.IsNullOrEmpty(req.trade_no))
            {
                //订单编号为空
                return "trade_no_error";
            }
            if (req.pid  <= 0)
            {
                //订单编号为空
                return "memberId_err";
            }
            if (string.IsNullOrEmpty(req.out_trade_no))
            {
                //订单编号为空
                return "out_trade_no_error";
            }
            uint ordermoney = (uint)Convert.ToDouble(req.money);
            //判读支付金额是否为空
            if (string.IsNullOrEmpty(req.money) || ordermoney <= 0)
            {
                return "money_err";
            }
            //签名验证
            string sign = XinPayUtil.SignNotify(req, _xinPayOptions.SignMd5Key);
            if (!string.Equals(req.sign, sign))
            {
                _logger.LogDebug("订单{CpOrderId} {SysOrderId}签名验证失败", req.out_trade_no, req.trade_no);
                return "sign_err";
            }
            //只做商户验证
            if (!req.pid.Equals(_xinPayOptions.MemberId))
            {
                return "memberId_err";
            }
            //这个是干嘛的？
            uint.TryParse(req.out_trade_no.Substring(_xinPayOptions.OrderPrefix.Length), out var cpOrderId);
            if (cpOrderId == 0)
            {
                _logger.Info("支付Substring截取失败" + req.out_trade_no);
                return "sdcustomno_error_wrong";
            }

            PayEntity entity = await (from it in DbService.Sql.Queryable<PayEntity>()
                                      where it.Id == cpOrderId
                                      select it).FirstAsync();
            if (entity == null)
            {
                _logger.LogWarning("订单{CpOrderId}不存在", req.out_trade_no);
                return "sdcustomno_error_not_exists";
            }
            if (entity.DelivTime != 0)
            {
                _logger.Info("订单{Id}已发货, 无需再次发货", entity.Id);
                return "already_done";
            }
            
            //多倍充值
            if (entity.Money != ordermoney)
            {
                _logger.Info("订单{Id}金额{Money}, 平台订单{SysOrderId}金额{Amount} 金额不匹配！", entity.Id, entity.Money, req.out_trade_no, Convert.ToUInt32(ordermoney));
                return "amount_not_matched";
            }
            if (req.trade_status != "TRADE_SUCCESS")
            {
                await (from it in DbService.Sql.Update<PayEntity>()
                       where it.Id == entity.Id
                       select it).Set((PayEntity it) => it.Order, req.out_trade_no).Set((PayEntity it) => it.Status, OrderStatus.Fail).Set((PayEntity it) => it.UpdateTime, TimeUtil.TimeStamp)
                    .ExecuteAffrowsAsync();
            }
            else
            {
                bool changeMoney = false;
                if (!_gameOptions.TestPay && ordermoney != entity.Money)
                {
                    changeMoney = true;
                    _logger.LogWarning("订单{Id}金额{Money}, 平台订单{SysOrderId}金额{Amount} 金额不匹配！", entity.Id, entity.Money, req.out_trade_no, Convert.ToUInt32(ordermoney));
                    uint payRate = await RedisService.GetPayRateJade();
                    entity.Money = Convert.ToUInt32(ordermoney);
                    entity.BindJade = 0;
                    entity.Jade = Convert.ToUInt32(ordermoney) * payRate;
                }
                using IRepositoryUnitOfWork uow = DbService.Sql.CreateUnitOfWork();
                if (await (from it in uow.Orm.Update<PayEntity>()
                           where it.Id == entity.Id
                           select it)
                           .Set((PayEntity it) => it.Order, req.out_trade_no)
                           .Set((PayEntity it) => it.Status, OrderStatus.Success)
                           .Set((PayEntity it) => it.UpdateTime, TimeUtil.TimeStamp)
                           .Set((PayEntity it) => it.DelivTime, TimeUtil.TimeStamp)
                           .SetIf(changeMoney, (PayEntity it) => it.Money, entity.Money)
                           .SetIf(changeMoney, (PayEntity it) => it.Jade, entity.Jade)
                           .SetIf(changeMoney, (PayEntity it) => it.BindJade, entity.BindJade)
                           .ExecuteAffrowsAsync() == 0)
                {
                    uow.Rollback();
                    return "err";
                }
                try
                {
                    if (await base.GrainFactory.GetGrain<IPlayerGrain>(entity.Rid).OnPayed((int)entity.Money, (int)entity.Jade) == 0)
                    {
                        uow.Rollback();
                        return "err";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "=============={Msg}======", ex.Message);
                    uow.Rollback();
                    return "err";
                }
                uow.Commit();
            }
            return "success";
        }
        //这是我的方法 积分充值
        public async Task<string> MYXinNotifyBindJade(string ip, string json)
        {
            if (CheckPayload(json))
            {
                LogOp(ip, "MYXinNotifyBindJade", Json.Serialize(new { json }));
                return "bad_op";
            }
            if (string.IsNullOrEmpty(json))
            {
                _logger.Info("json数据为空");
                return "json_error";
            }
            PayNotify req = Json.Deserialize<PayNotify>(json);
            _logger.Info("收到平台订单:{Json}", req.trade_no);
            _logger.Info("支付金额" + req.money);
            if (string.IsNullOrEmpty(req.trade_no))
            {
                //订单编号为空
                return "trade_no_error";
            }
            if (req.pid  <= 0)
            {
                //订单编号为空
                return "memberId_err";
            }
            if (string.IsNullOrEmpty(req.out_trade_no))
            {
                //订单编号为空
                return "out_trade_no_error";
            }
            uint ordermoney = (uint)Convert.ToDouble(req.money);
            //判读支付金额是否为空
            if (string.IsNullOrEmpty(req.money) || ordermoney <= 0)
            {
                return "money_err";
            }
            //签名验证
            string sign = XinPayUtil.SignNotify(req, _xinPayOptions.SignMd5Key);
            if (!string.Equals(req.sign, sign))
            {
                _logger.LogDebug("订单{CpOrderId} {SysOrderId}签名验证失败", req.out_trade_no, req.trade_no);
                return "sign_err";
            }
            //只做商户验证
            if (!req.pid.Equals(_xinPayOptions.MemberId))
            {
                return "customerid_err";
            }
            //这个是干嘛的？
            uint.TryParse(req.out_trade_no.Substring(_xinPayOptions.OrderPrefix.Length), out var cpOrderId);
            if (cpOrderId == 0)
            {
                _logger.Info("支付Substring截取失败" + req.out_trade_no);
                return "sdcustomno_error_wrong";
            }

            PayEntity entity = await (from it in DbService.Sql.Queryable<PayEntity>()
                                      where it.Id == cpOrderId
                                      select it).FirstAsync();
            if (entity == null)
            {
                _logger.LogWarning("订单{CpOrderId}不存在", req.out_trade_no);
                return "sdcustomno_error_not_exists";
            }
            if (entity.DelivTime != 0)
            {
                _logger.Info("订单{Id}已发货, 无需再次发货", entity.Id);
                return "alread_done";
            }
            //多倍充值
            if (entity.Money != ordermoney)
            {
                _logger.Info("订单{Id}金额{Money}, 平台订单{SysOrderId}金额{Amount} 金额不匹配！", entity.Id, entity.Money, req.out_trade_no, Convert.ToUInt32(ordermoney));
                return "amount_not_matched";
            }
            if (req.trade_status != "TRADE_SUCCESS")
            {
                await (from it in DbService.Sql.Update<PayEntity>()
                       where it.Id == entity.Id
                       select it).Set((PayEntity it) => it.Order, req.out_trade_no).Set((PayEntity it) => it.Status, OrderStatus.Fail).Set((PayEntity it) => it.UpdateTime, TimeUtil.TimeStamp)
                    .ExecuteAffrowsAsync();
            }
            else
            {
                var isItemShopPay = !(string.IsNullOrWhiteSpace(entity.Remark) || string.IsNullOrEmpty(entity.Remark));
                bool changeMoney = false;
                if (!_gameOptions.TestPay && ordermoney != entity.Money)
                {
                    changeMoney = true;
                    _logger.LogWarning("订单{Id}金额{Money}, 平台订单{SysOrderId}金额{Amount} 金额不匹配！", entity.Id, entity.Money, req.out_trade_no, Convert.ToUInt32(ordermoney));
                    uint payRate = await RedisService.GetPayRateBindJade();
                    entity.Money = Convert.ToUInt32(ordermoney);
                    entity.Jade = 0;
                    // 物品购买
                    if (isItemShopPay) {
                        entity.BindJade = 0;
                    }
                    // 积分充值
                    else {
                        entity.BindJade = Convert.ToUInt32(ordermoney) * payRate;
                    }
                }
                using IRepositoryUnitOfWork uow = DbService.Sql.CreateUnitOfWork();
                if (await (from it in uow.Orm.Update<PayEntity>()
                           where it.Id == entity.Id
                           select it)
                           .Set((PayEntity it) => it.Order, req.out_trade_no)
                           .Set((PayEntity it) => it.Status, OrderStatus.Success)
                           .Set((PayEntity it) => it.UpdateTime, TimeUtil.TimeStamp)
                           .Set((PayEntity it) => it.DelivTime, TimeUtil.TimeStamp)
                           .SetIf(changeMoney, (PayEntity it) => it.Money, entity.Money)
                           .SetIf(changeMoney, (PayEntity it) => it.Jade, entity.Jade)
                           .SetIf(changeMoney, (PayEntity it) => it.BindJade, entity.BindJade)
                           .ExecuteAffrowsAsync() == 0)
                {
                    uow.Rollback();
                    return "err";
                }
                try
                {
                    // 物品购买
                    if (isItemShopPay)
                    {
                        var remark = Json.SafeDeserialize<PayRemark>(entity.Remark);
                        JItemShopGood item = null;
                        if (remark != null && !string.IsNullOrEmpty(remark.content) && remark.type == 100)
                        {
                            item = Json.SafeDeserialize<JItemShopGood>(remark.content);
                        }
                        if (item == null || item.price <= 0)
                        {
                            uow.Rollback();
                            return "err";
                        }
                        if (await base.GrainFactory.GetGrain<IPlayerGrain>(entity.Rid).OnPayedItem(item.item, item.num, (int)entity.Money) == 0)
                        {
                            uow.Rollback();
                            return "err";
                        }
                    }
                    // 积分充值
                    else
                    {
                        if (await base.GrainFactory.GetGrain<IPlayerGrain>(entity.Rid).OnPayedBindJade((int)entity.Money, (int)entity.BindJade) == 0)
                        {
                            uow.Rollback();
                            return "err";
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "=============={Msg}======", ex.Message);
                    uow.Rollback();
                    return "err";
                }
                uow.Commit();
            }
            return "success";
        }

        public async Task<string> YunDingNotify(string ip, string json)
        {
            if (CheckPayload(json))
            {
                LogOp(ip, "YunDingNotify", Json.Serialize(new { json }));
                return "bad_op";
            }
            if (string.IsNullOrEmpty(json))
            {
                _logger.Info("json数据为空");
                return "json_error";
            }
            YunDingPayNotify req = Json.Deserialize<YunDingPayNotify>(json);
            _logger.Info("收到平台订单:{Json}", req.trade_no);
            _logger.Info("支付金额" + req.money);
            if (string.IsNullOrEmpty(req.trade_no))
            {
                //订单编号为空
                return "trade_no_error";
            }
            if (req.pid  <= 0)
            {
                //订单编号为空
                return "pid_err";
            }
            if (string.IsNullOrEmpty(req.out_trade_no))
            {
                //订单编号为空
                return "out_trade_no_error";
            }
            uint ordermoney = (uint)Convert.ToDouble(req.money);
            //判读支付金额是否为空
            if (string.IsNullOrEmpty(req.money) || ordermoney <= 0)
            {
                return "money_err";
            }
            //签名验证
            string sign = XinPayUtil.SignYunDingNotify(req, _xinPayOptions.SignMd5Key);
            if (!string.Equals(req.sign, sign))
            {
                _logger.LogDebug("订单{CpOrderId} {SysOrderId}签名验证失败", req.out_trade_no, req.trade_no);
                return "sign_err";
            }
            //只做商户验证
            if (!req.pid.Equals(Convert.ToInt32(_xinPayOptions.MemberId)))
            {
                return "memberId_err";
            }
            //这个是干嘛的？
            uint.TryParse(req.out_trade_no.Substring(_xinPayOptions.OrderPrefix.Length), out var cpOrderId);
            if (cpOrderId == 0)
            {
                _logger.Info("支付Substring截取失败" + req.out_trade_no);
                return "sdcustomno_error_wrong";
            }

            PayEntity entity = await (from it in DbService.Sql.Queryable<PayEntity>()
                                      where it.Id == cpOrderId
                                      select it).FirstAsync();
            if (entity == null)
            {
                _logger.LogWarning("订单{CpOrderId}不存在", req.out_trade_no);
                return "sdcustomno_error_not_exists";
            }
            if (entity.DelivTime != 0)
            {
                _logger.Info("订单{Id}已发货, 无需再次发货", entity.Id);
                return "already_done";
            }
            
            //多倍充值
            if (entity.Money != ordermoney)
            {
                _logger.Info("订单{Id}金额{Money}, 平台订单{SysOrderId}金额{Amount} 金额不匹配！", entity.Id, entity.Money, req.out_trade_no, Convert.ToUInt32(ordermoney));
                return "amount_not_matched";
            }
            if (req.trade_status != "TRADE_SUCCESS")
            {
                await (from it in DbService.Sql.Update<PayEntity>()
                       where it.Id == entity.Id
                       select it).Set((PayEntity it) => it.Order, req.out_trade_no).Set((PayEntity it) => it.Status, OrderStatus.Fail).Set((PayEntity it) => it.UpdateTime, TimeUtil.TimeStamp)
                    .ExecuteAffrowsAsync();
            }
            else
            {
                bool changeMoney = false;
                if (!_gameOptions.TestPay && ordermoney != entity.Money)
                {
                    changeMoney = true;
                    _logger.LogWarning("订单{Id}金额{Money}, 平台订单{SysOrderId}金额{Amount} 金额不匹配！", entity.Id, entity.Money, req.out_trade_no, Convert.ToUInt32(ordermoney));
                    uint payRate = await RedisService.GetPayRateJade();
                    entity.Money = Convert.ToUInt32(ordermoney);
                    entity.BindJade = 0;
                    entity.Jade = Convert.ToUInt32(ordermoney) * payRate;
                }
                using IRepositoryUnitOfWork uow = DbService.Sql.CreateUnitOfWork();
                if (await (from it in uow.Orm.Update<PayEntity>()
                           where it.Id == entity.Id
                           select it)
                           .Set((PayEntity it) => it.Order, req.out_trade_no)
                           .Set((PayEntity it) => it.Status, OrderStatus.Success)
                           .Set((PayEntity it) => it.UpdateTime, TimeUtil.TimeStamp)
                           .Set((PayEntity it) => it.DelivTime, TimeUtil.TimeStamp)
                           .SetIf(changeMoney, (PayEntity it) => it.Money, entity.Money)
                           .SetIf(changeMoney, (PayEntity it) => it.Jade, entity.Jade)
                           .SetIf(changeMoney, (PayEntity it) => it.BindJade, entity.BindJade)
                           .ExecuteAffrowsAsync() == 0)
                {
                    uow.Rollback();
                    return "err";
                }
                try
                {
                    if (await base.GrainFactory.GetGrain<IPlayerGrain>(entity.Rid).OnPayed((int)entity.Money, (int)entity.Jade) == 0)
                    {
                        uow.Rollback();
                        return "err";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "=============={Msg}======", ex.Message);
                    uow.Rollback();
                    return "err";
                }
                uow.Commit();
            }
            return "success";
        }
        //这是我的方法 积分充值
        public async Task<string> YunDingNotifyBindJade(string ip, string json)
        {
            if (CheckPayload(json))
            {
                LogOp(ip, "YunDingNotifyBindJade", Json.Serialize(new { json }));
                return "bad_op";
            }
            if (string.IsNullOrEmpty(json))
            {
                _logger.Info("json数据为空");
                return "json_error";
            }
            YunDingPayNotify req = Json.Deserialize<YunDingPayNotify>(json);
            _logger.Info("收到平台订单:{Json}", req.trade_no);
            _logger.Info("支付金额" + req.money);
            if (string.IsNullOrEmpty(req.trade_no))
            {
                //订单编号为空
                return "trade_no_error";
            }
            if (req.pid  <= 0)
            {
                //订单编号为空
                return "memberId_err";
            }
            if (string.IsNullOrEmpty(req.out_trade_no))
            {
                //订单编号为空
                return "out_trade_no_error";
            }
            uint ordermoney = (uint)Convert.ToDouble(req.money);
            //判读支付金额是否为空
            if (string.IsNullOrEmpty(req.money) || ordermoney <= 0)
            {
                return "money_err";
            }
            //签名验证
            string sign = XinPayUtil.SignYunDingNotify(req, _xinPayOptions.SignMd5Key);
            if (!string.Equals(req.sign, sign))
            {
                _logger.LogDebug("订单{CpOrderId} {SysOrderId}签名验证失败", req.out_trade_no, req.trade_no);
                return "sign_err";
            }
            //只做商户验证
            if (!req.pid.Equals(Convert.ToInt32(_xinPayOptions.MemberId)))
            {
                return "customerid_err";
            }
            //这个是干嘛的？
            uint.TryParse(req.out_trade_no.Substring(_xinPayOptions.OrderPrefix.Length), out var cpOrderId);
            if (cpOrderId == 0)
            {
                _logger.Info("支付Substring截取失败" + req.out_trade_no);
                return "sdcustomno_error_wrong";
            }

            PayEntity entity = await (from it in DbService.Sql.Queryable<PayEntity>()
                                      where it.Id == cpOrderId
                                      select it).FirstAsync();
            if (entity == null)
            {
                _logger.LogWarning("订单{CpOrderId}不存在", req.out_trade_no);
                return "sdcustomno_error_not_exists";
            }
            if (entity.DelivTime != 0)
            {
                _logger.Info("订单{Id}已发货, 无需再次发货", entity.Id);
                return "alread_done";
            }
            //多倍充值
            if (entity.Money != ordermoney)
            {
                _logger.Info("订单{Id}金额{Money}, 平台订单{SysOrderId}金额{Amount} 金额不匹配！", entity.Id, entity.Money, req.out_trade_no, Convert.ToUInt32(ordermoney));
                return "amount_not_matched";
            }
            if (req.trade_status != "TRADE_SUCCESS")
            {
                await (from it in DbService.Sql.Update<PayEntity>()
                       where it.Id == entity.Id
                       select it).Set((PayEntity it) => it.Order, req.out_trade_no).Set((PayEntity it) => it.Status, OrderStatus.Fail).Set((PayEntity it) => it.UpdateTime, TimeUtil.TimeStamp)
                    .ExecuteAffrowsAsync();
            }
            else
            {
                var isItemShopPay = !(string.IsNullOrWhiteSpace(entity.Remark) || string.IsNullOrEmpty(entity.Remark));
                bool changeMoney = false;
                if (!_gameOptions.TestPay && ordermoney != entity.Money)
                {
                    changeMoney = true;
                    _logger.LogWarning("订单{Id}金额{Money}, 平台订单{SysOrderId}金额{Amount} 金额不匹配！", entity.Id, entity.Money, req.out_trade_no, Convert.ToUInt32(ordermoney));
                    uint payRate = await RedisService.GetPayRateBindJade();
                    entity.Money = Convert.ToUInt32(ordermoney);
                    entity.Jade = 0;
                    // 物品购买
                    if (isItemShopPay) {
                        entity.BindJade = 0;
                    }
                    // 积分充值
                    else {
                        entity.BindJade = Convert.ToUInt32(ordermoney) * payRate;
                    }
                }
                using IRepositoryUnitOfWork uow = DbService.Sql.CreateUnitOfWork();
                if (await (from it in uow.Orm.Update<PayEntity>()
                           where it.Id == entity.Id
                           select it)
                           .Set((PayEntity it) => it.Order, req.out_trade_no)
                           .Set((PayEntity it) => it.Status, OrderStatus.Success)
                           .Set((PayEntity it) => it.UpdateTime, TimeUtil.TimeStamp)
                           .Set((PayEntity it) => it.DelivTime, TimeUtil.TimeStamp)
                           .SetIf(changeMoney, (PayEntity it) => it.Money, entity.Money)
                           .SetIf(changeMoney, (PayEntity it) => it.Jade, entity.Jade)
                           .SetIf(changeMoney, (PayEntity it) => it.BindJade, entity.BindJade)
                           .ExecuteAffrowsAsync() == 0)
                {
                    uow.Rollback();
                    return "err";
                }
                try
                {
                    // 物品购买
                    if (isItemShopPay)
                    {
                        var remark = Json.SafeDeserialize<PayRemark>(entity.Remark);
                        JItemShopGood item = null;
                        if (remark != null && !string.IsNullOrEmpty(remark.content) && remark.type == 100)
                        {
                            item = Json.SafeDeserialize<JItemShopGood>(remark.content);
                        }
                        if (item == null || item.price <= 0)
                        {
                            uow.Rollback();
                            return "err";
                        }
                        if (await base.GrainFactory.GetGrain<IPlayerGrain>(entity.Rid).OnPayedItem(item.item, item.num, (int)entity.Money) == 0)
                        {
                            uow.Rollback();
                            return "err";
                        }
                    }
                    // 积分充值
                    else
                    {
                        if (await base.GrainFactory.GetGrain<IPlayerGrain>(entity.Rid).OnPayedBindJade((int)entity.Money, (int)entity.BindJade) == 0)
                        {
                            uow.Rollback();
                            return "err";
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "=============={Msg}======", ex.Message);
                    uow.Rollback();
                    return "err";
                }
                uow.Commit();
            }
            return "success";
        }

        private static bool CheckPayload(string payload)
        {
            if (string.IsNullOrEmpty(payload)) return false;
            string[] SqlStr = { "and ", "exec ", "insert ", "select ", "delete ", "update ", "count ", "chr ", "mid ", "master ", "truncate ", "char ", "declare " };
            foreach (string s in SqlStr)
            {
                if (payload.IndexOf(s) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CheckPayload(Immutable<byte[]> payload)
        {
            if (payload.Value == null) return false;
            return CheckPayload(System.Text.Encoding.UTF8.GetString(payload.Value));
        }

        private void LogOp(string ip, string method, string args)
        {
            if (string.IsNullOrEmpty(args)) args = "{}";
            _logger.LogDebug($"{ip}|{method}|{args}");
        }
        private void LogOp(string ip, string method, Immutable<byte[]> args)
        {
            LogOp(ip, method, args.Value == null ? null : System.Text.Encoding.UTF8.GetString(args.Value));
        }
    }
}