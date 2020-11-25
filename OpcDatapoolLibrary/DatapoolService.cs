using System;
using System.Collections.Generic;
using System.Text;
using Thermo.Datapool;

namespace OpcDatapoolLibrary
{
    public class DatapoolService
    {
        public Datapool Datapool { get; } = new Datapool();

        public DatapoolService()
        {
            
        }

        public void Connect(string ipAddress)
        {
            Datapool.IpAddress = ipAddress;
            
        }
    }
}
