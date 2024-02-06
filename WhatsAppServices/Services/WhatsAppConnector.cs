using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsAppServices.Interfaces;
using WhatsAppServices.Models;
using OpenQA.Selenium.Chrome;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OfficeOpenXml;
using Microsoft.AspNetCore.Http;

namespace WhatsAppServices.Services
{
    public class WhatsAppConnector: IWhatsAppConnector
    {
        private ChromeDriver driver;
        private readonly IWhatsAppEventExecutor eventExecutor;
        private readonly WhatsAppOptions options;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WhatsAppConnector(IOptions<WhatsAppOptions> options, IWhatsAppEventExecutor eventExecutor, IHttpContextAccessor httpContextAccessor)
        {

            this.options = options.Value;
            this.eventExecutor = eventExecutor;
            _httpContextAccessor= httpContextAccessor;
        }

        private void OnStopped()
        {
            this.eventExecutor?.OnStoppedAsync();

        }
        private void OnStarted()
        {
            this.eventExecutor?.OnStartedAsync();

        }
        private void OnAuthenticated()
        {
            this.eventExecutor?.OnAuthenticatedAsync();

        }
        private void OnAuthenticating(string qrCodeBase64String)
        {
            this.eventExecutor?.OnAuthenticatingAsync(qrCodeBase64String);

        }
        private void OnUIReceived(byte[] ui,string folderBase)
        {
            this.eventExecutor?.OnScreenshotReceivedAsync(ui, folderBase);
        }
        private void OnMessagesReceived(UnreadMessage[] unreadMessages)
        {
            this.eventExecutor?.OnMessagesReceived(unreadMessages);
        }
        private IWebElement FindElement(ChromeDriver driver, By by)
        {
            try
            {
                return driver.FindElement(by);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }
        private IEnumerable<IWebElement> FindElementsCss(ChromeDriver driver, string css)
        {
            try
            {
                return driver.FindElements(By.CssSelector(css));

            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }
        private Task WaitForMessages(int interval)
        {

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            while (true)
            {
                var mode = (string)js.ExecuteScript(@"return Store.Stream.mode");

                if (mode != "MAIN")
                {

                    break;

                }
                var messagesFromUsers = GetUnreadMessages();
                if (messagesFromUsers.Any())
                {
                    OnMessagesReceived(messagesFromUsers.ToArray());
                }

                Thread.Sleep(interval);
            }

            return Task.CompletedTask;


        }
        private IEnumerable<UnreadMessage> GetUnreadMessages()
        {
            // driver.GetScreenshot().SaveAsFile(@"c:\whatsapp\screen.jpg");
            var javascriptDriver = (IJavaScriptExecutor)this.driver;// as IJavaScriptExecutor;
            var response = javascriptDriver.ExecuteScript(Scripting.GetUnReadMessages);
            var json = JsonConvert.SerializeObject(response);
            JArray messages = JArray.Parse(json);
            var unreadMessages = new List<UnreadMessage>();
            foreach (var message in messages)
            {
                JObject o = JObject.Parse(message.ToString());
                var unread = o.ToObject<UnreadMessage>();
                unreadMessages.Add(unread);
            }
            return unreadMessages;
        }
        private Task StreamUI(int interval)
        {
            var screenCookies = _httpContextAccessor.HttpContext.Request.Cookies["screenId"];
                Task.Run(() =>
                {
                    while (true)
                    {

                        OnUIReceived(driver.GetScreenshot().AsByteArray, screenCookies);
                        Thread.Sleep(interval);
                    }
                });
                    
            return Task.CompletedTask;
        }
        private Task Authenticate()
        {





            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;



            while (true)
            {

                var mode = (string)js.ExecuteScript(@"return Store.Stream.mode");

                if (mode == "MAIN")
                {

                    break;

                }
                else
                {
                    var element = FindElement(driver, By.ClassName("_2EZ_m"));
                    var reload = FindElement(driver, By.ClassName("HnNfm"));
                    if (reload != null)
                    {

                        reload.Click();

                    }
                    else

                    {
                        try
                        {
                            var img = element.FindElement(By.TagName("img"));
                            var qr = img.GetAttribute("src");
                            OnAuthenticating(qr);
                        }
                        catch (Exception ex)
                        {


                        }

                    }
                }
                Thread.Sleep(100);
            }




            return Task.CompletedTask;


        }
        private List<ExcelDto> ConvertExcelData(string path)
        {

            List<ExcelDto> excelData = new List<ExcelDto>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Assuming the data is in the first worksheet

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    var excelD = new ExcelDto
                    {
                        number = worksheet.Cells[row, 1].Value!=null? "+" + worksheet.Cells[row, 1].Value.ToString():null,
                        message = worksheet.Cells[row, 2].Value!=null? worksheet.Cells[row, 2].Value.ToString(): null,
                        filePath = worksheet.Cells[row, 3].Value!=null? worksheet.Cells[row, 3].Value.ToString(): null,
                        // Map other properties accordingly
                    };

                    excelData.Add(excelD);
                }
            }

            return excelData;





        }
             
