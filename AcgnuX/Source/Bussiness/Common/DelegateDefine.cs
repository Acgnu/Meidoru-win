using AcgnuX.Source.Model;
using AcgnuX.Source.Model.Ten.Dns;
/// <summary>
/// 定义委托
/// </summary>
namespace AcgnuX.Source.Bussiness.Common
{
    /// <summary>
    /// 对话框点击确定事件
    /// </summary>
    /// <typeparam name="T">数据对象</typeparam>
    /// <param name="data">数据对象</param>
    /// <returns>数据对象</returns>
    public delegate InvokeResult<T> EditConfirmHandler<T>(T data);

    /// <summary>
    /// 状态栏停止按钮点击事件
    /// </summary>
    public delegate void StopBtnClickHandler();

    /// <summary>
    /// 状态栏变更委托
    /// </summary>
    /// <param name="statusNotify"></param>
    public delegate void StatusBarNotifyHandler(MainWindowStatusNotify statusNotify);

    /// <summary>
    /// DNS编辑完成
    /// </summary>
    /// <param name="result"></param>
    public delegate void BackgroundFinishHandler(DnsOperatorResult result);

    /// <summary>
    /// IP池数量变化事件
    /// </summary>
    /// <param name="curNum"></param>
    public delegate void ProxyPoolCountChangeHandler(int curNum);

    /// <summary>
    /// 弹8曲谱下载成功事件
    /// </summary>
    /// <param name="pianoScore"></param>
    public delegate void Tan8SheetDownloadFinishHandler(PianoScore pianoScore);
}
