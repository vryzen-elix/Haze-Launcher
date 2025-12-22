using System;
using System.Diagnostics;
using System.IO;

namespace WpfApp6.Services.Launch
{
    public static class FakeAC
    {
        public static Process BEProcess;
        public static Process LauncherProcess;

        public static void Start(string root, string exeName, string args, string type)
        {
            string exe = Path.Combine(root, "FortniteGame\\Binaries\\Win64", exeName);
            if (!File.Exists(exe)) return;

            var startInfo = new ProcessStartInfo()
            {
                FileName = exe,
                Arguments = args,
 
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,

 
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var process = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = false
            };

            process.Start();

            if (type == "r")
                BEProcess = process;

            else if (type == "dsf")
                LauncherProcess = process;
        }
     } 
     }  
