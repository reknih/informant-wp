using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using UntisExp;
using UntisExp.Containers;

namespace vplan
{
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainPage : PhoneApplicationPage
    {
        private ObservableCollection<Data> _vertr1 = new ObservableCollection<Data>();
        private ObservableCollection<Data> _vertr2 = new ObservableCollection<Data>();
        private readonly Settings _settings = new Settings();
        private readonly Fetcher _fetcher;
        private readonly ProgressIndicator _pi;
        private Press _press;
        private ObservableCollection<News> _news;
        public static bool ShowBgDisabBox;
        // Konstruktor
        public MainPage()
        {
            InitializeComponent();
            String title ="CWS Informant";
#if LEHRER
            title += " für Lehrer";
#endif
#if DEBUG
            title += " BETA";
#endif
            Pano.Title = title;
            _pi = new ProgressIndicator
            {
                IsVisible = true,
                IsIndeterminate = true,
                Text = "Vertretungen werden aktualisiert"
            };
            SystemTray.SetProgressIndicator(this, _pi);
            SystemTray.SetBackgroundColor(this, Color.FromArgb(225, 0, 31, 63));
            SystemTray.SetForegroundColor(this, Color.FromArgb(225, 221, 221, 221));
            _fetcher = new Fetcher();
            _fetcher.RaiseErrorMessage += (sender, e) =>
            {
                Alert(e.MessageHead, e.MessageBody);
            };
            _fetcher.RaiseRetreivedScheduleItems += (sender, e) =>
            {
                Refresh(e.Schedule);
            };

            if (_settings.Read("oldDb1") != null)
            {
                _vertr1 = (ObservableCollection<Data>)_settings.Read("oldDb1");
            }
            else
            {
                _vertr1.Add(new Data());
            }
            if (_settings.Read("oldDb2") != null)
            {
                _vertr2 = (ObservableCollection<Data>)_settings.Read("oldDb2");
            }
            if (_settings.Read("oldDb1Tit") != null)
            {
                Agenda1.Header = (String)_settings.Read("oldDb1Tit");
            }
            if (_settings.Read("oldDb2Tit") != null)
            {
                Agenda2.Header = (String)_settings.Read("oldDb2Tit");
            }
            SetPanoItems();

        }

        private void Alert(string t, string msg)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (t == VConfig.NoPageErrTtl)
                {
                    _pi.IsVisible = false;
                   // _pi.Text = "Nachrichten werden abgefragt";
                    //ReachToPress();
                }
                MessageBox.Show(msg, t, MessageBoxButton.OK);
            });
        }
        // Daten für die ViewModel-Elemente laden
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

#if LEHRER
            srText.Text = "Hier könnte Ihre Werbung stehen!";
            srSign.Text = "Ihr SR.";

#endif
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
            if (ShowBgDisabBox && !(_settings.Read("BGAgentDisabled") != null && (bool)_settings.Read("BGAgentDisabled")))
            {
                MessageBox.Show("Der Hintergrundtask für diese App wurde vom Benutzer deaktiviert!");
                ShowBgDisabBox = false;
                _settings.Write("BGAgentDisabled", true);
            }
            else if (!ShowBgDisabBox && (_settings.Read("BGAgentDisabled") != null && (bool)_settings.Read("BGAgentDisabled")))
            {
                _settings.Write("BGAgentDisabled", false);
            }
