using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using Ionic.Zip;

namespace TestControlTool.TaskService
{
    class EmailReportService : IReportService
    {
        public void ProcessReport(Guid taskId, string taskName, string ownerName)
        {
            var report = GetZippedReports(taskId, taskName);

            SendEmail(new[] { ownerName }, string.Format("TestControlTool. Report. {0}", DateTime.Now), "Look into the attachments", new[] { report });
        }

        private string GetZippedReports(Guid taskId, string taskName)
        {
            var logsDirectory = new DirectoryInfo(ConfigurationManager.AppSettings["LogsFolder"]);

            var filesToZip = new List<FileInfo>();
            var directoriesToZip = new List<DirectoryInfo>();

            filesToZip.AddRange(logsDirectory.GetFiles(taskId + "*"));
            directoriesToZip.AddRange(logsDirectory.GetDirectories(taskId + "*"));

            var zipFileName = ConfigurationManager.AppSettings["LogsFolder"] + "\\" + taskName + ".zip";

            using (var zip = new ZipFile())
            {
                foreach (var file in filesToZip)
                {
                    try
                    {
                        zip.AddFile(file.FullName, "").FileName = file.Name.Replace(taskId.ToString(), taskName);
                    }
                    catch (Exception)
                    {
                    }
                }

                foreach (var directory in directoriesToZip)
                {
                    try
                    {
                        zip.AddDirectory(directory.FullName, directory.Name.Replace(taskId.ToString(), taskName));
                    }
                    catch (Exception)
                    {
                    }
                }

                zip.Save(zipFileName);
            }

            foreach (var info in filesToZip.Where(x => x.Name != taskId + ".log"))
            {
                try
                {
                    info.Delete();
                }
                catch (Exception)
                {
                }
            }

            foreach (var info in directoriesToZip)
            {
                try
                {
                    info.Delete(true);
                }
                catch (Exception)
                {
                }
            }

            return zipFileName;
        }

        public static void SendEmail(IEnumerable<string> to, string subject, string body, string[] attachmentsFileName)
        {
            
                try
                {
                    using (var mail = new MailMessage())
                    {
                        using (var smtpServer = new SmtpClient(ConfigurationManager.AppSettings["SmtpServer"]))
                        {
                            mail.From = new MailAddress(ConfigurationManager.AppSettings["SendFrom"]);
                            mail.Subject = subject;
                            mail.Body = body;
                            mail.IsBodyHtml = false;

                            foreach (var email in to)
                            {
                                try
                                {
                                    mail.To.Add(email);
                                }
                                catch (Exception)
                                {
                                }
                            }

                            if (attachmentsFileName != null)
                            {
                                foreach (
                                    var attachment in
                                        attachmentsFileName.Select(
                                            attachmentFileName => new Attachment(attachmentFileName)))
                                {
                                    mail.Attachments.Add(attachment);
                                }
                            }

                            smtpServer.Port = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
                            smtpServer.Credentials =
                                new System.Net.NetworkCredential(ConfigurationManager.AppSettings["EmailLogin"],
                                                                 ConfigurationManager.AppSettings["EmailPassword"]);
                            smtpServer.EnableSsl = ConfigurationManager.AppSettings["SmtpSSL"].ToLowerInvariant() ==
                                                   "true";

                            smtpServer.Send(mail);
                        }
                    }
                }
                catch (Exception e)
                {
                    File.WriteAllText(@"D:\email.txt", e.Message);
                }
            
        }
    }
}