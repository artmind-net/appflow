using ArtMind.AppFlow.Tasks;

namespace ArtMind.AppFlow.UseCase.Service.Workers
{
    public class FlowWorkerMiddle<TIn, Tout> : FlowTask<TIn, Tout>
    {
        public override Tout Execute(TIn input)
        {
            throw new System.NotImplementedException();
        }
    }
}
