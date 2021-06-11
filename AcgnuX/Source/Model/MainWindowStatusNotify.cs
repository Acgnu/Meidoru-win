using AcgnuX.Source.Bussiness.Constants;

namespace AcgnuX.Source.Model
{
    /// <summary>
    /// 主窗口状态栏信息显示
    /// </summary>
    public class MainWindowStatusNotify
    {
        //提示级别
        public AlertLevel alertLevel { get; set; } = AlertLevel.INFO;
        //提示内容
        public string message { get; set; } = "就绪";
        //标识是否对进度条做动画
        public bool animateProgress { get; set; } = false;
        //上个进度
        public double oldProgress { get; set; } = 0;
        //当前进度
        public double nowProgress { get; set; } = 100;
        //进度执行时间 (ms)
        public int progressDuration { get; set; } = 0;
    }
}
