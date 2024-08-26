using AcgnuX.Source.Bussiness.Common;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Taskx.Http;
using AcgnuX.Source.ViewModel;
using AcgnuX.WindowX;
using AcgnuX.WindowX.Dialog;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AcgnuX.Pages
{
    /// <summary>
    /// Tan8SheetReponsitory.xaml 的交互逻辑
    /// </summary>
    public partial class Tan8SheetReponsitory : BasePage
    {
        //弹吧播放器
        //private Tan8PlayerWindow mTan8Player;
        //tan8服务
        private readonly HttpWebServer _Tan8WebListener = new HttpWebServer();
        //private object mCollectLock = new object();
        //view model
        private Tan8SheetReponsitoryViewModel _ViewModel;


        public Tan8SheetReponsitory()
        {
            InitializeComponent();
            _ViewModel = DataContext as Tan8SheetReponsitoryViewModel;
            //BindingOperations.EnableCollectionSynchronization(mPianoScoreList, mCollectLock);
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if (_ViewModel.IsEmpty)
            {
                _ViewModel.Load();
            }

            //开启http监听
            if (false == _Tan8WebListener.IsListen)
            {
                _Tan8WebListener.StartListen();
            }
        }

        /// <summary>
        /// 打开下载管理器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnOpenDownloadManageClick(object sender, RoutedEventArgs e)
        {
            Tan8DownloadManageWindow _DownloadMangeWindow = null;
            if (null == _DownloadMangeWindow)
            {
                _DownloadMangeWindow = new Tan8DownloadManageWindow()
                {
                    ShowInTaskbar = true,
                    Owner = null
                };
                _Tan8WebListener.DownloadRequestAction = _DownloadMangeWindow.ContentDataContext.DoDownloadTaskDispatche;
            }
            _DownloadMangeWindow.Show();
        }

        /// <summary>
        /// 下载弹8曲谱
        /// 开启后台下载曲谱任务
        /// </summary>
        /// <param name="pianoScore">曲谱对象</param>
        /// <returns>公共函数返回结果</returns>
        /***
        private InvokeResult<Tan8Sheet> DownLoadTan8Music(Tan8Sheet pianoScore)
        {
            //校验基本参数
            if (null == pianoScore || null == pianoScore.id)
            {
                mMainWindow.SetStatustProgess(new MainWindowStatusNotify()
                {
                    alertLevel = AlertLevel.ERROR,
                    message = "乐谱ID未填写"
                });
                return InvokeFail(pianoScore);
            }

            //从ListBox获取播放器
            if (null == mTan8Player)
            {
                mTan8Player = PianoScoreListBox.Tag as Tan8PlayerWindow;
            }
            //校验播放器状态
            if (null == mTan8Player)
            {
                mMainWindow.SetStatustProgess(new MainWindowStatusNotify()
                {
                    alertLevel = AlertLevel.ERROR,
                    message = "播放器未打开"
                });
                return InvokeFail(pianoScore);
            }

            //校验任务任务运行状态
            if (bgworker.IsBusy)
            {
                mMainWindow.SetStatustProgess(new MainWindowStatusNotify()
                {
                    alertLevel = AlertLevel.ERROR,
                    message = "存在尚未结束的下载任务"
                });
                return InvokeFail(pianoScore);
            }
            mIsV2 = false;
            bgworker.RunWorkerAsync(pianoScore);
            return InvokeSuccess(pianoScore);
        }
        ***/

        /// <summary>
        /// 准备下载任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /****
        private void ReadyTask(object sender, DoWorkEventArgs e)
        {
            var pianoScore = e.Argument as Tan8Sheet;
            //获取将要下载的乐谱id
            var startId = pianoScore.id.GetValueOrDefault();
            do
            {
                //自增 startId, 如果是自动下载, 则下次ID会自动提升
                var result = StartDownLoad(pianoScore);

                //下载出错则给出提示, 并等待下次任务
                if (!result.success)
                {
                    bgworker.ReportProgress(0, WindowUtil.CalcProgress(new MainWindowStatusNotify()
                    {
                        alertLevel = pianoScore.autoDownload ? AlertLevel.RUN : AlertLevel.ERROR,
                        //animateProgress = false,
                    },
                    string.Format(string.Format("乐谱ID [{0}] 下载出错 {1}{2}", startId, result.message, pianoScore.autoDownload ? ", 等待任务中..." : "")),
                    pianoScore.autoDownload ? 99 : 100));

                    //如果是自动下载, 且由是由于网络连接出错, 则不保存下载记录, 直接重试
                    if ((result.code == 8 || result.code == 3 || result.code == 7) && pianoScore.autoDownload)
                    {
                        continue;
                    }
                }

                //每次下载完, 保存最后下载的记录ID
                SaveDownLoadRecord(startId++, pianoScore.autoDownload, result);

                //不是自动下载直接break
                if (!pianoScore.autoDownload) break;

                //判断任务是否已经中止
                if (bgworker.CancellationPending)
                {
                    //任务停止, 上报进度100%
                    bgworker.ReportProgress(0, WindowUtil.CalcProgress(new MainWindowStatusNotify()
                    {
                        alertLevel = AlertLevel.INFO,
                        animateProgress = true,
                        nowProgress = 0
                    }, "任务中止", 100));
                    return;
                }
            }
            while (pianoScore.autoDownload);   //如果开启了自动下载, 则无限循环
        }
        ***/

        /// <summary>
        ///  下载弹琴吧琴谱后台任务
        /// </summary>
        /// <param name="pianoScore">乐谱信息</param>
        /***
        private InvokeResult<object> StartDownLoad(Tan8Sheet pianoScore)
        {
            var winProgress = new MainWindowStatusNotify()
            {
                alertLevel = AlertLevel.RUN,
                animateProgress = true,
                progressDuration = 100,
                nowProgress = 0
            };

            //step.1 获取真实乐谱下载地址
            //var sample = "<string>http://www.77music.com/flash_get_yp_info.php?ypid=66138&amp;sccode=77c83a7bf44542486ff37815ab75c147&amp;r1=9185&amp;r2=6640&amp;input=123</string>";
            bgworker.ReportProgress(0, WindowUtil.CalcProgress(winProgress, string.Format("开始下载 [{0}], 解析下载地址", pianoScore.Name), 0));
            //使用v1版本, 从flash播放器读取
            var sample = mTan8Player.GetRealTan8URL(pianoScore.id.GetValueOrDefault());
            //xml字符串去除转义
            var urlFromSwf = DataUtil.GetXmlNodeValue(sample, "string").Replace("amp;", "");
            //由于播放器被本地化, 请求乐谱信息时会访问本地地址, 此处在线下载需要修改为线上地址
            var url = urlFromSwf.Replace("localhost:7777/yuepu/info", "www.77music.com/flash_get_yp_info.php");

            //step.2 请求乐谱地址, 得到乐谱信息
            bgworker.ReportProgress(0, WindowUtil.CalcProgress(winProgress, "下载地址解析成功, 开始加载乐谱信息", 10));
            var proxyAddress = ProxyFactory.GetRandProxy();
            var ypinfostring = RequestUtil.CrawlContentFromWebsit(url, proxyAddress).data;
            //var ypinfostring = @"<html><body>yp_create_time=<yp_create_time>1573183398</yp_create_time><br/>yp_title=<yp_title>说好不哭（文武贝钢琴版）</yp_title><br/>yp_page_count=<yp_page_count>3</yp_page_count><br/>yp_page_width=<yp_page_width>1089</yp_page_width><br/>yp_page_height=<yp_page_height>1540</yp_page_height><br/>yp_is_dadiao=<yp_is_dadiao>1</yp_is_dadiao><br/>yp_key_note=<yp_key_note>10</yp_key_note><br/>yp_is_yanyin=<yp_is_yanyin>1</yp_is_yanyin><br/>ypad_url=<ypad_url>http://www.tan8.com//yuepuku/132/66138/66138_hegiahcc.ypad</ypad_url>ypad_url2=<ypad_url2>http://www.tan8.com//yuepuku/132/66138/66138_hegiahcc.ypa2</ypad_url2></body></html>";
            //校验返回的乐谱信息
            var checkResult = CheckYuepuInfo(ypinfostring);
            ProxyFactory.RemoveTemporary(proxyAddress, checkResult == PianoScoreDownloadResult.VISTI_REACH_LIMIT ? 0 : 15 * 1000);
            if (checkResult != PianoScoreDownloadResult.SUCCESS)
            {
                return new InvokeResult<object>()
                {
                    success = false,
                    code = (byte)checkResult,
                    message = EnumLoader.GetEnumDesc(typeof(PianoScoreDownloadResult), checkResult.ToString()),
                    data = "未知"
                };
            }
            //从乐谱信息解析到对象
            var tan8Music = DataUtil.ParseToModel(ypinfostring);

            //step.3 创建文件夹
            bgworker.ReportProgress(0, WindowUtil.CalcProgress(winProgress, "乐谱加载成功, 创建归档文件夹", 20));
            var folder = string.IsNullOrEmpty(pianoScore.Name) ? tan8Music.yp_title : pianoScore.Name;
            //替换非法字符
            folder = FileUtil.ReplaceInvalidChar(folder);
            var folderPath = AcgnuConfig.GetContext().PianoScorePath + Path.DirectorySeparatorChar + folder;
            FileUtil.CreateFolder(folderPath);
            //添加分隔符
            folderPath += Path.DirectorySeparatorChar;

            //step.3 下载曲谱封面
            bgworker.ReportProgress(0, WindowUtil.CalcProgress(winProgress, "下载乐谱封面", 30));
            var coverUrl = tan8Music.ypad_url.Substring(0, tan8Music.ypad_url.IndexOf('_')) + "_prev.jpg";
            var downResult = new FileDownloader().DownloadFile(coverUrl, folderPath + DEFAULT_COVER_NAME);
            //封面下载失败不管
            if (downResult != 0)
            {
                //return new InvokeResult<object>()
                //{
                //    success = false,
                //    code = 4,
                //    message = "封面下载失败",
                //    data = folder
                //};
            }

            //封面下载完后校验图片是否有效
            var isValidPreviewImg = FileUtil.CheckImgIsValid(folderPath + DEFAULT_COVER_NAME);
            //如果文件损坏则删除
            if (!isValidPreviewImg) FileUtil.DeleteFile(folderPath + DEFAULT_COVER_NAME);

            //step.4 下载乐谱图片
            for (var i = 0; i < tan8Music.yp_page_count; i++)
            {
                var message = string.Format("下载乐谱 {0} / {1}", i + 1, tan8Music.yp_page_count);
                bgworker.ReportProgress(0, WindowUtil.CalcProgress(winProgress, message, 50 / tan8Music.yp_page_count + winProgress.nowProgress));

                var downloadUrl = tan8Music.ypad_url + string.Format(".{0}.png", i);
                int pageDownloadResult = new FileDownloader().DownloadFile(downloadUrl, folderPath + string.Format("page.{0}.png", i));
                //如果下载出错
                if (pageDownloadResult != 0)
                {
                    return new InvokeResult<object>()
                    {
                        success = false,
                        code = (byte)PianoScoreDownloadResult.PIANO_SCORE_DOWNLOAD_FAIL,
                        message = EnumLoader.GetEnumDesc(typeof(PianoScoreDownloadResult), PianoScoreDownloadResult.PIANO_SCORE_DOWNLOAD_FAIL.ToString()),
                        data = folder
                    };
                }
            }

            //step.5 下载播放文件 ( 弹8已移除旧版播放文件, 此代码无法正常工作, 现在使用v2 )
            bgworker.ReportProgress(0, WindowUtil.CalcProgress(winProgress, "下载播放文件", 80));
            downResult = new FileDownloader().DownloadFile(tan8Music.ypad_url2, folderPath + "play.ypa2");
            if (downResult != 0)
            {
                return new InvokeResult<object>()
                {
                    success = false,
                    code = (byte)PianoScoreDownloadResult.PLAY_FILE_DOWNLOAD_FAIL,
                    message = EnumLoader.GetEnumDesc(typeof(PianoScoreDownloadResult), PianoScoreDownloadResult.PLAY_FILE_DOWNLOAD_FAIL.ToString()),
                    data = folder
                };
            }

            //step.6 保存到数据库
            bgworker.ReportProgress(0, WindowUtil.CalcProgress(winProgress, "保存数据库", 90));
            SaveMusicToDB(pianoScore.id.GetValueOrDefault(), folder, tan8Music, ypinfostring);

            winProgress.alertLevel = AlertLevel.INFO;
            bgworker.ReportProgress(0, WindowUtil.CalcProgress(winProgress, "下载完成", 100));
            return new InvokeResult<object>()
            {
                success = true,
                code = (byte)PianoScoreDownloadResult.SUCCESS,
                message = EnumLoader.GetEnumDesc(typeof(PianoScoreDownloadResult), PianoScoreDownloadResult.SUCCESS.ToString()),
                data = folder
            };
        }
        ***/

        /// <summary>
        /// 删除事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var selected = _ViewModel.SelectedListData;
            if (null == selected) return;
            var confirmDialog = new ConfirmDialog(AlertLevel.WARN, string.Format(Properties.Resources.S_DeleteConfirm, selected.Name));
            if (confirmDialog.ShowDialog().GetValueOrDefault())
            {
                _ViewModel.DeleteItem(selected);
            }
        }
    }
}
