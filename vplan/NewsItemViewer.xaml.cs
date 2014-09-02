using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using UntisExp;

namespace vplan
{
    public partial class NewsItemViewer : PhoneApplicationPage
    {
        Settings settings = new Settings();
        News theNews;
        public NewsItemViewer()
        {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                theNews = (News)settings.read("selectedNews");
                DataContext = theNews;
            }
            catch {
                Uri uri = new Uri("/MainPage.xaml", UriKind.Relative);
                (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
            }
        }

        private void webBtn_Click(object sender, EventArgs e)
        {
            var wbt = new WebBrowserTask();
            wbt.Uri = theNews.Source;
            wbt.Show();
        }
    }
}