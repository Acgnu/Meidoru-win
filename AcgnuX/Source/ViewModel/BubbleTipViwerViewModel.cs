using AcgnuX.Source.Bussiness.Constants;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 气泡提示展示器ViewModel
    /// </summary>
    public class BubbleTipViwerViewModel : ViewModelBase
    {
        public ObservableCollection<BubbleTipViewModel> Items { get; set; } = new ObservableCollection<BubbleTipViewModel>();

        public BubbleTipViwerViewModel()
        {
            Messenger.Default.Register<BubbleTipViewModel>(this, OnTipMessage);
        }

        private void UnRegisteMessage()
        {
            //Messenger.Default.Unregister<BubbleTipViewModel>(this, OnTipMessage);
        }

        public async void OnTipMessage(BubbleTipViewModel content)
        {
            Items.Add(content);
            content.IsShow = true;
            await Task.Delay(2000);
            content.IsShow = false;
            content.AnimationEndAction = () => Items.Remove(content);
        }
    }
}
