using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Brite.Win.App.Core.Scripting
{
    public sealed class TaskManager : IDisposable
    {
        private readonly List<Task> _tasks;

        public TaskManager()
        {
            _tasks = new List<Task>();
        }

        public void EndAll()
        {
            lock (_tasks)
                foreach (var task in _tasks)
                    task.Dispose();
        }

        public void Run(Task task)
        {
            try
            {
                lock (_tasks) _tasks.Add(task);
                task.Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                lock (_tasks) _tasks.Remove(task);
            }
        }

        public T Run<T>(Task<T> task)
        {
            try
            {
                lock (_tasks) _tasks.Add(task);
                task.Wait();
                return task.Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                lock (_tasks) _tasks.Remove(task);
            }
        }

        public void Dispose()
        {
            foreach (var task in _tasks)
                task.Dispose();
        }
    }
}
