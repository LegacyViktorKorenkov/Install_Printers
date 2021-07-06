using Install_Printers_Lib.Models;
using System;
using System.Collections.Generic;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Install_Printers.Actions
{
    static class CheckPrinter
    {
        public static async Task<bool> CheckLanStatus(this string netName)
        {
            Ping ping = new Ping();

            bool status = false;

            PingReply reply;

            try
            {
                reply = await ping.SendPingAsync(netName);
            }
            catch
            {
                return status;
            }

            status = reply.Status == IPStatus.Success ? true : status;

            return status;
        }

        /// <summary>
        /// Метод получает установленные на компьютере драйверы принтеров и сравнивает с названием модели выбранного принтера.
        /// Если возникнет совпадение модели и драйвера, то метод возвращает значение true 
        /// и  драйвер устанавливаться не будет, произойдет только подключение принтера
        /// </summary>
        /// <param name="printerName"></param>
        /// <returns></returns>
        public static string GetPrinterDriver(this Printer printer)
        {
            // Экземпляр класса ManagementObjectSearcher для работы с WMI объектом Win32_PrinterDriver
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PrinterDriver");

            // Цикл для сравнения полученных имен драйверов и названия выбранной модели
            foreach (ManagementObject queryObj in searcher.Get())
            {
                // сравнение название модели и имени принтера. При совпадении метод вернет true
                if (queryObj["Name"].ToString().Contains(printer.PrinterName))
                {
                    var prnDrvName = queryObj["Name"].ToString();

                    return prnDrvName.Remove(prnDrvName.IndexOf(','));
                }
                else
                {
                    continue;
                }
            }

            // ответ метода при отсутствии совпадений
            return string.Empty;
        }
    }
}
