using System;
using System.Threading;

namespace Services
{
    //TODO: add thread pool
    public static class ThreadedActionService
    {
        public static void ExecuteAsynchronously(Action asyncJob, Action resultProcessor, Action<Action> enqueueAction)
        {
            Thread workerThread = new Thread(() =>
            {
                asyncJob.Invoke();
                enqueueAction.Invoke(resultProcessor);
            });
            workerThread.Start();
        }
    }
}