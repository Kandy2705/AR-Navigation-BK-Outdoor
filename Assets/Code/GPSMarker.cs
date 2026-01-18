using System.Collections;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;

public class GPSMarker : MonoBehaviour
{
    public GameObject xrOrigin;
    public GameObject marker;              // object tượng trưng vị trí người dùng
    public Transform mapPlane;
    public TextMeshProUGUI gpsText;
    public GameObject targetObject;

    private bool manual = true;

    const double a = 6378137.0;
    const double e2 = 6.694380004e-3;

    public double refLat = 10.7736444;
    public double refLon = 106.6593743;
    public double refAlt = 0.0;

    private Vector3 originENU;             // để map ENU về Unity space
    private ECEF refECEF;

    void Start()
    {
        StartCoroutine(StartLocationService());

        Input.compass.enabled = true;

        refECEF = LatLonAltToECEF(refLat, refLon, refAlt);
        originENU = Vector3.zero;
    }

    IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("GPS không cho phép");
            yield break;
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0 || Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Không thể xác nhận vị trí");
            yield break;
        }
    }

    public void updateGPS()
    {
        manual = !manual;
        Debug.Log($"GPS Manual changed");
    }

    void Update()
    {

        double lat = 10.7741875;
        double lon = 106.6606904;
        double alt = 0.0;

        if (manual)
        {
            Debug.Log($"GPS Manual Mode: {manual}");
            if (Input.location.status == LocationServiceStatus.Running)
            {
                lat = Input.location.lastData.latitude;
                lon = Input.location.lastData.longitude;
            }

            ECEF pointECEF = LatLonAltToECEF(lat, lon, alt);
            ENU enu = ECEFToENU(pointECEF, refECEF, refLat, refLon);

            Vector3 localPos = new Vector3((float)enu.e, (float)enu.u, (float)enu.n);
            Vector3 worldPos = mapPlane.TransformPoint(localPos);

            transform.position = worldPos;
            if (marker != null)
            {
                marker.transform.position = worldPos;
            }
 
        }

        if (gpsText != null)
        {
            float xrOriginHeading = xrOrigin.transform.rotation.eulerAngles.y;
            Transform markerTransform = transform;
            gpsText.text =
                $"X: {markerTransform.localPosition.x}" +
                $" Z: {markerTransform.localPosition.z} \n" +
                $"X target: {targetObject.transform.localPosition.x}" +
                $" Z target: {targetObject.transform.localPosition.z}\n" +
                // $"Lat: {lat:F7}" +
                // $" Lon: {lon:F7}\n" +
                // $"E: {enu.e:F2} m" +
                // $" N: {enu.n:F2} m" +
                // $" U: {enu.u:F2} m\n" +
                $"Heading: {Input.compass.trueHeading:F1}°" +
                $"Huong xoay XR: {xrOriginHeading:F1}°";
        }
    }

    public struct ECEF
    {
        public double x, y, z;
    }

    public struct ENU
    {
        public double e, n, u;
    }

    public ECEF GetRefECEF()
    {
        return refECEF;
    }

    public ECEF LatLonAltToECEF(double latDeg, double lonDeg, double alt)
    {
        double lat = latDeg * Mathf.Deg2Rad;
        double lon = lonDeg * Mathf.Deg2Rad;

        double sinLat = Mathf.Sin((float)lat);
        double cosLat = Mathf.Cos((float)lat);
        double cosLon = Mathf.Cos((float)lon);
        double sinLon = Mathf.Sin((float)lon);

        double N = a / System.Math.Sqrt(1.0 - e2 * sinLat * sinLat);

        ECEF result;
        result.x = (N + alt) * cosLat * cosLon;
        result.y = (N + alt) * cosLat * sinLon;
        result.z = (N * (1.0 - e2) + alt) * sinLat;

        return result;
    }

    public ENU ECEFToENU(ECEF point, ECEF refECEF, double refLatDeg, double refLonDeg)
    {
        double refLat = refLatDeg * Mathf.Deg2Rad;
        double refLon = refLonDeg * Mathf.Deg2Rad;

        double dx = point.x - refECEF.x;
        double dy = point.y - refECEF.y;
        double dz = point.z - refECEF.z;

        ENU enu;
        enu.e = -System.Math.Sin(refLon) * dx + System.Math.Cos(refLon) * dy;
        enu.n = -System.Math.Sin(refLat) * System.Math.Cos(refLon) * dx
              - System.Math.Sin(refLat) * System.Math.Sin(refLon) * dy
              + System.Math.Cos(refLat) * dz;
        enu.u = System.Math.Cos(refLat) * System.Math.Cos(refLon) * dx
              + System.Math.Cos(refLat) * System.Math.Sin(refLon) * dy
              + System.Math.Sin(refLat) * dz;

        return enu;
    }


}
