using ICsi.Core;

using McMaster.Extensions.CommandLineUtils;

namespace ICsi
{
    internal static class Program
    {
        private static int Main(string[] args)
            => CommandLineApplication.Execute<Application>(args);
    }
}
