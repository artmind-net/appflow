namespace ArtMind.AppFlow.UseCase.EtlService.Core
{
    internal interface IMyData
    {
        string Group { get; set; }
        string Name { get; set; }
        int Quantity { get; set; }
    }
}