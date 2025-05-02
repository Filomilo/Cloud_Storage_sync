using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cloud_Storage_Common
{
    public static class Awaiters
    {
        public static void AwaitTrue(Func<bool> func, long timeout = 1000)
        {
            if (Debugger.IsAttached)
            {
                timeout *= 100;
            }

            bool state = false;
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                state = func();
                if (state == true)
                    break;
                Thread.Sleep(100);
                if (stopwatch.ElapsedMilliseconds > timeout)
                    throw new TimeoutException($"Ensure true timouet {timeout}");
            }
        }

        public static void AwaitNotThrows(Action action, long timeout = 1000)
        {
            if (Debugger.IsAttached)
            {
                timeout *= 100;
            }

            bool state = false;
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                try
                {
                    action.Invoke();
                    break;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(100);
                    if (stopwatch.ElapsedMilliseconds > timeout)
                        throw new TimeoutException($"Ensure true timouet {timeout}");
                }
            }
        }
    }
}
