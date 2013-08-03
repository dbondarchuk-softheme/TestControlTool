namespace TestControlTool.Core.Contracts
{
    /// <summary>
    /// Descibes a child task
    /// </summary>
    public interface IChildTask
    {
        /// <summary>
        /// Name of the task
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Emits when child task got new output data
        /// </summary>
        event OutputDataGot OutputDataGotHandler;

        /// <summary>
        /// Runs the child task
        /// </summary>
        void Run();

        /// <summary>
        /// Stops the child task
        /// </summary>
        void Stop();
    }

    public delegate void OutputDataGot(string output);
}
