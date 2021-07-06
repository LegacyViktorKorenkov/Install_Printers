using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Install_Printers.Actions
{
    class printport
    {
        string DeviceID;
        string DriverName;
        string PortName;
        Boolean Shared;
        string ShareName;
        private ManagementScope managementScope = null;

        private ManagementClass InitClass(string className)
        {
            //specify Printer class management path
            ManagementPath managementPath = new ManagementPath(className);
            try
            {
                //create new WMI management class
                return new ManagementClass(managementScope, managementPath, null);
            }
            catch (Exception ex)
            {
                throw new Exception(
                String.Format(
                "WMI exception: {0}", ex.Message));
            }
        }

        public bool AddPrinter(string printerName, string printerDriver, string portName, bool sharedPrinter)
        {
            bool result = false;
            try
            {
                //init Win32_Printer class
                ManagementClass printerClass = InitClass("Win32_Printer");
                //create new Win32_Printer object
                ManagementObject printerObject = printerClass.CreateInstance();
                //set port parameters
                if (portName == null || portName.Length == 0)
                    printerObject["PortName"] = "LPT1:";
                else
                {
                    if (portName[portName.Length - 1] != ':')
                        printerObject["PortName"] = (portName + ":");
                    else
                        printerObject["PortName"] = portName;
                }
                //set driver and device names
                printerObject["DriverName"] = printerDriver;

                printerObject["DeviceID"] = printerName;
                //set sharing
                if (sharedPrinter)
                {
                    printerObject["Shared"] = sharedPrinter;
                    printerObject["ShareName"] = printerName;
                }
                // specify put options: update or create
                PutOptions options = new PutOptions();
                options.Type = PutType.UpdateOrCreate;
                //put a newly created object to WMI objects set
                printerObject.Put(options);

                result = true;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("WMI exception: {0}", ex.Message));
            }
            return result;
        }

    }
}
