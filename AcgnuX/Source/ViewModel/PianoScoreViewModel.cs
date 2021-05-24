using AcgnuX.Source.Model;

namespace AcgnuX.Source.ViewModel
{
    class PianoScoreViewModel : PianoScore
    {
        public string IdView
        {
            get
            {
                return "📄 " + id;
            }
        }
        public string NameView
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

        public string YpCountView
        {
            get
            {
                return "📒 " + YpCount;
            }
        }

        public string VerView
        {
            get
            {
                return Ver == 1 ? "🎵" : "🎹";
            }
        }
    }
}
