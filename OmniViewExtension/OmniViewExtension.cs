using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ThermoArgonautLibrary;
using ThermoArgonautViewerLibrary;
using static ThermoArgonautLibrary.CommonSystem;
using static ThermoArgonautViewerLibrary.CommonSystemViewer;

namespace OmniViewExtension
{
    public class OmniViewExtension : INotifyPropertyChanged
    {
        private bool enableExtension;
        private ServerItemUpdate svrItemUpdate = null;
        private Analysis.AnalysisCement Analysis = null;
        private BOSCtrl RAMOS = null;

        public event PropertyChangedEventHandler PropertyChanged;
        public bool EnableExtension
        {
            get { return enableExtension; }
            set
            {
                if (enableExtension != value)
                {
                    enableExtension = value;
                    OnPropertyChanged();
                    TestRamos();
                }
            }
        }
        private string testString = "abc";

        public string TestString
        {
            get { return testString; }
            set
            {
                if (testString != value)
                {
                    testString = value;
                    OnPropertyChanged();
                    
                }
            }
        }


        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TestRamos()
        {
            if (RAMOS == null)
            {
                RAMOS = new BOSCtrl(null);
            }
            var a = RAMOS.Recipe.Items.FirstOrDefault(q => q.QcName=="LSF");
            testString = a.Setpoint.ToString();
                        
        }
    }
}
