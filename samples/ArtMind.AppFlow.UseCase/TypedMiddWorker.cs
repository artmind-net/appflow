using ArtMind.AppFlow.Tasks;

namespace ArtMind.AppFlow.UseCase
{
    public class TypedMiddWorker : FlowTask<int, string>
    {
        public override string Execute(int input)
        {
            input++;

            Console.WriteLine($"Middle: {input}");
            
            return input.ToString();
        }
    }
}
