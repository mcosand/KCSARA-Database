using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using log4net;

namespace Sar.Database.Services
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
    private readonly ILog _log;

    public DefaultSendMessageService(IHost host, ILog log)
    {
      _host = host;
      _log = log;
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

      SmtpClient client = BuildEmailClient();
      if (client != null)
      {
        await client.SendMailAsync(email);
      }
    }

    private SmtpClient BuildEmailClient()
    {
      string server = _host.GetConfig("email:server");
      string portText = _host.GetConfig("email:port");
      string username = _host.GetConfig("email:username");
      string password = _host.GetConfig("email:password");

      if (!(string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(portText)))
      {
        int port;
        if (!int.TryParse(portText, out port))
        {
          _log.ErrorFormat("email:port should be an integer. Found {0} instead", portText);
          return null;
        }

        var client = new SmtpClient
        {
          Host = server,
          Port = port,
          DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
        {
          client.Credentials = new NetworkCredential(username, password);
          client.EnableSsl = true;
        }

        return client;
      }

      string dropPath = _host.GetConfig("email:dropPath");
      if (!string.IsNullOrWhiteSpace(dropPath))
      {
        dropPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dropPath));
        if (!Directory.Exists(dropPath))
        {
          _log.ErrorFormat("Can't write email to disk. {0} is not a folder", dropPath);
          return null;
        }
        return new SmtpClient
        {
          DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
          PickupDirectoryLocation = dropPath
        };
      }
      _log.Error("No email configured. Specify email:dropPath with folder relative to website root or email:server, :port, :username, and :password");
      return null;
    }
  }
}
