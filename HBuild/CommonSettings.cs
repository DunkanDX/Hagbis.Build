using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Hagbis.Build {
    public static class CommonSettings {
        static string bprToMakExe;
        static string makeExe;
        public static string MakeExe {
            get { return makeExe; }
            set { makeExe = value; }
        }
        public static string BprToMakExe {
            get { return bprToMakExe; }
            set { bprToMakExe = value; }
        }
        public static void Load() {
            bprToMakExe = ConfigurationManager.AppSettings["bpr2mak"];
            makeExe = ConfigurationManager.AppSettings["make"];
        }
    }
}
