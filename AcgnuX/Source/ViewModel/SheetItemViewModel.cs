﻿using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
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
        //进度级别 (仅存在此字段避免警告)
        public AlertLevel ProgressAlertLevel { get; set; } = AlertLevel.RUN;
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

        //导出的Worker
        private readonly BackgroundWorker mExportBgWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        private readonly Tan8SheetsRepo _Tan8SheetsRepo = Tan8SheetsRepo.Instance;

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
