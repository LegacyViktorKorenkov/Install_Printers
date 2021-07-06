using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Install_Printers_API.Attributes
{
    public class FormAttributes
    {
        public string TrustedFilePath { get; set; }
        public string TrustedFileName { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
        public string Guid { get; private set; } = System.Guid.NewGuid().ToString();
        public bool IsPrimary { get; set; }

        public override string ToString()
        {
            return $"{nameof(TrustedFilePath)}: [{TrustedFilePath}];" + Environment.NewLine +
                   $"{nameof(UserId)}: {UserId}; " + Environment.NewLine +
                   $"{nameof(Guid)}: {Guid}; " + Environment.NewLine +
                   $"{nameof(IsPrimary)}: {IsPrimary}; ";
        }
    }
}
