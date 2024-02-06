using Microsoft.AspNetCore.SignalR;
using WhatsAppServices.Interfaces;
using WhatsAppServices.Models;

namespace WhatsAppSenderBot
{
    public class WhatsappHub: Hub
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWhatsAppCookies _whatsAppCookies;

        public WhatsappHub(IHttpContextAccessor httpContextAccessor, IWhatsAppCookies whatsAppCookies)
        {
            _httpContextAccessor = httpContextAccessor;
            _whatsAppCookies= whatsAppCookies;
        }

        public async Task OnAuthenticatedAsync()
        {

            await Clients.All.SendAsync("Authenticated");
        }
        public async Task OnAuthenticatingAsync(string qrCodeBase64String)
        {
            await Clients.All.SendAsync("Authenticating", qrCodeBase64String);
        }

        public async Task OnMessagesReceived(UnreadMessage[] messages)
        {

            await Clients.All.SendAsync("MessagesReceived", messages);
        }

        public async Task OnScreenshotReceivedAsync(byte[] uiImage)
        {
            var folderName = Path.Combine("Resources", "Screen");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            string basicFolder = _whatsAppCookies.CheckWhatsCookies(); // cookies
            var fullPath = Path.Combine(pathToSave, basicFolder);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);

            }
            var filename = Path.Combine(fullPath, "screen.jpg");
            //var filename = @"c:\whatsapp\screen.jpg";
            await File.WriteAllBytesAsync(filename, uiImage);
            await Clients.All.SendAsync("UI");

        }

        public async Task OnStartedAsync()
        {
            await Clients.All.SendAsync("Started");
        }

        public async Task OnStoppedAsync()
        {
            await Clients.All.SendAsync("Stopped");
        }

        public async Task SendMessageAsync(string number,string msg)
        {
            await Clients.All.SendAsync("SendMessageAsync", number, msg);
        }

    }
}
