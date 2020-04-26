namespace ArtMind.AppFlow.UseCase.Service
{
    public class AppSettings
    {
        public int CounterStartsAt { get; set; }

        public static string CounterKey => "counter";
    }
}
