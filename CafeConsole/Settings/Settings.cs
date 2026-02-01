using System;
using System.Collections.Generic;
using System.Text;

namespace CafeConsole.Settings
{
    public class Settings
    {
        public string ServerIP { get; set; } = "localhost";
        public string ServerPort { get; set; } = "8080";
        public string ServerUsername { get; set; } = "user";
        public string ServerPassword { get; set; } = string.Empty;
        public string DatabaseIP { get; set; } = "localhost";
        public string DatabasePort { get; set; } = "50505";
        public string DatabaseUsername { get; set; } = "postgres";
        public string DatabasePassword { get; set; } = string.Empty;
        public bool IsInitialized { get; set; } = false;
    }
}
