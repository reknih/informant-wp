using System.IO.IsolatedStorage;
using UntisExp.Interfaces;


namespace vplan
{
    class Settings : ISettings
    {
        readonly IsolatedStorageSettings _settingsFile = IsolatedStorageSettings.ApplicationSettings;
        public void Write(string key, object data)
        {
            try
            {
                if (_settingsFile.Contains(key))
                {
                    _settingsFile[key] = data;
                    _settingsFile.Save();
                    return;
                }
                else
                {
                    _settingsFile.Add(key, data);
                    _settingsFile.Save();
                    return;
                }
            }
            catch {
                return;
            }
        }
        public object Read(string key)
        {
            if (_settingsFile.Contains(key))
            {
                return _settingsFile[key];
            }
            else { return null; }
        }
    }
}
