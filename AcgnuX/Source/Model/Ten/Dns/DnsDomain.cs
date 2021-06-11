namespace AcgnuX.Source.Model.Ten.Dns
{
    public class DnsDomain
    {
        public string id { get; set; }
        public string name { get; set; }
        //            punycode	String	punycode编码后的域名
        //        grade	String	域名的等级
        //            owner	String	域名所有者的邮箱帐号
        //        ext_status	String	域名扩展的状态信息，"notexist"、"dnserror"、"" 分别代表 "域名未注册"、"DNS 设置错误"、"正常"
        //        ttl	Int	域名下的解析记录默认的 TTL 值
        //            min_ttl	Int	当前域名允许的最小的 TTL
        //            dnspod_ns	Array	域名应该设置的 NS 地址
        //        status	String	域名的状态，"enable"、"pause"、"spam"、"lock" 分别代表 "正常"、"暂停解析"、"已被封禁"、"已锁定"
        //        q_project_id	Int	域名所在项目的 ID
    }
}