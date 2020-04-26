namespace ArtMind.AppFlow.Abstractions
{
    public interface IAppTask
    {
        void Execute(IAppContext context);
    }
}
