using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Sar.Services
{
  public interface ISendEmailService
  {
    Task SendEmailAsync(string to, string subject, string message, bool html = true);
    Task SendEmail(IEnumerable<string> to, string subject, string message, bool html = true);
  }
  public interface ISendTextService
  {
    Task SendText(string number, string message);
  }

  public class DefaultSendMessageService : ISendEmailService
  {
    private readonly IHost _host;
    public DefaultSendMessageService(IHost host)
    {
      _host = host;
    }

    public async Task SendEmailAsync(string to, string subject, string message, bool html = true)
    {
      await SendEmail(new [] { to }, subject, message, html);
    }

    public async Task SendEmail(IEnumerable<string> to, string subject, string message, bool html = true)
    {
      var email = new MailMessage
      {
        From = new MailAddress(_host.GetConfig("email:from") ?? "noone@example.com"),
        Subject = subject,
        Body = message,
        IsBodyHtml = html
      };
      foreach (var addr in to)
      {
        email.To.Add(addr);
      }

      SmtpClient client = new SmtpClient();
      await client.SendMailAsync(email);
    }
  }
}
