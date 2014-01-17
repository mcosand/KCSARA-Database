/*
 * Copyright 2012-2014 Matthew Cosand
 */
namespace ExpireNotify
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using Kcsar.Database.Model;

    public class ExpireNotify
    {
        static void Main(string[] args)
        {
            DateTime exp0;
            if (args.Length == 0 || !DateTime.TryParse(args[0], out exp0))
            {
                exp0 = DateTime.Today;
            }

            DateTime exp1 = exp0.AddDays(7);
            DateTime exp2 = exp0.AddMonths(1);
            DateTime exp3 = exp0.AddMonths(2);

            DateTime day0 = exp0.AddDays(1);
            DateTime day1 = exp1.AddDays(1);
            DateTime day2 = exp2.AddDays(1);
            DateTime day3 = exp3.AddDays(1);

            string[] unitConfigs = (ConfigurationManager.AppSettings["UnitsConfig"] ?? "").Split(',');
            ParticipatingUnit[] forUnits = new ParticipatingUnit[unitConfigs.Length];
            for (int i=0; i < unitConfigs.Length; i++)
            {
                var fields = unitConfigs[i].Split(':');
                forUnits[i] = new ParticipatingUnit { UnitId = new Guid(fields[0]), Email = fields[1], NotifyMembers = Boolean.Parse(fields[2]) };
            }

            if (forUnits.Length == 0)
            {
                Console.Error.WriteLine("No units configured");
                return;
            }

            string logFile = "sent-mail.log";
            MailAddress fromEmail = new MailAddress(ConfigurationManager.AppSettings["MailFrom"] ?? "webpage@kcsar.local");


            Dictionary<Guid, List<string>> byUnit = new Dictionary<Guid, List<string>>();
            Dictionary<Guid, List<string>> byMember = new Dictionary<Guid, List<string>>();
            Dictionary<Guid, string> memberEmails = new Dictionary<Guid, string>();

            using (var db = new KcsarContext())
            {
                var courses = db.TrainingCourses.Where(f => f.WacRequired > 0).ToDictionary(f => f.Id, f => new { Name = f.DisplayName, Required = f.WacRequired });

                var targets = db.ComputedTrainingAwards.Include("Member.ContactNumbers").Include("Member.Memberships.Status")
                    .Where(f => f.Course.WacRequired > 0)
                    .Where(f => f.Member.Memberships.Any(g => g.Status.IsActive && g.EndTime == null))
                    .Where(a => (a.Expiry >= exp0 && a.Expiry < day0)
                             || (a.Expiry >= exp1 && a.Expiry < day1)
                             || (a.Expiry >= exp2 && a.Expiry < day2)
                             || (a.Expiry >= exp3 && a.Expiry < day3)
                             )
                    .OrderBy(f => f.Expiry).ThenBy(f => f.Member.LastName).ThenBy(f => f.Member.FirstName).ToArray();

                // For every expiring training
                foreach (var row in targets)
                {
                    // Find all participating unit where this member is active.
                    var memberOfReportUnits = forUnits.Where(f => row.Member.Memberships.Any(g => g.Status.IsActive && g.EndTime == null && g.Unit.Id == f.UnitId));

                    // Pretty name of the expiring course
                    string courseName = courses[row.Course.Id].Name;

                    // For each unit participating in this report and for which this member is currently active:
                    foreach (var unit in memberOfReportUnits)
                    {
                        if (!byUnit.ContainsKey(unit.UnitId))
                        {
                            byUnit.Add(unit.UnitId, new List<string>());
                        }
                        byUnit[unit.UnitId].Add(string.Format("{0}: {1} expires on {2:D}", row.Member.FullName, courseName, row.Expiry));
                    }

                    // If the unit(s) don't want to notify members directly, skip the next step
                    if (memberOfReportUnits.Count(f => f.NotifyMembers) == 0)
                    {
                        continue;
                    }

                    // Add the member's email to the list of tracked emails
                    if (!byMember.ContainsKey(row.Member.Id))
                    {
                        byMember.Add(row.Member.Id, new List<string>());
                        memberEmails.Add(
                            row.Member.Id,
                            row.Member.ContactNumbers
                                .Where(f => f.Type == "email")
                                .OrderBy(f => f.Priority)
                                .Select(f => f.Value)
                                .FirstOrDefault());
                    }

                    byMember[row.Member.Id].Add(string.Format("{0} expires on {1:D}", courseName, row.Expiry));
                }

                SmtpClient client = new SmtpClient();
                foreach (var member in byMember)
                {
                    if (memberEmails[member.Key] == null) continue;

                    MailMessage msg = new MailMessage();
                    msg.To.Add(memberEmails[member.Key]);
                    msg.Subject = "KCSARA Database : Expiring Training";
                    msg.Body = "According to the KCSARA database, your training in the following areas will expire soon:\n\n    "
                        + string.Join("\n    ", member.Value.ToArray())
                        + "\n\nPlease work with your unit's training chair to make sure these trainings are renewed."
                        + " If you have a login for the KCSARA database, you can view your record at https://database.kcsara.org/Members/Detail/" + member.Key.ToString() + ".";
                    msg.From = fromEmail;
                    client.Send(msg);

                    //client.Send(
                    File.AppendAllText(logFile, string.Format("{0:yyyy-MM-dd}\t{1}\r\n", DateTime.Now, memberEmails[member.Key]));
                }

                foreach (var unit in byUnit)
                {
                    string email = forUnits.Single(f => f.UnitId == unit.Key).Email;

                    if (email == "") continue;

                    MailMessage msg = new MailMessage();
                    msg.To.Add(email);
                    msg.Subject = "KCSARA Database : Unit Expiring Training";
                    msg.Body = "According to the KCSARA database, the following members have training that will expire soon:\n\n    " + string.Join("\n    ", unit.Value.ToArray());
                    msg.From = fromEmail;
                    client.Send(msg);

                    File.AppendAllText(logFile, string.Format("{0:yyyy-MM-dd}\t{1}\r\n", DateTime.Now, email));
                }

                MailMessage finishMsg = new MailMessage();
                finishMsg.To.Add("matt@cosand.net");
                finishMsg.Subject = "KCSARA Database expiration report ran successfully";
                finishMsg.Body = "Completed with names\n - " + string.Join("\n - ", byUnit.SelectMany(f => f.Value).ToArray());
                finishMsg.From = fromEmail;
                client.Send(finishMsg);
            }
        }
    }
}
