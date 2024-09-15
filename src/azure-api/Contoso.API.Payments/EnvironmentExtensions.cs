using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.API.Payments
{
    // Determine the environment in which the application is running and if it is running in the test runner
    public static class EnvironmentExtensions
    {
        public static bool IsTestEnvironment()
        {
            return System.Diagnostics.Debugger.IsAttached || AppDomain.CurrentDomain.GetAssemblies()
                .Any(assembly => assembly.FullName.StartsWith("Microsoft.VisualStudio.TestPlatform"));
        }

        public static bool IsRunningLocally()
        {
            var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
            return string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);
        }
    }
}
