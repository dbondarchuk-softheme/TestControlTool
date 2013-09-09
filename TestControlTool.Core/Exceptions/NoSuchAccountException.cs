using System;

namespace TestControlTool.Core.Exceptions
{
    public class NoSuchAccountException : Exception
    {
        /// <summary>
        /// Create's new NoSuchAccountException with matched message
        /// </summary>
        /// <param name="message">Exception message</param>
        public NoSuchAccountException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create's new NoSuchAccountException for such account ID
        /// </summary>
        /// <param name="accountId">Account id to search</param>
        public NoSuchAccountException(Guid accountId)
            : base("No account with id = '" + accountId + "' was found in the database")
        {
        }

        /// <summary>
        /// Create's new NoSuchAccountException for such account login
        /// </summary>
        /// <param name="login">Login to search</param>
        /// <param name="accountId">Account id to search</param>
        public NoSuchAccountException(string login, Guid? accountId)
            : base("No account with login = '" + login + (accountId != null ? " and/or id = '" + accountId + "'" : "") + "' was found in the database")
        {
        }
    }
}
