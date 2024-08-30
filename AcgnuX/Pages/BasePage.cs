using AcgnuX.Source.Model;
using SharedLib.Model;
using System.Windows.Controls;

namespace AcgnuX.Pages
{
    /// <summary>
    /// Page基类
    /// </summary>
    public class BasePage : Page
    {
        //主窗口
        protected MainWindow mMainWindow = App.Current.MainWindow as MainWindow;

        /// <summary>
        /// 返回通用的成功结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        protected InvokeResult<T> InvokeSuccess<T>(T data)
        {
            return new InvokeResult<T>()
            {
                success = true,
                code = 0,
                message = "success",
                data = data
            };
        }

        /// <summary>
        /// 返回通用的处理失败结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        protected InvokeResult<T> InvokeFail<T>(T data)
        {
            return new InvokeResult<T>()
            {
                success = false,
                code = 10,
                message = "fail",
                data = data
            };
        }
    }
}
