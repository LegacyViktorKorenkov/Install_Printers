using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Install_Printers_API.Context;
using Install_Printers_Lib.Actions;
using Install_Printers_Lib.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Install_Printers_API.Controllers
{
    [RequestSizeLimit(1_000_000_000)]
    public class UploadController : Controller
    {
        Use_Install_Printers_Api api;

        private readonly ILogger _logger;

        private readonly DataContext context;

        public UploadController(ILogger<UploadController> logger, DataContext dataContext)
        {
            context = dataContext;

            _logger = logger;
        }

        public IActionResult Upload()
        {
            _logger.LogWarning("Подключение");

            ViewData["message"] = "Ожидание загрузки файла";

            return View();
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 1_000_000_000)]
        public async Task<IActionResult> UploadFile(string uploadFile, Printer newDriver)
        {
            _logger.LogWarning("Старт загрузки");

            _logger.LogWarning(uploadFile);

            _logger.LogWarning(newDriver.PrinterName);

            try
            {
                if (uploadFile.ToLower().Contains(".zip"))
                {
                    if (context.Printers.Where(x => x.PrinterName.Contains(newDriver.PrinterName)).ToList().Count == 0)
                    {
                        MultipartFormDataContent form = new MultipartFormDataContent();

                        using (var fileStream = new FileStream(uploadFile, FileMode.Open))
                        {
                            form.Add(new StreamContent(fileStream), "File");

                            //api = new Use_Install_Printers_Api(form, newDriver);

                            await api.UploadNewDriver();
                        }

                        _logger.LogWarning("Загрузка завершена");

                        ViewData["message"] = "Файл загружен";                        
                    }
                    else
                    {
                        ViewData["message"] = "Такой принтер уже загружен";
                    }
                }
                else
                {
                    ViewData["message"] = "Неправилдьный формат архива";
                }
            }
            catch (Exception e)
            {
                ViewData["message"] = $"Ошибка загрузки {e.Message}";

                _logger.LogWarning(e.Message);

                _logger.LogWarning(uploadFile);
            }

            return View("Upload");
        }

        //[HttpPost]
        //public async Task<IActionResult> UploadFile(string uploadFile, Printer newDriver)
        //{
        //    MultipartFormDataContent form = new MultipartFormDataContent();

        //    //form.Add(new StringContent(newDriver.PrinterName), "PrinterName");
        //    //form.Add(new StringContent(newDriver.NetPrinter.ToString()), "NetPrinter");

        //    var fileStream = new FileStream(uploadFile, FileMode.Open);

        //    form.Add(new StreamContent(fileStream), "File");

        //    api = new Use_Install_Printers_Api(form, newDriver);

        //    await api.UploadNewDriver();

        //    return Redirect("~/Upload/Upload");
        //}
    }
}