using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Ddxy.Common.Model.Api;
using Ddxy.GateServer.Extensions;
using Ddxy.GateServer.Network;
using Ddxy.GateServer.Util;
using Ddxy.GrainInterfaces.Gate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans.Concurrency;

namespace Ddxy.GateServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class ApiController : ControllerBase
    {
        private readonly SiloClient _siloClient;

        public ApiController(SiloClient siloClient)
        {
            _siloClient = siloClient;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("report_error")]
        public async Task ReportError([FromBody] ReportErrorReq req)
        {
            if (!TryFindApiGateGrain(out var grain)) return;
            var payload = new Immutable<byte[]>(Json.SerializeToBytes(req));
            await grain.ReportError(this.GetIp(), payload);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("get_notice")]
        public async Task GetNotice()
        {
            if (!TryFindApiGateGrain(out var grain)) return;
            var result = await grain.GetNotice();
            await Response.Body.WriteAsync(result.Value);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("signup")]
        public async Task Regist([FromBody] SignUpReq req)
        {
            if (!TryFindApiGateGrain(out var grain)) return;
            var result = await grain.SignUp(this.GetIp(), req.UserName, req.Password, req.InviteCode, req.Platform, req.DeviceId, req.Simulation, req.IsRobot);
            await Response.Body.WriteAsync(result.Value);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("signin")]
        public async Task Login([FromBody] SignInReq req)
        {
            if (!TryFindApiGateGrain(out var grain)) return;
            var result = await grain.SignIn(this.GetIp(), req.UserName, req.Password, req.Platform, req.DeviceId, req.Simulation);
            await Response.Body.WriteAsync(result.Value);
        }

        // [HttpPost]
        // [Route("signout")]
        // public async Task Logout()
        // {
        //     if (!this.CheckGameUser(out var info)) return;
        //     if (!TryFindApiGateGrain(out var grain)) return;
        //     _ = grain.SignOut(info.Id);
        //     await Task.CompletedTask;
        // }

        [HttpPost]
        [Route("create_role")]
        public async Task CreateRole([FromBody] CreateRoleReq req)
        {
            if (!this.CheckGameUser(out var info)) return;
            if (!TryFindApiGateGrain(out var grain)) return;
            var result = await grain.CreateRole(this.GetIp(), info.Id, req.ServerId.GetValueOrDefault(),
                req.CfgId.GetValueOrDefault(), req.Nickname);
            await Response.Body.WriteAsync(result.Value);
        }

        [HttpGet]
        [Route("list_server")]
        public async Task ListServer()
        {
            if (!this.CheckGameUser(out var info)) return;
            if (!TryFindApiGateGrain(out var grain)) return;
            var result = await grain.ListServer(this.GetIp(), info.Id);
            await Response.Body.WriteAsync(result.Value);
        }

        [HttpPost]
        [Route("enter_server")]
        public async Task EnterServer([Required] [FromQuery] uint? roleId)
        {
            if (!this.CheckGameUser(out var info)) return;
            if (!TryFindApiGateGrain(out var grain)) return;
            var result = await grain.EnterServer(this.GetIp(), info.Id, roleId.GetValueOrDefault());
            await Response.Body.WriteAsync(result.Value);
        }

        [HttpGet]
        [Route("xin_notify2")]
        [AllowAnonymous]
        public async Task<IActionResult> XinNotify2(PayNotify req)
        {
            if (TryFindApiGateGrain(out var grain))
            {
                string ret = await grain.MYXinNotify2(this.GetIp(), Json.Serialize(req));
                return await Task.Run(() =>
                {
                    return Content(ret);
                });
            }
            else
            {
                return await Task.Run(() =>
                {
                    return Content("错误偶错位教哦放假哦i发生纠纷撒豆");
                });
            }
        }

        [HttpGet]
        [Route("xin_notify_bind_jade")]
        [AllowAnonymous]
        public async Task<IActionResult> XinNotifyBindJade(PayNotify req)
        {
            if (TryFindApiGateGrain(out var grain))
            {
                string ret = await grain.MYXinNotifyBindJade(this.GetIp(), Json.Serialize(req));
                return await Task.Run(() =>
                {
                    return Content(ret);
                });
            }
            else
            {
                return await Task.Run(() =>
                {
                    return Content("错误偶错位教哦放假哦i发生纠纷撒豆");
                });
            }
        }

        [HttpGet]
        [Route("yunding_notify")]
        [AllowAnonymous]
        public async Task<IActionResult> YunDingNotify(YunDingPayNotify req)
        {
            if (TryFindApiGateGrain(out var grain))
            {
                string ret = await grain.YunDingNotify(this.GetIp(), Json.Serialize(req));
                return await Task.Run(() =>
                {
                    return Content(ret);
                });
            }
            else
            {
                return await Task.Run(() =>
                {
                    return Content("错误偶错位教哦放假哦i发生纠纷撒豆");
                });
            }
        }

        [HttpGet]
        [Route("yunding_notify_bind_jade")]
        [AllowAnonymous]
        public async Task<IActionResult> YunDingNotifyBindJade(YunDingPayNotify req)
        {
            if (TryFindApiGateGrain(out var grain))
            {
                string ret = await grain.YunDingNotifyBindJade(this.GetIp(), Json.Serialize(req));
                return await Task.Run(() =>
                {
                    return Content(ret);
                });
            }
            else
            {
                return await Task.Run(() =>
                {
                    return Content("错误偶错位教哦放假哦i发生纠纷撒豆");
                });
            }
        }

        private bool TryFindApiGateGrain(out IApiGateGrain grain)
        {
            grain = _siloClient.GetGrain<IApiGateGrain>(0);
            if (grain == null)
            {
                Response.StatusCode = (int) HttpStatusCode.ServiceUnavailable;
                return false;
            }

            return true;
        }
    }
}