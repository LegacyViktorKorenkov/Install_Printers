using Install_Printers_Lib.Actions;
using Install_Printers_Lib.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace Install_Printers_WPF.Actions
{
    class InstallDrivers
    {
        public delegate void OutDelegate(string outPut);
        public event OutDelegate OutEvent;

        public Printer Printer { get; set; }

        public string NetName { get; set; }

        public string DriverPath { get; set; }

        private Unzip _unzip;

        private string _driverName;

        public InstallDrivers(Printer printer, string netName, Unzip unzip)
        {
            Printer = printer;
            NetName = netName;
            _unzip = unzip;
        }

        public InstallDrivers(Printer printer, Unzip unzip)
        {
            Printer = printer;
            _unzip = unzip;
        }

        private async Task InstallPrinterDriver()
        {
            _driverName = Printer.GetPrinterDriver();

            if (string.IsNullOrEmpty(_driverName))
            {
                DownloadDriver download = new DownloadDriver(Printer);

                try
                {
                    if (await download.StartDownload())
                    {
                        if (await UnzipDriver())
                            await InstallProcess();
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }

            if (Printer.NetPrinter)
            {
                try
                {
                    await ConnectPrinter();
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        /// <summary>
        /// Метод создает сетевой порт принтера в соответствии с введенным именем принтера 
        /// и подключает принтер с именем соответствующим выбранному в cmbPrintersModel
        /// </summary>
        /// <param name="printerName"></param>
        /// <param name="lanName"></param>
        async Task ConnectPrinter()
        {
            if (!string.IsNullOrEmpty(NetName))
            {
                if (string.IsNullOrEmpty(_driverName))
                   _driverName = await GetDriverName();

                await Task.Run(() =>
                {
                    ManagementObject port = GetManagementObject(true);
                    ManagementObject printer = GetManagementObject(false);

                    // Запись параметров сетвого порта принтера
                    PutOptions options = new PutOptions();
                    options.Type = PutType.UpdateOrCreate;
                    // Применение параметров сетевого порта принтера
                    port.Put(options);

                    try
                    {
                        // Запись параметров подключаемого принтера
                        PutOptions optionsPrint = new PutOptions();
                        optionsPrint.Type = PutType.UpdateOrCreate;
                        // Применение параметров сетевого порта принтера
                        printer.Put(optionsPrint);
                    }
                    catch (Exception e)
                    {
                        // Сообщение при неудачном выполнении метода connectPrinter
                        throw new Exception(e.Message);
                    }
                });
            }
        }

        /// <summary>
        /// protocol true = Win32_TCPIPPrinterPort, protocol false = "Win32_Printer"
        /// </summary>
        /// <param name="mPath"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        private ManagementObject GetManagementObject(bool protocol)
        {
            if (protocol)
            {
                ManagementClass createPort = new ManagementClass("Win32_TCPIPPrinterPort");
                ManagementObject port = createPort.CreateInstance();

                // Параметры сетевого порта принтера
                port["Name"] = NetName;
                port["HostAddress"] = NetName;
                port["PortNumber"] = 9100;
                port["Protocol"] = 1;
                port["SNMPCommunity"] = "public";
                port["SNMPEnabled"] = true;
                port["SNMPDevIndex"] = 1;

                return port;
            }
            else
            {
                try
                {
                    ManagementClass createPrinter = new ManagementClass("Win32_Printer");
                    createPrinter.Get();
                    ManagementObject printer = createPrinter.CreateInstance();

                    // Параметры подключаемого принтера
                    printer["DriverName"] = _driverName;
                    printer["PortName"] = NetName;
                    printer["DeviceID"] = Printer.PrinterName;
                    printer["Network"] = true;

                    return printer;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        private ProcessStartInfo CreateProcessStartInfo(string fileName, string arguments)
        {
            return new ProcessStartInfo()
            {
                FileName = fileName,
                CreateNoWindow = true,
                UseShellExecute = false,
                Arguments = arguments
            };
        }

        public async Task Start()
        {
            try
            {
                await InstallPrinterDriver();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private async Task<string> GetDriverName()
        {
            string tempDriverName = string.Empty;

            await Task.Run(() =>
            {
                foreach (var f in Directory.GetFiles(DriverPath, "*.inf", SearchOption.AllDirectories))
                {
                    tempDriverName = File.ReadAllLines(f).Where(x => x.Contains(Printer.PrinterName)).FirstOrDefault();

                    if (!string.IsNullOrEmpty(tempDriverName))
                        break;
                }
            });
                       
            return tempDriverName.Remove(tempDriverName.LastIndexOf('"')).Remove(0, tempDriverName.IndexOf('"')).Trim().Trim('"');
        }

        private async Task InstallProcess()
        {
            await Task.Run(() =>
            {
                Process installProcess = new Process()
                {
                    StartInfo = CreateProcessStartInfo("cmd.exe", $@"/c pnputil /add-driver ""{DriverPath}\*.inf"" /install /subdirs")
                };

                try
                {
                    installProcess.Start();
                    installProcess.WaitForExit();
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            });
        }

        private async Task<bool> UnzipDriver()
        {
            try
            {
                DriverPath = await _unzip.UnzipDriver();

                return true;
            }
            catch (Exception e)
            {
                DriverPath = e.Message;

                return false;
            }
        }
    }
}
