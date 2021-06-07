using AcgnuX.Source.Bussiness.Common;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Taskx.Http;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using AcgnuX.Utils;
using AcgnuX.WindowX;
using AcgnuX.WindowX.Dialog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AcgnuX.Pages
{
    /// <summary>
    /// Tan8SheetReponsitory.xaml 的交互逻辑
    /// </summary>
    public partial class Tan8SheetReponsitory : BasePage
    {
        //默认的封面文件名
        private readonly string DEFAULT_COVER_NAME = "cover.jpg";
        //列表数据对象
        private ObservableCollection<PianoScoreViewModel> mPianoScoreList = new ObservableCollection<PianoScoreViewModel>();
        //下载worker
        //private readonly BackgroundWorker bgworker = new BackgroundWorker()
        //{
        //    WorkerReportsProgress = true,
        //    WorkerSupportsCancellation = true
        //};
        //弹吧播放器
        //private Tan8PlayerWindow mTan8Player;
        //分页
        private Pager pager = new Pager(1, 10);
        //tan8服务
        private HttpWebServer mTan8WebListener;
        //标识是否自动下载
        private bool isAutoDownload = false;
        //标识是否中止任务
        private bool isTaskStop = false;
        //下载进度变更事件
        public event StatusBarNotifyHandler OnTaskBarEvent;
        //曲谱下载完成事件
        public event Tan8SheetDownloadFinishHandler OnDownloadFinish;
        //下载记录窗口
        private Tan8DownloadRecordWindow mDownloadRecordWindow;
        //标记任务是否来自queue
        private bool mIsQueueTask = false;
        //需要下载的任务列表
        private Queue<int> mTaskQueue;

        public Tan8SheetReponsitory(MainWindow mainWin)
        {
            InitializeComponent();
            mMainWindow = mainWin;
            mDownloadRecordWindow = new Tan8DownloadRecordWindow();
            mDownloadRecordWindow.editConfirmHnadler += DownLoadTan8MusicV2;
            OnTaskBarEvent += mainWin.SetStatustProgess;
            OnDownloadFinish += mDownloadRecordWindow.OnTan8SheetDownloadFinish;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            LoadPianoScore();
            PianoScoreListBox.ItemsSource = mPianoScoreList;
            mMainWindow.OnClickStatusBarStop += ChangeTaskStatus;
            //工作任务
            //bgworker.DoWork += new DoWorkEventHandler(ReadyTask);
            //进度通知
            //bgworker.ProgressChanged += new ProgressChangedEventHandler(NotifyProgress);
            //下载完成事件
            //bgworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DownLoadComplate);
            //开启http监听
            if (null == mTan8WebListener)
            {
                mTan8WebListener = new HttpWebServer();
                mTan8WebListener.editConfirmHnadler += new EditConfirmHandler<PianoScore>(DownLoadTan8MusicV2Task);
                mTan8WebListener.StartListen();
            }
            DataContext = this;
        }

        /// <summary>
        /// 加载所有曲谱
        /// </summary>
        private void LoadPianoScore()
        {
            //var Loads = PianoScoreListBox.Dispatcher.BeginInvoke(new Action(() =>
            //{

            //mPianoScoreList.Clear();

            //查询关键字
            var keyword = SearchTextBox.Text;
            var sql = new StringBuilder(" FROM tan8_music WHERE 1 = 1");
            List<SQLiteParameter> sqlArgs = new List<SQLiteParameter>();
            if (!string.IsNullOrEmpty(keyword))
            {
                sql.Append(" and name like @name");
                sqlArgs.Add(new SQLiteParameter("@name", "%" + keyword + "%"));
                if (DataUtil.IsNum(keyword))
                {
                    sql.Append(" or ypid = @ypid");
                    sqlArgs.Add(new SQLiteParameter("@ypid", keyword));
                }
            }

            //查询总数
            var sqlRowResult = SQLite.sqlone("SELECT COUNT(1) total " + sql.ToString(), sqlArgs.ToArray());
            var totalRow = Convert.ToInt32(sqlRowResult);
            //没有查到内容
            if (totalRow == 0)
            {
                pager.TotalPage = 1;
                pager.CurrentPage = 1;
                mPianoScoreList.Clear();
                return;
            }
            pager.TotalPage = (totalRow + pager.MaxRow - 1) / pager.MaxRow;
            //查询记录
            sql.Append(" order by star desc limit @maxRow OFFSET @maxRow * (@curPage - 1)");
            sqlArgs.Add(new SQLiteParameter("@maxRow", pager.MaxRow));
            sqlArgs.Add(new SQLiteParameter("@curPage", pager.CurrentPage));

            var dataSet = SQLite.SqlTable("select ypid, name, star, yp_count, ver " + sql.ToString(), sqlArgs);

            //封装进对象
            var dataList = new List<PianoScoreViewModel>();
            foreach (DataRow dataRow in dataSet.Rows)
            {
                //拼接得到cover路径
                var imgDir = string.Format("{0}/{1}/{2}", ConfigUtil.Instance.PianoScorePath, dataRow["name"], DEFAULT_COVER_NAME);

                dataList.Add(new PianoScoreViewModel()
                {
                    id = Convert.ToInt32(dataRow["ypid"]),
                    Name = Convert.ToString(dataRow["name"]),
                    star = Convert.ToByte(dataRow["star"]),
                    YpCount = Convert.ToByte(dataRow["yp_count"]),
                    Ver = Convert.ToByte(dataRow["ver"]),
                    //对于不存在cover的路径使用默认图片
                    cover = File.Exists(imgDir) ? FileUtil.GetBitmapImage(imgDir) : new BitmapImage(new Uri("/Assets/Images/piano-cover-default.jpg", UriKind.Relative))
                });
            }

            switch (pager.Action)
            {
                case PageAction.PREVIOUS:
                    //如果是向上翻页, 则将数据反转后, 添加到0 位置
                    dataList.Reverse();
                    dataList.ForEach(e => mPianoScoreList.Insert(0, e));
                    break;
                case PageAction.NEXT:
                    //向下翻页直接添加到末尾
                    dataList.ForEach(e => mPianoScoreList.Add(e));
                    break;
                default:
                    //刷新则清空所有记录后重新添加
                    mPianoScoreList.Clear();
                    dataList.ForEach(e => mPianoScoreList.Add(e));
                    break;
            }
            //读取完成后将翻页动作设为当前
            pager.Action = PageAction.CURRENT;
            SetListBoxVisibility(true);
            //}));
            //Loads.Completed += new EventHandler(Loads_Completed);
        }

        /// <summary>
        /// 刷新按钮点击事件
        /// 刷新列表内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickRefreshButton(object sender, RoutedEventArgs e)
        {
            LoadPianoScore();
        }

        /// <summary>
        /// 刷新按钮点击事件
        /// 刷新列表内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadListButtonClick(object sender, RoutedEventArgs e)
        {
            mDownloadRecordWindow.Show();
        }

        /// <summary>
        /// 以主窗口为父窗口打开一个dialog
        /// </summary>
        /// <param name="pianoScore"></param>
        /// <returns></returns>
        private bool? OpenEditDialog(PianoScore pianoScore)
        {
            var dialog = new AddSinglePianoScoreDialog(pianoScore);
            //绑定窗口点击事件
            if (null == pianoScore)
            {
                //新增则绑定下载事件
                //dialog.editConfirmHnadler += new EditConfirmHandler<PianoScore>(DownLoadTan8Music);
                dialog.editConfirmHnadler += new EditConfirmHandler<PianoScore>(DownLoadTan8MusicV2);
            }
            else
            {
                //修改则绑定重命名文件夹事件
                dialog.editConfirmHnadler += new EditConfirmHandler<PianoScore>(RenameMusicName);
            }
            return dialog.ShowDialog();
        }

        /// <summary>
        /// 添加按钮点击事件
        /// 打开添加按钮对话框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnAddClick(object sender, RoutedEventArgs e)
        {
            OpenEditDialog(null);
        }

        /// <summary>
        /// 列表双击事件
        /// 打开数据修改对话框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListBoxDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = PianoScoreListBox.SelectedItem as PianoScore;
            var result = OpenEditDialog(selected);
            if (result.GetValueOrDefault())
            {

            }
        }

        /// <summary>
        /// 右键菜单展开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnContextMenuOpen(object sender, ContextMenuEventArgs e)
        {
        }

        /// <summary>
        /// 修改名称
        /// </summary>
        /// <param name="pianoScore"></param>
        private InvokeResult<PianoScore> RenameMusicName(PianoScore pianoScore)
        {
            //获取原始名称
            var dbName = SQLite.SqlRow(string.Format("SELECT name FROM tan8_music WHERE ypid = {0}", pianoScore.id));
            if (null == dbName || dbName.Length == 0)
            {
                return new InvokeResult<PianoScore>()
                {
                    success = false,
                    message = "无法获取原始数据"
                };
            }

            //修改文件夹名称
            FileUtil.RenameFolder(ConfigUtil.Instance.PianoScorePath + Path.DirectorySeparatorChar + dbName[0], pianoScore.Name);
            //修改数据库名称
            SQLite.ExecuteNonQuery("UPDATE tan8_music SET name = @name WHERE ypid = @ypid", new List<SQLiteParameter>
            { 
                new SQLiteParameter("@name", pianoScore.Name), 
                new SQLiteParameter("@ypid", pianoScore.id) 
            });
            //更新列表显示
            mPianoScoreList[PianoScoreListBox.SelectedIndex].NameView = pianoScore.Name;
            return new InvokeResult<PianoScore>()
            {
                success = true
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tan8SheetAddr"></param>
        private InvokeResult<PianoScore> DownLoadTan8MusicV2Task(PianoScore pianoScore)
        {
            //判断任务是否已经中止
            if (isTaskStop)
            {
                mIsQueueTask = false;
                isAutoDownload = false;
                Tan8PlayUtil.Exit();
                //任务停止, 上报进度100%
                OnTaskBarEvent?.Invoke(CalcProgress(new MainWindowStatusNotify()
                {
                    alertLevel = AlertLevel.INFO,
                    animateProgress = true,
                    nowProgress = 0
                }, "任务中止", 100));
                return InvokeSuccess(pianoScore);
            }

            //检查曲谱是否存在于数据库
            var sqlRowResult = SQLite.sqlone("SELECT COUNT(1) total FROM tan8_music where ypid = @ypid", 
                new SQLiteParameter[] { new SQLiteParameter("@ypid", pianoScore.id.GetValueOrDefault()) });
            if (!string.IsNullOrEmpty(sqlRowResult) && Convert.ToInt32(sqlRowResult) > 0)
            {
                if(isAutoDownload)
                {
                    Tan8PlayUtil.Restart(GetNextDownloadYpid(pianoScore.id.GetValueOrDefault()), 1, true);
                }
                return InvokeSuccess(pianoScore);
            }

            var startTimeMill = TimeUtil.CurrentMillis();
            var doContinue = true;
            InvokeResult<object> result;
            do
            {
                result = StartDownLoadV2(pianoScore);
                //下载出错则给出提示, 并等待下次任务
                if (!result.success)
                {
                    OnTaskBarEvent?.Invoke(CalcProgress(new MainWindowStatusNotify()
                    {
                        alertLevel = isAutoDownload ? AlertLevel.RUN : AlertLevel.ERROR,
                    },
                    string.Format(string.Format("乐谱ID [{0}] 下载出错 {1}{2}", pianoScore.id, result.message, isAutoDownload ? ", 等待任务中..." : "")),
                    isAutoDownload ? 99 : 100));

                    //如果是自动下载, 且由是由于网络连接出错, 则不保存下载记录, 直接重试
                    if ((result.code == 8 || result.code == 3 || result.code == 7) && isAutoDownload)
                    {
                        //25秒内不重启重试 ( flash请求等待时间超过30秒会弹窗报错, 此处限定为25秒)
                        if (TimeUtil.CurrentMillis() - startTimeMill < 25 * 1000)
                        {
                            continue;
                        }
                        //超过25秒, 重启播放器重试
                        Tan8PlayUtil.Restart(pianoScore.id, 1, true);
                        return InvokeSuccess(pianoScore);
                    }
                }
                doContinue = false;
            } while (doContinue && !isTaskStop);

            //每次下载完, 保存最后下载的记录ID
            SaveDownLoadRecord(pianoScore.id.GetValueOrDefault(), isAutoDownload, result);

            //触发下载完成事件
            OnDownloadFinish?.Invoke(pianoScore);

            //不是自动下载直接return
            if (!isAutoDownload)
            {
                Tan8PlayUtil.Exit();
                return InvokeSuccess(pianoScore);
            }

            //判断任务是否已经中止
            if (isTaskStop)
            {
                isAutoDownload = false;
                Tan8PlayUtil.Exit();
                //任务停止, 上报进度100%
                OnTaskBarEvent?.Invoke(CalcProgress(new MainWindowStatusNotify()
                {
                    alertLevel = AlertLevel.INFO,
                    animateProgress = true,
                    nowProgress = 0
                }, "任务中止", 100));
                return InvokeSuccess(pianoScore);
            }
            //如果开启了自动下载, 则无限循环
            Tan8PlayUtil.Restart(GetNextDownloadYpid(pianoScore.id.GetValueOrDefault()), 1, true);
            return InvokeSuccess(pianoScore);
        }

        /// <summary>
        /// 下载弹8曲谱
        /// 开启后台下载曲谱任务
        /// </summary>
        /// <param name="pianoScore">曲谱对象</param>
        /// <returns>公共函数返回结果</returns>
        private InvokeResult<PianoScore> DownLoadTan8MusicV2(PianoScore pianoScore)
        {
            //校验基本参数
            if (null == pianoScore || null == pianoScore.id)
            {
                //如果有待重新下载的任务, 优先下载
                var queueTaskIds = SQLite.sqlcolumn("SELECT ypid FROM tan8_music_down_task", null);
                if(null == mTaskQueue)
                {
                    mTaskQueue = new Queue<int>();
                }
                mTaskQueue.Clear();
                queueTaskIds.ForEach(e => mTaskQueue.Enqueue(Convert.ToInt32(e)));
                var lastYpid = 1;
                if (mTaskQueue.Count > 0)
                {
                    mIsQueueTask = true;
                    lastYpid = mTaskQueue.Dequeue();
                } 
                else
                {
                    var recordLastId = SQLite.sqlone("SELECT ypid FROM tan8_music_down_record WHERE code = @code1 or code = @code2 ORDER BY create_time DESC LIMIT 1",
                        new SQLiteParameter[] {
                        new SQLiteParameter("@code1", Convert.ToInt32(Tan8SheetDownloadResult.SUCCESS)),
                        new SQLiteParameter("@code2", Convert.ToInt32(Tan8SheetDownloadResult.PIANO_SCORE_NOT_EXSITS))
                    });
                    if(!string.IsNullOrEmpty(recordLastId))
                    {
                        lastYpid = Convert.ToInt32(recordLastId) + 1;
                    }
                }
                pianoScore = new PianoScore
                {
                    id = lastYpid,
                    autoDownload = true
                };
            }
            isTaskStop = false;
            isAutoDownload = pianoScore.autoDownload;
            //打开播放器, 触发主动下载
            Tan8PlayUtil.Exit();
            Tan8PlayUtil.ExePlayById(pianoScore.id.GetValueOrDefault(), 1, true);
            return InvokeSuccess(pianoScore);
        }

        /// <summary>
        /// 下载弹8曲谱
        /// 开启后台下载曲谱任务
        /// </summary>
        /// <param name="pianoScore">曲谱对象</param>
        /// <returns>公共函数返回结果</returns>
        /***
        private InvokeResult<PianoScore> DownLoadTan8Music(PianoScore pianoScore)
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
            var pianoScore = e.Argument as PianoScore;
            //获取将要下载的乐谱id
            var startId = pianoScore.id.GetValueOrDefault();
            do
            {
                //自增 startId, 如果是自动下载, 则下次ID会自动提升
                var result = StartDownLoad(pianoScore);

                //下载出错则给出提示, 并等待下次任务
                if (!result.success)
                {
                    bgworker.ReportProgress(0, CalcProgress(new MainWindowStatusNotify()
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
                    bgworker.ReportProgress(0, CalcProgress(new MainWindowStatusNotify()
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
        private InvokeResult<object> StartDownLoad(PianoScore pianoScore)
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
            bgworker.ReportProgress(0, CalcProgress(winProgress, string.Format("开始下载 [{0}], 解析下载地址", pianoScore.Name), 0));
            //使用v1版本, 从flash播放器读取
            var sample = mTan8Player.GetRealTan8URL(pianoScore.id.GetValueOrDefault());
            //xml字符串去除转义
            var urlFromSwf = DataUtil.GetXmlNodeValue(sample, "string").Replace("amp;", "");
            //由于播放器被本地化, 请求乐谱信息时会访问本地地址, 此处在线下载需要修改为线上地址
            var url = urlFromSwf.Replace("localhost:7777/yuepu/info", "www.77music.com/flash_get_yp_info.php");

            //step.2 请求乐谱地址, 得到乐谱信息
            bgworker.ReportProgress(0, CalcProgress(winProgress, "下载地址解析成功, 开始加载乐谱信息", 10));
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
            bgworker.ReportProgress(0, CalcProgress(winProgress, "乐谱加载成功, 创建归档文件夹", 20));
            var folder = string.IsNullOrEmpty(pianoScore.Name) ? tan8Music.yp_title : pianoScore.Name;
            //替换非法字符
            folder = FileUtil.ReplaceInvalidChar(folder);
            var folderPath = AcgnuConfig.GetContext().PianoScorePath + Path.DirectorySeparatorChar + folder;
            FileUtil.CreateFolder(folderPath);
            //添加分隔符
            folderPath += Path.DirectorySeparatorChar;

            //step.3 下载曲谱封面
            bgworker.ReportProgress(0, CalcProgress(winProgress, "下载乐谱封面", 30));
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
                bgworker.ReportProgress(0, CalcProgress(winProgress, message, 50 / tan8Music.yp_page_count + winProgress.nowProgress));

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
            bgworker.ReportProgress(0, CalcProgress(winProgress, "下载播放文件", 80));
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
            bgworker.ReportProgress(0, CalcProgress(winProgress, "保存数据库", 90));
            SaveMusicToDB(pianoScore.id.GetValueOrDefault(), folder, tan8Music, ypinfostring);

            winProgress.alertLevel = AlertLevel.INFO;
            bgworker.ReportProgress(0, CalcProgress(winProgress, "下载完成", 100));
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
        ///  下载弹琴吧琴谱后台任务 (不汇报进度)
        /// </summary>
        /// <param name="pianoScore">乐谱信息</param>
        private InvokeResult<object> StartDownLoadV2(PianoScore pianoScore)
        {
            var winProgress = new MainWindowStatusNotify()
            {
                alertLevel = AlertLevel.RUN,
                animateProgress = true,
                progressDuration = 100,
                nowProgress = 0
            };

            //直接使用v2版本的地址
            var url = pianoScore.SheetUrl;
            OnTaskBarEvent?.Invoke(CalcProgress(winProgress, string.Format("乐谱ID: {0} 下载地址解析成功, 开始加载乐谱信息", pianoScore.id), 10));

            //step.2 请求乐谱地址, 得到乐谱信息
            var proxyAddress = ProxyFactory.GetFirstProxy();
            var ypinfostring = RequestUtil.CrawlContentFromWebsit(url, proxyAddress).data;
            //var ypinfostring = @"<html><body>yp_create_time=<yp_create_time>1573183398</yp_create_time><br/>yp_title=<yp_title>说好不哭（文武贝钢琴版）</yp_title><br/>yp_page_count=<yp_page_count>3</yp_page_count><br/>yp_page_width=<yp_page_width>1089</yp_page_width><br/>yp_page_height=<yp_page_height>1540</yp_page_height><br/>yp_is_dadiao=<yp_is_dadiao>1</yp_is_dadiao><br/>yp_key_note=<yp_key_note>10</yp_key_note><br/>yp_is_yanyin=<yp_is_yanyin>1</yp_is_yanyin><br/>ypad_url=<ypad_url>http://www.tan8.com//yuepuku/132/66138/66138_hegiahcc.ypad</ypad_url>ypad_url2=<ypad_url2>http://www.tan8.com//yuepuku/132/66138/66138_hegiahcc.ypa2</ypad_url2></body></html>";
            //校验返回的乐谱信息
            var checkResult = CheckYuepuInfo(ypinfostring);
            ProxyFactory.RemoveProxy(proxyAddress, checkResult == Tan8SheetDownloadResult.VISTI_REACH_LIMIT ? 0 : 15 * 1000);
            if (checkResult != Tan8SheetDownloadResult.SUCCESS)
            {
                return new InvokeResult<object>()
                {
                    success = false,
                    code = (byte)checkResult,
                    message = EnumLoader.GetEnumDesc(typeof(Tan8SheetDownloadResult), checkResult.ToString()),
                    data = "未知"
                };
            }
            //从乐谱信息解析到对象
            var tan8Music = DataUtil.ParseToModel(ypinfostring);

            var ypNameFolder = string.IsNullOrEmpty(pianoScore.Name) ? tan8Music.yp_title : pianoScore.Name;
            //替换非法字符
            ypNameFolder = FileUtil.ReplaceInvalidChar(ypNameFolder);
            if(string.IsNullOrEmpty(ypNameFolder))
            {
                ypNameFolder = "非法名称_" + pianoScore.id;
            }
            //校验保存路径是否重复
            var libFolder = ConfigUtil.Instance.PianoScorePath + Path.DirectorySeparatorChar;
            if(Directory.Exists(libFolder + ypNameFolder))
            {
                ypNameFolder += "(" + pianoScore.id + ")";
            }
            var saveFullPath = libFolder + ypNameFolder;
            //step.3 创建文件夹
            FileUtil.CreateFolder(saveFullPath);
            //添加分隔符
            saveFullPath += Path.DirectorySeparatorChar;

            //step.3 下载曲谱封面
            OnTaskBarEvent?.Invoke(CalcProgress(winProgress, string.Format("下载 [{0}] 封面", ypNameFolder), 30));
            var coverUrl = tan8Music.ypad_url.Substring(0, tan8Music.ypad_url.IndexOf('_')) + "_prev.jpg";
            var downResult = new FileDownloader().DownloadFile(coverUrl, saveFullPath + DEFAULT_COVER_NAME);
            //封面下载失败不管
            if (downResult != 0) { }

            //封面下载完后校验图片是否有效
            var isValidPreviewImg = FileUtil.CheckImgIsValid(saveFullPath + DEFAULT_COVER_NAME);
            //如果文件损坏则删除
            if (!isValidPreviewImg) FileUtil.DeleteFile(saveFullPath + DEFAULT_COVER_NAME);

            //step.4 下载乐谱图片
            for (var i = 0; i < tan8Music.yp_page_count; i++)
            {
                var message = string.Format("下载 [{0}] 乐谱 {1} / {2}", ypNameFolder, i + 1, tan8Music.yp_page_count);
                OnTaskBarEvent?.Invoke(CalcProgress(winProgress, message, 50 / tan8Music.yp_page_count + winProgress.nowProgress));

                var downloadUrl = tan8Music.ypad_url + string.Format(".{0}.png", i);
                int pageDownloadResult = new FileDownloader().DownloadFile(downloadUrl, saveFullPath + string.Format("page.{0}.png", i));
                //如果下载出错
                if (pageDownloadResult != 0)
                {
                    //清理下载文件
                    FileUtil.DeleteDirWithName(libFolder, ypNameFolder);

                    return new InvokeResult<object>()
                    {
                        success = false,
                        code = (byte)Tan8SheetDownloadResult.PIANO_SCORE_DOWNLOAD_FAIL,
                        message = EnumLoader.GetEnumDesc(typeof(Tan8SheetDownloadResult), Tan8SheetDownloadResult.PIANO_SCORE_DOWNLOAD_FAIL.ToString()),
                        data = ypNameFolder
                    };
                }
            }
            //下载v2版播放文件
            OnTaskBarEvent?.Invoke(CalcProgress(winProgress, string.Format("下载 [{0}] 播放文件", ypNameFolder), 80));
            downResult = new FileDownloader().DownloadFile(tan8Music.ypad_url2, saveFullPath + "play.ypdx");
            if (downResult != 0)
            {
                return new InvokeResult<object>()
                {
                    success = false,
                    code = (byte)Tan8SheetDownloadResult.PLAY_FILE_DOWNLOAD_FAIL,
                    message = EnumLoader.GetEnumDesc(typeof(Tan8SheetDownloadResult), Tan8SheetDownloadResult.PLAY_FILE_DOWNLOAD_FAIL.ToString()),
                    data = ypNameFolder
                };
            }

            //step.6 保存到数据库
            OnTaskBarEvent?.Invoke(CalcProgress(winProgress, string.Format("[{0}] 保存数据库", ypNameFolder), 90));
            SaveMusicToDB(pianoScore.id.GetValueOrDefault(), ypNameFolder, tan8Music, ypinfostring);

            winProgress.alertLevel = AlertLevel.INFO;
            OnTaskBarEvent?.Invoke(CalcProgress(winProgress, string.Format("[{0}] 下载完成", ypNameFolder), 100));
            return new InvokeResult<object>()
            {
                success = true,
                code = (byte)Tan8SheetDownloadResult.SUCCESS,
                message = EnumLoader.GetEnumDesc(typeof(Tan8SheetDownloadResult), Tan8SheetDownloadResult.SUCCESS.ToString()),
                data = ypNameFolder
            };
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

        /// <summary>
        /// 保存乐谱信息到数据库
        /// </summary>
        /// <param name="ypid">弹琴吧的乐谱ID</param>
        /// <param name="name">保存的谱名</param>
        /// <param name="tan8Music">弹琴吧接口对象</param>
        /// <param name="originstr">原始接口数据</param>
        /// <returns>数据库操作成功条数</returns>
        private int SaveMusicToDB(int ypid, string name, Tan8music tan8Music, string originstr)
        {
            return SQLite.ExecuteNonQuery("insert or ignore into tan8_music(ypid, `name`, star, yp_count, origin_data) VALUES (@ypid, @name, @star, @yp_count, @origin_data)", 
                new List<SQLiteParameter>
                {
                    new SQLiteParameter("@ypid", ypid) ,
                    new SQLiteParameter("@name", name) ,
                    new SQLiteParameter("@star", 0 as object) ,
                    new SQLiteParameter("@yp_count", tan8Music.yp_page_count) ,
                    new SQLiteParameter("@origin_data", originstr)
                 });
        }

        /// <summary>
        /// 保存乐谱的下载记录
        /// </summary>
        /// <param name="ypid">乐谱ID</param>
        /// <param name="isAuto">是否自动下载</param>
        /// <param name="InvokeResult">下载结果</param>
        /// <returns></returns>
        private int SaveDownLoadRecord(int ypid, bool isAuto, InvokeResult<Object> invokeResult)
        {
            return SQLite.ExecuteNonQuery("INSERT INTO tan8_music_down_record(id, ypid, name, code, result, create_time, is_auto) VALUES((SELECT IFNULL(MAX(id),0)  + 1 FROM tan8_music_down_record), @ypid, @result, @code, @message, datetime('now', 'localtime'), @isAuto)",
                new List<SQLiteParameter> 
                {
                    new SQLiteParameter("@ypid", ypid) ,
                    new SQLiteParameter("@result", Convert.ToString(invokeResult.data)) ,
                    new SQLiteParameter("@code", invokeResult.code) ,
                    new SQLiteParameter("@message", invokeResult.message) ,
                    new SQLiteParameter("@isAuto", isAuto)
                 });
        }

        /// <summary>
        /// 下载完成事件, 刷新列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownLoadComplate(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadPianoScore();
        }

        /// <summary>
        /// 发送进度到主信息栏
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="message"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private MainWindowStatusNotify CalcProgress(MainWindowStatusNotify notify, string message, double value)
        {
            notify.message = message;
            notify.oldProgress = notify.nowProgress;
            notify.nowProgress = value;
            return notify;
        }

        /// <summary>
        /// 删除事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var selected = PianoScoreListBox.SelectedItem as PianoScoreViewModel;
            if (null == selected) return;
            var confirmDialog = new ConfirmDialog(AlertLevel.WARN, string.Format((string)Application.Current.FindResource("DeleteConfirm"), selected.Name));
            if (confirmDialog.ShowDialog().GetValueOrDefault())
            {
                //释放文件资源
                selected.cover = null;
                mPianoScoreList.Remove(selected);

                //删除文件夹
                if(!string.IsNullOrEmpty(selected.Name))
                {
                    FileUtil.DeleteDirWithName(ConfigUtil.Instance.PianoScorePath, selected.Name);
                }

                //删除数据库数据
                SQLite.ExecuteNonQuery("DELETE FROM tan8_music WHERE ypid = @ypid", new List<SQLiteParameter> { new SQLiteParameter("@ypid", selected.id)});

                //如果没有曲谱了, 则展示默认按钮
                if (mPianoScoreList.Count == 0) SetListBoxVisibility(false);
            }
        }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenFolderClick(object sender, RoutedEventArgs e)
        {
            var selected = PianoScoreListBox.SelectedItem as PianoScoreViewModel;
            if (null == selected) return;
            var fullPath = ConfigUtil.Instance.PianoScorePath + Path.DirectorySeparatorChar + selected.Name;
            if(Directory.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(fullPath);
            }
        }

        /// <summary>
        /// 当主窗口点击停止任务后, 改变任务状态
        /// </summary>
        private void ChangeTaskStatus()
        {
            isTaskStop = true;
            //bgworker.CancelAsync();
        }

        /// <summary>
        /// 默认播放按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void OnDefaultPlayButtonClick(object sender, RoutedEventArgs e)
        //{
        //    //播放器未打开, 则创建一个新的播放器
        //    if (null == mTan8Player)
        //    {
        //        mTan8Player = new Tan8PlayerWindow();
        //    }
        //    //播放所选曲谱
        //    mTan8Player.Show();
        //    mTan8Player.PlaySelected(new PianoScore() { id = 0 });
        //}

        /// <summary>
        /// 由于Flash被禁用, 使用内置的flash播放器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDefaultPlayButtonClickV2(object sender, RoutedEventArgs e)
        {
            Tan8PlayUtil.ExePlayById(0, 1, false);
        }

        /// <summary>
        /// 设置主列表和默认按钮的显示和隐藏
        /// </summary>
        /// <param name="showListBox"></param>
        private void SetListBoxVisibility(bool showListBox)
        {
            if (showListBox)
            {
                PianoScoreListBox.Visibility = Visibility.Visible;
                DefaultOpenPlayerButton.Visibility = Visibility.Collapsed;
                return;
            }
            PianoScoreListBox.Visibility = Visibility.Collapsed;
            DefaultOpenPlayerButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 曲谱列表滚动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPianoScoreListBoxScroll(object sender, ScrollChangedEventArgs e)
        {
            var scrollView = e.OriginalSource as ScrollViewer;
            if (XamlUtil.IsScrollToBottom(scrollView))
            {
                //触底
                if (pager.CurrentPage + 1 > pager.TotalPage) return;
                pager.CurrentPage++;
                pager.Action = PageAction.NEXT;
                //移除之前的
                CheckAndRemoveItem();
                LoadPianoScore();
            }
            if (scrollView.VerticalOffset == 0)
            {
                //触顶
                if (pager.CurrentPage - 1 < 1) return;
                pager.CurrentPage--;
                pager.Action = PageAction.PREVIOUS;
                //移除3页之后的
                CheckAndRemoveItem();
                LoadPianoScore();
            }
        }

        /// <summary>
        /// 如果数据量大于5页的数据, 则删除之前/后的数据
        /// </summary>
        private void CheckAndRemoveItem()
        {
            //将主数据控制一定范围内
            if (mPianoScoreList.Count > 4 * pager.MaxRow)
            {
                for (var i = 0; i < pager.MaxRow; i++)
                {
                    //向上翻页, 则删除末端的数据
                    //向下翻页, 则删掉顶部的数据
                    mPianoScoreList.RemoveAt(pager.Action == PageAction.PREVIOUS ? mPianoScoreList.Count - 1 : 0);
                }
            }
        }

        /// <summary>
        /// 搜索框键盘按下事件
        /// 按照输入框的内容进行条件筛选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFilterBoxKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                pager.CurrentPage = 1;
                LoadPianoScore();
            }
        }

        /// <summary>
        /// 返回下一个需要下载的乐谱ID
        /// 如果是重新下载, 返回下一个需要重新下载的任务, 如果没有了需要重新下载的, 则返回0
        /// 如果不是重新下载, 返回递增的任务ID
        /// </summary>
        /// <param name="curYpid"></param>
        /// <returns></returns>
        private int? GetNextDownloadYpid(int curYpid)
        {
            if (mIsQueueTask)
            {
                //取下一个ID时, 删除上一个ID
                SQLite.ExecuteNonQuery("DELETE FROM tan8_music_down_task WHERE ypid = @ypid", new List<SQLiteParameter> { new SQLiteParameter("@ypid", curYpid) });
                if (mTaskQueue.Count == 0)
                {
                    isTaskStop = true;
                    return 0;
                }
                return mTaskQueue.Dequeue();
            }
            return curYpid + 1;
        }
    }
}
