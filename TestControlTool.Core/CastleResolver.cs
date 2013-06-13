using Castle.Windsor;

namespace TestControlTool.Core
{
    /// <summary>
    /// Resolving dependencies using CastleWindsor
    /// </summary>
    public static class CastleResolver
    {
        private static readonly WindsorContainer Container = new WindsorContainer("castle.config");

        /// <summary>
        /// Resolving dependencie for T type
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <returns>Instance of the resolved type</returns>
        public static T Resolve<T>()
        {
            return Container.Resolve<T>();
        }

        /// <summary>
        /// Resolving dependencie for T type
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <param name="parameters">Parameters for the constructor</param>
        /// <returns>Instance of the resolved type</returns>
        public static T Resolve<T>(object parameters)
        {
            return Container.Resolve<T>(parameters);
        }
    }
}