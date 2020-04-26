using System;

namespace ArtMind.AppFlow.UseCase.Service.Dependencies
{
    public abstract class BaseDependency
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");

        public override string ToString()
        {
            return $"{this.GetType().Name}: {_instanceKey}";
        }
    }
}
