using AcgnuX.Source.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AcgnuX.Source.DesignModel
{
    public class ContactListDesignModel : ContactListViewModel
    {
        #region Singleton

        /// <summary>
        /// A single instance of the design model
        /// </summary>
        public static ContactListDesignModel Instance => new ContactListDesignModel();

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ContactListDesignModel()
        {
            Items = new ObservableCollection<ContactItemViewModel>
            {
                new ContactItemViewModel (null)
                {
                    Id = 1,
                    Name = "LM",
                },
                new ContactItemViewModel (null)
                {
                    Id = 2,
                    Name = "Jesse",
                }
            };
        }
        #endregion
    }
}
