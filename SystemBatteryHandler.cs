using System.Text;
using System.Management;
using System.Windows.Forms;
using System.Collections.Generic;

namespace BatteryLife
{
    public static class SystemBatteryHandler
    {
        public static float CurrentPercents => SystemInformation.PowerStatus.BatteryLifePercent * 100;

        private static List<PropertyData> _batteryProperties;
        public static List<PropertyData> BatteryProperties => _batteryProperties ?? (_batteryProperties = GetBatteryProperties());

        public static string PropertiesToString
        {
            get
            {
                StringBuilder properties = new StringBuilder();

                foreach (PropertyData property in _batteryProperties)
                {
                    properties.AppendLine($"\t{property.Name} — {property.Value ?? "NULL"}");
                }

                return properties.ToString();
            }
        }

        public static List<PropertyData> GetBatteryProperties()
        {
            List<PropertyData> batteryProperties = new List<PropertyData>();

            ObjectQuery _objectQuery = new ObjectQuery("SELECT * FROM Win32_Battery");
            ManagementObjectSearcher _objectSearcher = new ManagementObjectSearcher(_objectQuery);

            foreach (ManagementBaseObject managementObject in _objectSearcher.Get())
            {
                foreach (PropertyData propertyData in managementObject.Properties)
                {
                    batteryProperties.Add(propertyData);
                }
            }

            return batteryProperties;
        }

        public static void UpdateBatteryProperties() => _batteryProperties = GetBatteryProperties();
    }
}