using System.Diagnostics;
using System.Windows;
using System;
using Microsoft.Phone.Scheduler;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using vplan;

namespace DataUpdate
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        protected IsolatedStorageSettings settingsFile = IsolatedStorageSettings.ApplicationSettings;
        public Fetcher fetcher;
        protected string dayStr = " ";
        private ScheduledTask t;
        protected string kachel1;
        protected string kachel2;
        protected string kachel3;
        protected int vert;
        protected int mitb;
        protected int entf;
        protected int raum;
        protected int vera;
        private List<string> subjects;
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
            if (settingsFile.Contains("mode") != true)
            {
                NotifyComplete();
            }
            if ((int)settingsFile["mode"] == 0) {
                dayStr = " heute ";
            }
            else if ((int)settingsFile["mode"] == 1)
            {
                dayStr = " morgen ";
            }
            fetcher = new Fetcher(this, 2);
            t = task;
            if (settingsFile.Contains("group"))
            {
                fetcher.getTimes((int)settingsFile["group"] + 1, false);
            }
            else
            {
                Stop();
            }
        }
        public void Proceed(ObservableCollection<Data> v1)
        {
            ShellToast toast = new ShellToast();
            toast.Title = "Informant";
            if (v1.Count == 0) {
                toast.Content = "Es gibt" + dayStr + "keine Vertretungen.";
            }
            else if (v1.Count == 1) {
                toast.Content = "Es gibt" + dayStr + v1.Count.ToString() + " Vertretung!";
            }
            if (v1.Count == 0) { }
            toast.Content = "Es gibt" + dayStr + v1.Count.ToString() + " Vertretungsstunden!";
            try
            {
                if ((bool)settingsFile["notify"] == true && (int)settingsFile["oldCount"] != v1.Count)
                {
                    toast.Show();
                    write("oldCount", v1.Count);
                }
            }
            catch {
                try
                {
                    if ((bool)settingsFile["notify"] == true)
                    {
                        toast.Show();
                        write("oldCount", v1.Count);
                    }
                }
                catch 
                {}
            }
            IconicTileData TileData = new IconicTileData()
            {
               Title = "CWS Informant",
               Count = v1.Count,
            };
            foreach (var tile in ShellTile.ActiveTiles)
            {
                tile.Update(TileData);
            }
            Stop();
        }
        public void Stop()
        {
#if DEBUG
            ScheduledActionService.LaunchForTest(t.Name, TimeSpan.FromSeconds(60));
#endif
            NotifyComplete();
        }
        public bool write(string key, object data)
        {
            if (settingsFile.Contains(key))
            {
                settingsFile[key] = data;
                settingsFile.Save();
                return true;
            }
            else
            {
                settingsFile.Add(key, data);
                settingsFile.Save();
                return true;
            }
        }
    }
}