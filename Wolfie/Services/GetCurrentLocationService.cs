using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Services
{
    public class GetCurrentLocationService
    {
        public async Task<Location?> GetCurrentLocationAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                    {
                        // Handle the case where the user denies permission
                        Console.WriteLine("Location permission denied.");
                        return null;
                    }
                }
                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium , TimeSpan.FromSeconds(10));
                Location location = await Geolocation.GetLocationAsync(request);

                return location;
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
                Console.WriteLine($"Feature not supported: {fnsEx.Message}");
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle location services not enabled exception
                Console.WriteLine($"Feature not enabled: {fneEx.Message}");
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                Console.WriteLine($"Permission error: {pEx.Message}");
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return null;
        }

    }
}
