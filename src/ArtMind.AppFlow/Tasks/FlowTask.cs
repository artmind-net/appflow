namespace ArtMind.AppFlow.Tasks
{
    public abstract class FlowTaskStart<TResult> : IAppTask<IAppContext, TResult>
    {
        public abstract TResult Execute(IAppContext context);
    }

    public abstract class FlowTask<T, TResult> : IAppTask<T, TResult>
    {
        public abstract TResult Execute(T input);
    }

    public abstract class FlowTaskEnd<T> : IAppTask<T>
    {
        public abstract void Execute(T input);
    }
}
