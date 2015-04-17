using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;
using UntisExp;
using UntisExp.Containers;

namespace DataUpdate
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private readonly IsolatedStorageSettings _settingsFile = IsolatedStorageSettings.ApplicationSettings;
        private Fetcher _fetcher;
        private string _dayStr = " ";
        private string Kachel1;
        protected string Kachel2;
        protected string Kachel3;
        protected int Vert;
        protected int Mitb;
        protected int Entf;
        protected int Raum;
        protected int Vera;
        /// <remarks>
        /// ScheduledAgent-Konstruktor, initialisiert den UnhandledException-Handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Handler für verwaltete Ausnahmen abonnieren
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        public ScheduledAgent(string kachel1)
        {
            Kachel1 = kachel1;
        }

        /// Code, der bei nicht behandelten Ausnahmen ausgeführt wird
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // Eine nicht behandelte Ausnahme ist aufgetreten. Unterbrechen und Debugger öffnen
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent zum Ausführen einer geplanten Aufgabe
        /// </summary>
        /// <param name="task">
        /// Die aufgerufene Aufgabe
        /// </param>
        /// <remarks>
        /// Diese Methode wird aufgerufen, wenn eine regelmäßige oder ressourcenintensive Aufgabe aufgerufen wird
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            //TODO: Code zum Ausführen der Aufgabe im Hintergrund hinzufügen
            if (_settingsFile.Contains("mode") != true)
            {
                NotifyComplete();
            }
            if ((int)_settingsFile["mode"] == 0) {
                _dayStr = " heute ";
            }
            else if ((int)_settingsFile["mode"] == 1)
            {
                _dayStr = " morgen ";
            }
            _fetcher = new Fetcher((int)_settingsFile["mode"]);
            _fetcher.RaiseErrorMessage += (sender, e) =>
            {
                Stop();
            };
            _fetcher.RaiseRetreivedScheduleItems += (sender, e) =>
            {
                Proceed(e.Schedule);
            };
            if (_settingsFile.Contains("group"))
            {
                _fetcher.GetTimes((int)_settingsFile["group"] + 1);
            }
            else
            {
                Stop();
            }
        }

        private void Proceed(List<Data> v1)
        {
            ShellToast toast = new ShellToast {Title = "Informant"};
            if (v1.Count == 0)
                {
                    toast.Content = "Es gibt" + _dayStr + "keine Vertretungen.";
                }
                else if (v1.Count == 1)
                {
                    toast.Content = "Es gibt" + _dayStr + v1.Count.ToString() + " Vertretung!";
                }
                if (v1.Count == 0) { }
                toast.Content = "Es gibt" + _dayStr + v1.Count.ToString() + " Vertretungsstunden!";
                try
                {
                    if ((bool)_settingsFile["notify"] && (int)_settingsFile["oldCount"] != v1.Count)
                    {
                        toast.Show();
                        Write("oldCount", v1.Count);
                    }
                }
                catch
                {
                    try
                    {
                        if ((bool)_settingsFile["notify"])
                        {
                            //toast.Show();
                            Write("oldCount", v1.Count);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
                IconicTileData tileData = new IconicTileData()
                {
                    Title = "CWS Informant",
                    Count = v1.Count,
                };
                foreach (var tile in ShellTile.ActiveTiles)
                {
                    tile.Update(tileData);
                }
                Stop();
        }

        private void Stop()
        {
#if DEBUG
            ScheduledActionService.LaunchForTest(t.Name, TimeSpan.FromSeconds(60));
#endif
            NotifyComplete();
        }

        private bool Write(string key, object data)
        {
            try
            {
                if (_settingsFile.Contains(key))
                {
                    _settingsFile[key] = data;
                    _settingsFile.Save();
                    return true;
                }
                _settingsFile.Add(key, data);
                _settingsFile.Save();
                return true;
            }
            catch {
                return false;
            }
        }
    }
}