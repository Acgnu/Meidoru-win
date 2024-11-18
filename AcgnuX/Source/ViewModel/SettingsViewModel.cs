using AcgnuX.Properties;
using AcgnuX.Source.Taskx;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 设置的视图模型
    /// </summary>
    public class SettingsViewModel : ObservableObject
    {
        /// <summary>
        /// 账号文件
        /// </summary>
        public string AccountJsonPath
        {
            get => Settings.Default.AccountFilePath;
            set => SetProperty(Settings.Default.AccountFilePath, value, Settings.Default, (s, v) => { s.AccountFilePath = v; s.Save(); });
        }
        /// <summary>
        /// 乐谱目录
        /// </summary>
        public string PianoScorePath
        {
            get => Settings.Default.Tan8HomeDir;
            set => SetProperty(Settings.Default.Tan8HomeDir, value, Settings.Default, (s, v) => { s.Tan8HomeDir = v; s.Save(); });
        }

        /// <summary>
        /// 数据库文件目录
        /// </summary>
        public string DbFilePath
        {
            get => Settings.Default.DBFilePath;
            set => SetProperty(Settings.Default.DBFilePath, value, Settings.Default, (s, v) => { s.DBFilePath = v; s.Save(); });
        }
        /// <summary>
        /// 皮肤目录
        /// </summary>
        public string SkinFolderPath
        {
            get => Settings.Default.SkinFolderPath;
            set => SetProperty(Settings.Default.SkinFolderPath, value, Settings.Default, (s, v) => { s.SkinFolderPath = v; s.Save(); });
        }
        /// <summary>
        /// 代理
        /// </summary>
        public string HttpProxyAddress
        {
            get => Settings.Default.HttpProxyAddress;
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                if (!value.StartsWith("http://")) return;
                SetProperty(Settings.Default.HttpProxyAddress, value, Settings.Default, (s, v) => { s.HttpProxyAddress = v; s.Save(); });
            }
        }

        //IP代理数量
        private int _proxyCount = 0;
        public int ProxyCount { get => _proxyCount; set => SetProperty(ref _proxyCount, value); }

        private readonly ProxyFactoryV2 _ProxyFactoryV2;

        public SettingsViewModel(ProxyFactoryV2 proxyFactoryV2)
        {
            _ProxyFactoryV2 = proxyFactoryV2;
            //代理池数量变更监听
            ProxyCount = _ProxyFactoryV2.GetProxyCount;
            //IP代理池数量变化通知
            _ProxyFactoryV2.mProxyPoolCountChangeHandler += new Action<int>((curNum) => ProxyCount = curNum);
        }
    }
}
