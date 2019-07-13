using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Hires.ToDo.Helpers
{
    public sealed class NotifyTaskCompletion<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Task<T> Task { get; private set; }
        public T Result
        {
            get
            {
                return (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : default(T);
            }
        }

        public NotifyTaskCompletion(Task<T> task)
        {
            Task = task;

            if(!task.IsCompleted)
            {
                var _ = WatchTaskAsync(task);
            }
        }

        private async Task WatchTaskAsync(Task<T> task)
        {
            try
            {
                await task;
            }
            catch (Exception)
            {

            }

            var propertyChanged = PropertyChanged;

            if(!task.IsFaulted & !task.IsCanceled)
            {
                propertyChanged(this, new PropertyChangedEventArgs("Result"));
            }
        }
    }
}
