using AcgnuX.Source.Bussiness;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Utils;
using AcgnuX.ViewModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MediaDevices;
using Microsoft.Toolkit.Uwp.Notifications;
using SharedLib.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UsbMonitor;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 设备同步列表ViewModel
    /// </summary>
    public class DeviceSyncViewModel : ViewModelBase
    {
        //总数据
        public ObservableCollection<FileItemsCompareViewModel> ListData { get; set; } = new ObservableCollection<FileItemsCompareViewModel>();
        //进度值
        private double _ProgressValue;
        public double ProgressValue { get => _ProgressValue; set { _ProgressValue = value; RaisePropertyChanged(); } }
        //进度文本
        private string _ProgressText = "No Device Connected";
        public string ProgressText { get => _ProgressText; set { _ProgressText = value; RaisePropertyChanged(); } }
        //进度条警示级别
        private AlertLevel _ProgressAlertLevel;
        public AlertLevel ProgressAlertLevel { get => _ProgressAlertLevel; set { _ProgressAlertLevel = value; RaisePropertyChanged(); } }
        //重新读取命令
        public ICommand OnReloadCommand { get; set; }
        //停止同步命令
        public ICommand OnSyncStopComand { get; set; }

        //是否有设备接入
        private bool _IsDeviceConnected;
        public bool IsDeviceConnected { get => _IsDeviceConnected; set { _IsDeviceConnected = value; RaisePropertyChanged(); } }
        //所有设备列表
        public ObservableCollection<MediaDevice> DeviceListData { get; set; }  = new ObservableCollection<MediaDevice>();
        //标识设备是否选中
        //public bool IsDeviceSelected { get => SelectedDevice == null; set { RaisePropertyChanged(); } }
        //选中的设备
        private MediaDevice _SelectedDevice;
        public MediaDevice SelectedDevice { get => _SelectedDevice; 
            set 
            { 
                _SelectedDevice = value; 
                RaisePropertyChanged(); 
                ComboBoxDeviceSelectionChanged();
                NotifyUIStep();
            } 
        }

        //可用的驱动列表
        public ObservableCollection<DeviceDriverViewModel> DriverListData { get; set; } = new ObservableCollection<DeviceDriverViewModel>();
        //标识驱动是否选中
        //public bool IsDriverSelected { get => SelectedDevice == null; set { RaisePropertyChanged(); } }
        //选中的驱动器
        private DeviceDriverViewModel _SelectedDriver;
        public DeviceDriverViewModel SelectedDriver { get => _SelectedDriver; 
            set 
            { 
                _SelectedDriver = value; 
                RaisePropertyChanged();
                NotifyUIStep();
                ComboBoxDriverSelectionChanged();
            } 
        }

        //UI阶段展示
        public int Step { 
            get
            {
                if (IsInDesignMode) return 2;
                //第三阶段, 需要展示同步文件列表
                if (IsDeviceConnected && SelectedDevice != null && SelectedDriver != null) return 2;
                //第二阶段, 需要展示设备选择列表
                if (IsDeviceConnected) return 1;
                //默认阶段, 仅展示图标
                return 0;
            }
        }
        private void NotifyUIStep() { RaisePropertyChanged(nameof(Step)); }

        //监听USB
        private UsbMonitorManager UsbMonitor;

        //待同步的任务列表
        private static ConcurrentQueue<DeviceSyncTaskArgs> mSyncTaskQueue = new ConcurrentQueue<DeviceSyncTaskArgs>();
        //标识是否正在执行同步
        public bool IsFileSyncing { get => mSyncFileBgWorker.IsBusy; }
        private void NotifyFileSyncStatus() { RaisePropertyChanged(nameof(IsFileSyncing)); }

        //数据库对象
        private readonly MediaSyncConfigRepo _MediaSyncConfigRepo = MediaSyncConfigRepo.Instance;

        //读取文件的Worker
        private readonly BackgroundWorker mReadDeviceFileWorker = new BackgroundWorker()
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };
        //检查设备连接的Worker
        private readonly BackgroundWorker mCheckDeviceBgWorker = new BackgroundWorker()
        {
            WorkerReportsProgress = false,
            WorkerSupportsCancellation = true
        };
        //同步文件的worker
        private readonly BackgroundWorker mSyncFileBgWorker = new BackgroundWorker()
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };


        public DeviceSyncViewModel()
        {
            OnReloadCommand = new RelayCommand(OnReloadClick);
            OnSyncStopComand = new RelayCommand(OnStopSyncClick);

            mReadDeviceFileWorker.DoWork += new DoWorkEventHandler(DoReadDeviceFileTask);
            mReadDeviceFileWorker.ProgressChanged += new ProgressChangedEventHandler(OnReadDeviceFileProgress);
            mReadDeviceFileWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnReadDeviceFileTaskComplate);

            mCheckDeviceBgWorker.DoWork += new DoWorkEventHandler(DoCheckDeviceTask);
            mCheckDeviceBgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CheckDeviceTaskComplate);

            mSyncFileBgWorker.DoWork += new DoWorkEventHandler(DoSyncFileTask);
            mSyncFileBgWorker.ProgressChanged += new ProgressChangedEventHandler(OnSyncFileProgress);
            mSyncFileBgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SyncFileTaskComplate);

            if(!IsInDesignMode)
            {
                UsbMonitor = new UsbMonitorManager(Application.Current.MainWindow);
                //this.mUsbMonitor.UsbPort += OnUsb;
                UsbMonitor.UsbDeviceInterface += OnUsb;
                //this.mUsbMonitor.UsbChanged += OnUsb;
                CheckDevice(false);
            }

            //test data
            //ListData.Add(new FileItemsCompareViewModel
            //{
            //    MobileFolderPath = "DCIM",
            //    PcFolderPath = "C:/Image",
            //    PcFileItems = new FileItemsListViewModel
            //    {
            //        SyncDeviceType = SyncDeviceType.PC,
            //        FolderPath = "C:/Image",
            //        FileItems = new ObservableCollection<FileItemViewModel>()
            //        {
            //            new FileItemViewModel
            //            {
            //                Name = "MockName",
            //                ContentType = SyncContentType.OTHER
            //            }
            //        }
            //    }
            //});
        }

        /// <summary>
        /// 使用 UsbMonitor 监听USB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUsb(object sender, UsbEventArgs e)
        {
            var deviceInterface = e.ToString().Split(' ')[3].Split('=')[1];
            //只关注WPD ( 移动设备MTP连接 )
            if (!deviceInterface.Equals("WPD")) return;
            CheckDevice(false);
            //if (e.Action == UsbDeviceChangeEvent.Arrival)
            //{
            //    CheckDevice(true);
            //}
            //if (e.Action == UsbDeviceChangeEvent.RemoveComplete)
            //{
            //    CheckDevice(false);
            //}
        }

        /// <summary>
        /// 刷新按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReloadClick()
        {
            if (SelectedDevice != null)
            {
                //如果有选中的驱动, 则直接刷新文件
                ComboBoxDriverSelectionChanged();
            }
            else
            {
                //没有则重新读取设备
                CheckDevice(true);
            }
        }

        /// <summary>
        /// 停止同步
        /// </summary>
        private void OnStopSyncClick()
        {
            //发送提示文本
            //SetProgress(null, new BubbleTipViewModel
            //{
            //    AlertLevel = AlertLevel.RUN,
            //    Text = "正在停止.."
            //});
            if (mSyncFileBgWorker.IsBusy)
                mSyncFileBgWorker.CancelAsync();
            while (mSyncTaskQueue.Count > 0)
            {
                var item = new DeviceSyncTaskArgs();
                _ = mSyncTaskQueue.TryDequeue(out item);
            }
            if (mReadDeviceFileWorker.IsBusy)
                mReadDeviceFileWorker.CancelAsync();
        }

        #region 读取文件后台任务
        /// <summary>
        /// 读取文件工作任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoReadDeviceFileTask(object sender, DoWorkEventArgs workArgs)
        {
            var resultNotify = new BubbleTipViewModel()
            {
                Text = "文件读取完成"
            };
            workArgs.Result = resultNotify;
            //多设备且未手动选择设备
            if (null == SelectedDevice)
            {
                resultNotify.Text = "没有待同步的目标设备";
                resultNotify.AlertLevel = AlertLevel.ERROR;
                return;
            }
            //多盘符且未手动选择盘符
            if (null == SelectedDriver)
            {
                resultNotify.Text = "没有待同步的目标盘符";
                resultNotify.AlertLevel = AlertLevel.ERROR;
                return;
            }

            //进度通知
            var progressEvent = new DeviceSyncReadProgressItem
            {
                ProgressType = ReadSyncFileTaskProgressType.TEXT_ONLY,
                Notify = new BubbleTipViewModel
                {
                    Text = "正在读取路径配置...",
                    AlertLevel = AlertLevel.RUN
                }
            };
            mReadDeviceFileWorker.ReportProgress(0, progressEvent);
            //从数据库读取
            var pathConfigs = _MediaSyncConfigRepo.FindConfig(true);
            if (DataUtil.IsEmptyCollection(pathConfigs))
            {
                resultNotify.Text = "无法读取路径配置";
                resultNotify.AlertLevel = AlertLevel.WARN;
                return;
            }
            progressEvent.Notify.Text = "正在连接到设备...";
            mReadDeviceFileWorker.ReportProgress(0, progressEvent);
            //筛选WPD设备
            using (SelectedDevice)
            {
                SelectedDevice.Connect();
                foreach (var e in pathConfigs)
                {
                    bool isPcPathExists = Directory.Exists(e.PcPath),
                    isMobilePathExists = SelectedDevice.DirectoryExists(Path.Combine(SelectedDriver.NameView, e.MobilePath));

                    if (!isPcPathExists && !isMobilePathExists)
                        //如果两端都不存在目标文件夹, 则跳过
                        continue;

                    progressEvent.Notify.Text = string.Format("正在读取文件列表[{0}]...", e.PcPath);
                    mReadDeviceFileWorker.ReportProgress(0, progressEvent);
                    //读取PC目录下的文件列表
                    var pcFiles = isPcPathExists ? FileUtil.GetFileNameFromFullPath(Directory.GetFiles(e.PcPath)) : new string[0];
                    //读取移动设备下的文件列表
                    //var xxx = args.Device.GetFileSystemEntries(Path.Combine(args.MediaDrive.NameView, e.MobilePath));
                    //isMobilePathExists ? FileUtil.GetFileNameFromFullPath(args.Device.GetFiles(Path.Combine(args.MediaDrive.NameView, e.MobilePath))) : 
                    var mobileFileList = new List<string>();
                    if (isMobilePathExists)
                    {
                        var fullMobileFolderPath = Path.Combine(SelectedDriver.NameView, e.MobilePath);
                        var fullNames = SelectedDevice.EnumerateFiles(fullMobileFolderPath);
                        foreach (var fullName in fullNames)
                        {
                            //使用Win函数读取文件名
                            var winSupportName = Path.GetFileName(fullName);
                            //通过路径裁剪方式获得文件名
                            var substringName = fullName.Replace(fullMobileFolderPath + Path.DirectorySeparatorChar, string.Empty);
                            //如果两种方式取得的文件名不一致, 说明这个文件名在windows上是不合法的
                            if (!winSupportName.Equals(substringName))
                            {
                                //如果路径包含windows路径符, 记录错误并跳过
                                if (substringName.Contains(Path.DirectorySeparatorChar))
                                {
                                    //发送win10通知, 提醒人工处理此文件
                                    //通知官方文档  https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/send-local-toast-desktop
                                    new ToastContentBuilder()
                                        .AddText("文件收集失败")
                                        .AddText("不支持的文件名")
                                        .AddText(fullName)
                                        .Show();
                                    continue;
                                }
                                //取得后缀名
                                var extendName = Path.GetExtension(substringName);
                                //重命名移动设备上的文件名, 使用随机文件名
                                var newName = string.Format("{0}{1}{2}{3}", DateTime.Now.ToString("yyyyMMddHHmmss"), "_auto_rename_", RandomUtil.MakeSring(false, 6), extendName);
                                SelectedDevice.Rename(fullName, newName);
                                mobileFileList.Add(newName);
                                //发送win10通知, 提醒文件已改名
                                new ToastContentBuilder()
                                     .AddText("文件重命名提醒")
                                     .AddText(fullMobileFolderPath)
                                     .AddText(substringName + " -> " + newName)
                                     .Show();
                                continue;
                            }
                            mobileFileList.Add(winSupportName);
                        }
                    }
                    var mobileFilesArr = mobileFileList.ToArray();
                    //找出差异文件
                    var filesOnlyInMobile = DataUtil.FindDiffEls(pcFiles, mobileFilesArr);
                    var filesOnlyInPc = DataUtil.FindDiffEls(mobileFilesArr, pcFiles);
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
                            SelectedDevice.Disconnect();
                            resultNotify.Text = "已停止";
                            return;
                        }
                    }
                    if (!DataUtil.IsEmptyCollection(filesOnlyInMobile))
                    {
                        if (!CollectAndNotify(filesOnlyInMobile, workArgs, e, SyncDeviceType.PHONE))
                        {
                            SelectedDevice.Disconnect();
                            resultNotify.Text = "已停止";
                            return;
                        }
                    }
                }
                SelectedDevice.Disconnect();
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
            foreach (var fileName in files)
            {
                if (mReadDeviceFileWorker.CancellationPending)
                {
                    return false;
                }
                var targetFolderPath = deviceType == SyncDeviceType.PC ? e.PcPath : e.MobilePath;
                //汇报文本进度
                mReadDeviceFileWorker.ReportProgress(0, new DeviceSyncReadProgressItem
                {
                    ProgressType = ReadSyncFileTaskProgressType.TEXT_ONLY,
                    Notify = new BubbleTipViewModel
                    {
                        AlertLevel = AlertLevel.RUN,
                        Text = string.Format("正在收集差异文件  [{0}]", Path.Combine(targetFolderPath, fileName))
                        //nowProgress = 99
                    }
                });
                var item = CreateSyncItem(fileName, targetFolderPath, deviceType, SelectedDevice, SelectedDriver.NameView);
                //汇报文件进度
                mReadDeviceFileWorker.ReportProgress(0, new DeviceSyncReadProgressItem
                {
                    ProgressType = ReadSyncFileTaskProgressType.FILE_READED,
                    PcFolderNameView = e.PcPath,
                    MobileFolderNameView = e.MobilePath,
                    FileItem = item,
                    FileSource = deviceType
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
        private FileItemViewModel CreateSyncItem(string fileName, string folderPath, SyncDeviceType source, MediaDevice device, string driverName)
        {
            //判断文件是否存在于设备
            var item = new FileItemViewModel
            {
                Name = fileName,
                DeviceSyncViewModel = this
            };
            var mediaType = MediaUtil.GetMediaTypeByName(fileName);
            item.ContentType = mediaType;
            if (mediaType == SyncContentType.IMAGE || mediaType == SyncContentType.VIDEO)
            {
                if (source == SyncDeviceType.PHONE)
                {
                    var fileInfo = device.GetFileInfo(Path.Combine(driverName, folderPath, fileName));
                    var imgByte = GetMediaFileThumbnail(fileInfo);
                    if (null != imgByte)
                        item.PreviewImg = new ByteArray(imgByte);
                }
                else
                {
                    var imgByte = ImageUtil.CreateThumbnail(Path.Combine(folderPath, fileName), 100, 100);
                    if (null != imgByte)
                        item.PreviewImg = new ByteArray(ImageUtil.ImageToByteArray(imgByte));
                }
            }
            if (mediaType == SyncContentType.AUDIO)
            {
                //PC文件直接读取
                if (source == SyncDeviceType.PC)
                {
                    item.PreviewImg = new ByteArray(MediaUtil.GetAudioFileAlbum(Path.Combine(folderPath, fileName)));
                }
                else
                {
                    //手机文件, 先复制到临时文件夹, 再读取
                    var winTempFolder = Path.GetTempPath();
                    if (!string.IsNullOrEmpty(winTempFolder))
                    {
                        var tempFullPath = Path.Combine(winTempFolder, fileName);
                        if (System.IO.File.Exists(tempFullPath))
                        {
                            //已存在则直接读取
                            item.PreviewImg = new ByteArray(MediaUtil.GetAudioFileAlbum(tempFullPath));
                        }
                        else
                        {
                            //不存在则复制
                            var fileInfo = device.GetFileInfo(Path.Combine(driverName, folderPath, fileName));
                            fileInfo.CopyTo(tempFullPath);
                            item.PreviewImg = new ByteArray(MediaUtil.GetAudioFileAlbum(tempFullPath));
                        }
                    }
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
            var newItem = (DeviceSyncReadProgressItem)e.UserState;
            if (newItem.ProgressType == ReadSyncFileTaskProgressType.TEXT_ONLY)
            {
                //发送状态栏提示信息
                SetProgress(null, newItem.Notify);
                return;
            }
            //找到已添加的父列表
            var listItem = ListData.Where((el) => el.PcFolderPath.Equals(newItem.PcFolderNameView)).FirstOrDefault();
            if (null == listItem)
            {
                //没有则新建一个项
                var listItemPcFileList = new FileItemsListViewModel
                {
                    SyncDeviceType = SyncDeviceType.PC,
                    FolderPath = newItem.PcFolderNameView
                };
                var listItemMoblieFileList = new FileItemsListViewModel
                {
                    SyncDeviceType = SyncDeviceType.PHONE,
                    FolderPath = newItem.MobileFolderNameView
                };
                AddFolderListFileItem(listItemPcFileList, listItemMoblieFileList, newItem);
                ListData.Add(new FileItemsCompareViewModel
                {
                    PcFolderPath = newItem.PcFolderNameView,
                    MobileFolderPath = newItem.MobileFolderNameView,
                    PcFileItems = listItemPcFileList,
                    MobileFileItems = listItemMoblieFileList,
                    DeviceSyncViewModel = this
                });
            }
            else
            {
                //有则直接添加
                AddFolderListFileItem(listItem.PcFileItems, listItem.MobileFileItems, newItem);
            }
        }

        /// <summary>
        /// 区分添加子项目
        /// </summary>
        /// <param name="pcCollect"></param>
        /// <param name="mobileCollect"></param>
        /// <param name="newItem"></param>
        private void AddFolderListFileItem(FileItemsListViewModel pcCollect,
            FileItemsListViewModel mobileCollect,
            DeviceSyncReadProgressItem newItem)
        {
            if (newItem.FileSource == SyncDeviceType.PC)
            {
                newItem.FileItem.FileItemsListViewModel = pcCollect;
                pcCollect.FileItems.Add(CreateSyncItemViewModel(newItem.FileItem));
            }
            else
            {
                newItem.FileItem.FileItemsListViewModel = mobileCollect;
                mobileCollect.FileItems.Add(CreateSyncItemViewModel(newItem.FileItem));
            }
        }

        /// <summary>
        /// 创建视图模型项
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private FileItemViewModel CreateSyncItemViewModel(FileItemViewModel item)
        {
            return item;
            //return new FileItemViewModel
            //{
            //    Name= item.Name,
            //    FolderView = item.FolderView,
            //    SourceView = item.SourceView,
            //    ImgByte = item.ImgByte,
            //};
        }

        /// <summary>
        /// 读取设备文件任务完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReadDeviceFileTaskComplate(object sender, RunWorkerCompletedEventArgs e)
        {
            var notify = (BubbleTipViewModel)e.Result;
            SetProgress(100, notify);
        }


        /// <summary>
        /// 设备选择变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxDeviceSelectionChanged()
        {
            if (null == SelectedDevice) return;

            SelectedDevice.Connect(enableCache: false);
            //SelectedDevice.Connect(MediaDeviceAccess.GenericRead | MediaDeviceAccess.GenericWrite, MediaDeviceShare.Default, false);
            var deviceDrivers = SelectedDevice.GetDrives();
            if (null == deviceDrivers)
            {
                SelectedDevice.Disconnect();
                return;
            }
            DriverListData.Clear();
            for (var i = 0; i < deviceDrivers.Length; i++)
            {
                DriverListData.Add(new DeviceDriverViewModel()
                {
                    ValueView = deviceDrivers[i].Name,
                    NameView = deviceDrivers[i].Name
                });
            }
            SelectedDevice.Disconnect();
            if (DriverListData.Count == 1)
            {
                SelectedDriver = DriverListData[0];
            }
        }
        #endregion


        /// <summary>
        /// 设备驱动选择变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxDriverSelectionChanged()
        {
            //IsDeviceSelected = true;
            //DeviceSyncListView.Visibility = Visibility.Visible;
            //DeviceChoosePanel.Visibility = Visibility.Collapsed;
            if (mReadDeviceFileWorker.IsBusy)
            {
                mReadDeviceFileWorker.CancelAsync();
                return;
            }
            //清空当前列表
            ListData.Clear();
            mReadDeviceFileWorker.RunWorkerAsync();
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
            var mediaDevices = MediaDevice.GetDevices() as List<MediaDevice>;
            e.Result = mediaDevices;
            //Console.WriteLine("当前WPD数量: " + mediaDevices.Count());
            /**
            var returnList = new List<MediaDevice>();
            e.Result = returnList;
            //是否需要等待的参数
            var delay = e.Argument == null ? false : (bool) e.Argument;
            for(var i = 0; i < 25; i++)
            {
                //标识了取消则直接返回
                if(mCheckDeviceBgWorker.CancellationPending) return;

                var mediaDevices = MediaDevice.GetDevices() as List<MediaDevice>;
                if (mediaDevices.Count >0)
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
            **/
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
                //SelectedDevice = null;
                //SelectedDriver = null;
                //IsDeviceSelected = false;
                //IsDriverSelected = false;
                IsDeviceConnected = false;
                DeviceListData.Clear();
                DriverListData.Clear();
                //DefaultHintText.Visibility = Visibility.Visible;
                //DeviceChoosePanel.Visibility = Visibility.Collapsed;
                //DeviceSyncListView.Visibility = Visibility.Collapsed;
                //DriverListCombobox.ItemsSource = null;
                //DeviceListCombobox.ItemsSource = null;
                return;
            }
            //如果有设备, 检查此前选择的设备是否还存在
            if (null != SelectedDevice)
            {
                var nowDevice = mediaDevices.Find((el) => el.DeviceId.Equals(SelectedDevice.DeviceId));
                if (nowDevice != null)
                {
                    //如果选择的设备还存在, 不做变化
                    return;
                }
            }

            //如果选择的设备已经不存在了, 需要重新选择
            mediaDevices.ForEach(d => DeviceListData.Add(d));

            //如果只有一个设备
            if (mediaDevices.Count == 1)
            {
                IsDeviceConnected = true;
                SelectedDevice = DeviceListData[0];
                //Step = 1;
                return;
            }
            //如果有多个设备
            IsDeviceConnected = true;
            //IsDeviceSelected = false;
            //Step = 1;
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
            //int totalProgress = 0;
            var progressMessage = new BubbleTipViewModel()
            {
                AlertLevel = AlertLevel.RUN
            };
            var isTaskStop = false;
            //当前执行的队列编号
            var curTaskNo = 1;
            //当前队列最大元素数量
            var maxedQueueCount = mSyncTaskQueue.Count;
            while (mSyncTaskQueue.Count > 0)
            {
                var isPick = mSyncTaskQueue.TryDequeue(out DeviceSyncTaskArgs arg);
                if (!isPick) continue;
                //筛选WPD设备
                using (var device = SelectedDevice)
                {
                    device.Connect(enableCache: false);

                    //检查目标文件夹是否存在, 不存在则创建
                    if (!Directory.Exists(arg.Item.PcFolderPath)) FileUtil.CreateFolder(arg.Item.PcFolderPath);
                    var targetMobileFolder = Path.Combine(SelectedDriver.ValueView, arg.Item.MobileFolderPath);
                    if (!device.DirectoryExists(targetMobileFolder)) device.CreateDirectory(targetMobileFolder);

                    //将两端的文件合并到一个集合
                    //var allItems = arg.Item.PcFileItems.FileItems.ToList();
                    //allItems.AddRange(arg.Item.MobileFileItems.FileItems.ToList());
                    var fileCounter = 1;
                    var allFileCount = arg.Item.PcFileItems.FileItems.Count + arg.Item.MobileFileItems.FileItems.Count;
                    FileItemsListViewModel[] twoClientFileListArray = new FileItemsListViewModel[]
                    {
                        arg.Item.PcFileItems,
                        arg.Item.MobileFileItems
                    };
                    foreach (FileItemsListViewModel lv in twoClientFileListArray)
                    {
                        var lvFileItems = lv.FileItems.ToList();
                        foreach (var syncFile in lvFileItems)
                        {
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
                            //每个队列的分片进度
                            //计算每个队列的分片进度
                            double eachBaseProgress = 100 / curQueueCount;
                            //每个队列的基础进度, 从0开始, 以分片进度递增
                            double baseProgress = 100 - (mSyncTaskQueue.Count + 1) * eachBaseProgress;
                            //计算每个文件的分片进度
                            double eachFileProgress = eachBaseProgress / allFileCount;
                            //文件的执行进度, 从0开始递增
                            double fileProgress = eachFileProgress * fileCounter - eachFileProgress;
                            //总进度 = 基础进度 + 文件进度
                            progressMessage.Text = string.Format("主任务 [{0}/{1}], 子任务 [{2}/{3}], 正在复制 [ {4} ]", curTaskNo, curQueueCount, fileCounter, allFileCount, syncFile.Name);
                            arg.ProgressMessage = progressMessage;
                            arg.ProgressType = SyncTaskProgressType.SUB_ITEM_FINISH;
                            mSyncFileBgWorker.ReportProgress(Convert.ToInt32(baseProgress + fileProgress), arg);
                            //Thread.Sleep(1000); 
                            //执行同步
                            if (lv.SyncDeviceType == SyncDeviceType.PC)
                            {
                                //电脑端, 需要复制到移动端
                                var targetFolder = Path.Combine(SelectedDriver.ValueView, arg.Item.MobileFolderPath);
                                using (FileStream stream = System.IO.File.OpenRead(Path.Combine(arg.Item.PcFolderPath, syncFile.Name)))
                                {
                                    device.UploadFile(stream, Path.Combine(targetFolder, syncFile.Name));
                                }
                            }
                            else
                            {
                                MediaFileInfo fileInfo = device.GetFileInfo(Path.Combine(SelectedDriver.ValueView, arg.Item.MobileFolderPath, syncFile.Name));
                                fileInfo.CopyTo(Path.Combine(arg.Item.PcFolderPath, syncFile.Name));
                            }
                            fileProgress = eachFileProgress * fileCounter;
                            progressMessage.Text = string.Format("主任务 [{0}/{1}], 子任务 [{2}/{3}], 正在复制 [ {4} ]", curTaskNo, curQueueCount, fileCounter, allFileCount, syncFile.Name);
                            arg.Source = lv.SyncDeviceType;
                            arg.DeviceSyncItem = syncFile;
                            mSyncFileBgWorker.ReportProgress(Convert.ToInt32(baseProgress + fileProgress), arg);
                            fileCounter++;
                        }
                    }
                    device.Disconnect();
                }
                arg.IsOk = !isTaskStop;
                arg.ProgressType = SyncTaskProgressType.QUEUE_FINISH;
                if (!isTaskStop)
                {
                    //一个队列执行完之后, 发送当前队列进度
                    //progressMessage.Text = string.Format("主任务 [{0}/{1}]同步完成", curTaskNo, maxedQueueCount);
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
            if (SyncTaskProgressType.SUB_ITEM_FINISH == arg.ProgressType)
            {
                if (SyncDeviceType.PC == arg.Source)
                    arg.Item.PcFileItems.FileItems.Remove(arg.DeviceSyncItem);
                else
                    arg.Item.MobileFileItems.FileItems.Remove(arg.DeviceSyncItem);
                SetProgress(e.ProgressPercentage, arg.ProgressMessage);
            }
            else
            {
                if (arg.IsOk)
                    ListData.Remove(arg.Item);
                arg.Item.IsBusy = false;
            }
        }

        /// <summary>
        /// 同步文件后台任务执行完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncFileTaskComplate(object sender, RunWorkerCompletedEventArgs e)
        {
            var rb = (bool)e.Result;
            SetProgress(100, new BubbleTipViewModel
            {
                Text = rb ? "同步完成" : "任务中止",
                AlertLevel = AlertLevel.INFO
            });
            //NotifyUIStep();
            NotifyFileSyncStatus();
        }
        #endregion

        /// <summary>
        /// 添加任务并开始执行文件同步
        /// </summary>
        /// <param name="arg"></param>
        public void AddItemToQueueAndStartTask(FileItemsCompareViewModel compareViewModel)
        {
            //如果正在收集文件, 则不允许操作
            if (mReadDeviceFileWorker.IsBusy)
            {
                compareViewModel.IsBusy = false;
                return;
            }
            mSyncTaskQueue.Enqueue(new DeviceSyncTaskArgs
            {
                Item = compareViewModel
            });
            if (mSyncFileBgWorker.IsBusy)
            {
                //任务正在运行则直接返回
                return;
            }
            //如果任务没有运行, 则开启一个任务
            mSyncFileBgWorker.RunWorkerAsync();
            NotifyFileSyncStatus();
        }

        /// <summary>
        /// 设置消息
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="infoVm"></param>
        private void SetProgress(double? progress, BubbleTipViewModel infoVm)
        {
            if (null != progress)
            {
                ProgressValue = progress.GetValueOrDefault();
            }
            if (!string.IsNullOrEmpty(infoVm.Text))
            {
                ProgressText = infoVm.Text;
                ProgressAlertLevel = infoVm.AlertLevel;
            }
        }
    }
}
