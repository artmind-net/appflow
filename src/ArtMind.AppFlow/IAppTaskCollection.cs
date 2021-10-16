﻿using System;

namespace ArtMind.AppFlow
{
    public interface IAppTaskCollection
    {
        IAppTaskCollection UseAppTask<TAppTask>() where TAppTask : IAppTask;

        IAppTaskCollection UseAppTask(Func<Action<IAppContext>> appTaskResolver);

        //IAppTaskCollection UseSiblingsAppTasks(Func<IAppContext>[] appTasks);

        IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> ifBranchFlow, bool createNestedScope = false);
        IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> ifBranchFlow, Action<IAppTaskCollection> elseBranchFlow, bool createNestedScope = false);

        IAppTaskCollection UseWhileBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> branchFlow, bool createNestedScope = false);
    }
}
