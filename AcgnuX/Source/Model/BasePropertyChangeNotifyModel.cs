using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AcgnuX.Source.Model
{
    /// <summary>
    /// 视图模型基类
    /// </summary>
    public class BasePropertyChangeNotifyModel : INotifyPropertyChanged
    {
        //属性变更事件
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 通知属性变更
        /// </summary>
        /// <param name="name"></param>
        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
