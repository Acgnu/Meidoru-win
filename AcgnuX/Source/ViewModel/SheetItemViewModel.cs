using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Common;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Model;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using AcgnuX.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 乐谱库项目view model
    /// </summary>
    public class SheetItemViewModel : ViewModelBase
    {
        #region 属性 
        //乐谱ID
        public int Id { get; set; }
        //乐谱名称
        public string Name
        {
            get { var len = 45; return LongName.Length > len ? LongName.Substring(0, len) + "..." : LongName; }
            set { LongName = value; RaisePropertyChanged(); }
        }
        public string LongName { get; private set; }
        //乐谱页数
        private int _YpCount;
        public int YpCount {get { return _YpCount; }set { _YpCount = value; RaisePropertyChanged(); }}
        //乐谱版本
        public int Ver { get; set; }
        //是否收藏
        private byte _Star;
        public byte Star { get { return _Star; } set { _Star = value; RaisePropertyChanged(); } }
        //封面图
        private string _Cover;
        public string Cover { get { return _Cover; } set { _Cover = value; RaisePropertyChanged(); } }
        //进度
        private int _Progress;
        public int Progress { get { return _Progress; } set { _Progress = value; RaisePropertyChanged(); } }
        //是否执行任务中
        private bool _IsWorking;
        public bool IsWorking { get { return _IsWorking; } set { _IsWorking = value; RaisePropertyChanged(); } }
        //下载进度信息
        private string _ProgressText;
        public string ProgressText { get => _ProgressText; set { _ProgressText = value; RaisePropertyChanged(); } }
        //是否使用代理
        public bool UseProxy { get; set; }
        //是否为下载任务
        //public bool IsDownload { get; set; }
        //乐谱URL
        public string SheetUrl { get; set; }
        //是否初始化为下载时的临时名称
        public bool IsTempName { get; set; }
        //是否来自自动下载
        public bool AutoDownload { get; set; }
        #endregion

        #region 命令
        //播放按钮点击命令
        public ICommand OnItemPlayCommand { get; set; }
        //点击收藏命令
        public ICommand OnStarCommand { get; set; }
        //打开所在文件夹命令
        public ICommand OnOpenSheetFolderCommand { get; set; }
        //导出分享包命令
        public ICommand OnExportForShareCommand { get; set; }
        //重命名命令
        public ICommand OnEditSheetNameCommand { get; set; }
        #endregion

        #region 事件
        //曲谱下载完成事件
        public Action<int, bool, bool> DownloadFinishAction;
        #endregion

        //导出的Worker
        private readonly BackgroundWorker mExportBgWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        private readonly Tan8SheetsRepo _Tan8SheetsRepo = Tan8SheetsRepo.Instance;
        private readonly Tan8SheetCrawlRecordRepo _Tan8SheetCrawlRecordRepo = Tan8SheetCrawlRecordRepo.Instance;

        public SheetItemViewModel()
        {
            OnItemPlayCommand = new RelayCommand<Tan8SheetReponsitoryViewModel>(OnCallItemPlay);
            OnStarCommand = new RelayCommand(OnStar);
            OnOpenSheetFolderCommand = new RelayCommand(OnOpenSheetFolder);
            OnExportForShareCommand = new RelayCommand(OnExportForShare);
            OnEditSheetNameCommand = new RelayCommand(() => _Tan8SheetsRepo.UpdateName(Id, Name));

            //工作任务
            mExportBgWorker.DoWork += new DoWorkEventHandler(DoExportForShareTask);
            //进度通知
            mExportBgWorker.ProgressChanged += new ProgressChangedEventHandler(ReportExportForShareProgress);
            //下载完成事件
            mExportBgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ExportForShareComplate);
        }

        /// <summary>
        /// 调用播放
        /// </summary>
        private void OnCallItemPlay(Tan8SheetReponsitoryViewModel sheetRepoViewModel)
        {
            //检查曲谱可否播放
            //var tan8Music = _Tan8SheetsRepo.FindById(Id);
            byte b1 = 1;
            var playFileName = Ver == b1 ? "play.ypa2" : "play.ypdx";
            var playFilePath = Path.Combine(Settings.Default.Tan8HomeDir, Id.ToString(), playFileName);
            //手动选中行
            sheetRepoViewModel.SelectedListData = this;
            if (!File.Exists(playFilePath))
            {
                //无法播放的曲谱打开所在文件夹
                var fullPath = Path.Combine(Settings.Default.Tan8HomeDir, Id.ToString());
                if (Directory.Exists(fullPath))
                {
                    System.Diagnostics.Process.Start(fullPath);
                }
                return;
            }
            //播放所选曲谱
            Tan8PlayUtil.Exit(Id);
            Tan8PlayUtil.ExePlayById(Id, Ver, false);
        }

        /// <summary>
        /// 收藏
        /// </summary>
        private void OnStar()
        {
            var nStar = _Tan8SheetsRepo.UpdateStar(Id);
            Star = Convert.ToByte(nStar);
        }

        /// <summary>
        /// 打开乐谱所在文件夹
        /// </summary>
        private void OnOpenSheetFolder()
        {
            var fullPath = Path.Combine(Settings.Default.Tan8HomeDir, Id.ToString());
            if (Directory.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(fullPath);
            }
        }

        #region 导出分享后台任务/进度/完成事件
        /// <summary>
        /// 导出为分享包
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExportForShare()
        {
            //如果已经存在分享包, 直接打开目标文件夹
            var fullPath = Path.Combine(Settings.Default.Tan8HomeDir, Id.ToString());
            if (Directory.Exists(fullPath))
            {
                if (File.Exists(Path.Combine(fullPath, ApplicationConstant.SHARE_ZIP_NAME)))
                {
                    FileUtil.OpenAndChooseFile(Path.Combine(fullPath, ApplicationConstant.SHARE_ZIP_NAME));
                }
                else
                {
                    //不存在则开始执行转换任务
                    if (mExportBgWorker.IsBusy)
                    {
                        Messenger.Default.Send(new BubbleTipViewModel
                        {
                            AlertLevel = AlertLevel.ERROR,
                            Text = "有正在进行中的任务"
                        });
                        return;
                    }
                    mExportBgWorker.RunWorkerAsync(this);
                    IsWorking = true;
                }
            }
        }

        /// <summary>
        /// 执行导出分享任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoExportForShareTask(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.ReportProgress(5, "读取乐谱信息");
            //获取原始名称
            var tan8Sheet = _Tan8SheetsRepo.FindById(Id);
            if (string.IsNullOrEmpty(tan8Sheet.Name))
            {
                worker.ReportProgress(5, "乐谱信息有误");
                return;
            }

            var fullPath = Path.Combine(Settings.Default.Tan8HomeDir, Id.ToString());
            if (!Directory.Exists(fullPath))
            {
                worker.ReportProgress(5, "乐谱文件不存在");
                return;
            }
            worker.ReportProgress(10, "统计乐谱文件");

            //遍历所有乐谱图片
            var sheetFiles = Directory.GetFiles(fullPath);
            //乐谱总数
            var totalPage = 0;
            foreach (var sheetFile in sheetFiles)
            {
                var subFileName = Path.GetFileName(sheetFile);
                if (subFileName.StartsWith("page") && subFileName.EndsWith(ApplicationConstant.DEFAULT_SHEET_PAGE_FORMAT))
                {
                    totalPage++;
                }
            }

            //在当前目录下建立临时文件夹, 存储用于压缩的乐谱
            Directory.CreateDirectory(Path.Combine(fullPath, ApplicationConstant.SHARE_TEMP_FOLDER_NAME));
            int nowProgress = 10;
            for (int i = 0; i < totalPage; i++)
            {
                if (mExportBgWorker.CancellationPending)
                {
                    ClearExportFiles(worker, fullPath);
                    return;
                }
                nowProgress += 75 / totalPage;
                worker.ReportProgress(nowProgress, string.Format("正在转换第{0}张乐谱, 共{1}张", i + 1, totalPage));
                var pageFileName = string.Format("page.{0}.png", i);
                Bitmap rawImg = (Bitmap)Bitmap.FromFile(Path.Combine(fullPath, pageFileName));
                Bitmap bmp = ImageUtil.CreateIegalTan8Sheet(rawImg, tan8Sheet.Name, i + 1, totalPage, false);
                bmp.Save(Path.Combine(fullPath, ApplicationConstant.SHARE_TEMP_FOLDER_NAME, i + ApplicationConstant.DEFAULT_SHEET_PAGE_FORMAT), ImageFormat.Png);
                bmp.Dispose();
            }

            //调用压缩工具进行打包
            //可使用 -email 直接邮寄, bandzip帮助文档 https://www.bandisoft.com/bandizip/help/parameter/
            worker.ReportProgress(85, "正在压缩..");
            var zipProcess = System.Diagnostics.Process.Start("Bandizip.exe", string.Format("c -y \"{0}\" \"{1}\"",
                Path.Combine(fullPath, ApplicationConstant.SHARE_ZIP_NAME),
                Path.Combine(fullPath, ApplicationConstant.SHARE_TEMP_FOLDER_NAME)));

            //等待压缩完成
            zipProcess.WaitForExit();

            if (mExportBgWorker.CancellationPending)
            {
                ClearExportFiles(worker, fullPath);
                return;
            }

            //压缩后删除临时文件夹
            worker.ReportProgress(95, "正在清理临时文件...");
            FileUtil.DeleteDirWithName(fullPath, ApplicationConstant.SHARE_TEMP_FOLDER_NAME);

            e.Result = Path.Combine(fullPath, ApplicationConstant.SHARE_ZIP_NAME);
            worker.ReportProgress(100, "导出成功");
        }

        /// <summary>
        /// 清理中止任务产生的临时文件
        /// </summary>
        /// <param name="sheetDirPath"></param>
        private void ClearExportFiles(BackgroundWorker worker, string sheetDirPath)
        {
            worker.ReportProgress(90, "正在清理文件...");
            if (Directory.Exists(Path.Combine(sheetDirPath, ApplicationConstant.SHARE_TEMP_FOLDER_NAME)))
            {
                FileUtil.DeleteDirWithName(sheetDirPath, ApplicationConstant.SHARE_TEMP_FOLDER_NAME);
            }
            FileUtil.DeleteFile(Path.Combine(sheetDirPath, ApplicationConstant.SHARE_ZIP_NAME));
            worker.ReportProgress(100, "任务中止");
        }

        /// <summary>
        /// 报告导出分享进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReportExportForShareProgress(object sender, ProgressChangedEventArgs e)
        {
            SetProgress(e.ProgressPercentage, e.UserState as string);
        }

        /// <summary>
        /// 导出分享完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportForShareComplate(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!string.IsNullOrEmpty((string)e.Result))
            {
                //完成后打开目标文件
                FileUtil.OpenAndChooseFile(e.Result.ToString());
                IsWorking = false;
            }
        }
        #endregion

        #region 乐谱下载
        /// <summary>
        /// 中止下载消息
        /// </summary>
        public void OnStopDownloadEvent(ObservableCollection<SheetItemViewModel> listData)
        {
            if(IsWorking)
            {
                IsWorking = false;
                Tan8PlayUtil.Exit(Id);
                var sheet = _Tan8SheetsRepo.FindById(Id);
                if (sheet == null)
                {
                    //如果没有完成下载, 则从管理列表中删除
                    listData.Remove(this);
                }
            }
        }

        /// <summary>
        /// 乐谱下载任务
        /// </summary>
        /// <param name="tan8SheetAddr"></param>
        public void DownLoadTan8MusicV2Task()
        {
            //判断任务是否已经中止
            //if (isTaskStop)
            //已在分发处判断, 此处注释观察是否有问题
            //if (IsWorking == false)
            //{
            //OnStopDownloadEvent();
            //此处可能有问题, 下载的过程中, 遇到超时重启, 刚好碰上停止下载任务, 可能会找不到下载项, 或者flash没有退出
            //return;
            //}
            var startTimeMill = TimeUtil.CurrentMillis();
            InvokeResult<object> result;
            do
            {
                result = StartDownLoadV2();
                //下载出错则给出提示, 并等待下次任务
                if (!result.success)
                {
                    //失败则清理资源
                    FileUtil.DeleteDirWithName(Settings.Default.Tan8HomeDir, Id.ToString());

                    if (IsWorking)
                    {
                        SetProgress(null, string.Format("下载出错 {0}{1}", result.message, ", 正在重试..."));

                        //如果是自动下载, 且由是由于网络连接出错, 则不保存下载记录, 直接重试
                        if (result.code == 8 || result.code == 3 || result.code == 7)
                        {
                            //25秒内不重启重试 ( flash请求等待时间超过30秒会弹窗报错, 此处限定为25秒 )
                            if (TimeUtil.CurrentMillis() - startTimeMill < 25 * 1000)
                            {
                                continue;
                            }
                            //超过25秒, 重启播放器重试
                            Tan8PlayUtil.Restart(Id, 1, true);
                            return;
                        }
                        //以下代码仅下载成功执行, 如果下载成功, 不论是否中止, 都保存乐谱信息, 当作成功
                        //每次下载完, 保存最后下载的记录ID
                        _Tan8SheetCrawlRecordRepo.Save(Id, AutoDownload, Name, result.code, result.message);
                        Tan8PlayUtil.Exit(Id);

                        //触发下载完成事件
                        DownloadFinishAction?.Invoke(Id, result.success, IsWorking);
                        IsWorking = false;      //设为False将退出循环, 结束任务
                    }
                }
            } while (IsWorking);
        }

        /// <summary>
        ///  下载弹琴吧琴谱后台任务
        /// </summary>
        private InvokeResult<object> StartDownLoadV2()
        {
            //默认的下载中止的返回结果
            var downloadResult = new InvokeResult<object>
            {
                success = false,
                code = 0,       //随便一个默认值
                message = "任务中止",
            };

            //快速预检乐谱是否存在
            var contentResult = RequestUtil.CrawlContentFromWebsit(string.Format("http://www.tan8.com/yuepu-{0}.html", Id), null);
            if (contentResult.success && contentResult.data.Contains("404.jpg"))
            {
                downloadResult.code = (byte)Tan8SheetDownloadResult.PIANO_SCORE_NOT_EXSITS;
                downloadResult.message = EnumLoader.GetDesc(Tan8SheetDownloadResult.PIANO_SCORE_NOT_EXSITS);
                return downloadResult;
            }

            //直接使用v2版本的地址
            SetProgress(10, "下载地址解析成功, 开始加载乐谱信息");

            //step.2 请求乐谱地址, 得到乐谱信息
            string proxyAddress = null;
            if (UseProxy)
            {
                proxyAddress = ProxyFactory.GetFirstProxy();
            }
            var ypinfostring = RequestUtil.CrawlContentFromWebsit(SheetUrl, proxyAddress).data;
            //var ypinfostring = @"<html><body>yp_create_time=<yp_create_time>1573183398</yp_create_time><br/>yp_title=<yp_title>说好不哭（文武贝钢琴版）</yp_title><br/>yp_page_count=<yp_page_count>3</yp_page_count><br/>yp_page_width=<yp_page_width>1089</yp_page_width><br/>yp_page_height=<yp_page_height>1540</yp_page_height><br/>yp_is_dadiao=<yp_is_dadiao>1</yp_is_dadiao><br/>yp_key_note=<yp_key_note>10</yp_key_note><br/>yp_is_yanyin=<yp_is_yanyin>1</yp_is_yanyin><br/>ypad_url=<ypad_url>http://www.tan8.com//yuepuku/132/66138/66138_hegiahcc.ypad</ypad_url>ypad_url2=<ypad_url2>http://www.tan8.com//yuepuku/132/66138/66138_hegiahcc.ypa2</ypad_url2></body></html>";
            //校验返回的乐谱信息
            var checkResult = CheckYuepuInfo(ypinfostring);
            if (UseProxy)
            {
                ProxyFactory.RemoveProxy(proxyAddress, checkResult == Tan8SheetDownloadResult.VISTI_REACH_LIMIT ? 0 : 15 * 1000);
            }

            if (checkResult != Tan8SheetDownloadResult.SUCCESS)
            {
                downloadResult.code = (byte)checkResult;
                downloadResult.message = EnumLoader.GetDesc(checkResult);
                return downloadResult;
            }
            //从乐谱信息解析到对象
            var tan8Music = DataUtil.ParseToModel(ypinfostring);
            YpCount = tan8Music.yp_page_count;

            var ypNameFolder = IsTempName ? tan8Music.yp_title : Name;
            //替换非法字符
            ypNameFolder = FileUtil.ReplaceInvalidChar(ypNameFolder);
            if (string.IsNullOrEmpty(ypNameFolder))
            {
                ypNameFolder = "非法名称_" + Id;
            }
            //将名称更新到项目
            Name = ypNameFolder;

            //校验保存路径是否重复
            var libFolder = Settings.Default.Tan8HomeDir;

            var saveFullPath = Path.Combine(libFolder, Id.ToString());
            //step.3 创建文件夹
            if (IsWorking == false) return downloadResult;         //在IO操作之前检查任务是否停止
            FileUtil.CreateFolder(saveFullPath);

            //step.3 下载曲谱封面
            var coverSavePath = Path.Combine(saveFullPath, ApplicationConstant.DEFAULT_COVER_NAME);
            SetProgress(30, "下载封面...");
            var coverUrl = tan8Music.ypad_url.Substring(0, tan8Music.ypad_url.IndexOf('_')) + "_prev.jpg";

            if (IsWorking == false) return downloadResult;         //在IO操作之前检查任务是否停止
            var downResult = new FileDownloader().DownloadFile(coverUrl, coverSavePath);
            //封面下载失败不管, 下载成功则展示
            if (downResult == 0) Cover = Path.Combine(libFolder, Id.ToString(), ApplicationConstant.DEFAULT_COVER_NAME);

            //封面下载完后校验图片是否有效
            if (IsWorking == false) return downloadResult;         //在IO操作之前检查任务是否停止
            var isValidPreviewImg = ImageUtil.CheckImgIsValid(coverSavePath);

            //如果文件损坏则删除
            if (IsWorking == false) return downloadResult;         //在IO操作之前检查任务是否停止
            if (!isValidPreviewImg) FileUtil.DeleteFile(coverSavePath);

            //step.4 下载乐谱图片
            var nowProgress = 30;
            for (var i = 0; i < tan8Music.yp_page_count; i++)
            {
                var message = string.Format("下载乐谱 {0} / {1}", i + 1, tan8Music.yp_page_count);
                SetProgress(50 / tan8Music.yp_page_count + nowProgress, message);
                nowProgress += 50 / tan8Music.yp_page_count;

                var downloadUrl = tan8Music.ypad_url + string.Format(".{0}.png", i);
                if (IsWorking == false) return downloadResult;         //在IO操作之前检查任务是否停止
                int pageDownloadResult = new FileDownloader().DownloadFile(downloadUrl, Path.Combine(saveFullPath, string.Format("page.{0}.png", i)));
                //如果下载出错
                if (pageDownloadResult != 0)
                {
                    //清理下载文件
                    if (IsWorking == false) return downloadResult;         //在IO操作之前检查任务是否停止
                    FileUtil.DeleteDirWithName(libFolder, Id.ToString());

                    downloadResult.code = (byte)Tan8SheetDownloadResult.PIANO_SCORE_DOWNLOAD_FAIL;
                    downloadResult.message = EnumLoader.GetDesc(Tan8SheetDownloadResult.PIANO_SCORE_DOWNLOAD_FAIL);
                    return downloadResult;
                }
            }
            //下载v2版播放文件
            SetProgress(80, "下载播放文件...");
            if (IsWorking == false) return downloadResult;         //在IO操作之前检查任务是否停止
            downResult = new FileDownloader().DownloadFile(tan8Music.ypad_url2, Path.Combine(saveFullPath, "play.ypdx"));
            if (downResult != 0)
            {
                //没有播放文件, 又没有谱页的, 清理数据
                if (tan8Music.yp_page_count == 0)
                {
                    //清理下载文件
                    if (IsWorking == false) return downloadResult;         //在IO操作之前检查任务是否停止
                    FileUtil.DeleteDirWithName(libFolder, Id.ToString());
                }
                downloadResult.code = (byte)Tan8SheetDownloadResult.PLAY_FILE_DOWNLOAD_FAIL;
                downloadResult.message = EnumLoader.GetDesc(Tan8SheetDownloadResult.PLAY_FILE_DOWNLOAD_FAIL);
                return downloadResult;
            }

            //step.6 保存到数据库
            SetProgress(90, "保存数据库...");
            if (IsWorking == false) return downloadResult;         //在IO操作之前检查任务是否停止
            _Tan8SheetsRepo.Save(Id, ypNameFolder, tan8Music, ypinfostring);

            SetProgress(100, "下载完成");
            downloadResult.success = true;
            downloadResult.code = (byte)Tan8SheetDownloadResult.SUCCESS;
            downloadResult.message = EnumLoader.GetDesc(Tan8SheetDownloadResult.SUCCESS);
            return downloadResult;
        }

        /// <summary>
        /// 初步检查tan8返回的乐谱信息
        /// </summary>
        /// <param name="source">乐谱信息结果串</param>
        /// <returns></returns>
        private Tan8SheetDownloadResult CheckYuepuInfo(string source)
        {
            //无返回
            if (string.IsNullOrEmpty(source)) return Tan8SheetDownloadResult.PLAYER_NO_RESPONSE;
            //乐谱不存在
            if (source.Contains("该乐谱不存在!")) return Tan8SheetDownloadResult.PIANO_SCORE_NOT_EXSITS;
            if (source.Contains("您访问太频繁啦")) return Tan8SheetDownloadResult.TOO_MANY_VISIT;
            if (source.Contains("您访问的次数太多了")) return Tan8SheetDownloadResult.VISTI_REACH_LIMIT;
            if (source.Contains("验证错误")) return Tan8SheetDownloadResult.VALID_ERROR;
            //网络连接出错
            if (source.Equals(RequestUtil.CONNECTION_ERROR)) return Tan8SheetDownloadResult.NETWORK_ERROR;
            //默认返回成功
            return Tan8SheetDownloadResult.SUCCESS;
        }
        #endregion

        /// <summary>
        /// 设置进度和文本
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="message"></param>
        private void SetProgress(int? progress, string message)
        {
            if (null != progress)
            {
                Progress = progress.GetValueOrDefault();
            }
            ProgressText = message;
        }
    }
}
