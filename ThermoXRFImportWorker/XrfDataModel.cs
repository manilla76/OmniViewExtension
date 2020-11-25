namespace ThermoXRFImportWorker
{
    public class XrfDataModel
    {
        public int SamplePeriod { get; set; } = 3600;
        public string Path { get; set; } = "C:\\XRF\\";
        public string DatapoolIp { get; set; } = "127.0.0.1";
        public string Update { get; set; }
        public string DpGroup { get; set; }
        public string[] Oxides { get; set; } = { "Al2O3", "CaO", "Cl", "Fe2O3", "K2O", "MgO", "Mn2O3", "Na2O", "SiO2", "SO3", "TiO2" };
    }
}