using System;
using TestControlTool.Core.Contracts;

namespace TestControlTool.Core.Exceptions
{
    /// <summary>
    /// Generated, when such account is already presented in the database
    /// </summary>
    public class AddExistingAccountException : Exception
    {
        /// <summary>
        /// Create's new AddExistingAccountException with mathced message
        /// </summary>
        /// <param name="message">Exception message</param>
        public AddExistingAccountException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create's new AddExistingAccountException with mathced message and inner exception
        /// </summary>
        /// <param name="message">Exception messag</param>
        /// <param name="innerException">Inner exception</param>
        public AddExistingAccountException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Create's new AddExistingAccountException
        /// </summary>
        public AddExistingAccountException()
            : base("Such account is already presented in the database")
        {
        }

        /// <summary>
        /// Create's new AddExistingAccountException for trying to add matched account
        /// </summary>
        /// <param name="account">Account, which was tried to add</param>
        public AddExistingAccountException(IAccount account)
            : base("Account with id = " + account.Id + " or login = " + account.Login + " is already presented in the database")
        {
        }
    }
}
