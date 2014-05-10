using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using UntisExp;

namespace vplan
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        private ObservableCollection<Group> Groups = new ObservableCollection<Group>();
        private bool loaded;
        private Settings settings = new Settings();
        private ProgressIndicator pi;
        public SettingsPage()
        {
            InitializeComponent();
            pi = new ProgressIndicator();
            pi.IsVisible = true;
            pi.IsIndeterminate = true;
            pi.Text = "Vertretungen werden geladen";
            Fetcher fetcher;
            fetcher = new Fetcher(Alert, refresh);
            fetcher.getClasses();
            DataContext = Groups;
            // Datenkontext des Listenfeldsteuerelements auf die Beispieldaten festlegen

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                notSelect.SelectedIndex = (int)settings.read("mode");
            }
            catch { }
        }
        public void refresh(List<Group> grp)
        {
            Groups = new ObservableCollection<Group>(grp);
            Dispatcher.BeginInvoke(() =>
            {
                DataContext = Groups;
                pi.IsVisible = false;
                loaded = true;
                if (settings.read("group") != null)
                {
                    classSelect.SelectedIndex = (int)settings.read("group");
                }
            });
        }

        private void ListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loaded) {
                settings.write("group", classSelect.SelectedIndex);
            }
        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            Uri uri = new Uri("/MainPage.xaml", UriKind.Relative);
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
            base.OnBackKeyPress(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("/MainPage.xaml", UriKind.Relative);
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(uri);
        }

        private void notSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                settings.write("mode", notSelect.SelectedIndex);
            }
            catch { }
        }
        public void Alert(string t, string msg, string btn)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(msg, t, MessageBoxButton.OK);
            });
        }
    }
}