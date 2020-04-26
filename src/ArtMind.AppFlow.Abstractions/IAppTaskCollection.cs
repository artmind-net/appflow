using System;

namespace ArtMind.AppFlow.Abstractions
{
    public interface IAppTaskCollection
    {
        IAppTaskCollection UseAppTask<TAppTask>() where TAppTask : IAppTask;

        IAppTaskCollection UseAppTask(Action<IAppContext> appTaskDelegate);

        //IAppTaskCollection UseSiblingsAppTasks(Func<IAppContext>[] appTasks);

        IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> branchFlow, bool createNestedScope = true);

        IAppTaskCollection UseWhileBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> branchFlow, bool createNestedScope = true);
    }
}
