using AcgnuX.Controls;
using AcgnuX.Source.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AcgnuX.Source.ViewModel.Design
{
    public class ContactListDesignModel : ContactListViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ContactListDesignModel()
        {
            Items = new RangeObservableCollection<ContactItemViewModel>
            {
                new ContactItemViewModel (null)
                {
                    Id = 1,
                    Name = "Friend01",
                    Platform = Bussiness.Constants.ContactPlatform.WE,
                    Phone = "13333333333"
                },
                new ContactItemViewModel (null)
                {
                    Id = 2,      
                    Name = "Friend02",
                    Platform = Bussiness.Constants.ContactPlatform.QQ,
                    Phone = "15555555555"
                },
                new ContactItemViewModel (null)
                {
                    Id = 3,      
                    Name = "Friend03",
                    Platform = Bussiness.Constants.ContactPlatform.QQ,
                    Phone = "16666666666"
                }
            };
        }
    }
}
