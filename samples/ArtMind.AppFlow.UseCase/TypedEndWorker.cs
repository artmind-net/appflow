using ArtMind.AppFlow.Tasks;

namespace ArtMind.AppFlow.UseCase
{
    public class TypedEndWorker : FlowTaskEnd<string>
    {
        public override void Execute(string input)
        {
            input += " str";

            Console.WriteLine($"End: {input}");
        }
    }
}
