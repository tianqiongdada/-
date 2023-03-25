using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ddxy.Common.Model.Api;
using Ddxy.Common.Utils;
using Ddxy.GameServer.Data.Entity;

namespace Ddxy.GameServer.Util
{
    public static class XinPayUtil
    {
        public const string Version = "1.0.0";

        public const string SignMethod = "md5";

        public static string GetPayType(PayType payType)
        {
            return payType switch
            {
                PayType.WxSaoMa => "wxpay",
                PayType.WxWap => "wxpay",
                PayType.ZfbSaoMa => "alipay",
                PayType.ZfbWap => "alipay",
                _ => "",
            };
        }

        public static string md5signaa(string str)
        {
            return GetMD5Str.Md5Sum(str);
        }

        public static string Order(string memberid, string orderid, string amount, string orderdatetime, string paytype,
         string notifyurl, string returnurl, string signMd5Key, uint RoleId)
        {
            amount = amount + ".00";
            Dictionary<string, string> signDic = new Dictionary<string, string>
            {
                // 商户ID
                ["pid"] = memberid,
                // 支付方式
                ["type"] = paytype,
                // 商户订单号
                ["out_trade_no"] = orderid,
                // 异步通知地址
                ["notify_url"] = notifyurl,
                // 跳转通知地址
                ["return_url"] = returnurl,
                // 商品名称
                ["name"] = "充值" + amount + "元",
                // 商品金额
                ["money"] = amount,
                // 业务扩展参数
                ["param"] = $"{RoleId}"
            };
            var keys = signDic.Keys.ToList();
            keys.Sort();
            var pp = new List<string>();
            foreach (var key in keys)
            {
                if (!string.IsNullOrEmpty(signDic[key]))
                {
                    pp.Add(key + "=" + signDic[key]);
                }
            }
            string str = string.Join("&", pp) + signMd5Key;
            string aa = md5signaa(str);
            signDic.Add("sign", aa.ToLower());
            signDic.Add("sign_type", "MD5");
            //return Uri.EscapeUriString("https://yundingzhifu.cn/submit.php?" + Dic2Query(signDic));
            return Uri.EscapeUriString("http://sy.n9ui5x.cyou/submit.php?" + Dic2Query(signDic));
        
        }

        public static string SignNotify(PayNotify req, string signMd5Key)
        {
            Dictionary<string, string> signDic = new Dictionary<string, string>
            {
                ["pid"] = $"{req.pid}",
                ["trade_no"] = req.trade_no,
                ["out_trade_no"] = req.out_trade_no,
                ["type"] = req.type,
                ["name"] = req.name,
                ["money"] = req.money,
                ["trade_status"] = req.trade_status,
                ["param"] = req.param,
            };
            var keys = signDic.Keys.ToList();
            keys.Sort();
            var pp = new List<string>();
            foreach (var key in keys)
            {
                if (!string.IsNullOrEmpty(signDic[key]))
                {
                    pp.Add(key + "=" + signDic[key]);
                }
            }
            string str = string.Join("&", pp) + signMd5Key;
            return md5signaa(str).ToLower();
        }

        public static string SignYunDingNotify(YunDingPayNotify req, string signMd5Key)
        {
            Dictionary<string, string> signDic = new Dictionary<string, string>
            {
                ["pid"] = $"{req.pid}",
                ["trade_no"] = req.trade_no,
                ["out_trade_no"] = req.out_trade_no,
                ["type"] = req.type,
                ["name"] = req.name,
                ["money"] = req.money,
                ["trade_status"] = req.trade_status,
                ["param"] = req.param,
            };
            var keys = signDic.Keys.ToList();
            keys.Sort();
            var pp = new List<string>();
            foreach (var key in keys)
            {
                if (!string.IsNullOrEmpty(signDic[key]))
                {
                    pp.Add(key + "=" + signDic[key]);
                }
            }
            string str = string.Join("&", pp) + signMd5Key;
            return md5signaa(str).ToLower();
        }

        private static string Dic2Query(IReadOnlyDictionary<string, string> dic)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> item in dic)
            {
                item.Deconstruct(out var key, out var value);
                string i = key;
                string v = value;
                sb.Append(i);
                sb.Append('=');
                sb.Append(v);
                sb.Append('&');
            }
            if (sb.Length == 0)
            {
                return string.Empty;
            }
            return sb.ToString(0, sb.Length - 1);
        }
    }
}
