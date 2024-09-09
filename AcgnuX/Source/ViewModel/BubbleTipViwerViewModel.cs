using AcgnuX.Source.Bussiness.Constants;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Messaging;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 气泡提示展示器ViewModel
    /// </summary>
    public class BubbleTipViwerViewModel : ObservableObject
    {
        public ObservableCollection<BubbleTipViewModel> Items { get; set; } = new ObservableCollection<BubbleTipViewModel>();

        public BubbleTipViwerViewModel()
        {
            WeakReferenceMessenger.Default.Register<BubbleTipViewModel>(this, OnTipMessage);
        }

        private void UnRegisteMessage()
        {
            //Messenger.Unregister<BubbleTipViewModel>(this, OnTipMessage);
        }

        public async void OnTipMessage(object receiver, BubbleTipViewModel content)
        {
            Items.Add(content);
            content.IsShow = true;
            await Task.Delay(2000);
            content.IsShow = false;
            content.AnimationEndAction = () => Items.Remove(content);
        }
    }
}
