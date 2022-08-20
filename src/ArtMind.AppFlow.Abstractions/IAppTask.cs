using System.Threading.Tasks;

namespace ArtMind.AppFlow
{
    public interface IAppTask
    {
        void Execute(IAppContext context);
    }

    public interface IAppTask<T>
    {
        void Execute(T input);
    }

    public interface IAppTask<T, TResult>
    {
        TResult Execute(T input);
    }

    public interface IAppTaskAsync
    {
        Task ExecuteAsync(IAppContext context);
    }
}
