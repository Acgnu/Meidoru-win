using AcgnuX.Source.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AcgnuX.Source.ViewModel.Design
{
    public class BubbleTipDesignModel : BubbleTipViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BubbleTipDesignModel()
        {
            AlertLevel = Bussiness.Constants.AlertLevel.RUN;
            Text = "Test Text In Design Model";
            IsShow = true;
        }
    }
}
