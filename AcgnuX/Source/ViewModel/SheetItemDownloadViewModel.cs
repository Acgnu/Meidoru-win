using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Model;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using AcgnuX.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using SharedLib.Model;
using SharedLib.Utils;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using SharedLib.Data;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 乐谱库下载项 view model
    /// </summary>
    public class SheetItemDownloadViewModel : ObservableObject
    {
        #region 属性 
        //乐谱ID
        public int Id { get; set; }
        //乐谱名称
        private string name;
        public string Name { get => name; set => SetProperty(ref name, value); }
        //进度
        private int _Progress;
        public int Progress { get => _Progress; set => SetProperty(ref _Progress, value); }
        //下载进度信息
        private string _ProgressText;
        public string ProgressText { get => _ProgressText; set => SetProperty(ref _ProgressText, value); }
        //进度条级别
        public AlertLevel ProgressAlertLevel { get; set; } = AlertLevel.RUN;
        //是否使用代理
        public bool UseProxy { get; set; }
        //乐谱URL
        public string SheetUrl { get; set; }
        //是否初始化为下载时的临时名称
        public bool IsTempName { get; set; }
        //是否来自自动下载
        public bool AutoDownload { get; set; }
        //是否执行任务中
        private bool _IsWorking;
        public bool IsWorking { get { return _IsWorking; } set => SetProperty(ref _IsWorking, value); }
        #endregion

        #region 事件
        //曲谱下载完成事件
        public Action<int, bool, bool> DownloadFinishAction;
        #endregion

        private readonly Tan8SheetsRepo _Tan8SheetsRepo;
        private readonly Tan8SheetCrawlRecordRepo _Tan8SheetCrawlRecordRepo;
        private readonly ProxyAddressRepo _ProxyAddressRepo;

        public SheetItemDownloadViewModel()
        {
            this._Tan8SheetsRepo = App.Current.Services.GetService<Tan8SheetsRepo>();
            this._Tan8SheetCrawlRecordRepo = App.Current.Services.GetService<Tan8SheetCrawlRecordRepo>();
            this._ProxyAddressRepo = App.Current.Services.GetService<ProxyAddressRepo>();
        }

        #region 乐谱下载
        /// <summary>
        /// 中止下载消息
        /// </summary>
        public void OnStopDownloadEvent(ObservableCollection<SheetItemDownloadViewModel> listData)
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
                    if ((bool)result.data)
                    {
                        //下载失败, 并且标记是可以删除的, 则清理资源
                        FileUtil.DeleteDirWithName(Settings.Default.Tan8HomeDir, Id.ToString());
                    }

                    if (IsWorking)
                    {
                        SetProgress(null, string.Format("下载出错 {0}{1}", result.message, ", 正在重试..."));

                        //如果是自动下载, 且由是由于网络连接出错, 则不保存下载记录, 直接重试
                        if (result.code == 8 || result.code == 3 || result.code == 7)
                        {
                            //等待100毫秒, 避免阻塞UI
                            Thread.Sleep(100);
                            //25秒内不重启重试 ( flash请求等待时间超过30秒会弹窗报错, 此处限定为25秒 )
                            if (TimeUtil.CurrentMillis() - startTimeMill < 25 * 1000)
                            {
                                continue;
                            }
                            //超过25秒, 重启播放器重试
                            Tan8PlayUtil.Restart(Id, 1, true);
                            return;
                        }
                    }
                }
                if(IsWorking)
                {
                    //以下代码仅下载成功执行, 如果下载成功, 不论是否中止, 都保存乐谱信息, 当作成功
                    //每次下载完, 保存最后下载的记录ID
                    _Tan8SheetCrawlRecordRepo.Save(Id, AutoDownload, Name, result.code, result.message);
                    Tan8PlayUtil.Exit(Id);

                    //触发下载完成事件
                    DownloadFinishAction?.Invoke(Id, result.success, IsWorking);
                    IsWorking = false;      //设为False将退出循环, 结束任务
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
                data = true     //标记是否可以删除
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
                proxyAddress = _ProxyAddressRepo.GetFirstProxy();
            }
            var ypinfostring = RequestUtil.CrawlContentFromWebsit(SheetUrl, proxyAddress).data;
            //var ypinfostring = @"<html><body>yp_create_time=<yp_create_time>1573183398</yp_create_time><br/>yp_title=<yp_title>说好不哭（文武贝钢琴版）</yp_title><br/>yp_page_count=<yp_page_count>3</yp_page_count><br/>yp_page_width=<yp_page_width>1089</yp_page_width><br/>yp_page_height=<yp_page_height>1540</yp_page_height><br/>yp_is_dadiao=<yp_is_dadiao>1</yp_is_dadiao><br/>yp_key_note=<yp_key_note>10</yp_key_note><br/>yp_is_yanyin=<yp_is_yanyin>1</yp_is_yanyin><br/>ypad_url=<ypad_url>http://www.tan8.com//yuepuku/132/66138/66138_hegiahcc.ypad</ypad_url>ypad_url2=<ypad_url2>http://www.tan8.com//yuepuku/132/66138/66138_hegiahcc.ypa2</ypad_url2></body></html>";
            //校验返回的乐谱信息
            var checkResult = CheckYuepuInfo(ypinfostring);
            if (UseProxy)
            {
                _ProxyAddressRepo.RemoveProxy(proxyAddress, checkResult == Tan8SheetDownloadResult.VISTI_REACH_LIMIT ? 0 : 15 * 1000);
            }

            if (checkResult != Tan8SheetDownloadResult.SUCCESS)
            {
                downloadResult.code = (byte)checkResult;
                downloadResult.message = EnumLoader.GetDesc(checkResult);
                return downloadResult;
            }
            //从乐谱信息解析到对象
            var tan8Music = DataUtil.ParseToModel(ypinfostring);
            //YpCount = tan8Music.yp_page_count;

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
            //if (downResult == 0) Cover = Path.Combine(libFolder, Id.ToString(), ApplicationConstant.DEFAULT_COVER_NAME);

            //封面下载完后校验图片是否有效
            if (IsWorking == false) return downloadResult;         //在IO操作之前检查任务是否停止
            if (downResult == 0)
            {
                var isValidPreviewImg = ImageUtil.CheckImgIsValid(coverSavePath);
                //如果文件损坏则删除
                if (IsWorking == false) return downloadResult;         //在IO操作之前检查任务是否停止
                if (!isValidPreviewImg) FileUtil.DeleteFile(coverSavePath);
            }
   
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

            //下载v3版mp3试听文件
            SetProgress(70, "下载试听文件...");
            //试听文件不在乎是否下载成功, 丢弃下载结果
            var urlPrefix = tan8Music.ypad_url.Substring(0, tan8Music.ypad_url.LastIndexOf('/'));
            var mp3_url = urlPrefix + string.Format("/tan8_{0}.mp3", Id);
            _ = new FileDownloader().DownloadFile(mp3_url, Path.Combine(saveFullPath, "play.mp3"));

                //下载v2版播放文件
                SetProgress(80, "下载播放文件...");
            downResult = new FileDownloader().DownloadFile(tan8Music.ypad_url2, Path.Combine(saveFullPath, "play.ypdx"));
            //没有播放文件, 又没有谱页的, 清理数据
            if (downResult != 0 && tan8Music.yp_page_count == 0)
            {
                downloadResult.code = (byte)Tan8SheetDownloadResult.PLAY_FILE_DOWNLOAD_FAIL;
                downloadResult.message = EnumLoader.GetDesc(Tan8SheetDownloadResult.PLAY_FILE_DOWNLOAD_FAIL);
                return downloadResult;
            }

            //step.6 保存到数据库
            SetProgress(90, "保存数据库...");
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
