using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using UntisExp.Containers;

namespace vplan
{
    // ReSharper disable once RedundantExtendsListEntry
    // ReSharper disable once UnusedMember.Global
    public partial class NewsItemViewer : PhoneApplicationPage
    {
        readonly Settings _settings = new Settings();
        News _theNews;
        public NewsItemViewer()
        {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                _theNews = (News)_settings.Read("selectedNews");
                DataContext = _theNews;
            }
            catch {
                Uri uri = new Uri("/MainPage.xaml", UriKind.Relative);
                ((PhoneApplicationFrame) Application.Current.RootVisual).Navigate(uri);
            }
        }

        private void webBtn_Click(object sender, EventArgs e)
        {
            var wbt = new WebBrowserTask {Uri = _theNews.Source};
            wbt.Show();
        }
    }
}