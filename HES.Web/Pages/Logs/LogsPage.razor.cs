using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Logs
{
    public partial class LogsPage : ComponentBase, IDisposable
    {
        private readonly string _folderPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "logs");

        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<LogsPage> Logger { get; set; }
        [Parameter] public string FileName { get; set; }

        public Dictionary<string, string> FileNames = new Dictionary<string, string>();
        public List<LogModel> LogsList { get; set; }
        public LogModel CurrentLog { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public string SelectedFileName { get; set; }
        public bool LocalTime { get; set; }
        public bool IsBusy { get; set; }
        public bool IsShown { get; set; }
        public string ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                NavigationManager.LocationChanged += NavigationManager_LocationChanged;

                // Getting all files
                var items = new DirectoryInfo(_folderPath).GetFiles("*.log").OrderByDescending(x => x.Name);

                foreach (var item in items)
                    FileNames.Add(item.Name.Replace(".log", string.Empty), item.FullName);

                // If parameter exist show log
                if (FileName != null)
                    await ShowLogAsync(FileName);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private async void NavigationManager_LocationChanged(object sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        {
            try
            {
                var name = e.Location.Split("/").LastOrDefault();
                await ShowLogAsync(name);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private async Task ShowLogAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            IsBusy = true;

            try
            {
                SelectedFileName = $"{name}.log";

                var list = new List<LogModel>();
                var separator = "hes>";

                var path = FileNames[name];
                var text = await File.ReadAllTextAsync(path);
                var separated = text.Split(separator);

                foreach (var item in separated)
                {
                    if (item != "")
                    {
                        list.Add(new LogModel { Name = name, Date = item.Split("|")[0], Level = item.Split("|")[1], Logger = item.Split("|")[2], Message = item.Split("|")[3] });
                    }
                }

                LogsList = list.OrderByDescending(x => x.Date).ToList();
            }
            catch (KeyNotFoundException)
            {
                return;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
                StateHasChanged();
            }
        }

        private void ModalToggle(LogModel logModel = null)
        {
            if (logModel != null)
            {
                CurrentLog = logModel;
                IsShown = true;
            }
            else
            {
                IsShown = false;
                CurrentLog = null;
            }
        }

        public async Task DownloadFileAsync()
        {
            try
            {
                var path = Path.Combine(_folderPath, SelectedFileName);
                var content = File.ReadAllText(path);
                await JSRuntime.InvokeVoidAsync("downloadLog", SelectedFileName, content);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }
        }

        public void DeleteFile()
        {
            try
            {
                // Delete from disk
                var path = Path.Combine(_folderPath, SelectedFileName);
                File.Delete(path);
                // Remove from dictionary
                FileNames.Remove(SelectedFileName.Replace(".log", string.Empty));
                // Reset objects
                LogsList = null;
                CurrentLog = null;
                SelectedFileName = null;
                // Navigate to logs root path 
                NavigationManager.NavigateTo(NavigationManager.BaseUri + "logs");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }
        }

        public void Dispose()
        {
            NavigationManager.LocationChanged -= NavigationManager_LocationChanged;
        }
    }

    public class LogModel
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public string Level { get; set; }
        public string Logger { get; set; }
        public string Message { get; set; }
    }
}