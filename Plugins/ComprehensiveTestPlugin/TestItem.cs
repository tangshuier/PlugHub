using System.ComponentModel;

namespace ComprehensiveTestPlugin
{
    public class TestItem : INotifyPropertyChanged
    {
        private string _name;
        private bool _isSelected;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public string TestType { get; set; }

        public TestItem(string name, string testType, bool isSelected = true)
        {
            _name = name;
            TestType = testType;
            _isSelected = isSelected;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}