#if LEHRER
            if (_settings.Read("group") == null||_settings.Read("lehrer") == null)
            {
#else
            if (_settings.Read("group") == null)
            {
#endif                
                MessageBox.Show(VConfig.WelcomeText);

                Uri uri = new Uri("/SettingsPage.xaml", UriKind.Relative);
                ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(uri);
            }
            else
            {
                _fetcher.GetTimes((int)_settings.Read("group") + 1);
            }
            _pi.Text = "Vertretungen werden aktualisiert";
            _pi.IsVisible = true;
            SystemTray.SetProgressIndicator(this, _pi);

        }

        private void Refresh(List<Data> v1)
        {

            Dispatcher.BeginInvoke(() =>
            { 
                SplitUpList(v1);
                if (v1.Count == 0)
                {
                    var oc = new ObservableCollection<Data> {new Data()};
                    Agenda1Panel.DataContext = oc;
                }
                try
                {
                    _pi.Text = "Nachrichten werden abgefragt";
                    RefreshBtn.Click += refreshBtn_Click;
                }
                catch
                {
                    // ignored
                }
                ReachToPress();
            });
        }
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                while (NavigationService.RemoveBackEntry() != null)
                {
                    NavigationService.RemoveBackEntry();
                }
            }
            base.OnBackKeyPress(e);
        }

        private void refreshBtn_Click(object sender, EventArgs e)
        {
            try
            {
                _pi.IsVisible = true;
                RefreshBtn.Click -= refreshBtn_Click;
            }
            catch
            {
                // ignored
            }
            _fetcher.GetTimes((int)_settings.Read("group") + 1);
        }

        private void setGroup_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri("/SettingsPage.xaml", UriKind.Relative);
            ((PhoneApplicationFrame) Application.Current.RootVisual).Navigate(uri);
        }

        private void ReachToPress()
        {
            _press = new Press();
            _fetcher.RaiseRetreivedNewsItem += (sender, e) =>
            {
                AddNewsEntry(e.News);
            };
            try
            {
                _fetcher.GetTimes((int)_settings.Read("group") + 1, Activity.GetNews);
            }
            catch
            {
                _fetcher.GetTimes(5, Activity.GetNews);
            }
            _news = new ObservableCollection<News>();
            _press.RaiseRetreivedNewsItems += (sender, e) =>
            {
                AddNewsEntrys(e.News);
            };
            _press.FireEventForNews();
            Newspanel.DataContext = _news;
        }

        private void AddNewsEntry(News n)
        {
            Dispatcher.BeginInvoke(delegate
            {
                if (!_news.Contains(n))
                {
                    _news.Insert(0, n);
                    Newspanel.DataContext = _news;
                    Newspanel.ScrollTo(_news[0]);
                }
            });
        }

        private void AddNewsEntrys(List<News> n)
        {
            Dispatcher.BeginInvoke(delegate
            {
                foreach (var item in n)
                {
                    _news.Add(item);
                }
                Newspanel.DataContext = _news;
                _pi.Text = Helpers.GetRandomArrayItem(VConfig.SuccessJokes);
#pragma warning disable 4014
                PutDelay();
#pragma warning restore 4014
            });
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private async Task PutDelay()
        {
            try
            {
                await Task.Delay(1500);
            }
            catch
            {
                // ignored
            }
            Dispatcher.BeginInvoke(() =>
            {
                _pi.IsVisible = false;
            });
        }

        private void newspanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ((LongListSelector)sender).SelectedItem as News;
            _settings.Write("selectedNews", item);
            Uri uri = new Uri("/NewsItemViewer.xaml", UriKind.Relative);
            ((PhoneApplicationFrame) Application.Current.RootVisual).Navigate(uri);
        }
        private void SplitUpList(List<Data> vRaw)
        {
            Agenda1.Header = "Agenda";
            Agenda2.Header = "Agenda";
            int day = 0;
            List<Data> v1 = new List<Data>();
            List<Data> v2 = new List<Data>();
            foreach (Data elem in vRaw)
            {
                if (elem.DateHeader)
                {
                    if (day == 0)
                    {
                        Agenda1.Header = elem.Line1;
                    }
                    else if (day == 1)
                    {
                        Agenda2.Header = elem.Line1;
                    }
                    else if(day>=2)
                    {
                        v2.Add(elem);
                    }
                    day++;
                }
                else if (day == 1)
                {
                    v1.Add(elem);
                }
                else if (day >= 2)
                {
                    v2.Add(elem);
                }
            }
            _vertr1 = new ObservableCollection<Data>(v1);
            _vertr2 = new ObservableCollection<Data>(v2);
            SetPanoItems();

        }
        private void SetPanoItems()
        {
            if (_vertr2.Count == 0)
            {
                Agenda2.Visibility = Visibility.Collapsed;
            }
            else
            {
                Agenda2.Visibility = Visibility.Visible;
            }
            Agenda1Panel.DataContext = _vertr1;
            Agenda2Panel.DataContext = _vertr2;
            _settings.Write("oldDb1", _vertr1);
            _settings.Write("oldDb2", _vertr2);
            if ((String)Agenda1.Header != "Agenda")
            {
                _settings.Write("oldDb1Tit", Agenda1.Header);

                if ((String)Agenda2.Header != "Agenda")
                {
                    _settings.Write("oldDb2Tit", Agenda2.Header);
                }
            }
        }

        private void agenda2_loaded(object sender, RoutedEventArgs e)
        {

        }

        private void agenda2Titel_loaded(object sender, RoutedEventArgs e)
        {

        }

        private void agenda1Panel_loaded(object sender, RoutedEventArgs e)
        {

        }

        private void agenda1Titel_loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}