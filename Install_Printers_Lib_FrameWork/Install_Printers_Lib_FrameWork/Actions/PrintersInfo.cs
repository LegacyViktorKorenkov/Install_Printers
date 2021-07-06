using System;
using System.Collections.Generic;
using System.Management;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace Install_Printers_Lib.Actions
{
    public class PrintersInfo
    {
        /// <summary>
        /// Метод получает установленные в системе принтеры
        /// </summary>
        public List<string> InstalledPrinters()
        {
            return PrinterSettings.InstalledPrinters.Cast<string>().ToList();
        }

        /// <summary>
        /// Метод посылает на печать тестовую страницу выбранного принтера
        /// </summary>
        public void TestPrinteing(string printerName)
        {
            ManagementObject classInstance = new ManagementObject(@"root\cimv2", $"Win32_Printer.DeviceID='{printerName}'", null);

            classInstance.InvokeMethod("PrintTestPage", null, null);
        }

        /// <summary>
        /// Метод удаляет выбранный принтер из системы
        /// </summary>
        public void DeletePrinter(string printerName)
        {
            string compName = string.Empty;

            if (printerName.StartsWith(@"\\"))
            {
                compName = printerName.Split('\\').FirstOrDefault();

                printerName = $"{printerName.Substring(printerName.LastIndexOf(@"\")).Trim('\\')}";
            }

            ManagementClass win32Printer = new ManagementClass("Win32_Printer");
            List<ManagementObject> printers = win32Printer.GetInstances().Cast<ManagementObject>().ToList();

            var usbNetPrinter = printers.Where(x => x.Path.Path.Contains(compName)).FirstOrDefault();

            if (usbNetPrinter != null && usbNetPrinter.Path.Path.Contains(printerName))
                usbNetPrinter.Delete();
            else
                printers.Where(x => x.Path.Path.Contains(printerName)).FirstOrDefault().Delete();
        }

        /// <summary>
        /// Метод устанавливает выбранный принтер по умолчанию в системе
        /// </summary>
        public void DefaultPrinter(string printerName)
        {
            ManagementObject classInstance = new ManagementObject(@"root\cimv2", $"Win32_Printer.DeviceID='{printerName}'", null);

            classInstance.InvokeMethod("SetDefaultPrinter", null, null);
        }
    }
}
