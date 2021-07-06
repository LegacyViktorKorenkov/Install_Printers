using Install_Printers_Lib.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Install_Printers_Lib.Actions
{
    public class Use_Install_Printers_Api
    {
        private HttpClient httpClient;

        private Printer _newDriver;

        MultipartFormDataContent _newData;

        public static string _serverAddress = "http://sky-244:5100";
        public static string _serverAddress2 = "http://sky-160:5100";

        public Use_Install_Printers_Api(Printer newDriver)
        {
            _newDriver = newDriver;

            httpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(60) };
        }

        public Use_Install_Printers_Api(MultipartFormDataContent newData)
        {
            _newData = newData;

            httpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(60) };
        }

        public Use_Install_Printers_Api()
        {
            httpClient = new HttpClient();
        }

        public async Task<List<Printer>> GetPrinters()
        {
            var url = $@"{_serverAddress}/GetPrinterInfo/GetDriver/GetPrintersList";

            try
            {
                var printersList = JsonConvert.DeserializeObject<List<Printer>>(await httpClient.GetStringAsync(url));

                return printersList;
            }
            catch
            {
                return new List<Printer>();
            }
        }

        public async Task<Printer> GetPrinterDriver(int id)
        {
            var url = $@"{_serverAddress}/GetPrinterInfo/GetDriver/GetDriver/{id}";

            return JsonConvert.DeserializeObject<Printer>(await httpClient.GetStringAsync(url));
        }

        public async Task<HttpResponseMessage> UploadNewDriver()
        {
            var url = $@"{_serverAddress}/GetPrinterInfo/GetDriver/AddDriver/{_newDriver.PrinterName}/{_newDriver.NetPrinter}";

            #region _
            //httpClient.DefaultRequestHeaders.Add("printerName", _newDriver.PrinterName);
            //httpClient.DefaultRequestHeaders.Add("net", _newDriver.NetPrinter.ToString());
            #endregion

            try
            {
                var result = await httpClient.PostAsync(url, _newData);

                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<HttpResponseMessage> UploadNewDriverRequest()
        {
            var url = $@"{_serverAddress}/GetPrinterInfo/GetDriver/AddDriver";

            #region _
            //httpClient.DefaultRequestHeaders.Add("printerName", _newDriver.PrinterName);
            //httpClient.DefaultRequestHeaders.Add("net", _newDriver.NetPrinter.ToString());
            #endregion

            try
            {
                var result = await httpClient.PostAsync(url, _newData);

                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> CheckName(string printerName)
        {
            var url = $@"{_serverAddress}/GetPrinterInfo/GetDriver/CheckName/{printerName}";

            try
            {
                return JsonConvert.DeserializeObject<bool>(await httpClient.GetStringAsync(url));
            }
            catch (Exception e)
            {
                throw new Exception(e.InnerException.Message);
            }
        }
    }
}
