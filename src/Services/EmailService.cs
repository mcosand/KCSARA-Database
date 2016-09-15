namespace Kcsara.Database.Web.Services
{
  using System.Configuration;
  using System.Net.Mail;

  public static class EmailService
  {
    public static void SendMail(string toAddresses, string subject, string body)
    {
      MailMessage msg = new MailMessage();
      msg.To.Add(toAddresses);
      msg.Subject = subject;
      msg.Body = body;

      SendMail(msg);
    }

    public static void SendMail(MailMessage msg)
    {
      SmtpClient client = new SmtpClient();
      msg.From = new MailAddress(ConfigurationManager.AppSettings["MailFrom"] ?? "webpage@kcsar.local");
      client.Send(msg);
    }
  }
}
