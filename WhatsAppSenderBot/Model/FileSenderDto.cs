namespace WhatsAppSenderBot.Model
{
    public class FileSenderDto
    {
        public string number { get;set; }
        public string message { get; set; }
        public IFormFile? imageFile { get; set; }
        public IFormFile? fileFile { get; set; }
        public IFormFile? excelFile { get; set; }

    }
}
