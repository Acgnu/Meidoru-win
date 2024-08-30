using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.ViewModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SharedLib.Model;
using SharedLib.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 密码管理ViewModel
    /// </summary>
    public class PwdRepositoryViewModel : ViewModelBase
    {
        //数据集
        private List<Account> _CachedAccounts = new List<Account>();
        public ObservableCollection<AccountViewModel> Accounts { get; set; } = new ObservableCollection<AccountViewModel>();
        //是否忙
        private bool _IsBusy = true;
        public bool IsBusy
        {
            get { return _IsBusy; }
            set { _IsBusy = value; RaisePropertyChanged(); }
        }
        //过滤文本
        public string FilterText { get; set; }
        //命令
        public ICommand OnCopyAccountCommand { get; set; }
        public ICommand OnCopyPasswordCommand { get; set; }

        public PwdRepositoryViewModel()
        {
            OnCopyAccountCommand = new RelayCommand<AccountViewModel>((e) => Clipboard.SetDataObject(e.Uname));
            OnCopyPasswordCommand = new RelayCommand<AccountViewModel>((e) => Clipboard.SetDataObject(e.Upass));
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        public async void Load()
        {
            if (DataUtil.IsEmptyCollection(_CachedAccounts))
            {
                IsBusy = true;
                //如果没有缓存, 则从文件中加载数据
                var data = await LoadAllPasswordAsync();
                _CachedAccounts = data;
            }
            //从缓存数据集中过滤
            Accounts.Clear();
            _CachedAccounts
                .Where(e => string.IsNullOrEmpty(FilterText) || e.Site.Contains(FilterText) || e.Describe.Contains(FilterText))
                .ToList()
                .ForEach(e => Accounts.Add(new AccountViewModel
            {
                    //转换成视图项
                Id = e.Id,
                Site = e.Site,
                Describe = e.Describe,
                Uname = e.Uname,
                Upass = e.Upass,
                Remark = e.Remark
            }));
            if (_IsBusy) IsBusy = false;
        }

        /// <summary>
        /// 重新加载数据
        /// </summary>
        private async Task<List<Account>> LoadAllPasswordAsync()
        {
            return await Task.Run(() =>
            {
                var accountFilePath = Settings.Default.AccountFilePath;
                var itemsInFile = FileUtil.DeserializeJsonFromFile<List<Account>>(accountFilePath);
                return itemsInFile;
            });
        }

        /// <summary>
        /// 删除项目
        /// </summary>
        /// <param name="selected"></param>
        internal void DeleteItem(AccountViewModel selected)
        {
            Accounts.Remove(selected);
            var item = _CachedAccounts.Where(e => e.Id.Equals(selected.Id)).FirstOrDefault();
            _CachedAccounts.Remove(item);
            FileUtil.IncrSaveJsonToFile(_CachedAccounts, Settings.Default.AccountFilePath);
        }

        /// <summary>
        /// 保存项目
        /// </summary>
        /// <param name="accountVm"></param>
        internal async Task<InvokeResult<object>> SaveItem(AccountViewModel accountVm)
        {
            if (string.IsNullOrEmpty(accountVm.Site) || string.IsNullOrEmpty(accountVm.Uname) || string.IsNullOrEmpty(accountVm.Upass))
            {
                Messenger.Default.Send(new BubbleTipViewModel
                {
                    AlertLevel = AlertLevel.ERROR,
                    Text = "啥都不填想啥呢"
                });
                return new InvokeResult<object>()
                {
                    success = false,
                    code = 10,
                    message = "fail"
                };
            }
            var account = new Account
            {
                Id = accountVm.Id,
                Site = accountVm.Site,
                Describe = accountVm.Describe,
                Uname = accountVm.Uname,
                Upass = accountVm.Upass,
                Remark = accountVm.Remark
            };
            if (null == account.Id)
            {
                //新增操作, 查询列表里所有账号的ID, 得到最大ID作为新账号的ID
                var maxId = _CachedAccounts.Max(e => e.Id);
                account.Id = null == maxId ? 1 : ++maxId;
                _CachedAccounts.Add(account);
                accountVm.Id = account.Id;
                Accounts.Add(accountVm);
            }
            else
            {
                //从缓存中替换
                for (var i = 0; i < _CachedAccounts.Count; i++)
                {
                    var item = _CachedAccounts[i];
                    if (item.Id.Equals(account.Id))
                    {
                        _CachedAccounts.RemoveAt(i);
                        _CachedAccounts.Insert(i, account);
                        break;
                    }
                }
                //添加到VM
                for (var i = 0; i < Accounts.Count; i++)
                {
                    var item = Accounts[i];
                    if (item.Id.Equals(account.Id))
                    {
                        Accounts.RemoveAt(i);
                        Accounts.Insert(i, accountVm);
                        break;
                    }
                }
            }
            //保存到文件
            await Task.Run(() => FileUtil.IncrSaveJsonToFile(_CachedAccounts, Settings.Default.AccountFilePath));
            return new InvokeResult<object>()
            {
                success = true,
                code = 0,
                message = "success"
            };
        }
    }
}
