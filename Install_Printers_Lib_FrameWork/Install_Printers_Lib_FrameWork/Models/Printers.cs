using Install_Printers_Lib.ProcessClass;
using System;
using System.IO;
using System.Web.ModelBinding;

namespace Install_Printers_Lib.Models
{
    public class Printer : PropChange
    {
        public int Id { get; set; }

        private string printerName;       
        public string PrinterName
        {
            get => printerName;
            set { printerName = value; OnPropertyChange(nameof(PrinterName)); }
        }

        private Link link;
        public Link Link
        {
            get => link;
            set { link = value; OnPropertyChange(nameof(Link)); }
        }

        private bool netPrinter;
        public bool NetPrinter
        {
            get => netPrinter;
            set { netPrinter = value; OnPropertyChange(nameof(NetPrinter)); }
        }
    }
}
