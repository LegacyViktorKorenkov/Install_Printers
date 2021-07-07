using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Install_Printers_API.Context;
using Microsoft.AspNetCore.Mvc;
using Install_Printers_Lib.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using UploadDriverPage.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Install_Printers_API.Attributes;

namespace Install_Printers_API.Controllers
{
    [Route("GetPrinterInfo/[controller]")]
    [ApiController]
    public class GetDriverController : ControllerBase
    {
        ILogger log;

        private DataContext context;

        string _printerName;

        string _netPrinter;

        MultipartReader _reader;

        MultipartSection _section;

        public GetDriverController(DataContext dataContext, ILogger<GetDriverController> logger)
        {
            context = dataContext;

            log = logger;
        }

        [HttpGet]
        [Route("GetPrintersList")]
        public List<Printer> GetPrintersList()
        {
            log.LogWarning("Запрос списка принтеров");

            DataContext dataContext = new DataContext();

            var printersList = dataContext.Printers.OrderBy(x => x.PrinterName).ToList();

            return printersList;
        }

        [HttpGet]
        [Route("GetDriver/{id}")]
        public Printer GetPrinterDriver(int id)
        {
            return context.Printers.Include(x => x.Link).FirstOrDefault(t => t.Id == id);
        }

        [HttpPost]
        [Route("AddDriver")]
        public async Task<IActionResult> AddDriver()
        {
            log.LogWarning("Новая загрузка\n");

            try
            {
                await GetFormInfo();

                Directory.CreateDirectory($@"wwwroot\Drivers\{_printerName}");
                // Path to the save file.
                string filePath = $@"wwwroot\Drivers\{_printerName}";
                // The name of the saved file. 
                string fileName = $@"{_printerName}-driver.zip";
                // Full path to the saved file.
                string path = Path.Combine(filePath, fileName);

                log.LogWarning($"путь сохранения: {path}\n");

                _section = await _reader.ReadNextSectionAsync();

                // Stream of the read part of the request.
                using (Stream fileStream = _section.Body)
                {
                    if (fileStream != null)
                    {
                        // Stream for writing the file.
                        using (FileStream writeStream = new FileStream(path, FileMode.Create))
                        {
                            // Copy the data of the created stream to the file write stream.
                            fileStream.CopyTo(writeStream);
                        }
                    }
                    else
                    {
                        log.LogWarning("FileStream Null");
                    }
                }

                await AddDb(_printerName, Convert.ToBoolean(_netPrinter));

                log.LogWarning($"готово\n");

                return Ok();
            }
            catch (Exception exception)
            {
                log.LogWarning($"{exception.Message}");

                return BadRequest();
            }
        }

        async Task GetFormInfo()
        {
            var boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(Request.ContentType).Boundary).Value;

            _reader = new MultipartReader(boundary, Request.Body);

            _section = await _reader.ReadNextSectionAsync();

            _printerName = await _section.ReadAsStringAsync();

            _section = await _reader.ReadNextSectionAsync();

            _netPrinter = await _section.ReadAsStringAsync();
        }

        private async Task<bool> AddDb(string printerName, bool net)
        {
            Printer newDriver = new Printer()
            {
                PrinterName = printerName,
                Link = new Link() { FileLink = $@"Drivers/{printerName}/{printerName}-driver.zip" },
                NetPrinter = net
            };

            try
            {
                context.Printers.Add(newDriver);

                await context.SaveChangesAsync();

                log.LogWarning($"Запись в БД завершена\n");

                return true;
            }
            catch (Exception e)
            {
                log.LogWarning($"ошибка записи в БД\n");

                throw new Exception(e.Message);
            }
        }

        [HttpGet]
        [Route("CheckName/{printerName}")]
        public bool CheckName(string printerName)
        {
            log.LogWarning("Test");

            DataContext dataContext = new DataContext();

            if (dataContext.Printers.Select(x => x.PrinterName).Contains(printerName))
                return true;
            else
                return false;
        }
    }
}