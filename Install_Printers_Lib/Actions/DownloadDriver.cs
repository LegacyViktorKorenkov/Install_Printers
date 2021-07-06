using Install_Printers_Lib.Models;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Install_Printers_Lib_core.Actions
{
    public class DownloadDriver
    {
        private Printer _downloadFile;

        WebClient webClient;

        private string _serverAddress = "http://sky-160:5100";

        public DownloadDriver(Printer downloadFile)
        {
            _downloadFile = downloadFile;

            webClient = new WebClient();
        }

        private async Task<bool> DownloadDriverFileAsync()
        {
            if (!Directory.Exists("Temp"))
                Directory.CreateDirectory("Temp");

            try
            {
                await Task.Run(() => webClient.DownloadFile(new Uri($@"{_serverAddress}/{_downloadFile.Link.FileLink}"), $@"Temp\{_downloadFile.PrinterName}-driver.zip"));

                return true;
            }
            catch (Exception e)
            {
                throw new Exception($@"Ошибка скачивания. {e.Message}");
            }
            finally
            {
                webClient.Dispose();
            }
        }

        public async Task<bool> StartDownload()
        {
            try
            {
                await DownloadDriverFileAsync();

                return true;
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
