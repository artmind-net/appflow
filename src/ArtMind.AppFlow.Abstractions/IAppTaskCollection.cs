using System;
using Microsoft.Extensions.Configuration;

namespace ArtMind.AppFlow
{
    public interface IAppTaskCollection
    {
        IAppTaskCollection UseAppTask<TAppTask>() where TAppTask : class, IAppTask;

        [Obsolete("This method is obsolete. Call UseAppTask(Action<IAppContext> appTaskAction)", false)]
        IAppTaskCollection UseAppTask(Func<Action<IAppContext>> appTaskResolver);
        IAppTaskCollection UseAppTask(Action<IAppContext> appTaskAction);

        //IAppTaskCollection UseSiblingsAppTasks(Func<IAppContext>[] appTasks);

        IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> ifBranchFlow, bool createNestedScope = true);
        IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> ifBranchFlow, Action<IAppTaskCollection> elseBranchFlow, bool createNestedScope = true);
        IAppTaskCollection UseWhileBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> branchFlow, bool createNestedScope = true);

        IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppTaskCollection> ifBranchFlow, bool createNestedScope = true);
        IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppTaskCollection> ifBranchFlow, Action<IConfiguration, IAppTaskCollection> elseBranchFlow, bool createNestedScope = true);
        IAppTaskCollection UseWhileBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppTaskCollection> branchFlow, bool createNestedScope = true);

        IAppTaskCollection<TOut> UseAppTask<TFlowTask, TOut>() where TFlowTask : class, IAppTask<IAppContext, TOut>;
    }

    public interface IAppTaskCollection<TIn>
    {
        IAppTaskCollection<TOut> UseAppTask<TFlowTask, TOut>() where TFlowTask : class, IAppTask<TIn, TOut>;
        IAppTaskCollection UseAppTask<TFlowTask>() where TFlowTask : class, IAppTask<TIn>;
    }
}
