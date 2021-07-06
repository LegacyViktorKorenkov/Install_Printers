using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Install_Printers_Lib.Actions;
using Install_Printers_Lib.Models;
using Microsoft.AspNetCore.Mvc;

namespace UploadDriverPage.Controllers
{
    [RequestSizeLimit(1_000_000_000)]
    public class UploadController : Controller
    {
        Use_Install_Printers_Api api;

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(string uploadFile, Printer newDriver)
        {
            MultipartFormDataContent form = new MultipartFormDataContent();

            //form.Add(new StringContent(newDriver.PrinterName), "PrinterName");
            //form.Add(new StringContent(newDriver.NetPrinter.ToString()), "NetPrinter");

            var fileStream = new FileStream(uploadFile, FileMode.Open);

            form.Add(new StreamContent(fileStream), "File");

            api = new Use_Install_Printers_Api(form, newDriver);

            await api.UploadNewDriver();

            return Redirect("~/Upload/Upload");
        }
    }
}
