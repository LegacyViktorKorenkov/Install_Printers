using System;
using System.Collections.Generic;
using System.Management;

namespace Install_Printers_Lib_core.Actions
{
    public class PrintersInfo
    {
        /// <summary>
        /// Метод получает установленные в системе принтеры
        /// </summary>
        public List<string> InstalledPrinters()
        {
            List<string> printermass = new List<string>();
            // Инициализация класса WMI для работы с принтерами
            ManagementObjectSearcher searcherInstallPrinters = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Printer");

            // Цикл добавления полученных имен принтеров в коллекцию printermass
            foreach (ManagementObject queryObj in searcherInstallPrinters.Get())
            {
                printermass.Add(queryObj["Name"].ToString());
            }
            return printermass;
        }

        /// <summary>
        /// Метод посылает на печать тестовую страницу выбранного принтера
        /// </summary>
        public void TestPrinteing(string printerName)
        {
            ManagementObject classInstance = new ManagementObject(@"root\cimv2", $"Win32_Printer.DeviceID='{printerName}'", null);

            try
            {
                classInstance.InvokeMethod("PrintTestPage", null, null);
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Метод удаляет выбранный принтер из системы
        /// </summary>
        public void DeletePrinter(string printerName)
        {
            SelectQuery printer = new SelectQuery("Win32_Printer", $@"Name = ""{printerName}""");

            ManagementObjectSearcher delete = new ManagementObjectSearcher(printer);

            foreach (ManagementObject prn in delete.Get())
            {
                try
                {
                    prn.Delete();
                }
                catch(Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
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
