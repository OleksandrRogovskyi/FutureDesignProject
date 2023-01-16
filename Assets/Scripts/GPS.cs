using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class GPS : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI[] description;
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
        //Ask for location
        //if (Permission.HasUserAuthorizedPermission(Permission.FineLocation)) yield return ;

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
            Debug.LogError("location not enabled");
            yield break;
        }
            

        // Starts the location service.
        Input.location.Start();

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Debug.LogError("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location");
            yield break;
        }
        else
        {
            // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
            print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

            description[0].text = "Longitude: " + Input.location.lastData.longitude;
            description[1].text = "Latitude: " + Input.location.lastData.latitude;
            description[2].text = "Altitude: " + Input.location.lastData.altitude;
            description[3].text = "Horizontal: " + Input.location.lastData.horizontalAccuracy;
        }

        // Stops the location service if there is no need to query location updates continuously.
        //Input.location.Stop();
    }
}