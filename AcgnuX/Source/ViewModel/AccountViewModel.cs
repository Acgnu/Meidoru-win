using AcgnuX.Source.Model;
using System.ComponentModel;

namespace AcgnuX.ViewModel
{
    class AccountViewModel : Account, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void DoNotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
