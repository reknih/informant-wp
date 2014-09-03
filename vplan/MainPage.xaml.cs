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
        private ObservableCollection<UntisExp.Data> Vertr = new ObservableCollection<UntisExp.Data>();
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

            if (settings.read("oldDb") != null)
            {
                Vertr = (ObservableCollection<UntisExp.Data>)settings.read("oldDb");
            }
            else {
                Vertr.Add(new UntisExp.Data());
            }
            
            // Datenkontext des Listenfeldsteuerelements auf die Beispieldaten festlegen
            DataContext = Vertr;

        }
        public void Clear() {
        }
        public void Alert(string t, string msg, string btn) {
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
            else {
                fetcher.getTimes((int)settings.read("group") + 1, Activity.ParseFirstSchedule);
            }
            pi.Text = "Vertretungen werden aktualisiert";
            pi.IsVisible = true;
            SystemTray.SetProgressIndicator(this, pi);

        }
        public void refresh(List<UntisExp.Data> v1)
        {

            Vertr = new ObservableCollection<UntisExp.Data>(v1);
            Dispatcher.BeginInvoke(() =>
            {
                DataContext = Vertr;
                settings.write("oldDb", Vertr);
                if (v1.Count == 0)
                {
                    var oc = new ObservableCollection<UntisExp.Data>();
                    oc.Add(new UntisExp.Data());
                    DataContext = oc;
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
            Dispatcher.BeginInvoke(() => {
                Vertr.Add(d);
                pi.IsVisible = false;
                settings.write("oldDb", Vertr);
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
        protected void reachToPress() {
            press = new Press();
            try
            {
                new Fetcher(addNewsEntry, (int)settings.read("group") + 1);
            }
            catch {
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
        protected async Task PutDelay() {
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
    }
}