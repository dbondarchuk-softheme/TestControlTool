using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace TestControlTool.UpdateService
{
    /// <summary>
    /// Responses for start and stop services
    /// </summary>
    internal class StartStopServiceJob : IJob
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ServiceController _service;
        
        /// <summary>
        /// Service command
        /// </summary>
        public enum Command
        {
            Start,
            Stop
        }

        /// <summary>
        /// Service command
        /// </summary>
        public Command ServiceCommand { get; set; }

        public StartStopServiceJob(string service)
        {
            _service = new ServiceController(service);
        }

        public StartStopServiceJob(string service, Command command) : this(service)
        {
            ServiceCommand = command;
        }

        /// <summary>
        /// Starts the job
        /// </summary>
        public void Run()
        {
            try
            {
                switch (ServiceCommand)
                {
                    case (Command.Start):

                        Logger.Info("Starting service '" + _service.ServiceName + "'...");

                        if (_service.Status == ServiceControllerStatus.Running)
                        {
                            Logger.Error("Service is already running");

                            break;
                        }

                        _service.Start();
                        _service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 10, 0));

                        Logger.Info("Service has been started");

                        break;

                    case (Command.Stop):

                        Logger.Info("Stopping service '" + _service.ServiceName + "'...");

                        if (_service.Status == ServiceControllerStatus.Stopped)
                        {
                            Logger.Error("Service is already Stopped");

                            break;
                        }

                        _service.Stop();
                        _service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 10, 0));

                        Logger.Info("Service has been stopped");

                        break;
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.ErrorException(ex.Message, ex);
            }
            catch (System.ServiceProcess.TimeoutException ex)
            {
                Logger.ErrorException(ex.Message, ex);
            }
        }
    }
}
