using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Common;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 乐谱管理ViewModel
    /// </summary>
    public class Tan8SheetReponsitoryViewModel : ViewModelBase
    {
        //乐谱列表对象
        public ObservableCollection<SheetItemViewModel> ListData { get; set; } = new ObservableCollection<SheetItemViewModel>();
        //下载中的乐谱列表
        private readonly List<SheetItemViewModel> _DownloadSheetItems = new List<SheetItemViewModel>();
        //选中的列表对象
        private SheetItemViewModel _SelectedListData;
        public SheetItemViewModel SelectedListData { get => _SelectedListData; set { _SelectedListData = value; RaisePropertyChanged(); } }
        //是否忙
        private bool _IsBusy = false;
        public bool IsBusy { get { return _IsBusy; } set { _IsBusy = value; RaisePropertyChanged(); } }
        //是否没有数据
        public bool IsEmpty { get { return ListData.Count == 0; } set { RaisePropertyChanged(); } }
        //过滤文本
        public string FilterText { get; set; }
        //是否正在执行下载任务
        public bool IsDownloading { get => _DownloadTaskWorker.IsBusy; set => RaisePropertyChanged(); }

        //刷新命令
        public ICommand OnRefreshCommand { get; set; }
        //中止下载命令
        public ICommand OnStopDownloadCommand { get; set; }

        //乐谱库数据库
        private readonly Tan8SheetsRepo _Tan8SheetRepo = Tan8SheetsRepo.Instance;
        private readonly Tan8SheetCrawlTaskRepo _SheetCrawlTaskRepo = Tan8SheetCrawlTaskRepo.Instance;
        private readonly Tan8SheetCrawlRecordRepo _Tan8SheetCrawlRecordRepo = Tan8SheetCrawlRecordRepo.Instance;

        //停止任务的委托
        private event StopAllTan8SheetDownload _StopBtnClickHandler;

        #region 其他
        //允许最大的任务数量
        private readonly int _MaxTaskNum = 3;
        //已经下载完成的数量(包括成功/失败)
        //private int _DownloadFinishNum = 0;
        //当前正在进行中的任务数量
        private int _CurTaskNum = 0;
        //需要下载的任务列表
        private readonly Queue<int> mTaskQueue = new Queue<int>();
        //下载任务管理器
        private readonly BackgroundWorker _DownloadTaskWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };
        #endregion

        public Tan8SheetReponsitoryViewModel()
        {
            OnRefreshCommand = new RelayCommand(Load);
            OnStopDownloadCommand = new RelayCommand(OnStopDownload);

            //乐谱下载队列任务
            _DownloadTaskWorker.DoWork += new DoWorkEventHandler(DoDownloadTaskCreate);
            _DownloadTaskWorker.ProgressChanged += new ProgressChangedEventHandler(ReportDownloadTaskCreateProgress);
            _DownloadTaskWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DownloadTaskCreateComplate);
        }

        /// <summary>
        /// 加载所有曲谱
        /// </summary>
        /// <param name="kw">查询关键字</param>
        public async void Load()
        {
            IsBusy = true;
            var dataList = await LoadAsync(FilterText);
            //刷新则清空所有记录后重新添加
            ListData.Clear();
            if (DataUtil.IsEmptyCollection(dataList))
            {
                InsertDownloadList();
                IsEmpty = true;
                IsBusy = false;
                return;
            }
            dataList.ForEach(e => ListData.Add(CreateViewInstance(e)));
            InsertDownloadList();
            IsEmpty = false;
            IsBusy = false;
        }

        /// <summary>
        /// 如果不带搜索条件, 且有正在下载的任务, 将其添加到乐谱列表
        /// </summary>
        private void InsertDownloadList()
        {
            if(string.IsNullOrEmpty(FilterText) && !DataUtil.IsEmptyCollection(_DownloadSheetItems))
            {
                _DownloadSheetItems.ForEach(e => ListData.Insert(0, e));
            }
        }

        /// <summary>
        /// 发送停止下载消息
        /// </summary>
        private void OnStopDownload()
        {
            _StopBtnClickHandler?.Invoke(ListData);
            _DownloadTaskWorker.CancelAsync();
        }

        /// <summary>
        /// 异步加载数据
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        private async Task<List<Tan8Sheet>> LoadAsync(string keyword)
        {
            return await Task.Run(() =>
            {
                return _Tan8SheetRepo.Find(keyword, 1, 500);
            });
        }


        /// <summary>
        /// 根据乐谱创建view实例
        /// </summary>
        /// <param name="tan8Sheet"></param>
        /// <returns></returns>
        private SheetItemViewModel CreateViewInstance(Tan8Sheet tan8Sheet)
        {
            var imgDir = Path.Combine(Settings.Default.Tan8HomeDir, tan8Sheet.Ypid.ToString(), ApplicationConstant.DEFAULT_COVER_NAME);
            return new SheetItemViewModel()
            {
                //对于不存在cover的路径使用默认图片
                Cover = File.Exists(imgDir) ? imgDir : null,
                //Cover = imgDir,
                Name = tan8Sheet.Name,
                Id = tan8Sheet.Ypid,
                Ver = tan8Sheet.Ver,
                Star = tan8Sheet.Star,
                YpCount = tan8Sheet.YpCount
            };
        }

        /// <summary>
        /// 创建下载任务项目
        /// </summary>
        /// <param name="crawlArg"></param>
        /// <returns></returns>
        private SheetItemViewModel CreateTaskViewInstance(Tan8SheetCrawlArg crawlArg)
        {
            var taskItemInstance = new SheetItemViewModel()
            {
                Name = string.IsNullOrEmpty(crawlArg.Name) ? string.Format("新乐谱任务 [{0}]", crawlArg.Ypid.GetValueOrDefault()) : crawlArg.Name,
                IsTempName = string.IsNullOrEmpty(crawlArg.Name),
                Id = crawlArg.Ypid.GetValueOrDefault(),
                Star = 0,
                Ver = crawlArg.Ver,
                UseProxy = crawlArg.UseProxy,
                YpCount = 0,
                IsWorking = true,
                ProgressText = "等待中...",
                AutoDownload = crawlArg.AutoDownload,
                //单个乐谱下载完成事件
                DownloadFinishAction = OnSheetItemDownloadComplete
            };
            _StopBtnClickHandler += taskItemInstance.OnStopDownloadEvent;
            return taskItemInstance;
        }

        /// <summary>
        /// 单个乐谱下载完成触发事件
        /// </summary>
        /// <param name="ypid"></param>
        private void OnSheetItemDownloadComplete(int ypid, bool success, bool isWorking)
        {
            //增加数量, 每完成一个子任务, 完成数量+1
            //Interlocked.Increment(ref _DownloadFinishNum);
            //从下载列表中删除
            _DownloadSheetItems.Remove(_DownloadSheetItems.Where(e => e.Id == ypid).FirstOrDefault());
            Interlocked.Decrement(ref _CurTaskNum);
            //下载失败的, 还要从展示的列表中移除 (删不掉, 可能是线程问题)
            //if(!success)
            //{
            //    var listDataItem = ListData.Where(e => e.Id == ypid).FirstOrDefault();
            //    if (null != listDataItem)
            //    {
            //        ListData.Remove(listDataItem);
            //    }
            //}
            //不是中止的, 不论下载成功/失败, 都发送通知
            if(isWorking)
            {
                //发送下载完成通知
                Messenger.Default.Send(ypid, ApplicationConstant.TAN8_DOWNLOAD_COMPLETE);
            }
        }

        /// <summary>
        /// 删除条目
        /// </summary>
        /// <param name="itemVm"></param>
        public void DeleteItem(SheetItemViewModel itemVm)
        {
            //释放文件资源
            ListData.Remove(itemVm);

            //删除文件夹
            if (!string.IsNullOrEmpty(itemVm.Name))
            {
                FileUtil.DeleteDirWithName(Settings.Default.Tan8HomeDir, itemVm.Id.ToString());
            }

            //删除数据库数据
            _Tan8SheetRepo.DeleteById(itemVm.Id);

            //如果没有曲谱了, 则展示默认按钮
            if (DataUtil.IsEmptyCollection(ListData)) IsEmpty = true;
        }

        /// <summary>
        /// 下载弹8曲谱
        /// 开启后台下载曲谱任务
        /// </summary>
        /// <param name="crawlArg">曲谱对象</param>
        /// <returns>公共函数返回结果</returns>
        public void TriggerTan8DownLoadTask(Tan8SheetCrawlArg crawlArg)
        {
            if(_DownloadTaskWorker.IsBusy)
            {
                //如果有正在执行的任务, 取消并提醒稍后重试
                _DownloadTaskWorker.CancelAsync();
                Messenger.Default.Send(new BubbleTipViewModel
                {
                    AlertLevel = AlertLevel.WARN,
                    Text = "任务取消中, 稍后重试"
                });
                return;
            }
            //校验基本参数
            if (null == crawlArg.Ypid)
            {
                //如果有待重新下载的任务, 优先下载
                var taskYpids = _SheetCrawlTaskRepo.FindAllTaskYpid();
                mTaskQueue.Clear();
                taskYpids.ForEach(e => mTaskQueue.Enqueue(e));
                var lastYpid = 1;
                if (mTaskQueue.Count > 0)
                {
                    crawlArg.IsQueueTask = true;
                    lastYpid = mTaskQueue.Dequeue();
                }
                else
                {
                    var recordLastYpid = _Tan8SheetCrawlRecordRepo.FindLastCrawlYpid();
                    if (recordLastYpid > 0)
                    {
                        lastYpid = recordLastYpid + 1;
                    }
                }
                crawlArg.Ypid = lastYpid;
                crawlArg.AutoDownload = true;
            }

            //重设下载完成数量
            //_DownloadFinishNum = 0;
            _CurTaskNum = 0;
            //开启下载任务
            _DownloadTaskWorker.RunWorkerAsync(crawlArg);
            IsDownloading = true;
        }

        /// <summary>
        /// 分发批量下载任务
        /// </summary>
        /// <param name="crawlArg"></param>
        public void DoDownloadTaskDispatche(Tan8SheetCrawlArg crawlArg)
        {
            var taskSheetItem = _DownloadSheetItems.Where(e => e.Id.Equals(crawlArg.Ypid)).FirstOrDefault();
            if (null == taskSheetItem || taskSheetItem.IsWorking == false)
            {
                //如果找不到目标下载项, 可能是在重启Flash播放器的过程中停止下载了, 需要关闭
                Tan8PlayUtil.Exit(crawlArg.Ypid.GetValueOrDefault());
                return;
            }
            taskSheetItem.SheetUrl = crawlArg.SheetUrl;
            taskSheetItem.DownLoadTan8MusicV2Task();
        }

        /// <summary>
        /// 返回下一个需要下载的乐谱ID
        /// 如果是重新下载, 返回下一个需要重新下载的任务, 如果没有了需要重新下载的, 则返回0
        /// 如果不是重新下载, 返回递增的任务ID
        /// </summary>
        /// <param name="crawlArg"></param>
        private void SetNextDownloadYpid(Tan8SheetCrawlArg crawlArg)
        {
            if (crawlArg.IsQueueTask)
            {
                //取下一个ID时, 删除上一个ID
                _Tan8SheetCrawlRecordRepo.DelByYpid(crawlArg.Ypid.GetValueOrDefault());
               if (DataUtil.IsEmptyCollection(mTaskQueue))
                {
                    OnStopDownload();
                    return;
                }
                crawlArg.Ypid = mTaskQueue.Dequeue();
                return;
            }
            crawlArg.Ypid++;
        }

        /// <summary>
        /// 执行下载管理任务分发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoDownloadTaskCreate(object sender, DoWorkEventArgs e)
        {
            var crawlArg = e.Argument as Tan8SheetCrawlArg;
            bool waiting = false;
            //任务未中断则一直循环
            while (!_DownloadTaskWorker.CancellationPending)
            {
                //检查任务数量
                if(_CurTaskNum >= _MaxTaskNum)
                {
                    //如果超过最大任务数, 等待之后再继续执行
                    Thread.Sleep(100);
                    continue;
                }
                //waiting = true表明已经有正在下载的任务, 且不是自动下载任务, Count == 0表明下载已经完成, 因此分发任务完成
                //如果是自动下载任务,  waiting不会=true
                if (waiting == true)
                {
                    if (_CurTaskNum > 0)
                    {
                        //如果任务未完成, 则继续等待
                        Thread.Sleep(100);
                        continue;
                    }
                    return;
                }
                //检查曲谱是否存在于数据库
                var isYpExist = _Tan8SheetRepo.IsYpidExist(crawlArg.Ypid.GetValueOrDefault());
                //存在直接下一个
                if (isYpExist)
                {               
                    if (crawlArg.AutoDownload)
                    {
                        //检查任务是否中断
                        if (_DownloadTaskWorker.CancellationPending) break;
                        SetNextDownloadYpid(crawlArg);
                        continue;
                    }
                    return;
                }
                //检查任务是否中断
                if (_DownloadTaskWorker.CancellationPending) break;
                //创建并添加到乐谱列表
                _DownloadTaskWorker.ReportProgress(0, new Tan8SheetCrawlArg
                {
                    Ypid = crawlArg.Ypid,
                    Name = crawlArg.Name,
                    UseProxy = crawlArg.UseProxy,
                    AutoDownload = crawlArg.AutoDownload,
                    Ver = crawlArg.Ver
                });
                Interlocked.Increment(ref _CurTaskNum);
                //检查任务是否中断
                //if (_DownloadTaskWorker.CancellationPending) break;
                //打开播放器, 触发主动下载
                Tan8PlayUtil.ExePlayById(crawlArg.Ypid.GetValueOrDefault(), 1, true);
                //如果不是自动下载, 则不增长下一个需要下载的ID, 直接循环等待子任务完成
                if (!crawlArg.AutoDownload)
                {
                    waiting = true;
                    continue;
                }
                //设置下一个需要下载的乐谱ID
                SetNextDownloadYpid(crawlArg);
            }
            //e.Result = ;
        }

        /// <summary>
        /// 下载任务总进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReportDownloadTaskCreateProgress(object sender, ProgressChangedEventArgs e)
        {
            //创建一个新的item, 开启下载任务
            var taskSheetItem = CreateTaskViewInstance(e.UserState as Tan8SheetCrawlArg);
            //如果当前乐谱列表显示全部, 即无关键字, 则添加到头部, 并在下载列表存储一份
            if(string.IsNullOrEmpty(FilterText) && IsBusy == false)
            {
                ListData.Insert(0, taskSheetItem);
            }
            _DownloadSheetItems.Insert(0, taskSheetItem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadTaskCreateComplate(object sender, RunWorkerCompletedEventArgs e)
        {
            //清理资源
            //_DownloadSheetItems.ForEach(item =>
            //{
            //    if (ListData.Contains(item))
            //        ListData.Remove(item);
            //});
            //_DownloadSheetItems.Clear();
            //提示结果
            //if (!string.IsNullOrEmpty((string)e.Result))
            //{

            //}
            IsDownloading = false;
        }
    }
}
