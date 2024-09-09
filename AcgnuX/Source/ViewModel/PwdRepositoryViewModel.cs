using AcgnuX.Properties;
using AcgnuX.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedLib.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.IO;
using System.ComponentModel;
using System.Windows.Data;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 密码管理ViewModel
    /// </summary>
    public class PwdRepositoryViewModel : ObservableObject
    {
        //数据集
        public ObservableCollection<AccountViewModel> Accounts { get; set; } = new ObservableCollection<AccountViewModel>();
        //是否忙
        private bool _IsBusy = false;
        public bool IsBusy { get => _IsBusy; set => SetProperty(ref _IsBusy, value); }
        //过滤文本
        private string filterText;
        public string FilterText { get => filterText; set { filterText = value; OnFilterTextChanged(); } }
        private ICollectionView _CollectionView;
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
            if (Accounts.Count > 0) return; //only load once

            if (!File.Exists(Settings.Default.AccountFilePath)) return;

            IsBusy = true;
            //如果没有缓存, 则从文件中加载数据
            var document = await FileUtil.ParseJsonDocumentAsync(Settings.Default.AccountFilePath);
            using (document)
            {
                foreach (JsonElement item in document.RootElement.EnumerateArray())
                {
                    Accounts.Add(new AccountViewModel
                    {
                        //转换成视图项
                        Id = item.GetProperty("Id").GetInt64(),
                        Site = item.GetProperty("Site").GetString(),
                        Describe = item.GetProperty("Describe").GetString(),
                        Uname = item.GetProperty("Uname").GetString(),
                        Upass = item.GetProperty("Upass").GetString(),
                        Remark = item.GetProperty("Remark").GetString()
                    });
                }
                _CollectionView = CollectionViewSource.GetDefaultView(Accounts);
            }
            if (_IsBusy) IsBusy = false;
        }

        /// <summary>
        /// 删除项目
        /// </summary>
        /// <param name="selected"></param>
        internal async Task DeleteItem(AccountViewModel selected)
        {
            //备份源文件
            var backupPath = FileUtil.Backup(Settings.Default.AccountFilePath);

            using (var inputStream = File.OpenRead(backupPath))
            using (var outpuStream = File.OpenWrite(Settings.Default.AccountFilePath))
            {
                var node = await JsonNode.ParseAsync(inputStream);
                var nodeArray = node.AsArray();
                var target = nodeArray.Where((n) => n["Id"].GetValue<long>().Equals(selected.Id.GetValueOrDefault())).First();
                nodeArray.Remove(target);

                //写入到新文件
                var writter = new Utf8JsonWriter(outpuStream, new JsonWriterOptions()
                {
                    Indented = true
                });
                node.WriteTo(writter);
                await writter.FlushAsync();
            }
            //从viewModel中删除
            Accounts.Remove(selected);
        }

        /// <summary>
        /// 搜索过滤
        /// </summary>
        private void OnFilterTextChanged()
        {
            if (null == _CollectionView)
            {
                return;
            }
            if (string.IsNullOrEmpty(FilterText) && _CollectionView.Filter != null)
            {
                _CollectionView.Filter = null;
            }
            if (!string.IsNullOrEmpty(FilterText))
            {
                _CollectionView.Filter = new Predicate<object>((e) =>
                {
                    var item = e as AccountViewModel;
                    if (item.Site.Contains(FilterText) || item.Describe.Contains(FilterText)) return true;
                    if (!string.IsNullOrEmpty(item.Remark) && item.Remark.Contains(FilterText)) return true;
                    return false;
                });
            }
        }
    }
}
