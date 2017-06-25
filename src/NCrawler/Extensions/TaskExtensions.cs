using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCrawler.Extensions
{
    static class TaskExtensions
    {
        public static Task<T> WithTimeout<T>(this Task<T> task, TimeSpan? timeout)
        {
            if (timeout.HasValue)
            {
                return Task.Factory.StartNew<T>(() =>
                {
                    if (task.Wait(timeout.Value))
                    {
                        return task.Result;
                    }

                    throw new TimeoutException();
                });
            }

            return task;
        }
    }
}
