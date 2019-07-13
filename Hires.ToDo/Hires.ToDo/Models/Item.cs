using System;
using System.ComponentModel;

namespace Hires.ToDo.Models
{
    public class Item : INotifyPropertyChanged
    {
        private DateTime created;
        public DateTime Created
        {
            get { return created; }
            set
            {
                created = value;
                OnPropertyChanged("Created");
            }
        }


        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
