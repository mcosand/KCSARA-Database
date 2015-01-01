/*
 * Copyright 2013-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace Kcsara.Database.Web.Services
{
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
