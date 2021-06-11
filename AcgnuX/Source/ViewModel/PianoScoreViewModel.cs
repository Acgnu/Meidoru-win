using AcgnuX.Source.Model;

namespace AcgnuX.Source.ViewModel
{
    class PianoScoreViewModel : PianoScore
    {
        public string NameAndView
        {
            get 
            {
                var len = 45;
                return Name.Length > len ? Name.Substring(0, len) + "..." : Name; 
            }
            set
            {
                Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }
}
