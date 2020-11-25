using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Thermo.Datapool.Datapool;

namespace OpcDatapoolLibrary
{
    public class ThermoDatapoolService
    {
        private Dictionary<uint, ITagInfo> tags = new Dictionary<uint, ITagInfo>();
        private uint key = 0;
        public void AddTag(string group, string tag, dpTypes type)
        {
            ITagInfo newTag = DatapoolSvr.CreateTagInfo(group, tag, type);
            if (!tags.ContainsValue(newTag))
            {
                tags.Add(key, newTag);               
                tags[key].UpdateValueEvent += NewTag_UpdateValueEvent;
                key++;
            }
        }

        private void NewTag_UpdateValueEvent(ITagInfo e)
        {

            //Update stuff when tag value changes.
        }
    }
}
