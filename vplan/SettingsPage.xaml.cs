using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
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
        private readonly Settings _settings = new Settings();
        private readonly ProgressIndicator _pi;
        public SettingsPage()
        {
            InitializeComponent();
#if LEHRER
            if (_settings.Read("group") == null)
            {
                showPopup(false);
            }
#endif
            _pi = new ProgressIndicator {IsVisible = true, IsIndeterminate = true, Text = "Vertretungen werden geladen"};
            var fetcher = new Fetcher();
            fetcher.RaiseErrorMessage += (sender, e) =>
            {
                Alert(e.MessageHead, e.MessageBody);
            };
            fetcher.RaiseRetreivedGroupItems += (sender, e) =>
            {
                Refresh(e.Groups);
            };
            fetcher.GetClasses();
            DataContext = _groups;
#if !LEHRER
            if (_settings.Read("group") == null)
            {
                _settings.Write("group", 0);
            }
            // Datenkontext des Listenfeldsteuerelements auf die Beispieldaten festlegen
#endif
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
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
        private void showPopup(bool wrongPw)
        {
                ContentPanel.Visibility = Visibility.Collapsed;
                //LayoutRoot.Background=Application.Current.Resources.
                PasswordBox pwBox = new PasswordBox();

                TiltEffect.SetIsTiltEnabled(pwBox, true);
                CustomMessageBox messageBox = new CustomMessageBox()
                {
                    Caption = "Bitte Passwort eingeben",
                    Content = pwBox,
                    LeftButtonContent = "ok",
                    IsRightButtonEnabled = false,
                    IsFullScreen = false
                };
                if (wrongPw)
                {
                    messageBox.Message = "Falsches Passwort!!";
                }
                else{
                    messageBox.Message = UntisExp.VConfig.enterPW;
                }

                

                //Create a new custom message box
                

                //Define the dismissed event handler
                messageBox.Dismissed += (s1, e1) =>
                {
                    switch (e1.Result)
                    {
                        case CustomMessageBoxResult.LeftButton:
                            if (pwBox.Password == UntisExp.VConfig.password)
                            {
                                _settings.Write("group", 0);
                                ContentPanel.Visibility = Visibility.Visible;
                            }
                            else { showPopup(true); return; }
                            break;
                        case CustomMessageBoxResult.None:
                            Application.Current.Terminate();
                            break;
                        default:
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
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
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
