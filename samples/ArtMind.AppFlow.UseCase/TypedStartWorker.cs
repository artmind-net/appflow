using ArtMind.AppFlow.Tasks;

namespace ArtMind.AppFlow.UseCase
{
    public class TypedStartWorker : FlowTaskStart<int>
    {
        public override int Execute(IAppContext context)
        {
            var startValue = 10;

            Console.WriteLine($"Start: {10}");

            return startValue;
        }
    }
}
