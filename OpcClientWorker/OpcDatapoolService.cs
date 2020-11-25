using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace OpcClientWorker
{
    public class OpcDatapoolService
    {        
        public void OpcReadDatapoolWrite(OpcDatapoolModel dataModel)
        {            
            // Read the OPC Server value and write it to the associated datapool tag
            switch (dataModel.OpcValue.Variant.Type)
            {
                case VariantType.Boolean:
                    dataModel.TagInfo.Write((bool)dataModel.OpcValue.Value, dataModel.OpcValue.ServerTimestamp, 0);
                    break;
                case VariantType.Int16:
                    dataModel.TagInfo.Write((int)dataModel.OpcValue.Value, dataModel.OpcValue.ServerTimestamp, 0);
                    break;
                case VariantType.UInt16:
                    dataModel.TagInfo.Write((int)dataModel.OpcValue.Value, dataModel.OpcValue.ServerTimestamp, 0);
                    break;
                case VariantType.Int32:
                    dataModel.TagInfo.Write((int)dataModel.OpcValue.Value, dataModel.OpcValue.ServerTimestamp, 0);
                    break;
                case VariantType.UInt32:
                    dataModel.TagInfo.Write((int)dataModel.OpcValue.Value, dataModel.OpcValue.ServerTimestamp, 0);
                    break;
                case VariantType.Int64:
                    dataModel.TagInfo.Write((int)dataModel.OpcValue.Value, dataModel.OpcValue.ServerTimestamp, 0);
                    break;
                case VariantType.UInt64:
                    dataModel.TagInfo.Write((int)dataModel.OpcValue.Value, dataModel.OpcValue.ServerTimestamp, 0);
                    break;
                case VariantType.Float:
                    dataModel.TagInfo.Write((double)dataModel.OpcValue.Value, dataModel.OpcValue.ServerTimestamp, 0);
                    break;
                case VariantType.Double:
                    dataModel.TagInfo.Write((double)dataModel.OpcValue.Value, dataModel.OpcValue.ServerTimestamp, 0);
                    break;
                case VariantType.String:
                    dataModel.TagInfo.Write((string)dataModel.OpcValue.Value, dataModel.OpcValue.ServerTimestamp, 0);
                    break;
                default:
                    throw new InvalidCastException($"Cannot write {dataModel.OpcValue.Variant.Type.ToString()} data type to Thermo Datapool.");

            }
        }
        public async Task DatapoolReadOpcWriteAsync(OpcDatapoolModel dataModel, UaTcpSessionChannel channel)
        {
            // Read the Thermo tag value and write to the OPC Server
            DataValue value;
            switch (dataModel.TagInfo.Type)
            {
                case Thermo.Datapool.Datapool.dpTypes.FLOAT:
                    value = new DataValue(new Variant(dataModel.TagInfo.AsDouble), sourceTimestamp: DateTime.Now, serverTimestamp: DateTime.Now);
                    break;
                case Thermo.Datapool.Datapool.dpTypes.INT:
                    value = new DataValue(new Variant(dataModel.TagInfo.AsInt), sourceTimestamp: DateTime.Now);
                    break;
                case Thermo.Datapool.Datapool.dpTypes.STRING:
                    value = new DataValue(new Variant(dataModel.TagInfo.AsString), sourceTimestamp: DateTime.Now);
                    break;
                case Thermo.Datapool.Datapool.dpTypes.BOOL:
                    value = new DataValue(new Variant(dataModel.TagInfo.AsBoolean), sourceTimestamp: DateTime.Now);
                    break;
                default:
                    throw new InvalidCastException($"Cannot write {dataModel.TagInfo.Type} data type to OPC.");
                
            }
            var writeRequest = new WriteRequest { NodesToWrite = new WriteValue[] { new WriteValue { NodeId = dataModel.NodeId, Value = value, AttributeId = AttributeIds.Value } } };
            var response = await channel.WriteAsync(writeRequest);
            
        }
    }
}