using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsAppServices.Interfaces
{
    public interface IWhatsAppConnector
    {
        Task SendMessageAsync(string number, string message,string ? imagePath,string ? filePath,string ? excelPath);
        Task StartAsync();
        Task StopAsync();
    }
}
