using Install_Printers_Lib.Models;
using System;
using System.Collections.Generic;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Install_Printers.Actions
{
    /// <summary>
    /// Contains logic for checking printer statuses and driver availability
    /// </summary>
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
        /// The method gets the printer drivers installed on the computer and compares them with the model name of the selected printer.
        /// If there is a match between the model and the driver, then the method returns true
        /// and the driver will not be installed, only the printer will be connected
        /// </summary>
        /// <param name="printerName"></param>
        /// <returns></returns>
        public static string GetPrinterDriver(this Printer printer)
        {
            // An instance of the ManagementObjectSearcher class for working with the WMI object Win32_PrinterDriver
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PrinterDriver");

            // Loop to compare the received driver names and the name of the selected model 
            foreach (ManagementObject queryObj in searcher.Get())
            {
                // compare model name and printer name. If matched, the method will return true
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

            // method response if there are no matches
            return string.Empty;
        }
    }
}
