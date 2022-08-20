using ArtMind.AppFlow.Tasks;
using ArtMind.AppFlow.UseCase.EtlService.Core;

namespace ArtMind.AppFlow.UseCase.EtlService.Workers
{
    internal class ExtractionWorker : FlowTaskStart<IEnumerable<IMyData>>
    {
        private readonly int _batchSize;
        private readonly string _direcotry;

        public ExtractionWorker(AppSettings appSettings)
        {
            _batchSize = appSettings.BatchSize;
            _direcotry = appSettings.SourceDirectory;
        }
        
        public override IEnumerable<IMyData> Execute(IAppContext context)
        {
            return ReadData(_batchSize);
        }

        private IEnumerable<IMyData> ReadData(int batchSize) // function have to use only arguments, so thath it can be isolded from the worker context.
        {
            // dummy data
            //for (int i = 1; i <= batchSize; i++)
            //{
            //    yield return MyDataDto.Generate(i % 2 == 0 ? "odd" : "even", i.ToString(), i);
            //}

            // read files
            foreach (var file in Directory.GetFiles(_direcotry, "etl*.txt"))
            {
                foreach(var line in File.ReadAllLines(file))
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    var items = line.Split("\t,; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    yield return MyDataDto.Generate(items[0], items[1], int.Parse(items[2]));
                }
            }
        }
    }
}
