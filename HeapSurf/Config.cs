using System;
using System.Collections.Generic;
using System.Text;

namespace HeapSurf
{
    public class Config
    {
        public int PollingIntervalSeconds { get; set; }
        public string FilePath { get; set; }
        public List<string> Processes { get; set; }
        public bool ConsoleOutput { get; set; }
        public bool FallBackToProcdump { get; set; }

        public static Config Default = new Config
        {
            PollingIntervalSeconds = 2,
            FilePath = @"C:\cc.xml",
            Processes = new List<string>
            {
                "POS.exe",
                "POS.vshost",
                "posgateway",
                //"chrome",
                //"iexplore",
                //"firefox"
                "notepad"
            },
            ConsoleOutput = true,
            FallBackToProcdump = true,
        };

        public static Config Load(string file)
        {
            try
            {
                return FileHelper.Load<Config>(file);
            }
            catch
            {
                var config = Config.Default;
                config.Save(file);
                return config;
            }
        }

        public void Save(string file)
        {
            FileHelper.Save<Config>(this, file);
        }

        public override string ToString()
        {
            var s = "Config File: " + this.FilePath + "\n";
            s += "Processes:\n";
            foreach (var process in this.Processes)
            {
                s += process + "\n";
            }
            s += "Console Output: " + this.ConsoleOutput + "\n";
            s += "Fall Back to ProcDump: " + this.FallBackToProcdump + "\n";
            s += "Polling Interval: " + this.PollingIntervalSeconds + "\n";
            return s;
        }
    }
}
