namespace ArtMind.AppFlow.UseCase
{
    public class HelloWorldWorker : IAppTask
    {
        public void Execute(IAppContext context)
        {
            Console.WriteLine("Hello World!");

            Task.Delay(1000).Wait();
        }
    }
}
