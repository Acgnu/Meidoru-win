using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 设备列表同步路径配置vm
    /// </summary>
    public class DeviceSyncPathConfigDialogViewModel : ViewModelBase
    {
        //同步配置
        public ObservableCollection<SyncConfigViewModel> SyncConfigs { get; set; } = new ObservableCollection<SyncConfigViewModel>();
        //表格选中项
        public SyncConfigViewModel SelectedItem { get; set; }
        //全选/反选事件
        public ICommand OnHeaderCheckboxClick { get; set; }
        //单选事件
        public ICommand OnItemsCheckboxClick { get; set; }
        //是否全选
        public bool IsSyncConfigCheckedAll {
            get => SyncConfigs.Where(item => item.Enable).Count() == SyncConfigs.Count(); 
            set
            { 
                foreach (var item in SyncConfigs)
                {
                    item.Enable = value;
                }
                RaisePropertyChanged();
            } 
        }

        //数据库访问
        private readonly MediaSyncConfigRepo _MediaSyncConfigRepo = MediaSyncConfigRepo.Instance;

        public DeviceSyncPathConfigDialogViewModel()
        {
            OnHeaderCheckboxClick = new RelayCommand<bool>((v) => OnSyncConfigGridCheckBoxClick(v, true));
            OnItemsCheckboxClick = new RelayCommand<bool>((v) => OnSyncConfigGridCheckBoxClick(v, false));
            InitSyncConfig();
        }

        /// <summary>
        /// 初始同步配置
        /// </summary>
        private void InitSyncConfig()
        {
            //从数据库读取
            var configs = _MediaSyncConfigRepo.FindConfig(null);
            if (DataUtil.IsEmptyCollection(configs)) return;
            //封装进对象
            configs.ForEach(e => SyncConfigs.Add(new SyncConfigViewModel
            {
                Id = e.Id,
                PcPath = e.PcPath,
                MobilePath = e.MobilePath,
                Enable = e.Enable
            }));
            NotifyCheckBoxChange();
        }

        /// <summary>
        /// 删除选中的配置
        /// </summary>
        internal void DeleteSelected()
        {
            if(null == SelectedItem)
            {
                return;
            }
            _MediaSyncConfigRepo.DeleteById(SelectedItem.Id);
            SyncConfigs.Remove(SelectedItem);
            NotifyCheckBoxChange();
        }

        /// <summary>
        /// 添加新的路径
        /// </summary>
        /// <param name="syncConfig"></param>
        internal void AddNewSyncConfig(SyncConfigViewModel syncConfig)
        {
            //由于添加重复策略是替换, 因此新增成功之后需要检查当前vm中重复的项
            for (var i = 0; i < SyncConfigs.Count; i++)
            {
                //如果存在PcPath重复或者MobilePath重复, 都需要删除 (Replace 策略会将任意一个重复的都删除 )
                var item = SyncConfigs[i];
                if (item.PcPath.Equals(syncConfig.PcPath) || item.MobilePath.Equals(syncConfig.MobilePath))
                {
                    SyncConfigs.Remove(item);
                    i--;
                }
            }
            SyncConfigs.Add(syncConfig);
            NotifyCheckBoxChange();
        }

        /// <summary>
        /// 同步配置表格checkbox点击事件
        /// </summary>
        /// <param name="sender"></param>
        private void OnSyncConfigGridCheckBoxClick(bool value, bool isAll)
        {
            if (isAll)
            {
                //全选
                foreach (var syncConfig in SyncConfigs)
                {
                    syncConfig.Enable = IsSyncConfigCheckedAll;
                }
                _MediaSyncConfigRepo.UpdateSyncConfigEnable(null, IsSyncConfigCheckedAll);
            }
            else
            {
                //单选
                _MediaSyncConfigRepo.UpdateSyncConfigEnable(SelectedItem.Id, value);
                NotifyCheckBoxChange();
            }
        }

        /// <summary>
        /// 检查是否全选/全反选
        /// </summary>
        internal void NotifyCheckBoxChange()
        {
            RaisePropertyChanged(nameof(IsSyncConfigCheckedAll));
        }
    }
}
