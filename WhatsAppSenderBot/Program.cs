using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.SignalR;
using System.IO;

//using Microsoft.AspNetCore.SignalR.Client;
using WhatsAppSenderBot;
using WhatsAppServices.Helper;
using WhatsAppServices.Interfaces;
using WhatsAppServices.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddWhatsApp<MyWhatsappEvents>(o =>
{
    o.ScreenshotInterval = 100;
    o.TakeScreenshot = true;

});
builder.Services.AddMvc();
builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseBrowserLink();

    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseEndpoints(endpoints =>
{

    endpoints.MapHub<WhatsappHub>("whatsapp");

});


app.UseAuthorization();

app.MapRazorPages();

app.Run();
public class MyWhatsappEvents : IWhatsAppEventExecutor
{
    private readonly IHubContext<WhatsappHub> hub;
    private readonly IWhatsAppCookies _whatsAppCookies;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MyWhatsappEvents(IHubContext<WhatsappHub> hub, IWhatsAppCookies whatsAppCookies, IHttpContextAccessor httpContextAccessor)
    {
        this.hub = hub;
        this._whatsAppCookies = whatsAppCookies;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task OnAuthenticatedAsync()
    {
       
        await hub.Clients.All.SendAsync("Authenticated");
    }

    public async Task OnAuthenticatingAsync(string qrCodeBase64String)
    {
        await hub.Clients.All.SendAsync("Authenticating", qrCodeBase64String);
    }

    public async Task OnMessagesReceived(UnreadMessage[] messages)
    {

        await hub.Clients.All.SendAsync("MessagesReceived", messages);
    }

    public async Task OnScreenshotReceivedAsync(byte[] uiImage, string folderBase)
    {

        var folderName = Path.Combine("Resources", "Screen");
        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

        string basicFolder = folderBase;// cookies
        var fullPath = Path.Combine(pathToSave, basicFolder);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);

        }
        var filename = Path.Combine(fullPath, "screen.jpg");
        //var filename = @"c:\whatsapp\screen.jpg";
        await File.WriteAllBytesAsync(filename, uiImage);
        await hub.Clients.All.SendAsync("UI");

    }

    public async Task OnStartedAsync()
    {
        await hub.Clients.All.SendAsync("Started");
    }

    public async Task OnStoppedAsync()
    {
        await hub.Clients.All.SendAsync("Stopped");
    }
}
