using HES.Core.Enums;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HES.Web.Pages.Update
{
    public partial class UpdatePage : HESPageBase
    {
        [Inject] public IWebHostEnvironment Environment { get; set; }

        private const string linuxUpdateFile = "update.sh";
        private const string windowsUpdateFile = "update.ps1";
        private const string platformNotSupported = "not supported";
        private const string updateFileNotFound = "update file not found";

        public string CurrentPlatform { get; set; }
        public string CurrentScript { get; set; }
        public string Command { get; set; }
        public string Args { get; set; }

        protected override void OnInitialized()
        {
            bool isLunux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            CurrentScript = updateFileNotFound;

            if (isLunux)
            {
                CurrentPlatform = OSPlatform.Linux.ToString();
                var path = Path.Combine(Environment.ContentRootPath, linuxUpdateFile);
                if (File.Exists(path))
                {
                    CurrentScript = linuxUpdateFile;
                    Command = "bash";
                    Args = $"\"{path}\"";
                }
            }
            else if (isWindows)
            {
                CurrentPlatform = OSPlatform.Windows.ToString();
                var path = Path.Combine(Environment.ContentRootPath, windowsUpdateFile);
                if (File.Exists(path))
                {
                    CurrentScript = windowsUpdateFile;
                    Command = "powershell";
                    Args = $"-f \"{path}\"";
                }
            }
            else
            {
                CurrentPlatform = platformNotSupported;
            }
        }

        public async Task RunCommand()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Command))
                {
                    return;
                }

                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Command,
                        Arguments = Args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                process.Start();
                //string output = process.StandardOutput.ReadToEnd();
                //string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }
    }
}