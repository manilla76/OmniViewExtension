using OpcDatapoolLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using Thermo.Datapool;
using Workstation.ServiceModel.Ua;

namespace OpcClientWorker
{
    public class OpcDatapoolModel
    {
        public Datapool.ITagInfo TagInfo { get; set; }
        public DataValue OpcValue { get; set; }
        public NodeId NodeId { get; set; }
        public DatapoolService Datapool { get; set; }
    }
}
