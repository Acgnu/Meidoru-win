using AcgnuX.Controls;

namespace AcgnuX.Source.ViewModel.Design
{
    public class ContactListDesignModel
    {
        public RangeObservableCollection<ContactItemViewModel> ContactItems { get; }
        /// <summary>
        /// Default constructor
        /// </summary>
        public ContactListDesignModel()
        {
            ContactItems = new RangeObservableCollection<ContactItemViewModel>
            {
                new ContactItemViewModel ()
                {
                    Id = 1,
                    Name = "Friend01",
                    Platform = Bussiness.Constants.ContactPlatform.WE,
                    Phone = "13333333333"
                },
                new ContactItemViewModel ()
                {
                    Id = 2,
                    Name = "Friend02",
                    Platform = Bussiness.Constants.ContactPlatform.QQ,
                    Phone = "15555555555"
                },
                new ContactItemViewModel ()
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
