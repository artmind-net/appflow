namespace ArtMind.AppFlow
{
    public enum CancellationTokenPropagation
    {
        /// <summary>
        /// CancellationToken request is checked before each new cycle initiation.
        /// </summary>
        AtFlowCycle,
        /// <summary>
        /// CancellationToken request is checked before each new cycle initiation and before each flow-task initiation.
        /// </summary>
        InFlowDepth
    }
}
