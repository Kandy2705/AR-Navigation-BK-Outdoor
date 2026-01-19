using System.Collections;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;

public class GPSMarker : MonoBehaviour
{
    public GameObject xrOrigin;
    public Camera mainCamera;
    public Transform mapPlane;
    public TextMeshProUGUI gpsText;
    public GameObject targetObject;
    public AlignXROriginToUser alignXROriginToUser;

    private bool manual = true;
    private bool gpsAvailable = false;

    const double a = 6378137.0;
    const double e2 = 6.694380004e-3;

    public double refLat = 10.7736444;
    public double refLon = 106.6593743;
    public double refAlt = 0.0;

    private ECEF refECEF;

    public double lat = 10.7741875;
    public double lon = 106.6606904;
    public double alt = 0.0;


    public bool aligned = false;

    private Vector3 lastUserENU;
    public float alignThreshold = 2.0f;
    public float alignStrength = 2.0f;

    [Header("Mock Movement")]
    public bool useMockMovement = false;
    public float mockSpeed = 1.5f;

    [Header("Heading Align")]
    public bool rotationAligned = false;
    public float headingAlignThreshold = 1.0f;

    [Header("Mock Compass (Editor Test)")]
    public bool useMockCompass = true;
    [Range(0f, 360f)]
    public float mockCompassHeading = 0f;


    void Start()
    {
        StartCoroutine(StartLocationService());

        Input.compass.enabled = true;

        refECEF = LatLonAltToECEF(refLat, refLon, refAlt);
        //marker.transform.localPosition = Vector3.zero;
    }

    IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("GPS không cho phép — sử dụng tọa độ mặc định");
            gpsAvailable = false;
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
            Debug.Log("Không thể xác nhận vị trí — sử dụng tọa độ mặc định");
            gpsAvailable = false;
            yield break;
        }

        // location service is running
        gpsAvailable = true;
        Debug.Log("GPS available");
    }

    public void updateGPS()
    {
        manual = !manual;
        Debug.Log($"GPS Manual changed");
    }

    float GetCurrentHeading()
    {
        if (useMockCompass)
            return mockCompassHeading;

        if (Input.compass.enabled)
            return Input.compass.trueHeading;

        return 0f;
    }


    void AlignMapRotationWithXR(float compassHeading)
    {
        float xrYaw = xrOrigin.transform.rotation.eulerAngles.y;

        float mapYaw = xrYaw - compassHeading;

        mapPlane.rotation = Quaternion.Euler(0f, mapYaw, 0f);

        rotationAligned = true;

        Debug.Log($"[ALIGN ROTATION] XR: {xrYaw:F1} | Compass: {compassHeading:F1} | MapYaw: {mapYaw:F1}");
    }


    Vector3 GetMockUserENU()
    {
        float e = 0f;
        float n = 0f;

        if (Input.GetKey(KeyCode.W)) n += 1f;
        if (Input.GetKey(KeyCode.S)) n -= 1f;
        if (Input.GetKey(KeyCode.D)) e += 1f;
        if (Input.GetKey(KeyCode.A)) e -= 1f;

        Vector3 dir = new Vector3(e, 0f, n);

        if (dir.sqrMagnitude > 1f)
            dir.Normalize();

        return transform.localPosition + dir * mockSpeed * Time.deltaTime;
    }


    //void AlignEnvironmentToXR()
    //{
    //    Vector3 offset = mainCamera.transform.position - transform.position;
    //
    //    offset.y = 0f;
    //
    //    mapPlane.position += offset * alignStrength;
    //}

    void AlignEnvironmentToXR() { 
        Vector3 offset = mainCamera.transform.position - transform.position; 
        offset.y = 0f; 
        mapPlane.position += offset * alignStrength; 
    }

    void Update()
    {

        if (manual)
        {
            Debug.Log($"GPS Manual Mode: {manual}");

            //if (Input.location.status == LocationServiceStatus.Running)
            //{
            //    lat = Input.location.lastData.latitude;
            //    lon = Input.location.lastData.longitude;
            //}

            //if (lat != Input.location.lastData.latitude || lon != Input.location.lastData.longitude)
            //{
            Debug.Log($"Manual GPS Update: Lat {lat}, Lon {lon}");
            if (Input.location.lastData.latitude != 0 && Input.location.lastData.longitude != 0)
            {
                lat = Input.location.lastData.latitude;
                lon = Input.location.lastData.longitude;
            }
            //}

            ECEF pointECEF = LatLonAltToECEF(lat, lon, alt);
            ENU enu = ECEFToENU(pointECEF, refECEF, refLat, refLon);

            Vector3 userENU;
            Vector3 worldPos;

            Debug.Log($"useMock{useMockMovement}");
            if (useMockMovement)
            {
                // Get mock local ENU position and apply it to the marker (marker is local to mapPlane)
                userENU = GetMockUserENU();
                transform.localPosition = userENU;

                // Also shift the map so the user icon on the map lines up with the main camera
                AlignEnvironmentToXR();
            }
            else
            {
                userENU = new Vector3((float)enu.e, 0f, (float)enu.n);
                worldPos = mapPlane.TransformPoint(userENU);
                transform.position = worldPos;

                AlignEnvironmentToXR();
            }

            // nay cap nhat tren dien thoai
            if (!rotationAligned && Input.compass.enabled)
            {
                float heading = Input.compass.trueHeading;

                if (!rotationAligned && heading > headingAlignThreshold)
                {
                    AlignMapRotationWithXR(heading);
                    //if (alignXROriginToUser.aligned) AlignEnvironmentToXR();
                    rotationAligned = true;
                }

            }

            // nay cap nhat tren laptop
            //if (!rotationAligned)
            //{
            //    float heading = GetCurrentHeading();

            //    if (Mathf.Abs(heading) > headingAlignThreshold)
            //    {
            //        AlignMapRotationWithXR(heading);
            //    }
            //}

            if (aligned)
            {
                Debug.Log("userENU: " + userENU);
                Debug.Log("lastuserENU: " + lastUserENU);
                Vector3 deltaENU = userENU - lastUserENU;

                // chỉ align khi GPS thực sự thay đổi đáng kể
                if (deltaENU.magnitude > alignThreshold)
                {
                    if (alignXROriginToUser.aligned) AlignEnvironmentToXR();
                }
            }
            else
            {
                aligned = true;
            }
            lastUserENU = userENU;


        }

        if (gpsText != null)
        {
            float mapPlantHeading = mapPlane.transform.rotation.eulerAngles.y;
            Transform markerTransform = transform;
            gpsText.text =
                $"X: {markerTransform.position.x}" +
                $" Z: {markerTransform.position.z} \n" +
                $"X camera: {mainCamera.transform.position.x}" +
                $" Z camera: {mainCamera.transform.position.z}\n" +
                $"Lat: {lat:F7}" +
                $" Lon: {lon:F7}\n" +
                $"rotationAligned: {rotationAligned}" +
                $" Input.compass.enabled: {Input.compass.enabled}\n" +
                // $"E: {enu.e:F2} m" +
                // $" N: {enu.n:F2} m" +
                // $" U: {enu.u:F2} m\n" +
                $"Huong xoay mapPlant: {mapPlantHeading:F1}°";
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
        enu.u = 0;

        return enu;
    }


}
