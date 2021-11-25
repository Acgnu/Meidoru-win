
using AcgnuX.Source.Bussiness;
using AcgnuX.Source.Bussiness.Common;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using AcgnuX.Utils;
using AcgnuX.ViewModel;
using AcgnuX.WindowX.Dialog;
using MediaDevices;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace AcgnuX.Pages
{
    /// <summary>
    /// MobileDeviceControl.xaml 的交互逻辑
    /// </summary>
    public partial class MobileDeviceControl : BasePage
    {
        //列表数据对象
        private ObservableCollection<DeviceSyncListViewModel> mSyncDataList = new ObservableCollection<DeviceSyncListViewModel>();
        //标识是否已经监听了串口
        private bool mIsHookedUsb = false;
        //同步进度变更事件
        public event StatusBarNotifyHandler OnTaskBarEvent;
        //待同步的任务列表
        private static ConcurrentQueue<DeviceSyncTaskArgs> mSyncTaskQueue = new ConcurrentQueue<DeviceSyncTaskArgs>();
        //检查设备连接的Worker
        private readonly BackgroundWorker mCheckDeviceBgWorker = new BackgroundWorker()
        {
            WorkerReportsProgress = false,
            WorkerSupportsCancellation = true
        };
        //检查设备连接的Worker
        private readonly BackgroundWorker mSyncFileBgWorker = new BackgroundWorker()
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };
        //读取文件的Worker
        private readonly BackgroundWorker mReadDeviceFileWorker = new BackgroundWorker()
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        public MobileDeviceControl(MainWindow mainWin)
        {
            InitializeComponent();
            mMainWindow = mainWin;
            mMainWindow.OnClickStatusBarStop += ChangeTaskStatus;
            OnTaskBarEvent += mainWin.SetStatustProgess;

            mCheckDeviceBgWorker.DoWork += new DoWorkEventHandler(DoCheckDeviceTask);
            mCheckDeviceBgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CheckDeviceTaskComplate);

            mSyncFileBgWorker.DoWork += new DoWorkEventHandler(DoSyncFileTask);
            mSyncFileBgWorker.ProgressChanged += new ProgressChangedEventHandler(OnSyncFileProgress);
            mSyncFileBgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SyncFileTaskComplate);

            mReadDeviceFileWorker.DoWork += new DoWorkEventHandler(DoReadDeviceFileTask);
            mReadDeviceFileWorker.ProgressChanged += new ProgressChangedEventHandler(OnReadDeviceFileProgress);
            mReadDeviceFileWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnReadDeviceFileTaskComplate);
            
            CheckDevice(false);
            DeviceSyncListView.ItemsSource = mSyncDataList;
        }

        /// <summary>
        /// 页面加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnPageLoaded(object sender, System.Windows.RoutedEventArgs eventArgs)
        {
            if (!mIsHookedUsb)
            {
                //用于监听Windows消息 
                //注意获取窗口句柄一定要写在窗口loaded事件里，才能获取到窗口句柄，否则为空
                HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;//窗口过程
                if (hwndSource != null)
                    hwndSource.AddHook(new HwndSourceHook(DeveiceChanged));  //挂钩
                mIsHookedUsb = true;
            }
        }

        /// <summary>
        /// 监听串口设备
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private IntPtr DeveiceChanged(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WindowsMessage.WM_DEVICECHANGE)
            {
                switch (wParam.ToInt32())
                {
                    case WindowsMessage.DBT_DEVICEARRIVAL://设备插入  
                        CheckDevice(true);
                        break;
                    case WindowsMessage.DBT_DEVICEREMOVECOMPLETE: //设备卸载
                        CheckDevice(false);
                        break;
                    default:
                        break;
                }
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 当主窗口点击停止任务后, 改变任务状态
        /// </summary>
        private void ChangeTaskStatus()
        {
            if (mSyncFileBgWorker.IsBusy)
            {
                mSyncFileBgWorker.CancelAsync();
            }
            while (mSyncTaskQueue.Count > 0)
            {
                var item = new DeviceSyncTaskArgs();
                _ = mSyncTaskQueue.TryDequeue(out item);
            }
        }

        #region 检查设备连接后台任务
        /// <summary>
        /// 检查设备连接状态
        /// </summary>
        private void CheckDevice(bool delay)
        {
            if (mCheckDeviceBgWorker.IsBusy)
            {
                mCheckDeviceBgWorker.CancelAsync();
                return;
            }
            mCheckDeviceBgWorker.RunWorkerAsync(delay);
        }

        /// <summary>
        /// 后台执行已连接的设备检查
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoCheckDeviceTask(object sender, DoWorkEventArgs e)
        {
            var returnList = new List<MediaDevice>();
            e.Result = returnList;
            //是否需要等待的参数
            var delay = e.Argument == null ? false : (bool) e.Argument;
            for(var i = 0; i < 25; i++)
            {
                //标识了取消则直接返回
                if(mCheckDeviceBgWorker.CancellationPending) return;

                var mediaDevices = MediaDevice.GetDevices() as List<MediaDevice>;
                if(mediaDevices.Count >0)
                {
                    e.Result = mediaDevices;
                    return;
                }
                //需要等待的时候才执行等待
                if (delay)
                    Thread.Sleep(50);
                else
                    return;
            }
        }

        /// <summary>
        /// 连接设备检查执行结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckDeviceTaskComplate(object sender, RunWorkerCompletedEventArgs e)
        {
            var mediaDevices = e.Result as List<MediaDevice>;
            //如果没有设备了
            if (mediaDevices.Count == 0)
            {
                DefaultHintText.Visibility = Visibility.Visible;
                DeviceChoosePanel.Visibility = Visibility.Collapsed;
                DeviceSyncListView.Visibility = Visibility.Collapsed;
                DriverListCombobox.ItemsSource = null;
                DeviceListCombobox.ItemsSource = null;
                return;
            }
            //如果有设备, 检查此前选择的设备是否还存在
            var selectedDevice = (MediaDevice)DeviceListCombobox.SelectedItem;
            if (null != selectedDevice)
            {
                var nowDevice = mediaDevices.Find((el) => el.DeviceId.Equals(selectedDevice.DeviceId));
                if (nowDevice != null)
                {
                    //如果选择的设备还存在, 不做变化
                    return;
                }
            }

            //如果选择的设备已经不存在了, 需要重新选择
            DeviceListCombobox.ItemsSource = mediaDevices;

            //如果只有一个设备
            if (mediaDevices.Count == 1)
            {
                DefaultHintText.Visibility = Visibility.Collapsed;
                DeviceListCombobox.SelectedIndex = 0;
                return;
            }
            //如果有多个设备
            DefaultHintText.Visibility = Visibility.Collapsed;
            DeviceChoosePanel.Visibility = Visibility.Visible;
            DeviceSyncListView.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 文件同步后台任务
        /// <summary>
        /// 执行同步文件的后台任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoSyncFileTask(object sender, DoWorkEventArgs e)
        {
            var winProgress = new MainWindowStatusNotify()
            {
                alertLevel = AlertLevel.RUN,
                animateProgress = true,
                progressDuration = 100,
                nowProgress = 0
            };
            var isTaskStop = false;
            //当前执行的队列编号
            var curTaskNo = 1;
            //当前队列最大元素数量
            var maxedQueueCount = mSyncTaskQueue.Count;
            while (mSyncTaskQueue.Count > 0)
            {
                var arg = new DeviceSyncTaskArgs();
                var isPick = mSyncTaskQueue.TryDequeue(out arg);
                if (!isPick) continue;
                //每个队列的分片进度
                double eachBaseProgress = 0;
                //筛选WPD设备
                using (var device = arg.Device)
                {
                    device.Connect();

                    //检查目标文件夹是否存在, 不存在则创建
                    if (!Directory.Exists(arg.Item.PcFolderNameView)) FileUtil.CreateFolder(arg.Item.PcFolderNameView);
                    var targetMobileFolder = Path.Combine(arg.DevicePath, arg.Item.MobileFolderNameView);
                    if (!device.DirectoryExists(targetMobileFolder)) device.CreateDirectory(targetMobileFolder);

                    //将两端的文件合并到一个集合
                    var allItems = arg.Item.PcItemList.ToList();
                    allItems.AddRange(arg.Item.MobileItemList.ToList());
                    var fileCounter = 1;
                    allItems.ForEach((syncFile) => {
                        //检查任务是否取消
                        if (mSyncFileBgWorker.CancellationPending)
                        {
                            isTaskStop = true;
                            return;
                        }
                        //如果队列数量增大超过初始量, 记录一个最大数量
                        if (mSyncTaskQueue.Count + 1 > maxedQueueCount)
                            maxedQueueCount = mSyncTaskQueue.Count + 1;
                        //当前队列数量=此队列拥有过的最大数量
                        var curQueueCount = maxedQueueCount;
                        //计算每个队列的分片进度
                        eachBaseProgress = 100 / curQueueCount;
                        //每个队列的基础进度, 从0开始, 以分片进度递增
                        double baseProgress = 100 - (mSyncTaskQueue.Count + 1) * eachBaseProgress;
                        //计算每个文件的分片进度
                        double eachFileProgress = eachBaseProgress / allItems.Count;
                        //文件的执行进度, 从0开始递增
                        double fileProgress = eachFileProgress * fileCounter - eachFileProgress;
                        //总进度 = 基础进度 + 文件进度
                        WindowUtil.CalcProgress(winProgress,
                            string.Format("主任务 [{0}/{1}], 子任务 [{2}/{3}], 正在复制 [ {4} ]", curTaskNo, curQueueCount, fileCounter, allItems.Count, syncFile.NameView),
                            baseProgress + fileProgress);
                        arg.Progress = winProgress;
                        arg.ProgressType = SyncTaskProgressType.SUB_ITEM_FINISH;
                        mSyncFileBgWorker.ReportProgress(0, arg);
                        //执行同步
                        if (syncFile.SourceView == SyncDeviceType.PC)
                        {
                            //电脑端, 需要复制到移动端
                            var targetFolder = Path.Combine(arg.DevicePath, arg.Item.MobileFolderNameView);
                            using (FileStream stream = File.OpenRead(Path.Combine(arg.Item.PcFolderNameView, syncFile.NameView)))
                            {
                                device.UploadFile(stream, Path.Combine(targetFolder, syncFile.NameView));
                            }
                        }
                        else
                        {
                            MediaFileInfo fileInfo = device.GetFileInfo(Path.Combine(arg.DevicePath, arg.Item.MobileFolderNameView, syncFile.NameView));
                            fileInfo.CopyTo(Path.Combine(arg.Item.PcFolderNameView, syncFile.NameView));
                        }
                        fileProgress = eachFileProgress * fileCounter;
                        WindowUtil.CalcProgress(winProgress,
                            string.Format("主任务 [{0}/{1}], 子任务 [{2}/{3}], 正在复制 [ {4} ]", curTaskNo, curQueueCount, fileCounter, allItems.Count, syncFile.NameView),
                            baseProgress + fileProgress);
                        arg.Source = syncFile.SourceView;
                        arg.DeviceSyncItem = syncFile;
                        mSyncFileBgWorker.ReportProgress(0, arg);
                        fileCounter++;
                    });
                    device.Disconnect();
                }
                arg.IsOk = !isTaskStop;
                arg.ProgressType = SyncTaskProgressType.QUEUE_FINISH;
                if (!isTaskStop)
                {
                    //一个队列执行完之后, 发送当前队列进度
                    WindowUtil.CalcProgress(arg.Progress, string.Format("主任务 [{0}/{1}]同步完成", curTaskNo, maxedQueueCount), curTaskNo * eachBaseProgress);
                    mSyncFileBgWorker.ReportProgress(0, arg);
                    curTaskNo++;
                }
                else
                {
                    mSyncFileBgWorker.ReportProgress(0, arg);
                }
            }
            e.Result = !isTaskStop;
        }

        /// <summary>
        /// 文件同步进度事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSyncFileProgress(object sender, ProgressChangedEventArgs e)
        {
            var arg = e.UserState as DeviceSyncTaskArgs;
            if(SyncTaskProgressType.SUB_ITEM_FINISH == arg.ProgressType)
            {
                if(SyncDeviceType.PC == arg.Source)
                    arg.Item.PcItemList.Remove(arg.DeviceSyncItem);
                else
                    arg.Item.MobileItemList.Remove(arg.DeviceSyncItem);
            }
            else
            {
                if (arg.IsOk)
                    mSyncDataList.Remove(arg.Item);
                arg.ThatButton.IsEnabled = true;
            }
            OnTaskBarEvent?.Invoke(arg.Progress);
        }

        /// <summary>
        /// 同步文件后台任务执行完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncFileTaskComplate(object sender, RunWorkerCompletedEventArgs e)
        {
            var rb = (bool)e.Result;
            OnTaskBarEvent?.Invoke(WindowUtil.CalcProgress(new MainWindowStatusNotify(), rb ? "同步完成" : "任务中止", 100));
        }
        #endregion

        #region 读取文件后台任务
        /// <summary>
        /// 读取文件工作任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoReadDeviceFileTask(object sender, DoWorkEventArgs workArgs)
        {
            var args = (DeviceSyncReadArgs)workArgs.Argument;
            var resultNotify = new MainWindowStatusNotify()
            {
                message = "文件读取完成"
            };
            workArgs.Result = resultNotify;
            //多设备且未手动选择设备
            if (null == args.Device)
            {
                resultNotify.message = "没有待同步的目标设备";
                resultNotify.alertLevel = AlertLevel.ERROR;
                return;
            }
            //多盘符且未手动选择盘符
            if (null == args.MediaDrive)
            {
                resultNotify.message = "没有待同步的目标盘符";
                resultNotify.alertLevel = AlertLevel.ERROR;
                return;
            }

            //进度通知
            var progressEvent = new DeviceSyncReadProgressItem
            {
                ProgressType = 1,
                Notify = new MainWindowStatusNotify
                {
                    message = "正在读取路径配置...",
                    alertLevel = AlertLevel.RUN,
                    nowProgress = 99
                }
            };
            mReadDeviceFileWorker.ReportProgress(0, progressEvent);
            //从数据库读取
            var dataSet = SQLite.SqlTable("SELECT pc_path, mobile_path FROM media_sync_config where enable = 1", null);
            if (null == dataSet || dataSet.Rows.Count == 0)
            {
                resultNotify.message = "无法读取路径配置";
                resultNotify.alertLevel = AlertLevel.WARN;
                return;
            }
            //封装进对象
            var syncPathList = new List<MediaSyncConfig>();
            foreach (DataRow dataRow in dataSet.Rows)
            {
                syncPathList.Add(new SyncConfigViewModel()
                {
                    PcPath = Convert.ToString(dataRow["pc_path"]),
                    MobilePath = Convert.ToString(dataRow["mobile_path"]),
                });
            }
            progressEvent.Notify.message = "正在连接到设备...";
            mReadDeviceFileWorker.ReportProgress(0, progressEvent);
            //筛选WPD设备
            using (args.Device)
            {
                args.Device.Connect();
                foreach(var e in syncPathList)
                {
                    bool isPcPathExists = Directory.Exists(e.PcPath),
                    isMobilePathExists = args.Device.DirectoryExists(Path.Combine(args.MediaDrive.NameView, e.MobilePath));

                    if (!isPcPathExists && !isMobilePathExists)
                        //如果两端都不存在目标文件夹, 则跳过
                        continue;

                    progressEvent.Notify.message = string.Format("正在读取文件列表[{0}]...", e.PcPath);
                    mReadDeviceFileWorker.ReportProgress(0, progressEvent);
                    //读取PC目录下的文件列表
                    var pcFiles = isPcPathExists ? FileUtil.GetFileNameFromFullPath(Directory.GetFiles(e.PcPath)) : new string[0];
                    //读取移动设备下的文件列表
                    var mobileFiles = isMobilePathExists ? FileUtil.GetFileNameFromFullPath(args.Device.GetFiles(Path.Combine(args.MediaDrive.NameView, e.MobilePath))) : new string[0];
                    //找出差异文件
                    var filesOnlyInMobile = DataUtil.FindDiffEls(pcFiles, mobileFiles);
                    var filesOnlyInPc = DataUtil.FindDiffEls(mobileFiles, pcFiles);
                    //不存在差异则直接跳过当前文件夹
                    if (DataUtil.IsEmptyCollection(filesOnlyInMobile) && DataUtil.IsEmptyCollection(filesOnlyInPc))
                    {
                        continue;
                    }
                    if (!DataUtil.IsEmptyCollection(filesOnlyInPc))
                    {
                        if (!CollectAndNotify(filesOnlyInPc, workArgs, e, SyncDeviceType.PC))
                        {
                            //不成功通常是任务中断
                            args.Device.Disconnect();
                            resultNotify.message = "已停止";
                            return;
                        }
                    }
                    if (!DataUtil.IsEmptyCollection(filesOnlyInMobile))
                    {
                        if (!CollectAndNotify(filesOnlyInMobile, workArgs, e, SyncDeviceType.PHONE))
                        {
                            args.Device.Disconnect();
                            resultNotify.message = "已停止";
                            return;
                        }
                    }
                }
                args.Device.Disconnect();
            }
        }

        /// <summary>
        /// 收集并提醒
        /// </summary>
        /// <param name="files"></param>
        /// <param name="workArgs"></param>
        /// <param name="progressEvent"></param>
        /// <param name="e"></param>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        private bool CollectAndNotify(List<string> files, DoWorkEventArgs workArgs, MediaSyncConfig e, SyncDeviceType deviceType)
        {
            var args = (DeviceSyncReadArgs)workArgs.Argument;
            foreach (var fileName in files)
            {
                if (mReadDeviceFileWorker.CancellationPending)
                {
                    return false;
                }
                var item = CreateSyncItem(fileName,
                    deviceType == SyncDeviceType.PC ? e.PcPath : e.MobilePath, deviceType, args.Device, args.MediaDrive.NameView);
                //汇报进度
                mReadDeviceFileWorker.ReportProgress(0, new DeviceSyncReadProgressItem
                {
                    ProgressType = 2,
                    PcFolderNameView = e.PcPath, 
                    MobileFolderNameView = e.MobilePath,
                    FileItem = item,
                    FileSource = deviceType, 
                    Notify = new MainWindowStatusNotify
                    {
                        alertLevel = AlertLevel.RUN,
                        message = string.Format("正在收集差异文件  [{0}]", Path.Combine(item.FolderView, item.NameView)),
                        nowProgress = 99
                    }
                });
            }
            return true;
        }

        /// <summary>
        /// 创建同步项
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="folderPath">文件路径</param>
        /// <param name="source">源 1 PC, 2 移动设备</param>
        /// <param name="device">设备</param>
        /// <param name="driverName">设备驱动</param>
        /// <returns></returns>
        private DeviceSyncItem CreateSyncItem(string fileName, string folderPath, SyncDeviceType source, MediaDevice device, string driverName)
        {
            var item = new DeviceSyncItem()
            {
                NameView = fileName,
                SourceView = source,
                FolderView = folderPath,
            };
            var mediaType = FileUtil.GetMediaTypeByName(fileName);
            item.ContentType = mediaType;
            if (mediaType == SyncContentType.IMAGE || mediaType == SyncContentType.VIDEO)
            {
                if (source == SyncDeviceType.PHONE)
                {
                    var fileInfo = device.GetFileInfo(Path.Combine(driverName, folderPath, fileName));
                    var imgByte = GetMediaFileThumbnail(fileInfo);
                    if (null != imgByte)
                        item.ImgByte = imgByte;
                }
                else
                {
                    var imgByte = ImageUtil.CreateThumbnail(Path.Combine(folderPath, fileName), 100, 100);
                    item.ImgByte = ImageUtil.ImageToByteArray(imgByte);
                }
            }
            return item;
        }

        /// <summary>
        /// 读取媒体文件缩略图
        /// </summary>
        /// <param name="mediaFileInfo"></param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        private byte[] GetMediaFileThumbnail(MediaFileInfo mediaFileInfo)
        {
            try
            {
                using (Stream stream = mediaFileInfo.OpenThumbnail())
                {
                    return FileUtil.Stream2Bytes(stream);
                }
            }
            catch (Exception) { }
            return null;
        }

        /// <summary>
        /// 读取文件进度变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReadDeviceFileProgress(object sender, ProgressChangedEventArgs e)
        {
            var newItem = (DeviceSyncReadProgressItem) e.UserState;
            if(newItem.ProgressType == 1)
            {
                //发送状态栏提示信息
                OnTaskBarEvent?.Invoke(newItem.Notify);
                return;
            }
            //找到已添加的父列表
            var listItem = mSyncDataList.Where((el) => el.PcFolderNameView.Equals(newItem.PcFolderNameView)).FirstOrDefault();
            if(null == listItem)
            {
                //没有则新建一个项
                var listItemPcFileList = new ObservableCollection<DeviceSyncItem>();
                var listItemMoblieFileList = new ObservableCollection<DeviceSyncItem>();
                AddFolderListFileItem(listItemPcFileList, listItemMoblieFileList, newItem);
                mSyncDataList.Add(new DeviceSyncListViewModel
                {
                    PcFolderNameView = newItem.PcFolderNameView,
                    MobileFolderNameView = newItem.MobileFolderNameView,
                    PcItemList = listItemPcFileList,
                    MobileItemList = listItemMoblieFileList
                });
            }
            else
            {
                //有则直接添加
                AddFolderListFileItem(listItem.PcItemList, listItem.MobileItemList, newItem);
            }
            //发送状态栏提示信息
            OnTaskBarEvent?.Invoke(newItem.Notify);
        }

        /// <summary>
        /// 读取设备文件任务完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReadDeviceFileTaskComplate(object sender, RunWorkerCompletedEventArgs e)
        {
            var notify = (MainWindowStatusNotify) e.Result;
            OnTaskBarEvent?.Invoke(notify);
        }

        /// <summary>
        /// 区分添加子项目
        /// </summary>
        /// <param name="pcCollect"></param>
        /// <param name="mobileCollect"></param>
        /// <param name="newItem"></param>
        private void AddFolderListFileItem(ObservableCollection<DeviceSyncItem> pcCollect,
            ObservableCollection<DeviceSyncItem> mobileCollect,
            DeviceSyncReadProgressItem newItem)
        {
            if (newItem.FileSource == SyncDeviceType.PC)
                pcCollect.Add(CreateSyncItemViewModel(newItem.FileItem));
            else
                mobileCollect.Add(CreateSyncItemViewModel(newItem.FileItem));
        }

        /// <summary>
        /// 创建视图模型项
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private DeviceSyncItem CreateSyncItemViewModel(DeviceSyncItem item)
        {
            var viewItem = new DeviceSyncItem
            {
                NameView = item.NameView,
                FolderView = item.FolderView,
                SourceView = item.SourceView
            };
            if (null != item.ImgByte)
            {
                //有真实预览图则显示预览图
                viewItem.BitImg = ImageUtil.GetBitmapImage(item.ImgByte);
                item.ImgByte = null;
            }
            else
            {
                //没有预览图则根据文件类型显示默认图标
                switch (item.ContentType)
                {
                    case SyncContentType.AUDIO: viewItem.BitImg = ApplicationConstant.GetDefaultAudioIcon(); break;
                    case SyncContentType.IMAGE: viewItem.BitImg = ApplicationConstant.GetDefaultImageIcon(); break;
                    case SyncContentType.VIDEO: viewItem.BitImg = ApplicationConstant.GetDefaultVideoIcon(); break;
                    case SyncContentType.OTHER: viewItem.BitImg = ApplicationConstant.GetDefaultFileIcon(); break;
                }
            }
            return viewItem;
        }
        #endregion

        #region WFP页面元素事件
        /// <summary>
        /// 刷新按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRefreshButtonClick(object sender, System.Windows.RoutedEventArgs eventArgs)
        {
            if(DriverListCombobox.SelectedItem != null)
            {
                //如果有选中的驱动, 则直接刷新文件
                ComboBoxDriverSelectionChanged(null, null);
            }
            else
            {
                //没有则重新读取设备
                CheckDevice(true);
            }
        }

        /// <summary>
        /// 设备选择变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxDeviceSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var device = (MediaDevice) DeviceListCombobox.SelectedItem;
            if (null == device) return;

            device.Connect();
            var deviceDrivers = device.GetDrives();
            if(null == deviceDrivers)
            {
                device.Disconnect();
                return;
            }
            var deviceDriverViews = new List<DeviceDriverViewModel>();
            for (var i = 0; i < deviceDrivers.Length; i++)
            {
                deviceDriverViews.Add(new DeviceDriverViewModel()
                {
                    ValueView = deviceDrivers[i].Name,
                    NameView = deviceDrivers[i].Name
                });
            }
            device.Disconnect();
            DriverListCombobox.ItemsSource = deviceDriverViews;
            if(deviceDriverViews.Count == 1)
            {
                DriverListCombobox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 设备驱动选择变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxDriverSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            DeviceSyncListView.Visibility = Visibility.Visible;
            DeviceChoosePanel.Visibility = Visibility.Collapsed;
            if(mReadDeviceFileWorker.IsBusy)
            {
                mReadDeviceFileWorker.CancelAsync();
                return;
            }
            //清空当前列表
            mSyncDataList.Clear();
            //任务参数
            var taskArg = new DeviceSyncReadArgs
            {
                //选中的设备
                Device = (MediaDevice)DeviceListCombobox.SelectedItem,
                //选中的设备盘
                MediaDrive = (DeviceDriverViewModel)DriverListCombobox.SelectedItem
            };
            mReadDeviceFileWorker.RunWorkerAsync(taskArg);
        }

        /// <summary>
        /// 具体同步项右击事件, 移除项目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgItemRightClick(object sender, MouseButtonEventArgs e)
        {
            if(mSyncFileBgWorker.IsBusy)
            {
                //正在同步时不允许删除项
                return;
            }
            var subListView = XamlUtil.GetParentListView(e);
            //根据触发按钮获取点击的行
            var selected = ((ListViewItem)subListView.ContainerFromElement(sender as StackPanel)).Content;
            var ImgItemList = (ObservableCollection<DeviceSyncItem>)subListView.ItemsSource;
            ImgItemList.Remove(selected as DeviceSyncItem);
        }

        /// <summary>
        /// 项目双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgItemLeftClick(object sender, MouseButtonEventArgs e)
        {
            var subListView = XamlUtil.GetParentListView(e);
            //根据触发按钮获取点击的行
            var selected = (DeviceSyncItem)((ListViewItem)subListView.ContainerFromElement(sender as StackPanel)).Content;
            var curTimeMill = TimeUtil.CurrentMillis();
            var lastClick = selected.LastLeftMouseClickTime;
            selected.LastLeftMouseClickTime = curTimeMill;
            //双击才执行操作
            if (curTimeMill - lastClick < 200)
            {
                if(selected.SourceView == SyncDeviceType.PC)
                {
                    //如果文件存在于PC, 则打开文件
                    System.Diagnostics.Process.Start(Path.Combine(selected.FolderView, selected.NameView));
                }
                else
                {
                    //不允许重复调用
                    if (selected.IsCopying) return;

                    //读取系统的临时文件夹
                    var winTempFolder = Path.GetTempPath();
                    if (null == winTempFolder)
                    {
                        OnTaskBarEvent?.Invoke(new MainWindowStatusNotify
                        {
                            message = "无法获取临时文件夹",
                            alertLevel = AlertLevel.ERROR,
                        });
                        return;
                    }
                    var targetFullPath = Path.Combine(winTempFolder, selected.NameView);
                    //如果已经存在于临时文件夹, 则直接打开
                    if (File.Exists(targetFullPath))
                    {
                        System.Diagnostics.Process.Start(targetFullPath);
                        return;
                    }

                    //发送进度消息
                    selected.IsCopying = true;
                    OnTaskBarEvent?.Invoke(new MainWindowStatusNotify
                    {
                        message = "正在复制文件",
                        alertLevel = AlertLevel.RUN,
                        nowProgress = 99
                    });
                    //先复制目标文件到临时文件夹, 再打开
                    using (var device = (MediaDevice) DeviceListCombobox.SelectedItem)
                    {
                        device.Connect();
                        var filePath = Path.Combine(((DeviceDriverViewModel)DriverListCombobox.SelectedItem).ValueView, selected.FolderView, selected.NameView);
                        MediaFileInfo fileInfo = device.GetFileInfo(filePath);
                        fileInfo.CopyTo(targetFullPath);
                        device.Disconnect();
                    }
                    selected.IsCopying = false;
                    OnTaskBarEvent?.Invoke(new MainWindowStatusNotify
                    {
                        message = string.Format("已复制到临时文件夹 {0}", targetFullPath)
                    });
                    System.Diagnostics.Process.Start(targetFullPath);
                }
            }
            else
            {
                //显示文件名
                OnTaskBarEvent?.Invoke(new MainWindowStatusNotify
                {
                    message = string.Format("{0}", Path.Combine(selected.FolderView, selected.NameView))
                });
            }
        }

        /// <summary>
        /// 点击同步按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSyncButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.IsEnabled = false;
            var selected = (DeviceSyncListViewModel)((ListViewItem)DeviceSyncListView.ContainerFromElement(button)).Content;
            var taskArgs = new DeviceSyncTaskArgs
            {
                ThatButton = button,
                Item = selected,
                Device = DeviceListCombobox.SelectedItem as MediaDevice,
                DevicePath = ((DeviceDriverViewModel) DriverListCombobox.SelectedItem ).ValueView
            };
            //将任务添加进队列
            mSyncTaskQueue.Enqueue(taskArgs);
            if (mSyncFileBgWorker.IsBusy)
            {
                //任务正在运行则直接返回
                return;
            }
            //如果任务没有运行, 则开启一个任务
            mSyncFileBgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// 子列表项目键盘事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubItemOnKeyDown(object sender, KeyEventArgs e)
        {
            //按下删除键
            if (e.Key == Key.Delete)
            {
                var subListView = (ListView)sender;
                //获取选中对象
                var selected = subListView.SelectedItem as DeviceSyncItem;
                if (null == selected)
                {
                    return;
                }
                var confirmDialog = new ConfirmDialog(AlertLevel.WARN, string.Format((string)Application.Current.FindResource("DeleteConfirm"), selected.NameView));
                //确认删除
                if (confirmDialog.ShowDialog().GetValueOrDefault())
                {
                    //从电脑删除
                    if (selected.SourceView == SyncDeviceType.PC)
                    {
                        FileUtil.DeleteFile(Path.Combine(selected.FolderView, selected.NameView));
                    }
                    else
                    {
                        //从手机删除
                        List<MediaDevice> MediaDeviceList = MediaDevice.GetDevices() as List<MediaDevice>;
                        using (var device = MediaDeviceList.First())
                        {
                            device.Connect();
                            var drivers = device.GetDrives();
                            device.DeleteFile(Path.Combine(drivers[0].Name, selected.FolderView, selected.NameView));
                            device.Disconnect();
                        }
                    }
                    //从vm中移除对象
                    var subListViewItemSource = (ObservableCollection<DeviceSyncItem>)subListView.ItemsSource;
                    subListViewItemSource.Remove(selected);
                }
            }
        }

        /// <summary>
        /// 打开文件夹图标点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenFolderClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selected = (DeviceSyncListViewModel)((ListViewItem)DeviceSyncListView.ContainerFromElement((DependencyObject) sender)).Content;
            System.Diagnostics.Process.Start(selected.PcFolderNameView);
        }

        private void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
        #endregion
    }
}
