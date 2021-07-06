using Install_Printers_Lib.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Install_Printers_Lib_core.Actions
{
    public class Use_Install_Printers_Api
    {
        private HttpClient httpClient;

        private Printer _newDriver;

        MultipartFormDataContent _newData;

        private string _serverAddress = "http://sky-160:5101";

        public Use_Install_Printers_Api(Printer newDriver)
        {
            _newDriver = newDriver;

            httpClient = new HttpClient();
        }

        public Use_Install_Printers_Api(MultipartFormDataContent newData, Printer newDriver)
        {
            _newData = newData;

            _newDriver = newDriver;

            httpClient = new HttpClient();
        }

        public Use_Install_Printers_Api()
        {
            httpClient = new HttpClient();
        }

        public List<Printer> GetPrinters()
        {
            var url = $@"{_serverAddress}/GetPrinterInfo/GetDriver/GetPrintersList";

            httpClient.DefaultRequestHeaders.Add("printer", new List<string>() {"Hp 4555", "true" });

            try
            {
                return JsonConvert.DeserializeObject<List<Printer>>(httpClient.GetStringAsync(url).Result);
            }
            catch
            {
                return new List<Printer>();
            }
        }

        public Printer GetPrinterDriver(int id)
        {
            var url = $@"{_serverAddress}/GetPrinterInfo/GetDriver/GetDriver/{id}";

            return JsonConvert.DeserializeObject<Printer>(httpClient.GetStringAsync(url).Result);
        }

        public async Task<HttpResponseMessage> UploadNewDriver()
        {
            var url = $@"{_serverAddress}/GetPrinterInfo/GetDriver/AddDriver/{_newDriver.PrinterName}/{_newDriver.NetPrinter}";

            //httpClient.DefaultRequestHeaders.Add("printerName", _newDriver.PrinterName);
            //httpClient.DefaultRequestHeaders.Add("net", _newDriver.NetPrinter.ToString());

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
    }
}
