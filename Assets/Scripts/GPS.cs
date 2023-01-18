using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

public class GPS : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onEndLocationReached;
    
    [SerializeField]
    private TextMeshProUGUI[] description;

    private double[] currentLocation;// [0] - lat , [1] - long
    [Tooltip("[0] - latitude, [1] - longitude")]
    [SerializeField]private double[] endLocation;
    [SerializeField]private double[] testLocation;

    [HideInInspector] public bool canUseLocation;
    [SerializeField] private GameObject button;

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
        button.SetActive(false);
        if (onEndLocationReached == null)
            onEndLocationReached = new UnityEvent();

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
            description[0].text = "Location not enabled. Please turn on your location services and restart the app." +
                                  "";
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
            canUseLocation = false;
            description[0].text = "Unable to determine device location";
            Debug.LogError("Unable to determine device location");
            yield break;
        }
        else
        {
            description[0].gameObject.SetActive(false);
            button.SetActive(true);
            /*canUseLocation = true;
            
            description[0].text = "Current location: " + Input.location.lastData.latitude + " , "+ Input.location.lastData.longitude;
            
            _distanceToEnd = CalculateDistance(Input.location.lastData.latitude, Input.location.lastData.longitude,
                endLocation[0], endLocation[1]);
            
            description[1].text = "Distance to end location: " + String.Format("{0:0.00}", _distanceToEnd);*/
            StartCoroutine(UpdateDistance());

            //description[2].text = "Altitude: " + Input.location.lastData.altitude;
            //description[3].text = "Horizontal: " + Input.location.lastData.horizontalAccuracy;
        }

        // Stops the location service if there is no need to query location updates continuously.
        //Input.location.Stop();
    }

    public void CanUseLocation( bool canUse)
    {
        canUseLocation = canUse;
        description[0].gameObject.SetActive(true);
        button.SetActive(false);
    }
    public void ReachedEndLoc()
    {
        description[1].text = "Arrived at end location";
        onEndLocationReached.Invoke();
        locReached = true;
    }
    private double _distanceToEnd;
    private double _distanceToTest;
    private bool locReached = false; 
    /*private void Update()
    {
        if (Input.location.status != LocationServiceStatus.Failed && !locReached && canUseLocation)
        {
            description[0].text = "Current location: " + Input.location.lastData.latitude + " , "+ Input.location.lastData.longitude;

            _distanceToEnd = CalculateDistance(Input.location.lastData.latitude, Input.location.lastData.longitude,
                endLocation[0], endLocation[1]);
            _distanceToTest = CalculateDistance(Input.location.lastData.latitude, Input.location.lastData.longitude,
                testLocation[0], testLocation[1]);
            
            description[1].text = "Distance to end location: " + String.Format("{0:0.000}", _distanceToEnd) + " km";
            description[2].text = "Distance to test location: " + _distanceToTest + " km";

            if (_distanceToEnd <= 0.01f)
            {
                description[2].text = "Arrived at end location";
                onEndLocationReached.Invoke();
                locReached = true;
                //Input.location.Stop();
            }
        }
    }*/

    IEnumerator UpdateDistance()
    {
        yield return new WaitUntil(() => canUseLocation);
        while (Input.location.status != LocationServiceStatus.Failed && !locReached && canUseLocation)
        {
            yield return new WaitForSeconds(0.0001f);
            description[0].text = "Current location: " + Input.location.lastData.latitude + " , "+ Input.location.lastData.longitude;

            _distanceToEnd = CalculateDistance(Input.location.lastData.latitude, Input.location.lastData.longitude,
                endLocation[0], endLocation[1]);
            /*_distanceToTest = CalculateDistance(Input.location.lastData.latitude, Input.location.lastData.longitude,
                testLocation[0], testLocation[1]);*/
            
            description[1].text = "Distance to end location: " + String.Format("{0:0.00}", _distanceToEnd) + " km";

            if (_distanceToEnd <= 0.01f)
            {
                description[1].text = "Arrived at end location";
                onEndLocationReached.Invoke();
                locReached = true;
                //Input.location.Stop();
                
                yield break;
            }
        }

        yield return null;
    }

    public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2) {
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