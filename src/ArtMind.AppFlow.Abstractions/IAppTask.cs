using System.Threading.Tasks;

namespace ArtMind.AppFlow
{
    public interface IAppTask
    {
        void Execute(IAppContext context);
    }

    public interface IAppTaskAsync
    {
        Task ExecuteAsync(IAppContext context);
    }
}
