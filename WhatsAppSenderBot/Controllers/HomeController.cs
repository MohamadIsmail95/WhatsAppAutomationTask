using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;
using OpenQA.Selenium.DevTools.V119.Tethering;
using System.Net.Http.Headers;
using WhatsAppSenderBot.Model;
using WhatsAppServices.Interfaces;

namespace WhatsAppSenderBot.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWhatsAppConnector whatsapp;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IWhatsAppConnector whatsapp, IHttpContextAccessor httpContextAccessor)
        {
            this.whatsapp = whatsapp;
            _httpContextAccessor = httpContextAccessor;
        }
        public  IActionResult Index()
        {
            whatsapp.StartAsync();
            return View();

        }     

        [HttpGet("~/whatsapp/ui")]
        public IActionResult Screenshot()
        {
            try
            {

                var folderName = Path.Combine("Resources", "Screen");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                string cookie = _httpContextAccessor.HttpContext.Request.Cookies["screenId"];
                var fullPath = Path.Combine(pathToSave, cookie);
                var filename = Path.Combine(fullPath, "screen.jpg");

                if (string.IsNullOrEmpty(cookie))
                {
                    return BadRequest();
                }
                var contents = System.IO.File.ReadAllBytes(filename);
                return File(contents, "image/jpg");
            }
            catch(Exception ex)
            {
               return BadRequest(ex);
            }
        }

         [HttpPost("~/whatsapp/send")] 
        public async Task<IActionResult> Send([FromForm] FileSenderDto input)
        {
            try
            {
                              
                IFormFile file = null;
                var folderName = Path.Combine("Resources", "Attachments");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (input.imageFile!=null)
                {
                    file = input.imageFile;
                    if (file.Length > 0)
                    {
                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var fullPath = Path.Combine(pathToSave, fileName);
                        var dbPath = Path.Combine(folderName, fileName);
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        await whatsapp.SendMessageAsync(input.number, input.message, fullPath, null,null);
                    }
                }
                if(input.fileFile!=null)
                {
                    file = input.fileFile;
                    if (file.Length > 0)
                    {
                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var fullPath = Path.Combine(pathToSave, fileName);
                        var dbPath = Path.Combine(folderName, fileName);
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        await whatsapp.SendMessageAsync(input.number, input.message, null, fullPath,null);
                    }
                }
                if (input.excelFile != null)
                {
                    file = input.excelFile;
                    if (file.Length > 0)
                    {
                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var fullPath = Path.Combine(pathToSave, fileName);
                        var dbPath = Path.Combine(folderName, fileName);
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        await whatsapp.SendMessageAsync(input.number, input.message, null, null, fullPath);
                    }
                }
                else
                {
                    await whatsapp.SendMessageAsync(input.number, input.message, null, null, null);

                }
                return Ok();

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        [HttpGet("~/whatsapp/DownloadFile")]
        public IActionResult DownloadFile()
        {
            var folderName = Path.Combine("Resources", "Attachments");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fullPath = Path.Combine(pathToSave, "Excylate.xlsx");
            var fs = new FileStream(fullPath, FileMode.Open); // convert it to a stream
            // Return the file. A byte array can also be used instead of a stream
            return File(fs, "application/octet-stream", "Excylate.xlsx");
        }
    }
}
