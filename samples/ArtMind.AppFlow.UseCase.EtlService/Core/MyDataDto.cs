namespace ArtMind.AppFlow.UseCase.EtlService.Core
{
    internal class MyDataDto : IMyData
    {
        public string Group { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public static IMyData Generate(string group, string name, int quantity)
        {
            return new MyDataDto
            {
                Group = group, Name = name, Quantity = quantity
            };
        }
    }
}
