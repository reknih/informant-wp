using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace vplan
{
    public partial class MainPage : PhoneApplicationPage
    {
        private ObservableCollection<Data> Vertr = new ObservableCollection<Data>();
        private Settings settings = new Settings();
        private Fetcher fetcher;
        private ProgressIndicator pi;
        // Konstruktor
        public MainPage()
        {
            InitializeComponent();
            pi = new ProgressIndicator();
            pi.IsVisible = true;
            pi.IsIndeterminate = true;
            pi.Text = "Vertretungen werden geladen";
            SystemTray.SetProgressIndicator(this, pi);
            fetcher = new Fetcher(this);
            if (settings.read("oldDb") != null)
            {
                Vertr = (ObservableCollection<Data>)settings.read("oldDb");
            }
            else {
                Vertr.Add(new Data());
            }
            fetcher = new Fetcher(this);

            try
            {
                fetcher.getTimes((int)settings.read("group") + 1, false);
                refreshBtn.Click -= refreshBtn_Click;
            }
            catch { }

            
            // Datenkontext des Listenfeldsteuerelements auf die Beispieldaten festlegen
            DataContext = Vertr;

        }
        // Daten für die ViewModel-Elemente laden
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
            if (settings.read("group") == null)
            {
                MessageBox.Show("Hallo und danke für den Download der App! Wir schicken dich jetzt zur Klassenauswahl, die App merkt sich danach diese Klasse. Wenn du einen Fehler findest schreib ihn uns doch bitte. Denn es ist alles noch ganz neu hier. Wir wünschen viel Ausfall!");
                Uri uri = new Uri("/SettingsPage.xaml", UriKind.Relative);
                (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
            }
        }
        public void refresh(ObservableCollection<Data> v1) {
            Vertr = v1;
            DataContext = Vertr;
            settings.write("oldDb", Vertr);
            try
            {
                pi.IsVisible = false;
                refreshBtn.Click += refreshBtn_Click;
            }
            catch { }
        }
        public void clear() {
            Vertr.Clear();
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
        public void add(Data d) {
            Vertr.Add(d);
            pi.IsVisible = false;
            settings.write("oldDb", Vertr);
        }

        private void refreshBtn_Click(object sender, EventArgs e)
        {
            try
            {
                pi.IsVisible = true;
                refreshBtn.Click -= refreshBtn_Click;
            }
            catch { }
            fetcher.getTimes((int)settings.read("group") + 1, false);
        }

        private void setGroup_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri("/SettingsPage.xaml", UriKind.Relative);
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
        }
    }
}