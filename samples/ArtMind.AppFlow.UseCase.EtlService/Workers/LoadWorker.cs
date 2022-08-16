using ArtMind.AppFlow.Tasks;
using ArtMind.AppFlow.UseCase.EtlService.Core;
using System.Text;

namespace ArtMind.AppFlow.UseCase.EtlService.Workers
{
    internal class LoadWorker : FlowTaskEnd<IEnumerable<IGrouping<string, IMyData>>>
    {
        private readonly string _outputDir;

        public LoadWorker(AppSettings appSettings)
        {
            _outputDir = Path.Combine(appSettings.SourceDirectory, "output");
        }

        public override void Execute(IEnumerable<IGrouping<string, IMyData>> input)
        {
            // dummy destination
            //foreach (var g in input)
            //{
            //    Console.WriteLine($"group: {g.Key}, numbers: [{string.Join(",", g.Select(x => x.Group).ToList())}]");
            //}

            //Console.WriteLine();

            Directory.CreateDirectory(_outputDir);

            foreach (var g in input)
            {
                var outputStr = new StringBuilder();

                foreach (var item in g)
                {
                    outputStr.AppendLine($"{item.Name}\t{item.Quantity}");
                }

                var outputFileName = Path.Combine(_outputDir, $"{g.Key}.txt");
                File.WriteAllText(outputFileName, outputStr.ToString());
                
                Console.WriteLine($"Writing data to {outputFileName} ...");
            }
        }
    }
}
