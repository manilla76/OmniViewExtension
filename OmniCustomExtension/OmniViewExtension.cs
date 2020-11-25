using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using ThermoArgonautViewerLibrary;
using static Thermo.Datapool.Datapool;

namespace OmniCustomExtension
{
    public class OmniViewExtension : INotifyPropertyChanged
    {
        private bool enableExtension;
        private CommonSystemViewer.BOSCtrl RAMOS = null;

        public event PropertyChangedEventHandler PropertyChanged;
        private readonly ITagInfo tag;
        private double target = 0;
        private string message;
        public string Message { get => message; set { if (message != value) { message = value; OnPropertyChanged(nameof(Message)); } } }

        public double Target
        {
            get { return target; }
            set
            {
                if (target != value)
                {
                    target = value;
                    OnPropertyChanged();
                }
            }
        }
        public OmniViewExtension()
        {            
            tag = Thermo.Datapool.Datapool.DatapoolSvr.CreateTagInfo("custom", "string", Thermo.Datapool.Datapool.dpTypes.STRING);
            if (!tag.Exists)
            {
                tag.Create();
            }
            tag.UpdateValueEvent += Temp_UpdateValueEvent;
            Message = tag.AsString;
        }


        private void Temp_UpdateValueEvent(Thermo.Datapool.Datapool.ITagInfo e)
        {
            Message = e.AsString;
        }

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
            if (propertyName == nameof(Message))
            {
                tag.Write(Message);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TestRamos()
        {           
            if (RAMOS == null)
            {
                RAMOS = new ThermoArgonautViewerLibrary.CommonSystemViewer.BOSCtrl(null);
            }

            CommonSystemViewer.System = new CommonSystemViewer(null);            
            var b = CommonSystemViewer.System.ArgonautCom.GetRaMOSConfiguration();
            var a = CommonSystemViewer.System.ArgonautCom.GetRaMOSRecipe();
            RAMOS.Configuration = CommonSystemViewer.System.ArgonautCom.GetRaMOSConfiguration();
            var d = a.Items.FirstOrDefault(q => q.QcName == "LSF");
            Target = d.Setpoint;
            d.Setpoint = enableExtension ? d.Setpoint + 1 : d.Setpoint - 1;
            CommonSystemViewer.System.ArgonautCom.SetRecipe(a);
            TestString = d.Setpoint.ToString();
        }

        private void ResetProduct()
        {
            CommonSystemViewer.System = new CommonSystemViewer(null);
            var a = CommonSystemViewer.System.ProductList.ProductList.First((p) => p.ProductId == 15);
            a.CreationDate = new DateTimeOffset(DateTime.Now);
            a.Tons = 0;
        }

    }
}
