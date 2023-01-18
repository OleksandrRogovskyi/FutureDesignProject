using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class GPS : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI[] description;

    private double[] currentLocation;// [0] - lat , [1] - long
    [Tooltip("[0] - latitude, [1] - longitude")]
    [SerializeField]private double[] endLocation;

    private List<GPSCoordinate> area;
    internal void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
    }

    internal void PermissionCallbacks_PermissionGranted(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
    }

    internal void PermissionCallbacks_PermissionDenied(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
    }
    
    IEnumerator Start()
    {
        
        /*currentLocation = new double [2];
        endLocation = new double [2];
        startLocation = new double [2];
        currentLocation[1] = 53.2339;//lat
        currentLocation[0] = 6.557598;//long
        endLocation[1] = 53.2419615;//lat
        endLocation[0] = 6.5320775;//long
        startLocation[1] = 53.2432299;//lat
        startLocation[0] = 6.5319715;//long*/
        
        
       // description[3].text = "Distance between start and end location: " + Distance(endLocation[1],endLocation[0],startLocation[1], startLocation[0]);

        //Ask for location
        if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            description[0].text = "Sotehtin did not work out";
            yield return null ;
        }

        bool useCallbacks = false;
        if (!useCallbacks)
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        else
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
            callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
            callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;
            Permission.RequestUserPermission(Permission.FineLocation, callbacks);
        }
        
        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)
        {
            description[0].text = "location not enabled";
            Debug.LogError("location not enabled");
            yield break;
        }
        
        // Starts the location service.
        Input.location.Start();
        
        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            description[0].text = "Initializing location services...";
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            description[0].text = "Timed out";
            Debug.LogError("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            description[0].text = "Unable to determine device location";
            Debug.LogError("Unable to determine device location");
            yield break;
        }
        else
        {
            // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
            print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

            description[0].text = "Longitude: " + Input.location.lastData.longitude;
            description[1].text = "Latitude: " + Input.location.lastData.latitude;
            description[2].text = "Distance to end location: " + Distance(Input.location.lastData.latitude,Input.location.lastData.longitude,endLocation[0], endLocation[1]);
            //description[2].text = "Altitude: " + Input.location.lastData.altitude;
            //description[3].text = "Horizontal: " + Input.location.lastData.horizontalAccuracy;
        }

        // Stops the location service if there is no need to query location updates continuously.
        //Input.location.Stop();
    }


    private void Update()
    {
        if(Input.location.status != LocationServiceStatus.Failed)
            description[2].text = "Distance to end location: " + Distance(Input.location.lastData.latitude,Input.location.lastData.longitude,endLocation[0], endLocation[1]);

    }

    public static double Distance(double lat1, double lon1, double lat2, double lon2) {
        double R = 6371; // Earth's radius in kilometers
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);
        lat1 = ToRadians(lat1);
        lat2 = ToRadians(lat2);
        double a = Math.Sin(dLat/2) * Math.Sin(dLat/2) +
                   Math.Sin(dLon/2) * Math.Sin(dLon/2) * Math.Cos(lat1) * Math.Cos(lat2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
        double distance = R * c;
        return distance;
    }

    public static double ToRadians(double angle) {
        return (Math.PI / 180) * angle;
    }

   
}
class MainClass {
    public static void Main (string[] args) {
        double lat = 40.730610;
        double lon = -73.935242;
        double radius = 20; // 20 meters

        // Call the GetArea method and store the result in a list of GPSCoordinate
        List<GPSCoordinate> area = GetArea(lat, lon, radius);
        // Iterate through the list and print the latitude and longitude of each coordinate
        foreach (GPSCoordinate coordinate in area) {
            Console.WriteLine("Latitude: " + coordinate.Latitude + ", Longitude: " + coordinate.Longitude);
        }
    }

    public static List<GPSCoordinate> GetArea(double lat, double lon, double radius) {
        // Earth's radius in kilometers
        double R = 6371; 
        // Convert radius to kilometers
        double d = radius / 1000; 
        // Calculate the difference in latitude and longitude
        double dLat = d/R;
        double dLon = d/(R*Math.Cos(Math.PI*lat/180));

        // Calculate the minimum and maximum latitude and longitude
        double minLat = lat - dLat * 180/Math.PI;
        double maxLat = lat + dLat * 180/Math.PI;
        double minLon = lon - dLon * 180/Math.PI;
        double maxLon = lon + dLon * 180/Math.PI;

        // Create a new list to store the coordinates
        List<GPSCoordinate> area = new List<GPSCoordinate>();
        // Iterate through the range of latitude and longitude
        for (double i = minLat; i <= maxLat; i = i + 0.0001) {
            for (double j = minLon; j <= maxLon; j = j + 0.0001) {
                // Add the coordinate to the list
                area.Add(new GPSCoordinate(i, j));
            }
        }
        // Return the list of coordinates
        return area;
    }
}

// Class to represent a GPS coordinate
class GPSCoordinate {
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public GPSCoordinate(double lat, double lon) {
        Latitude = lat;
        Longitude = lon;
    }
}

