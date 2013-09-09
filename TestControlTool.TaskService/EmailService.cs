using System;
using System.IO;
using System.Linq;
using TestControlTool.Core;
using TestControlTool.Core.Contracts;
using TestControlTool.Core.Exceptions;

namespace TestControlTool.TaskService
{
    internal static class EmailService
    {
        private static readonly IAccountController AccountController = CastleResolver.Resolve<IAccountController>();

        public static void SendEmail(string userName, string subject, string message)
        {
            var user = AccountController.Accounts.SingleOrDefault(x => x.Login == userName);

            if (user == null)
            {
                throw new NoSuchAccountException(userName, null);
            }

            EmailReportService.SendEmail(new[] { userName }, subject, message, null);
        }

        public static void SendEmailToAll(string subject, string message)
        {
            var emails = AccountController.Accounts.Select(x => x.Login).ToArray();
            EmailReportService.SendEmail(emails, subject, message, null);
        }
    }
}
