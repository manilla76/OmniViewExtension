using System.ComponentModel;

namespace OmniViewExtension
{
    public class OmniViewExtension : INotifyPropertyChanged
    {
        private int myVar;
        public event PropertyChangedEventHandler PropertyChanged;
        public int MyProperty
        {
            get { return myVar; }
            set
            {
                if (myVar != value)
                {
                    myVar = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
