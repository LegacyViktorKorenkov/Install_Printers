using Install_Printers_Lib.Actions;
using Install_Printers_Lib.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TestInstallPrintersApp
{
    class Program
    {
        static string path = string.Empty;

        static void Main(string[] args)
        {
            Printer newDriver;
            bool net;

            Console.WriteLine("Введите модель принтера, затем нажмите Enter. Например HP Color LaserJet Pro MFP M274");

            var name = Console.ReadLine();

            Console.WriteLine(@"Принтер\МФУ является сетевым? y - да, n - нет");

        inputNet:
            {
                var input = Console.ReadKey();

                if (input.Key == ConsoleKey.Y)
                    net = true;
                else if (input.Key == ConsoleKey.N)
                    net = false;
                else
                {
                    Console.WriteLine("\nОшибка. Повторите ввод. Принтер\\МФУ является сетевым? y - да, n - нет");

                    goto inputNet;
                }
            }

            newDriver = new Printer()
            {
                PrinterName = name,
                Link = new Link(),
                NetPrinter = net
            };

            #region _
            //var api = new Use_Install_Printers_Api(newDriver);

            //var driverNetPath = api.UploadNewDriver();

            //using (WebClient client = new WebClient())
            //{
            //    //client.Credentials = CredentialCache.DefaultCredentials;

            //    //WebRequest request = WebRequest.Create(driverNetPath);
            //    //WebResponse response;
            //    //response = request.GetResponse();
            //    //response.Close();

            //    client.UploadValues(driverNetPath, Console.ReadLine());
            //}
            #endregion

            #region _
            MultipartFormDataContent form = new MultipartFormDataContent();

            Console.WriteLine("\nЗапакуйте папку с драйверами в .zip архив и укажите путь к архиву, затем нажмите Enter и ожидайте ответа программы.");

        inputPath:
            {
                path = Console.ReadLine();
            }

            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path) && path.Contains(".zip"))
            {
                var fileStream = new FileStream(path, FileMode.Open);

                form.Add(new StreamContent(fileStream), "File");

                var api = new Use_Install_Printers_Api(form, newDriver);

                try
                {
                    Console.WriteLine(api.UploadNewDriver().Result);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                #endregion

                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Неправильный путь или расширение файла. Повторите ввод.");

                goto inputPath;
            }
        }
    }
}
