using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Install_Printers.Actions
{
    /// <summary>
    /// Contains the logic for unpacking the archive with the driver
    /// </summary>
    class Unzip
    {
        private string _packageName;

        public Unzip(string packagePath)
        {
            _packageName = packagePath;
        }

        /// <summary>
        /// Unpacks the driver to a temporary folder
        /// </summary>
        /// <returns></returns>
        public async Task<string> UnzipDriver()
        {
            //Directory.CreateDirectory($@"{_packageName}");

            try
            {
                await Task.Run(() => ZipFile.ExtractToDirectory($@"{_packageName}.zip", "Temp" /*$@"{_packageName}"*/));

                File.Delete($@"{_packageName}.zip");

                return $@"{_packageName}";
            }
            catch (Exception e)
            {
                throw new Exception($@"Ошибка распаковки. {e.Message}");
            }
        }
    }
}
