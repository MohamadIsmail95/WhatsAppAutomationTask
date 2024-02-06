using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsAppServices.Interfaces;

namespace WhatsAppServices.Services
{
    public class WhatsAppHostedService: IHostedService
    {
        //private readonly IHubContext<WhatsappHub> hubcontext;
        private readonly IWhatsAppConnector whatsappConnector;
        // private Whatsapp.WhatsappConnector whatsapp;
        public WhatsAppHostedService(IWhatsAppConnector whatsappConnector)
        {
            this.whatsappConnector = whatsappConnector;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {

            //  whatsappConnector.OnQRCodeChanged += Whatsapp_QRCodeChanged;
            // whatsappConnector.OnMessagesReceived += Whatsapp_ReceivedMessages;
            //  whatsappConnector.OnUIReceived += Whatsapp_OnUIReceived;

            await whatsappConnector.StartAsync();

        }

        

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await whatsappConnector.StopAsync();

        }
    }
}
