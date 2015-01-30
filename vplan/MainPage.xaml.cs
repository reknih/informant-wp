using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using UntisExp;


namespace vplan
{
    public partial class MainPage : PhoneApplicationPage
    {
        private ObservableCollection<UntisExp.Data> Vertr1 = new ObservableCollection<UntisExp.Data>();
        private ObservableCollection<UntisExp.Data> Vertr2 = new ObservableCollection<UntisExp.Data>();
        private Settings settings = new Settings();
        private Fetcher fetcher;
        private ProgressIndicator pi;
        private Press press;
        private ObservableCollection<News> news;
        public static bool showBGDisabBox = false;
        // Konstruktor
        public MainPage()
        {
            InitializeComponent();
            pi = new ProgressIndicator();
            pi.IsVisible = true;
            pi.IsIndeterminate = true;
            pi.Text = "Vertretungen werden aktualisiert";
            SystemTray.SetProgressIndicator(this, pi);
            SystemTray.SetBackgroundColor(this, Color.FromArgb(225, 0, 31, 63));
            SystemTray.SetForegroundColor(this, Color.FromArgb(225, 221, 221, 221));
            fetcher = new Fetcher(Clear, Alert, refresh, add);

            if (settings.read("oldDb1") != null)
            {
                Vertr1 = (ObservableCollection<UntisExp.Data>)settings.read("oldDb1");
            }
            else
            {
                Vertr1.Add(new UntisExp.Data());
            }
            if (settings.read("oldDb2") != null)
            {
                Vertr2 = (ObservableCollection<UntisExp.Data>)settings.read("oldDb2");
            }
            if (settings.read("oldDb1Tit") != null)
            {
                Agenda1.Header = (String)settings.read("oldDb1Tit");
            }
            if (settings.read("oldDb2Tit") != null)
            {
                Agenda2.Header = (String)settings.read("oldDb2Tit");
            }
            setPanoItems();

        }
        public void Clear()
        {
        }
        public void Alert(string t, string msg, string btn)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (t == VConfig.noPageErrTtl)
                {
                    pi.Text = "Nachrichten werden abgefragt";
                    reachToPress();
                }
                MessageBox.Show(msg, t, MessageBoxButton.OK);
            });
        }
        // Daten für die ViewModel-Elemente laden
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //fetcher.getTimes((int)settings.read("group") + 1, Activity.ParseFirstSchedule);

            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
            if (showBGDisabBox && !(settings.read("BGAgentDisabled") == null ? false : (bool)settings.read("BGAgentDisabled")))
            {
                MessageBox.Show("Der Hintergrundtask für diese App wurde vom Benutzer deaktiviert!");
                showBGDisabBox = false;
                settings.write("BGAgentDisabled", true);
            }
            else if (!showBGDisabBox && (settings.read("BGAgentDisabled") == null ? false : (bool)settings.read("BGAgentDisabled")))
            {
                settings.write("BGAgentDisabled", false);
            }
            if (settings.read("group") == null)
            {
                MessageBox.Show("Hallo und danke für den Download der App! Wir schicken dich jetzt zur Klassenauswahl, die App merkt sich danach diese Klasse. Wenn du einen Fehler findest schreib ihn uns doch bitte. Denn es ist alles noch ganz neu hier. Wir wünschen viel Ausfall!");
                Uri uri = new Uri("/SettingsPage.xaml", UriKind.Relative);
                (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
            }
            else
            {
                fetcher.getTimes((int)settings.read("group") + 1, Activity.ParseFirstSchedule);
            }
            pi.Text = "Vertretungen werden aktualisiert";
            pi.IsVisible = true;
            SystemTray.SetProgressIndicator(this, pi);

        }
        public void refresh(List<UntisExp.Data> v1)
        {
            

            Dispatcher.BeginInvoke(() =>
            {
                splitUpList(v1);
                if (v1.Count == 0)
                {
                    var oc = new ObservableCollection<UntisExp.Data>();
                    oc.Add(new UntisExp.Data());
                    Agenda1Panel.DataContext = oc;
                }
                try
                {
                    pi.Text = "Nachrichten werden abgefragt";
                    refreshBtn.Click += refreshBtn_Click;
                }
                catch { }
                reachToPress();
            });
        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
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
        public void add(UntisExp.Data d)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (d.DateHeader)
                {
                    if ((String)Agenda1.Header == "Agenda")
                    {
                        Agenda1.Header = d.Line1;
                    }
                    else if ((String)Agenda2.Header == "Agenda")
                    {
                        Agenda2.Header = d.Line1;
                    }
                }
                else if ((String)Agenda2.Header != "Agenda")
                {
                    Vertr2.Add(d);
                }
                else
                {
                    Vertr1.Add(d);
                }
                setPanoItems();
                pi.IsVisible = false;
            });
        }

        private void refreshBtn_Click(object sender, EventArgs e)
        {
            try
            {
                pi.IsVisible = true;
                refreshBtn.Click -= refreshBtn_Click;
            }
            catch { }
            fetcher.getTimes((int)settings.read("group") + 1, Activity.ParseFirstSchedule);
        }

        private void setGroup_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri("/SettingsPage.xaml", UriKind.Relative);
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
        }
        protected void reachToPress()
        {
            press = new Press();
            try
            {
                new Fetcher(addNewsEntry, (int)settings.read("group") + 1);
            }
            catch
            {
                new Fetcher(addNewsEntry, 5);
            }
            news = new ObservableCollection<News>();
            press.getCalledBackForNews(addNewsEntrys);
            newspanel.DataContext = news;
        }
        protected void addNewsEntry(News n)
        {
            Dispatcher.BeginInvoke(delegate
            {
                if (!news.Contains(n))
                {
                    news.Insert(0, n);
                    newspanel.DataContext = news;
                    newspanel.ScrollTo(news[0]);
                }
            });
        }
        protected void addNewsEntrys(List<News> n)
        {
            Dispatcher.BeginInvoke(delegate
            {
                foreach (var item in n)
                {
                    news.Add(item);
                }
                newspanel.DataContext = news;
                pi.Text = Helpers.getRandomArrayItem(VConfig.successJokes);
                PutDelay();
            });
        }
        protected async Task PutDelay()
        {
            try
            {
                await Task.Delay(1500);
            }
            catch { }
            Dispatcher.BeginInvoke(() =>
            {
                pi.IsVisible = false;
            });
        }

        private void newspanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ((LongListSelector)sender).SelectedItem as News;
            settings.write("selectedNews", item);
            Uri uri = new Uri("/NewsItemViewer.xaml", UriKind.Relative);
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
        }
        private void splitUpList(List<UntisExp.Data> vRaw)
        {
            Agenda1.Header = "Agenda";
            Agenda2.Header = "Agenda";
            int day = 0;
            List<UntisExp.Data> v1 = new List<UntisExp.Data>();
            List<UntisExp.Data> v2 = new List<UntisExp.Data>();
            foreach (UntisExp.Data elem in vRaw)
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
                    day++;
                }
                else if (day == 1)
                {
                    v1.Add(elem);
                }
                else if (day == 2)
                {
                    v2.Add(elem);
                }
            }
            Vertr1 = new ObservableCollection<UntisExp.Data>(v1);
            Vertr2 = new ObservableCollection<UntisExp.Data>(v2);
            setPanoItems();

        }
        private void setPanoItems()
        {
            if (Vertr2.Count == 0)
            {
                Agenda2.Visibility = Visibility.Collapsed;
            }
            else
            {
                Agenda2.Visibility = Visibility.Visible;
            }
            Agenda1Panel.DataContext = Vertr1;
            Agenda2Panel.DataContext = Vertr2;
            settings.write("oldDb1", Vertr1);
            settings.write("oldDb2", Vertr2);
            if ((String)Agenda1.Header != "Agenda")
            {
                settings.write("oldDb1Tit", Agenda1.Header);

                if ((String)Agenda2.Header != "Agenda")
                {
                    settings.write("oldDb2Tit", Agenda2.Header);
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