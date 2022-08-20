using ArtMind.AppFlow.Tasks;
using ArtMind.AppFlow.UseCase.EtlService.Core;

namespace ArtMind.AppFlow.UseCase.EtlService.Workers
{
    internal class TransformationWorker : FlowTask<IEnumerable<IMyData>, IEnumerable<IGrouping<string, IMyData>>>
    {
        public override IEnumerable<IGrouping<string, IMyData>> Execute(IEnumerable<IMyData> input)
        {
            return input
                .Where(x => x.Quantity > 0) // magic numbers
                .GroupBy(x => new { x.Group, x.Name })
                .Select(x => MyDataDto.Generate(x.Key.Group, x.Key.Name, x.Sum(x => x.Quantity)))
                .GroupBy(x => x.Group);
        }
    }
}
