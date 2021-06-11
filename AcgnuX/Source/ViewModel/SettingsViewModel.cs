using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 设置的视图模型
    /// </summary>
    class SettingsViewModel : AcgnuConfigContext
    {
        public string AccountJsonPathAndView
        {
            get { return accountJsonPath; }
            set
            {
                accountJsonPath = value;
                OnPropertyChanged(nameof(accountJsonPath));
            }
        }

        public string PianoScorePathAndView
        {
            get { return pianoScorePath; }
            set
            {
                pianoScorePath = value;
                OnPropertyChanged(nameof(pianoScorePath));
            }
        }
    }
}
