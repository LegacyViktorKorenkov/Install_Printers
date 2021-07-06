using System;
using System.Collections.Generic;
using System.Text;

namespace Install_Printers_Lib.Models
{
    public class UploadDriver
    {
        public Printer newPrinter { get; set; }

        public string FileName { get; set; }

        public byte[] UploadFile { get; set; }
    }
}
