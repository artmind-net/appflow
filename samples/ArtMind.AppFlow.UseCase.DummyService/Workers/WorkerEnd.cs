using ArtMind.AppFlow.Tasks;

namespace ArtMind.AppFlow.UseCase.DummyService.Workers
{
    internal class WorkerEnd : FlowTaskEnd<string>
    {
        public override void Execute(string input)
        {
            input += " str";

            Console.WriteLine($"End: {input}");
        }
    }
}
