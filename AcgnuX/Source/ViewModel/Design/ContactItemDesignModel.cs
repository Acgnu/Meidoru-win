using AcgnuX.Pages;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using System;
using System.IO;
using System.Windows;
using System.Windows.Resources;

namespace AcgnuX.Source.ViewModel.Design
{
    public class ContactItemDesignModel : ContactItemViewModel
    {
        public ContactItemDesignModel() : base()
        {
            Id = 1;
            Name = "Acgnu";
            Platform = ContactPlatform.WE;
            Uid = "mock_we_no";
            Phone = "13333333333";
            //Avatar = new ByteArray(new byte[]);
        }
    }
}
