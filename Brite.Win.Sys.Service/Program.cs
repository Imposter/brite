using System.ServiceProcess;

namespace Brite.Win.Sys.Service
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            ServiceBase.Run(new ServiceBase[]
            {
                new BriteService()
            });
        }
    }
}
