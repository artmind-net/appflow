using ArtMind.AppFlow;
using ArtMind.AppFlow.Tasks;
using Microsoft.Extensions.Logging;

namespace ArtMind.AppFlow.UseCase
{
    public class HelloWorldWorker : TraceTask
    {
        public HelloWorldWorker(ILogger logger) : base(logger)
        {
        }

        protected override void Execute(IAppContext context)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
