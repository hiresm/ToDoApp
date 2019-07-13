using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Hires.ToDo.Helpers
{
    public class ObservableCollectionWithItemNotify<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public ObservableCollectionWithItemNotify()
        {
            this.CollectionChanged += ObservableCollectionItemNotify_CollectionChanged;
        }

        private void ObservableCollectionItemNotify_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e != null)
            {
                if(e.OldItems != null)
                {
                    foreach (INotifyPropertyChanged item in e.OldItems)
                    {
                        item.PropertyChanged -= ObservableCollectionItemNotify_PropertyChanged;
                    }
                }
                if(e.NewItems != null)
                {
                    foreach (INotifyPropertyChanged item in e.NewItems)
                    {
                        item.PropertyChanged += ObservableCollectionItemNotify_PropertyChanged;
                    }
                }
            }
        }

        private void ObservableCollectionItemNotify_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var reset = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            this.OnCollectionChanged(reset);
        }
    }
}
