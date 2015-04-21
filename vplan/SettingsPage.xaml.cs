using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using UntisExp;
using UntisExp.Containers;

namespace vplan
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once RedundantExtendsListEntry
    public partial class SettingsPage : PhoneApplicationPage
    {
        private ObservableCollection<Group> _groups = new ObservableCollection<Group>();
        private bool _loaded;
        private Settings _settings = new Settings();
        private readonly ProgressIndicator _pi;
        private Fetcher _fetcher;
        public SettingsPage()
        {
            _fetcher = new Fetcher();

            _fetcher.RaiseErrorMessage += (sender, e) =>
            {
                Alert(e.MessageHead, e.MessageBody);
            };
            _fetcher.RaiseRetreivedGroupItems += (sender, e) =>
            {
                Refresh(e.Groups);
            };
            InitializeComponent();
            DataContext = _groups;
#if LEHRER
            if (_settings.Read("lehrer") == null)
            {
                ShowPopup(false);
            }
            else
            {
                _fetcher.GetClasses();
            }
#endif
            _pi = new ProgressIndicator {IsVisible = true, IsIndeterminate = true, Text = "Vertretungen werden geladen"};
#if !LEHRER
            _fetcher.GetClasses();
            if (_settings.Read("group") == null)
            {
                _settings.Write("group", 0);
            }
            // Datenkontext des Listenfeldsteuerelements auf die Beispieldaten festlegen
#endif
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
#if LEHRER
            yourClass.Text = "Ihr Kürzel:";
            howTile.Text = "Wie wollen Sie Ihre Kachel?";
#endif
            try
            {
                NotSelect.SelectedIndex = (int)_settings.Read("mode");
            }
            catch
            {
                // ignored
            }
        }
#if LEHRER
        private void ShowPopup(bool wrongPw)
        {
                ContentPanel.Visibility = Visibility.Collapsed;
                PasswordBox pwBox = new PasswordBox();

                TiltEffect.SetIsTiltEnabled(pwBox, true);
                CustomMessageBox messageBox = new CustomMessageBox
                {
                    Caption = "Bitte Passwort eingeben",
                    Content = pwBox,
                    LeftButtonContent = "ok",
                    IsRightButtonEnabled = false,
                    IsFullScreen = false,
                    Message = wrongPw ? "Falsches Passwort!!" : VConfig.EnterPw
                };



                //Create a new custom message box
                

                //Define the dismissed event handler
                messageBox.Dismissed += (s1, e1) =>
                {
                    switch (e1.Result)
                    {
                        case CustomMessageBoxResult.LeftButton:
                            if (pwBox.Password == VConfig.Password)
                            {
                                _settings.Write("lehrer", 1);
                                if (_settings.Read("group") == null)
                                {
                                    _settings.Write("group", 0);
                                }
                                _fetcher = new Fetcher();
                                _fetcher.RaiseErrorMessage += (sender, e) =>
                                {
                                    Alert(e.MessageHead, e.MessageBody);
                                };
                                _fetcher.RaiseRetreivedGroupItems += (sender, e) =>
                                {
                                    Refresh(e.Groups);
                                };
                                _fetcher.GetClasses();
                                try
                                {
                                    ContentPanel.Visibility = Visibility.Visible;
                                }
                                catch (NullReferenceException) { }

                            }
                            else
                            {
                                ShowPopup(true);
                            }
                            break;
                        case CustomMessageBoxResult.None:
                            Application.Current.Terminate();
                            break;
                    }
                };

                //launch the task
                messageBox.Show();
        }
#endif

        private void Refresh(List<Group> grp)
        {
            _groups = new ObservableCollection<Group>(grp);
            Dispatcher.BeginInvoke(() =>
            {
                DataContext = _groups;
                _pi.IsVisible = false;
                _loaded = true;
                if (_settings.Read("group") != null)
                {
                    ClassSelect.SelectedIndex = (int)_settings.Read("group");
                }
            });
        }

        private void ListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loaded) {
                _settings.Write("group", ClassSelect.SelectedIndex);
            }
        }
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            Uri uri = new Uri("/MainPage.xaml", UriKind.Relative);
            ((PhoneApplicationFrame) Application.Current.RootVisual).Navigate(uri);
            base.OnBackKeyPress(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("/MainPage.xaml", UriKind.Relative);
            ((PhoneApplicationFrame) Application.Current.RootVisual).Navigate(uri);
        }

        private void notSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _settings.Write("mode", NotSelect.SelectedIndex);
            }
            catch
            {
                // ignored
            }
        }

        private void Alert(string t, string msg)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(msg, t, MessageBoxButton.OK);
            });
        }

    }
}
