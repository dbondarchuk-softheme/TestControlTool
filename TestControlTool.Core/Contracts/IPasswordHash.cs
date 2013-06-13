namespace TestControlTool.Core.Contracts
{
    /// <summary>
    /// Describes hash provider
    /// </summary>
    public interface IPasswordHash
    {
        /// <summary>
        /// Gets hash from the string
        /// </summary>
        /// <param name="stringToHash">String to hash</param>
        /// <returns>Hash</returns>
        string GetHash(string stringToHash);
    }
}
