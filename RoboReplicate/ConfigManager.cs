using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboReplicate
{
    class ConfigManager
    {
        static List<Tuple<string, string>> ReadAllSettings()
        {
            if (System.Configuration.ConfigurationManager.AppSettings.Count == 0)
            {
                return null;
            }

            List<Tuple<string,string>> settings = new List<Tuple<string,string>>();

            foreach (string key in System.Configuration.ConfigurationManager.AppSettings.AllKeys)
            {
                settings.Add(new Tuple<string,string>(key, System.Configuration.ConfigurationManager.AppSettings[key]));
            }
            return settings;
        }

        static string ReadSettings(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key] ?? null;
        }

        public static dynamic LoadConfig(dynamic o)
        {
            o = Activator.CreateInstance(o.GetType());
            List<Tuple<string, string>> settings = ReadAllSettings();
            foreach (Tuple<string, string> s in settings)
            {
                o.GetType().GetField(s.Item1.ToString().Trim()).SetValue(o, s.Item2);
            }
            return o;
        }
    }
}
