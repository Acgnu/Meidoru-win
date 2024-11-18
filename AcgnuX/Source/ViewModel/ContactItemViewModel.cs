using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace AcgnuX.Source.ViewModel
{
    public class ContactItemViewModel : ObservableObject
    {
        /// <summary>
        /// 数据ID
        /// </summary>
        public int? Id { get; set; }
        /// <summary>
        /// 联系人平台 
        /// </summary>
        private ContactPlatform platform = ContactPlatform.QQ;
        public ContactPlatform Platform { get => platform; set => SetProperty(ref platform, value); }
        /// <summary>
        /// 联系人名称/备注名称
        /// </summary>
        private string name;
        public string Name { get => name; set => SetProperty(ref name, value); }
        /// <summary>
        /// 联系人UID
        /// </summary>
        private string uid;
        public string Uid { get => uid; set => SetProperty(ref uid, value); }
        /// <summary>
        /// 联系人手机号
        /// </summary>
        private string phone;
        public string Phone { get => phone; set => SetProperty(ref phone, value); }
        /// <summary>
        /// 联系人头像/二维码
        /// </summary>
        private ByteArray avatar;
        public ByteArray Avatar { get => avatar; set => SetProperty(ref avatar, value); }
        //临时头像
        public ByteArray TempAvatar { get; set; }

        /// <summary>
        /// True if this item is currently selected
        /// </summary>
        private bool isSelected = false;
        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }

        private readonly ContactRepo _ContactRepo = App.Current.Services.GetService<ContactRepo>();


        /// <summary>
        /// 联系人编辑完成
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            CopyTempAvatar();
            if (null == Id)
            {
                Id = _ContactRepo.Add(Platform.ToString(), Uid, Name, Phone, Avatar);
            }
            else
            {
                _ContactRepo.Update(Id.GetValueOrDefault(), Platform.ToString(), Uid, Name, Phone, Avatar);
            }
            return true;
        }

        internal void CopyTempAvatar()
        {
            if (null != TempAvatar) //修改了头像的情况
            {
                Avatar = TempAvatar;
                TempAvatar = null;
                return;
            }
            if (null == Avatar)     //没有头像的情况, 即新增, 读取默认头像
            {
                var uri = new Uri("pack://application:,,,/Assets/Images/avatar_default.jpg", UriKind.Absolute);
                var streamInfo = App.GetResourceStream(uri);
                using (var imageStream = streamInfo.Stream)
                {
                    byte[] bytes = new byte[imageStream.Length];
                    imageStream.Read(bytes, 0, (int)imageStream.Length - 1);
                    Avatar = new ByteArray(bytes);
                }
            }
        }
    }
}
