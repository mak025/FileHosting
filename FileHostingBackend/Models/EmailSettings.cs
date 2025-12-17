namespace FileHostingBackend.Models
{
    public class EmailSettings
    {
		public string UserName { get; set; }
		public string Password { get; set; }
		public string SMTPServer { get; set; }
		public string Sender { get; set; }
		public int Port { get; set; }
    }
}