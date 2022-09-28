using AcgnuX.Pages;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Resources;

namespace AcgnuX.Source.DesignModel
{
    public class ContactItemDesignModel : ContactItemViewModel
    {
        /// <summary>
        /// A single instance of the design model
        /// </summary>
        public static ContactItemDesignModel Instance => new ContactItemDesignModel();

        public ContactItemDesignModel() : base(null)
        {
            var steamInfo = FileUtil.GetApplicationResourceAsStream(@"/Assets/Images/avatar_default.jpg");
            Stream s = steamInfo.Stream;
            var size = 512;
            var copyBuff = new byte[size];
            int len;
            var r = new MemoryStream();
            using (s)
            {
                while ((len = s.Read(copyBuff, 0, size)) > 0)
                    r.Write(copyBuff, 0, len);
                r.Seek(0, SeekOrigin.Begin);
            }
            Id = 1;
            Name = "Acgnu";
            Platform = ContactPlatform.WE;
            Uid = "Acgnu";
            Phone = "13333333333";
            Avatar = r.ToArray();
        }
    }
}
