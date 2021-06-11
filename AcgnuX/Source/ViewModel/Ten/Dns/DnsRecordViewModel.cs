using AcgnuX.Model.Ten.Dns;
using System.ComponentModel;

namespace AcgnuX.ViewModel.Ten.Dns
{
    class DnsRecordViewModel : DnsRecord, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void DoNotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
