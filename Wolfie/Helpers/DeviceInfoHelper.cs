using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Helpers
{
    // Special class to get device information
    public class DeviceInfoHelper
    {
        public  string GetAllDeviceInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Manufacturer: {DeviceInfo.Manufacturer}");
            sb.AppendLine($"Model: {DeviceInfo.Model}");
            sb.AppendLine($"Name: {DeviceInfo.Name}");

            sb.AppendLine($"Platform: {DeviceInfo.Platform}");
            sb.AppendLine($"OS Version: {DeviceInfo.VersionString}");
            sb.AppendLine($"OS Version (full): {DeviceInfo.Version}");

            sb.AppendLine($"Idiom: {DeviceInfo.Idiom}");
            sb.AppendLine($"Device Type: {DeviceInfo.DeviceType}");

            return sb.ToString();
        }

        public  string GetDeviceManufacture()
        {
            return DeviceInfo.Manufacturer.ToString();
        }

        public  string GetDeviceType()
        {
            return DeviceInfo.Platform.ToString();
        }
    }
}
