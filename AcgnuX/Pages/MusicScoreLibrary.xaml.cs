﻿using AcgnuX.Source.Bussiness.Common;
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
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AcgnuX.Pages
{
    /// <summary>
    /// MusicScoreLibrary.xaml 的交互逻辑
    /// </summary>
    public partial class MusicScoreLibrary : BasePage
    {
        //默认的封面文件名
        private readonly string DEFAULT_COVER_NAME = "cover.jpg";
        //列表数据对象
        private ObservableCollection<PianoScoreViewModel> mPianoScoreList = new ObservableCollection<PianoScoreViewModel>();
        //下载worker
        private BackgroundWorker bgworker = new BackgroundWorker();
        //弹吧播放器
        private Tan8PlayerWindow mTan8Player;
        //分页
        private Pager pager = new Pager(1, 10);
        //tan8服务
        private HttpWebServer mTan8WebListener;
        //标识
        private bool mIsV2 = false;

        public MusicScoreLibrary(MainWindow mainWin)
        {
            InitializeComponent();
            mMainWindow = mainWin;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            LoadPianoScore();
            PianoScoreListBox.ItemsSource = mPianoScoreList;
            mMainWindow.OnClickStatusBarStop += ChangeTaskStatus;
            //开启异步任务
            bgworker.WorkerReportsProgress = true;
            //支持取消
            bgworker.WorkerSupportsCancellation = true;
            //工作任务
            bgworker.DoWork += new DoWorkEventHandler(ReadyTask);
            //进度通知
            bgworker.ProgressChanged += new ProgressChangedEventHandler(NotifyProgress);
            //下载完成事件
            bgworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DownLoadComplate);
            //开启http监听
            if(null == mTan8WebListener)
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
            var condition = "";
            if (!string.IsNullOrEmpty(keyword))
            {
                condition = string.Format("where name like '%{0}%'", keyword);
                if (DataUtil.IsNum(keyword))
                {
                    condition += string.Format(" or ypid = {0}", keyword);
                }
            }

            //查询总数
            var sqlRowResult = SQLite.sqlone(string.Format("SELECT COUNT(1) total FROM tan8_music {0}", condition));
            if (string.IsNullOrEmpty(sqlRowResult)) return;
            var totalRow = Convert.ToInt32(sqlRowResult);
            //没有查到内容直接返回
            if (totalRow == 0) return;
            pager.TotalPage = (totalRow + pager.MaxRow - 1) / pager.MaxRow;
            //查询记录
            var dataSet = SQLite.SqlTable(string.Format("select ypid, name, star FROM tan8_music {0} order by star desc limit {1} OFFSET {1} * ({2} - 1)", condition, pager.MaxRow, pager.CurrentPage));

            //封装进对象
            var dataList = new List<PianoScoreViewModel>();
            foreach (DataRow dataRow in dataSet.Rows)
            {
                //拼接得到cover路径
                var imgDir = string.Format("{0}/{1}/{2}", AcgnuConfig.GetContext().pianoScorePath, dataRow["name"], DEFAULT_COVER_NAME);

                dataList.Add(new PianoScoreViewModel()
                {
                    id = Convert.ToInt32(dataRow["ypid"]),
                    Name = Convert.ToString(dataRow["name"]),
                    star = Convert.ToByte(dataRow["star"]),
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
            new Tan8DownloadRecordWindow().Show();
        }

        /// <summary>
        /// 以主窗口为父窗口打开一个dialog
        /// </summary>
        /// <param name="pianoScore"></param>
        /// <returns></returns>
        private bool? OpenEditDialog(PianoScore pianoScore)
        {
            var dialog = new AddSinglePianoScoreDialog(pianoScore)
            {
                Owner = mMainWindow,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
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
            FileUtil.RenameFolder(AcgnuConfig.GetContext().pianoScorePath + Path.DirectorySeparatorChar + dbName[0], pianoScore.Name);
            //修改数据库名称
            SQLite.ExecuteNonQuery(string.Format("UPDATE tan8_music SET name = '{0}' WHERE ypid = {1}", pianoScore.Name, pianoScore.id));
            //更新列表显示
            mPianoScoreList[PianoScoreListBox.SelectedIndex].NameAndView = pianoScore.Name;
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
            //检查曲谱是否存在于数据库
            var sqlRowResult = SQLite.sqlone(string.Format("SELECT COUNT(1) total FROM tan8_music where ypid = {0}", pianoScore.id.GetValueOrDefault()));
            if (!string.IsNullOrEmpty(sqlRowResult) && Convert.ToInt32(sqlRowResult) > 0)
            {
                //已存在则不下载
                return InvokeSuccess(pianoScore);
            }
            //校验任务任务运行状态
            if (bgworker.IsBusy)
            {
                mMainWindow.SetStatustProgess(new MainWindowStatusNotify()
                {
                    alertLevel = AlertLevel.ERROR,
                    message = "存在尚未结束的下载任务"
                });
            }
            mIsV2 = true;
            bgworker.RunWorkerAsync(pianoScore);
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
                mMainWindow.SetStatustProgess(new MainWindowStatusNotify()
                {
                    alertLevel = AlertLevel.ERROR,
                    message = "乐谱ID未填写"
                });
                return InvokeFail(pianoScore);
            }
            //打开播放器, 触发主动下载
            FlashPlayUtil.ExePlayById(pianoScore.id.GetValueOrDefault());
            return InvokeSuccess(pianoScore);
        }

        /// <summary>
        /// 下载弹8曲谱
        /// 开启后台下载曲谱任务
        /// </summary>
        /// <param name="pianoScore">曲谱对象</param>
        /// <returns>公共函数返回结果</returns>
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

        /// <summary>
        /// 准备下载任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        ///  下载弹琴吧琴谱后台任务
        /// </summary>
        /// <param name="pianoScore">乐谱信息</param>
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
            //默认直接使用v2版本的地址
            var url = pianoScore.SheetUrl;
            //如果没有v2版本地址
            if(string.IsNullOrEmpty(pianoScore.SheetUrl))
            {
                //使用v1版本, 从flash播放器读取
                var sample = mTan8Player.GetRealTan8URL(pianoScore.id.GetValueOrDefault());
                //xml字符串去除转义
                var urlFromSwf = DataUtil.GetXmlNodeValue(sample, "string").Replace("amp;", "");
                //由于播放器被本地化, 请求乐谱信息时会访问本地地址, 此处在线下载需要修改为线上地址
                url = urlFromSwf.Replace("localhost:7777/yuepu/info", "www.77music.com/flash_get_yp_info.php");
            }

            //step.2 请求乐谱地址, 得到乐谱信息
            bgworker.ReportProgress(0, CalcProgress(winProgress, "下载地址解析成功, 开始加载乐谱信息", 10));
            var proxyAddress = ProxyFactory.GetRandProxy();
            var ypinfostring = RequestUtil.CrawlContentFromWebsit(url, proxyAddress).data;
            //var ypinfostring = @"<html><body>yp_create_time=<yp_create_time>1573183398</yp_create_time><br/>yp_title=<yp_title>说好不哭（文武贝钢琴版）</yp_title><br/>yp_page_count=<yp_page_count>3</yp_page_count><br/>yp_page_width=<yp_page_width>1089</yp_page_width><br/>yp_page_height=<yp_page_height>1540</yp_page_height><br/>yp_is_dadiao=<yp_is_dadiao>1</yp_is_dadiao><br/>yp_key_note=<yp_key_note>10</yp_key_note><br/>yp_is_yanyin=<yp_is_yanyin>1</yp_is_yanyin><br/>ypad_url=<ypad_url>http://www.tan8.com//yuepuku/132/66138/66138_hegiahcc.ypad</ypad_url>ypad_url2=<ypad_url2>http://www.tan8.com//yuepuku/132/66138/66138_hegiahcc.ypa2</ypad_url2></body></html>";
            //校验返回的乐谱信息
            var checkResult = CheckYuepuInfo(ypinfostring);
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
            ProxyFactory.RemoveTemporary(proxyAddress, checkResult);
            //从乐谱信息解析到对象
            var tan8Music = DataUtil.ParseToModel(ypinfostring);

            //step.3 创建文件夹
            bgworker.ReportProgress(0, CalcProgress(winProgress, "乐谱加载成功, 创建归档文件夹", 20));
            var folder = string.IsNullOrEmpty(pianoScore.Name) ? tan8Music.yp_title : pianoScore.Name;
            //替换非法字符
            folder = FileUtil.ReplaceInvalidChar(folder);
            var folderPath = AcgnuConfig.GetContext().pianoScorePath + Path.DirectorySeparatorChar + folder;
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

            //step.5 下载播放文件
            //bgworker.ReportProgress(0, CalcProgress(winProgress, "下载播放文件", 80));
            //downResult = new FileDownloader().DownloadFile(tan8Music.ypad_url2, folderPath + "play.ypa2");
            //if (downResult != 0)
            //{
            //    return new InvokeResult<object>()
            //    {
            //        success = false,
            //        code = (byte)PianoScoreDownloadResult.PLAY_FILE_DOWNLOAD_FAIL,
            //        message = EnumLoader.GetEnumDesc(typeof(PianoScoreDownloadResult), PianoScoreDownloadResult.PLAY_FILE_DOWNLOAD_FAIL.ToString()),
            //        data = folder
            //    };
            //}
            //下载v2版播放文件 ( flash无法播放, 仅下载 )
            bgworker.ReportProgress(0, CalcProgress(winProgress, "下载播放文件", 80));
            //var ypdxAddr = tan8Music.ypad_url2.Substring(0, tan8Music.ypad_url2.Length - 2) + "dx";
            downResult = new FileDownloader().DownloadFile(tan8Music.ypad_url2, folderPath + "play.ypdx");
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

        /// <summary>
        /// 初步检查tan8返回的乐谱信息
        /// </summary>
        /// <param name="source">乐谱信息结果串</param>
        /// <returns></returns>
        private PianoScoreDownloadResult CheckYuepuInfo(string source)
        {
            //无返回
            if (string.IsNullOrEmpty(source)) return PianoScoreDownloadResult.PLAYER_NO_RESPONSE;
            //乐谱不存在
            if (source.Contains("该乐谱不存在!")) return PianoScoreDownloadResult.PIANO_SCORE_NOT_EXSITS;
            if (source.Contains("您访问太频繁啦")) return PianoScoreDownloadResult.TOO_MANY_VISIT;
            if (source.Contains("您访问的次数太多了")) return PianoScoreDownloadResult.VISTI_REACH_LIMIT;
            if (source.Contains("验证错误")) return PianoScoreDownloadResult.VALID_ERROR;
            //网络连接出错
            if (source.Equals(RequestUtil.CONNECTION_ERROR)) return PianoScoreDownloadResult.NETWORK_ERROR;
            //默认返回成功
            return PianoScoreDownloadResult.SUCCESS;
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
            var sql = string.Format("insert or ignore into tan8_music(ypid, `name`, star, yp_count, origin_data) VALUES ({0}, '{1}', 0, '{2}', '{3}')",
                ypid,
                name,
                tan8Music.yp_page_count,
                originstr);
            return SQLite.ExecuteNonQuery(sql);
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
            var sql = string.Format("INSERT INTO tan8_music_down_record(id, ypid, name, code, result, create_time, is_auto) VALUES((SELECT IFNULL(MAX(id),0)  + 1 FROM tan8_music_down_record), {0}, '{1}', {2}, '{3}', datetime('now', 'localtime'), {4})",
               ypid,
               Convert.ToString(invokeResult.data),
               invokeResult.code,
               invokeResult.message,
               isAuto);
            return SQLite.ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 下载进度发生变化时通知主窗口改变状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyProgress(object sender, ProgressChangedEventArgs e)
        {
            if(!mIsV2)
            {
                var windowStatusNotify = e.UserState as MainWindowStatusNotify;
                mMainWindow.SetStatustProgess(windowStatusNotify);
            } 
            else
            {
                FlashPlayUtil.ExitFlashPlayer();
            }
        }

        /// <summary>
        /// 下载完成事件, 刷新列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownLoadComplate(object sender, RunWorkerCompletedEventArgs e)
        {
            if(!mIsV2)
            {
                LoadPianoScore();
            }
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
                FileUtil.DeleteDir(AcgnuConfig.GetContext().pianoScorePath + Path.DirectorySeparatorChar + selected.Name);

                //删除数据库数据
                SQLite.ExecuteNonQuery(string.Format("DELETE FROM tan8_music WHERE ypid = {0}", selected.id));

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
            var fullPath = AcgnuConfig.GetContext().pianoScorePath + Path.DirectorySeparatorChar + selected.Name;
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
            bgworker.CancelAsync();
        }

        /// <summary>
        /// 默认播放按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDefaultPlayButtonClick(object sender, RoutedEventArgs e)
        {
            //播放器未打开, 则创建一个新的播放器
            if (null == mTan8Player)
            {
                mTan8Player = new Tan8PlayerWindow();
            }
            //播放所选曲谱
            mTan8Player.Show();
            mTan8Player.PlaySelected(new PianoScore() { id = 0 });
        }

        /// <summary>
        /// 由于Flash被禁用, 使用内置的flash播放器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDefaultPlayButtonClickV2(object sender, RoutedEventArgs e)
        {
            FlashPlayUtil.ExePlayById(0);
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
    }
}
