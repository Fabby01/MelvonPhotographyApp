namespace MRMstudios.Models
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPass { get; set; } = string.Empty;
        public string FromAddress { get; set; } = "mel.dimplz@gmail.com";
        public string FromName { get; set; } = "MRMstudios";
        public bool EnableSsl { get; set; } = true;
        public string OwnerEmail { get; set; } = "mel.dimplz@gmail.com";
    }
}
