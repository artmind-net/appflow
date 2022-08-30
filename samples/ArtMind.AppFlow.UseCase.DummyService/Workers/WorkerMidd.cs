using ArtMind.AppFlow.Tasks;

namespace ArtMind.AppFlow.UseCase.DummyService.Workers
{
    internal class WorkerMidd : FlowTask<int, string>
    {
        public override string Execute(int input)
        {
            input++;

            Console.WriteLine($"Middle: {input}");
            
            return input.ToString();
        }
    }
}
