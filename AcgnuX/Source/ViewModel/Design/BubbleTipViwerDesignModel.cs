using AcgnuX.Source.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AcgnuX.Source.ViewModel.Design
{
    public class BubbleTipViwerDesignModel : BubbleTipViwerViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BubbleTipViwerDesignModel()
        {
            Items = new ObservableCollection<BubbleTipViewModel>
            {
                new BubbleTipViewModel
                {
                    AlertLevel = Bussiness.Constants.AlertLevel.ERROR,
                    Text = "Error Text Test"
                },
                new BubbleTipViewModel
                {
                    AlertLevel = Bussiness.Constants.AlertLevel.RUN,
                    Text = "Running Text Test"
                },
                new BubbleTipViewModel
                {
                    AlertLevel = Bussiness.Constants.AlertLevel.WARN,
                    Text = "Warning Text Test"
                }
            };
        }
    }
}
