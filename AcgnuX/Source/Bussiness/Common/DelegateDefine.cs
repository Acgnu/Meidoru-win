using AcgnuX.Source.Model;
using AcgnuX.Source.Model.Ten.Dns;
using AcgnuX.Source.ViewModel;
using System.Collections.ObjectModel;
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
    /// 停止所有的乐谱下载任务
    /// </summary>
    /// <param name="ListData"></param>
    public delegate void StopAllTan8SheetDownload(ObservableCollection<SheetItemViewModel> ListData);

    /// <summary>
    /// 状态栏变更委托
    /// </summary>
    /// <param name="statusNotify"></param>
    public delegate void StatusBarNotifyHandler(MainWindowStatusNotify statusNotify);

    /// <summary>
    /// IP池数量变化事件
    /// </summary>
    /// <param name="curNum"></param>
    public delegate void ProxyPoolCountChangeHandler(int curNum);

    /// <summary>
    /// 指定了数据库文件事件
    /// </summary>
    public delegate void OnDatabaseFileSetHandler();
}
