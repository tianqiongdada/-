using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Ddxy.Common.Model.Api
{
    [Serializable]
    public class PayNotify
    {
        // 商户ID	pid	是	Int	1001	
        // 易支付订单号	trade_no	是	String	20160806151343349021	核易云付订单号
        // 商户订单号	out_trade_no	是	String	20160806151343349	商户系统内部的订单号
        // 支付方式	type	是	String	alipay	支付方式列表
        // 商品名称	name	是	String	VIP会员	
        // 商品金额	money	是	String	1.00	
        // 支付状态	trade_status	是	String	TRADE_SUCCESS	只有TRADE_SUCCESS是成功
        // 业务扩展参数	param	否	String		
        // 签名字符串	sign	是	String	202cb962ac59075b964b07152d234b70	签名算法与支付宝签名算法相同
        // 签名类型	sign_type	是	String	MD5	默认为MD5
        public int pid { get; set; }
        public string trade_no { get; set; }
        public string out_trade_no { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string money { get; set; }
        public string trade_status { get; set; }
        public string param { get; set; }
        public string sign { get; set; }
        public string sign_type { get; set; }
    }

    [Serializable]
    public class YunDingPayNotify
    {
        /*
        商户ID          pid             是      Int         1001                                
        易支付订单号    trade_no        是      String      20160806151343349021                云鼎支付订单号
        商户订单号      out_trade_no    是      String      20160806151343349                   商户系统内部的订单号
        支付方式        type            是      String      alipay                              支付方式列表
        商品名称        name            是      String      VIP会员                             
        商品金额        money           是      String      1.00                                
        支付状态        trade_status    是      String      TRADE_SUCCESS                       只有TRADE_SUCCESS是成功
        业务扩展参数    param           否      String                                          
        签名字符串      sign            是      String      202cb962ac59075b964b07152d234b70    签名算法点此查看
        签名类型        sign_type       是      String      MD5                                 默认为MD5
        */
        public int pid { get; set; }
        public string trade_no { get; set; }
        public string out_trade_no { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string money { get; set; }
        public string trade_status { get; set; }
        public string param { get; set; }
        public string sign { get; set; }
        public string sign_type { get; set; }
    }
}
