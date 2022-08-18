using ArtMind.AppFlow.Tasks;

namespace ArtMind.AppFlow.UseCase.Service.Workers
{
    public class FlowWorkerEnd<TInput> : FlowTaskEnd<TInput>
    {
        public override void Execute(TInput input)
        {
            throw new System.NotImplementedException();
        }
    }
}
