using System;
using Microsoft.Extensions.Configuration;

namespace ArtMind.AppFlow
{
    public interface IAppFlowBuilder
    {
        IAppFlowBuilder UseAppTask<TAppTask>() where TAppTask : class, IAppTask;

        [Obsolete("This method is obsolete. Call UseAppTask(Action<IAppContext> appTaskAction)", false)]
        IAppFlowBuilder UseAppTask(Func<Action<IAppContext>> appTaskResolver);
        IAppFlowBuilder UseAppTask(Action<IAppContext> appTaskAction);

        //IAppTaskCollection UseSiblingsAppTasks(Func<IAppContext>[] appTasks);

        IAppFlowBuilder UseIfBranch(Predicate<IAppContext> predicate, Action<IAppFlowBuilder> ifBranchFlow, bool createNestedScope = false);
        IAppFlowBuilder UseIfBranch(Predicate<IAppContext> predicate, Action<IAppFlowBuilder> ifBranchFlow, Action<IAppFlowBuilder> elseBranchFlow, bool createNestedScope = false);
        IAppFlowBuilder UseWhileBranch(Predicate<IAppContext> predicate, Action<IAppFlowBuilder> branchFlow, bool createNestedScope = false);

        IAppFlowBuilder UseIfBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppFlowBuilder> ifBranchFlow, bool createNestedScope = false);
        IAppFlowBuilder UseIfBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppFlowBuilder> ifBranchFlow, Action<IConfiguration, IAppFlowBuilder> elseBranchFlow, bool createNestedScope = false);
        IAppFlowBuilder UseWhileBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppFlowBuilder> branchFlow, bool createNestedScope = false);

        IAppFlowBuilder<TResult> UseAppTask<TFlowTask, TResult>() where TFlowTask : class, IAppTask<IAppContext, TResult>;
    }

    public interface IAppFlowBuilder<T>
    {
        IAppFlowBuilder<TResult> UseAppTask<TFlowTask, TResult>() where TFlowTask : class, IAppTask<T, TResult>;
        IAppFlowBuilder UseAppTask<TFlowTask>() where TFlowTask : class, IAppTask<T>;
    }
}
