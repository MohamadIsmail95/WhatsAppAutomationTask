using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsAppServices.Models;

namespace WhatsAppServices.Interfaces
{
    public interface IWhatsAppEventExecutor
    {
        Task OnScreenshotReceivedAsync(byte[] uiImage, string folderbase);
        Task OnMessagesReceived(UnreadMessage[] messages);
        Task OnStartedAsync();
        Task OnStoppedAsync();
        Task OnAuthenticatingAsync(string qrCodeBase64String);
        Task OnAuthenticatedAsync();
    }
}
