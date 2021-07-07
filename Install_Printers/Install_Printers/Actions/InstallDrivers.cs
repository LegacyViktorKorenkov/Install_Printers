using Install_Printers_Lib.Actions;
using Install_Printers_Lib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace Install_Printers.Actions
{
    /// <summary>
    /// Contains the logic of the driver installation process
    /// </summary>
    class InstallDrivers
    {
        //public delegate void OutDelegate(string outPut);
        //public event OutDelegate OutEvent;

        /// <summary>
        /// Printer object
        /// </summary>
        public Printer Printer { get; set; }

        /// <summary>
        /// Printer network name
        /// </summary>
        public string NetName { get; set; }

        /// <summary>
        /// Path to printer driver
        /// </summary>
        public string DriverPath { get; set; }

        /// <summary>
        /// Unpacker object
        /// </summary>
        private Unzip _unzip;

        /// <summary>
        /// Printer driver name
        /// </summary>
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

        /// <summary>
        /// Downloads, unpacks and runs the driver installation
        /// </summary>
        /// <returns></returns>
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
        /// The method creates a network printer port according to the entered printer name
        /// and connects a printer with a name corresponding to the one selected in cmbPrintersModel 
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
                        // Применение параметров принтера
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

        /// <summary>
        /// Creates a process to install the driver
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Running the entire installation cycle
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Get driver name from .inf file
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a process object for installing a driver
        /// </summary>
        /// <returns></returns>
        private async Task InstallProcess()
        {
            await Task.Run(() =>
            {
                #region cab install
                var test = GetCab();

                foreach(var c in test)
                {
                    Process installProcess = new Process()
                    {
                        StartInfo = CreateProcessStartInfo(@"dism.exe", $@"/Online /Add-Package /packagepath:{c}")
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
                }
                #endregion

                #region inf install
                //Process installProcess = new Process()
                //{
                //    StartInfo = CreateProcessStartInfo(@"cmd.exe", $@"/k pnputil /add-driver ""{DriverPath}\*.inf"" /install /subdirs")
                //};

                //try
                //{
                //    installProcess.Start();
                //    installProcess.WaitForExit();
                //}
                //catch (Exception e)
                //{
                //    throw new Exception(e.Message);
                //}
                #endregion
            });
        }

        /// <summary>
        /// Receives all.сab packages 
        /// </summary>
        /// <returns></returns>
        private List<string> GetCab()
        {
            return Directory.GetFiles(DriverPath, "*.cab", SearchOption.AllDirectories).ToList();
        }

        /// <summary>
        /// Unpacks the driver
        /// </summary>
        /// <returns></returns>
        private async Task<bool> UnzipDriver()
        {
            try
            {
                await _unzip.UnzipDriver();

                DriverPath = "Temp";

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