        public async Task StartAsync()
        {
            var chromeOption = new ChromeOptions();
            if (options.Incognito)
            {
                chromeOption.AddArgument("--incognito");
            }
            if (options.Headless)
            {
                chromeOption.AddArgument("--headless");
            }
            if (options.DisableGPU)
            {
                chromeOption.AddArgument("--disable-gpu");
            }
            if (!string.IsNullOrEmpty(options.UserAgent))
            {
                chromeOption.AddArgument($"--user-agent={options.UserAgent}");
            }

            var driverLocation = AppContext.BaseDirectory;
            if (!string.IsNullOrEmpty(options.ChromeDriverDirectory))
            {
                driverLocation = options.ChromeDriverDirectory;
            }

            driver = new ChromeDriver(driverLocation, chromeOption);

            driver.Navigate().GoToUrl(options.WhatsappUrl);

            if (options.TakeScreenshot)
            {
                StreamUI(options.ScreenshotInterval);
            }

            OnStarted();

            while (true)
            {
                await Authenticate();

                OnAuthenticated();

                await WaitForMessages(options.CheckForNewMessagesInterval);
            }



        }
        public async Task StopAsync()
        {
            this.driver.Quit();

            OnStopped();
            await Task.CompletedTask;
        }
        public async Task SendMessageAsync(string number, string ? message , string ? imagePath ,string ? filePath, string? excelPath)
        {
            if (number == null && string.IsNullOrEmpty(excelPath))
            {
                throw new ArgumentNullException(nameof(number));
            }
            if (message == null && string.IsNullOrEmpty(excelPath))
            {
                throw new ArgumentNullException(nameof(message));
            }
            try
            {
                //-----------------------------Element Xpath from WhatsApp---------------------------------------
                string senderButton = "//button[@class='tvf2evcx oq44ahr5 lb5m6g5c svlsagor p2rjqpw5 epia9gcq']";
                string externalAtachButton = "/html/body/div[1]/div/div[2]/div[4]/div/footer/div[1]/div/span[2]/div/div[1]/div/div/div";
                string imageAttachButton = "//*[@id=\"main\"]/footer/div[1]/div/span[2]/div/div[1]/div/div/span/div/ul/div/div[2]/li/div/input";
                string sendImageButton = "//*[@id=\"app\"]/div/div[2]/div[2]/div[2]/span/div/span/div/div/div[2]/div/div[2]/div[2]/div/div";
                string fileAttachButton = "//*[@id=\"main\"]/footer/div[1]/div/span[2]/div/div[1]/div/div/span/div/ul/div/div[1]/li/div/input";
                string sendAttachButton = "//*[@id=\"app\"]/div/div[2]/div[2]/div[2]/span/div/span/div/div/div[2]/div/div[2]/div[2]/div/div";

                // driver.FindElement(By.CssSelector("body")).SendKeys(Keys.Control + "t");
                // driver.SwitchTo().Window(driver.WindowHandles.Last());
              
                //text message input

                if(!string.IsNullOrEmpty(imagePath))
                {
                    driver.Navigate().GoToUrl($"https://web.whatsapp.com/send?phone=" + number + "&text=" + message);
                    Thread.Sleep(60000);
                    var messageInput = driver.FindElement(By.XPath(senderButton));

                    //attach (+) button
                    var attachInput = driver.FindElement(By.XPath(externalAtachButton));
                    attachInput.SendKeys(Keys.Enter);
                    Thread.Sleep(1000);

                    //image button in attach list
                    var imageInput = driver.FindElement(By.XPath(imageAttachButton));
                    imageInput.SendKeys(imagePath);
                    Thread.Sleep(1000);
                    var imagesender = driver.FindElement(By.XPath(sendImageButton));
                    imagesender.SendKeys(Keys.Enter);
                }
                if (!string.IsNullOrEmpty(filePath))
                {
                    driver.Navigate().GoToUrl($"https://web.whatsapp.com/send?phone=" + number + "&text=" + message);
                    Thread.Sleep(60000);
                    var messageInput = driver.FindElement(By.XPath(senderButton));

                    //attach (+) button
                    var attachInput = driver.FindElement(By.XPath(externalAtachButton));
                    attachInput.SendKeys(Keys.Enter);
                    Thread.Sleep(1000);

                    //image button in attach list
                    var fileInput = driver.FindElement(By.XPath(fileAttachButton));
                    fileInput.SendKeys(filePath);
                    Thread.Sleep(1000);
                    var fileSender = driver.FindElement(By.XPath(sendAttachButton));
                    fileSender.SendKeys(Keys.Enter);
                }
                if(!string.IsNullOrEmpty(excelPath))
                {
                    
                    List<ExcelDto> DataConvt = ConvertExcelData(excelPath);

                    foreach(ExcelDto s in DataConvt)
                    {
                      await  SendMessageAsync(s.number,s.message,null,s.filePath,null);
                    }
                }
                else
                {
                    driver.Navigate().GoToUrl($"https://web.whatsapp.com/send?phone=" + number + "&text=" + message);
                    Thread.Sleep(60000);
                    var messageInput = driver.FindElement(By.XPath(senderButton));

                    messageInput.SendKeys(Keys.Enter);
                }

                await Task.CompletedTask;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

    }
}
