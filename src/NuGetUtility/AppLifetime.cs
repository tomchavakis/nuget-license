// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

namespace NuGetUtility
{
    /// <summary>
    ///     Inspired by: https://github.com/dotnet/aspnetcore/search?q=WebHostLifetime
    /// </summary>
    public class AppLifetime
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ManualResetEventSlim _doneEvent = new ManualResetEventSlim();

        public AppLifetime()
        {
            AppDomain.CurrentDomain.ProcessExit += ProcessExit;
            Console.CancelKeyPress += CancelKeyPress;
        }

        public CancellationToken Token
        {
            get
            {
                lock (_cts)
                {
                    return _cts.Token;
                }
            }
        }

        private void CancelKeyPress(object? sender, ConsoleCancelEventArgs eventArgs)
        {
            Shutdown();
            // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
            eventArgs.Cancel = true;
        }

        private void ProcessExit(object? sender, EventArgs e)
        {
            Shutdown();
            // no need to override the exit code - it is done when completion is signaled
        }

        private void Shutdown()
        {
            lock (_cts)
            {
                if (!_cts.IsCancellationRequested)
                {
                    _cts.Cancel();
                }
            }

            _doneEvent.Wait();
        }

        public void Done(int result)
        {
            Environment.ExitCode = result;
            _doneEvent.Set();
        }
    }
}
