using System.Windows;

namespace AcgnuX.Pages
{
    /// <summary>
    /// Index.xaml 的交互逻辑
    /// </summary>
    public partial class Index
    {
        public bool Bv { get; set; } = true;
        public Index()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void LoadData(object sender, RoutedEventArgs e)
        {
        }

        public void Button_Click_1(object sender, RoutedEventArgs e)
        {
        }
    }
}
