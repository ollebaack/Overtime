using System.ComponentModel;

namespace Overtime
{
    public record WorkedOvertime : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int id;
        public int Id
        {
            get { return id; }
            init { id = value; }
        }

        private double hours;
        public double Hours
        {
            get { return hours; }
            init
            {
                if (hours != value)
                {
                    hours = value;
                    OnPropertyChanged(nameof(Hours));
                }
            }
        }

        private DateTime date;
        public DateTime Date
        {
            get { return date; }
            init
            {
                if (date != value)
                {
                    date = value;
                    OnPropertyChanged(nameof(Date));
                }
            }
        }

        private bool deleted;
        public bool Deleted
        {
            get { return deleted; }
            set
            {
                if (deleted != value)
                {
                    deleted = value;
                    OnPropertyChanged(nameof(Deleted));
                }
            }
        }

        public WorkedOvertime(int id, double hours, DateTime date)
        {
            Id = id;
            Hours = hours;
            Date = date;
        }

        public WorkedOvertime(double hours, DateTime date)
        {
            Hours = hours;
            Date = date;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}