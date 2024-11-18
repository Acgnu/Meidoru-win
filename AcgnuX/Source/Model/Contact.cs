using AcgnuX.Source.Bussiness.Constants;

namespace AcgnuX.Source.Model
{
    /// <summary>
    /// 联系人
    /// </summary>
    public class Contact
    {
        /// <summary>
        /// 数据ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 联系人平台 
        /// </summary>
        public ContactPlatform Platform { get; set; }
        /// <summary>
        /// 联系人名称/备注名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 联系人UID
        /// </summary>
        public string Uid { get; set; }
        /// <summary>
        /// 联系人手机号
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 联系人头像/二维码
        /// </summary>
        public byte[] Avatar { get; set; }
    }
}
