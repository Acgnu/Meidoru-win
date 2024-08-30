using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Common;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Utils;
using AcgnuX.WindowX.Dialog;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SharedLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 曲谱下载记录
    /// </summary>
    public class PianoScoreDownloadRecordViewModel : ViewModelBase
    {
        //正在下载任务列表
        public ObservableCollection<SheetItemDownloadViewModel> DownloadingData { get; set; } = new ObservableCollection<SheetItemDownloadViewModel>();
        //历史纪录列表数据对象
        public ObservableCollection<Tan8SheetDownloadRecord> DownloadRecordList { get; set; } = new ObservableCollection<Tan8SheetDownloadRecord>();
        //筛选框对象
        public ObservableCollection<PianoScoreDownloadRecordFilterBox> FilterBoxList { get; set; } = new ObservableCollection<PianoScoreDownloadRecordFilterBox>()
        {
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetDesc(Tan8SheetDownloadResult.SUCCESS), Convert.ToByte(Tan8SheetDownloadResult.SUCCESS)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetDesc(Tan8SheetDownloadResult.PLAYER_NO_RESPONSE), Convert.ToByte(Tan8SheetDownloadResult.PLAYER_NO_RESPONSE)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetDesc(Tan8SheetDownloadResult.PIANO_SCORE_NOT_EXSITS), Convert.ToByte(Tan8SheetDownloadResult.PIANO_SCORE_NOT_EXSITS)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetDesc(Tan8SheetDownloadResult.PIANO_SCORE_DOWNLOAD_FAIL), Convert.ToByte(Tan8SheetDownloadResult.PIANO_SCORE_DOWNLOAD_FAIL)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetDesc(Tan8SheetDownloadResult.PLAY_FILE_DOWNLOAD_FAIL), Convert.ToByte(Tan8SheetDownloadResult.PLAY_FILE_DOWNLOAD_FAIL))
        };

        //删除条目
        public ICommand OnDeleteKeyCommand { get; set; }
        //复制增长id
        public ICommand OnCopyIncrIdCommand { get; set; }
        //中止下载命令
        public ICommand OnStopDownloadCommand { get; set; }
        //继续下载命令
        public ICommand OnContinueDownloadCommand { get; set; }

        private readonly Tan8SheetsRepo _Tan8SheetRepo = Tan8SheetsRepo.Instance;
        public readonly Tan8SheetCrawlRecordRepo _Tan8SheetCrawlRecordRepo = Tan8SheetCrawlRecordRepo.Instance;
        private readonly Tan8SheetCrawlTaskRepo _SheetCrawlTaskRepo = Tan8SheetCrawlTaskRepo.Instance;

        #region 下载
        //下载任务管理器
        private readonly BackgroundWorker _DownloadTaskWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };
        //下载列表是否为空
        //public bool IsDownloadListEmpty { get { if (IsInDesignMode) return false; return DownloadingData.Count == 0; }}
        //private void NotifyIsDownloadListEmpty () { RaisePropertyChanged(nameof(IsDownloadListEmpty)); }
        //悬浮按钮展示阶段 1-点击新增, 2-点击继续, 3-点击停止
        private int _STEP_ADD = 1, _STEP_CONTINUE = 2, _STEP_STOP = 3;
        public int ButtonStep 
        { 
            get 
            {
                if (IsInDesignMode) return _STEP_STOP;
                if (_DownloadTaskWorker.IsBusy) return _STEP_STOP;
                if (DownloadingData.Count == 0) return _STEP_ADD;
                return _STEP_CONTINUE;
            } 
        }
        private void NotifyButtonStep() { RaisePropertyChanged(nameof(ButtonStep)); }
        //允许最大的任务数量
        private readonly int _MaxTaskNum = 5;
        //已经下载完成的数量(包括成功/失败)
        //private int _DownloadFinishNum = 0;
        //当前正在进行中的任务数量
        private int _CurTaskNum = 0;
        //需要下载的任务列表
        private readonly Queue<int> mTaskQueue = new Queue<int>();
        //停止任务的委托
        private event StopAllTan8SheetDownload _StopBtnClickHandler;
        #endregion

        //标识条件复选框是否初始化完成
        //private bool IsCheckBoxInited = false;

        public PianoScoreDownloadRecordViewModel()
        {
            OnDeleteKeyCommand = new RelayCommand<IList>(OnDelete);
            OnCopyIncrIdCommand = new RelayCommand<Tan8SheetDownloadRecord>(OnCopyIncrYpid);
            OnStopDownloadCommand = new RelayCommand(OnStopDownload);
            OnContinueDownloadCommand = new RelayCommand(OnContinueDownload);
            //IsCheckBoxInited = true;

            //乐谱下载队列任务
            _DownloadTaskWorker.DoWork += new DoWorkEventHandler(DoDownloadTaskCreate);
            _DownloadTaskWorker.ProgressChanged += new ProgressChangedEventHandler(ReportDownloadTaskCreateProgress);
            _DownloadTaskWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DownloadTaskCreateComplate);

            if(IsInDesignMode)
            {
                //设计模式填充虚拟数据
                for(int i = 0; i < 3; i++)
                {
                    DownloadingData.Add(new SheetItemDownloadViewModel
                    {
                        Id = i,
                        Name = "模拟" + i,
                        Progress = 50,
                        ProgressText = "正在干饭..."
                    });
                }
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        public void Load()
        {
            List<int> codes = new List<int>();
            foreach (var item in FilterBoxList)
            {
                if(item.IsChecked)
                    codes.Add(item.Value);
            }
            var data = _Tan8SheetCrawlRecordRepo.Find(codes);
            DownloadRecordList.Clear();
            if (!DataUtil.IsEmptyCollection(data))
            {
                data.ForEach(e => DownloadRecordList.Add(e));
            }
            if (DownloadingData.Count == 0)
            {
                var queueTaskData = _SheetCrawlTaskRepo.FindAllTaskYpid();
                if (!DataUtil.IsEmptyCollection(queueTaskData))
                {
                    foreach (var ypid in queueTaskData)
                    {
                        var sheetComplete = _Tan8SheetRepo.FindById(ypid);
                        if (null != sheetComplete)
                        {
                            _SheetCrawlTaskRepo.DelByYpid(ypid);
                            continue;
                        }
                        var taskItem = CreateTaskViewInstance(new Tan8SheetCrawlArg
                        {
                            Ypid = ypid,
                            IsQueueTask = true
                        });
                        DownloadingData.Insert(0, taskItem);
                        //NotifyIsDownloadListEmpty();
                        NotifyButtonStep();
                    }
                }
            }
        }

        #region 命令事件
        /// <summary>
        /// 继续下载命令
        /// </summary>
        private void OnContinueDownload()
        {
            if (DownloadingData.Count == 0)
            {
                NotifyButtonStep();
                return;
            }
            TriggerTan8DownLoadTask(new Tan8SheetCrawlArg()
            {
                UseProxy = true,
            });
        }

        /// <summary>
        /// 发送停止下载消息
        /// </summary>
        private void OnStopDownload()
        {
            //如果有正在下载但是还没下载完的乐谱, 在停止时存入下次优先下载的列表中
            foreach (var item in DownloadingData)
            {
                var sheetComplete = _Tan8SheetRepo.FindById(item.Id);
                if (null == sheetComplete)
                {
                    _SheetCrawlTaskRepo.AddNew(item.Id);
                }
            }
            _StopBtnClickHandler?.Invoke(DownloadingData);
            //NotifyIsDownloadListEmpty();
            _DownloadTaskWorker.CancelAsync();
        }

        /// <summary>
        /// 删除乐谱下载记录
        /// </summary>
        private void OnDelete(IList selectedList)
        {
            //System.Windows.Controls.SelectedItemCollection
            List<int> selectedIds = new List<int>();
            while (selectedList.Count > 0)
            {
                var vm = selectedList[0] as Tan8SheetDownloadRecord;
                selectedIds.Add(vm.Id);
                DownloadRecordList.Remove(vm);
            }
            _Tan8SheetCrawlRecordRepo.DelByIds(selectedIds);
        }

        /// <summary>
        /// 以递增方式复制ID
        /// </summary>
        private void OnCopyIncrYpid(Tan8SheetDownloadRecord record)
        {
            //XamlUtil.SelectRow(DownloadRecordDataGrid, e);
            if (null == record) return;
            var arg = new Tan8SheetCrawlArg()
            {
                AutoDownload = true,
                Ypid = record.Ypid + 1,
                UseProxy = true,
                IsQueueTask = false,
                //Ver = 2
            };
            TriggerTan8DownLoadTask(arg);
        }
        #endregion

        #region 下载相关
        /// <summary>
        /// 新的乐谱下载完成事件
        /// </summary>
        /// <param name="newItem"></param>
        public void OnSheetItemDownloadComplete(int newYpid, System.Windows.Controls.DataGrid dataGrid)
        {
            //从展示的列表中移除
            var listDataItem = DownloadingData.Where(e => e.Id == newYpid).FirstOrDefault();
            if (null != listDataItem)
            {
                DownloadingData.Remove(listDataItem);
            }
            //NotifyIsDownloadListEmpty();
            var record = _Tan8SheetCrawlRecordRepo.FindByYpid(newYpid);
            DownloadRecordList.Insert(0, record);
            dataGrid.ScrollIntoView(record);
        }

        /// <summary>
        /// 下载弹8曲谱
        /// 开启后台下载曲谱任务
        /// </summary>
        /// <param name="crawlArg">曲谱对象</param>
        /// <returns>公共函数返回结果</returns>
        public void TriggerTan8DownLoadTask(Tan8SheetCrawlArg crawlArg)
        {
            if (_DownloadTaskWorker.IsBusy)
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
            NotifyButtonStep();
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
                if (_CurTaskNum >= _MaxTaskNum)
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
                    else if (crawlArg.IsQueueTask)
                    {
                        //不是自动下载, 但是是队列下载, 要从数据中删除队列
                        _SheetCrawlTaskRepo.DelByYpid(crawlArg.Ypid.GetValueOrDefault());
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
                    IsQueueTask = crawlArg.IsQueueTask
                    //Ver = crawlArg.Ver
                });
                Interlocked.Increment(ref _CurTaskNum);
                //检查任务是否中断
                //if (_DownloadTaskWorker.CancellationPending) break;
                //打开播放器, 触发主动下载
                Tan8PlayUtil.ExePlayById(crawlArg.Ypid.GetValueOrDefault(), 1, true);
                //如果不是自动下载, 则不增长下一个需要下载的ID, 直接循环等待子任务完成
                if (!crawlArg.AutoDownload || (crawlArg.IsQueueTask && DataUtil.IsEmptyCollection(mTaskQueue)))
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
            var crawlArg = e.UserState as Tan8SheetCrawlArg;
            var existsEl = DownloadingData.Where(el => el.Id == crawlArg.Ypid.GetValueOrDefault()).FirstOrDefault();
            //如果是队列任务, 那么此时下载列表里应该已经存在创建好的view对象, 只需更新部分信息即可
            if (crawlArg.IsQueueTask && existsEl != null)
            {
                existsEl.AutoDownload = crawlArg.AutoDownload;
            } 
            else
            {
                //创建一个新的item, 开启下载任务
                var taskSheetItem = CreateTaskViewInstance(crawlArg);
                DownloadingData.Insert(0, taskSheetItem);
                //NotifyIsDownloadListEmpty();
            }
        }

        /// <summary>
        /// 创建下载任务项目
        /// </summary>
        /// <param name="crawlArg"></param>
        /// <returns></returns>
        private SheetItemDownloadViewModel CreateTaskViewInstance(Tan8SheetCrawlArg crawlArg)
        {
            var taskItemInstance = new SheetItemDownloadViewModel()
            {
                Name = string.IsNullOrEmpty(crawlArg.Name) ? string.Format("新乐谱任务 [{0}]", crawlArg.Ypid.GetValueOrDefault()) : crawlArg.Name,
                IsTempName = string.IsNullOrEmpty(crawlArg.Name),
                Id = crawlArg.Ypid.GetValueOrDefault(),
                UseProxy = crawlArg.UseProxy,
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
                _SheetCrawlTaskRepo.DelByYpid(crawlArg.Ypid.GetValueOrDefault());
                if (DataUtil.IsEmptyCollection(mTaskQueue) && _CurTaskNum == 0)
                {
                    OnStopDownload();
                    return;
                }
                if (mTaskQueue.Count > 0)
                {
                    crawlArg.Ypid = mTaskQueue.Dequeue();
                }
                return;
            }
            crawlArg.Ypid++;
        }

        /// <summary>
        /// 分发批量下载任务
        /// </summary>
        /// <param name="crawlArg"></param>
        public void DoDownloadTaskDispatche(Tan8SheetCrawlArg crawlArg)
        {
            var ypid = crawlArg.Ypid.GetValueOrDefault();
            var taskSheetItem = DownloadingData.Where(e => e.Id.Equals(ypid)).FirstOrDefault();
            if (null == taskSheetItem || taskSheetItem.IsWorking == false)
            {
                var isHideStarted = Tan8PlayUtil.IsHideStarted(ypid);
                if (isHideStarted != null && isHideStarted.GetValueOrDefault())
                {         
                    //如果是隐藏启动(即以下载为目的的启动), 找不到目标下载项, 可能是在重启Flash播放器的过程中停止下载了, 需要关闭
                    Tan8PlayUtil.Exit(ypid);
                }
                return;
            }
            taskSheetItem.SheetUrl = crawlArg.SheetUrl;
            taskSheetItem.DownLoadTan8MusicV2Task();
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
            NotifyButtonStep();
            Messenger.Default.Send(new BubbleTipViewModel
            {
                AlertLevel = AlertLevel.INFO,
                Text = "下载完成"
            });
        }

        /// <summary>
        /// 单个乐谱下载完成触发事件
        /// </summary>
        /// <param name="ypid"></param>
        private void OnSheetItemDownloadComplete(int ypid, bool success, bool isWorking)
        {
            //增加数量, 每完成一个子任务, 完成数量+1
            //Interlocked.Increment(ref _DownloadFinishNum);
            Interlocked.Decrement(ref _CurTaskNum);
            //不是中止的, 不论下载成功/失败, 都发送通知
            if (isWorking)
            {
                //发送下载完成通知
                Messenger.Default.Send(ypid, ApplicationConstant.TAN8_DOWNLOAD_COMPLETE);
            }
        }
        #endregion
    }


    /// <summary>
    /// 筛选框View Model
    /// </summary>
    public class PianoScoreDownloadRecordFilterBox : ViewModelBase
    {
        //筛选标题
        public string Title { get; set; }
        //筛选值
        public byte Value { get; set; }
        //选中状态
        private bool _IsChecked = true;
        public bool IsChecked { get => _IsChecked; set { _IsChecked = value; RaisePropertyChanged(); } }

        public PianoScoreDownloadRecordFilterBox(string title, byte value)
        {
            Title = title;
            Value = value;
        }

        /// <summary>
        /// 过滤框点击
        /// </summary>
        //private void OnFilter(PianoScoreDownloadRecordViewModel listViewModel)
        //{
            //IsChecked = !IsChecked;
            //由于页面创建的时候就会触发选中事件, 此处手动限制, 只有在所有数据加载之后才会触发此事件
            //comment-on 2022-11-29, mvvm模式不会在初始化下触发
            //if (!IsCheckBoxInited) return;
            //listViewModel.Load();
        //}
    }
}
