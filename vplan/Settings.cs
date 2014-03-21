using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;


namespace vplan
{
    class Settings
    {
        IsolatedStorageSettings settingsFile = IsolatedStorageSettings.ApplicationSettings;
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
        public object read(string key)
        {
            if (settingsFile.Contains(key))
            {
                return settingsFile[key];
            }
            else { return null; }
        }
    }
}
