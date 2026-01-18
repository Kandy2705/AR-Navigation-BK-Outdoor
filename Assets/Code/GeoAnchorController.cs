using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Google.XR.ARCoreExtensions;

public class GeoAnchorController : MonoBehaviour
{
    [Header("References")]
    public ARSession arSession;
    public ARAnchorManager anchorManager;
    public ARCoreExtensions arCoreExtensions;
    public AREarthManager earthManager;
    public GameObject anchorPrefab;

    [Header("Settings")]
    public float waitTimeoutSeconds = 30f;

    private ARGeospatialAnchor currentAnchor;

    void Start()
    {
    }

    public IEnumerator EnsureGeospatialReady()
    {
        float start = Time.time;
        Debug.Log("[Geo] Waiting for Earth Tracking...");

        while (true)
        {
            if (arCoreExtensions == null)
            {
                Debug.LogError("[Geo] ARCoreExtensions not assigned!");
                yield break;
            }

            var earth = earthManager;
            if (earth != null && earth.EarthTrackingState == TrackingState.Tracking)
            {
                Debug.Log("[Geo] Earth is tracking.");
                yield break;
            }

            if (Time.time - start > waitTimeoutSeconds)
            {
                Debug.LogWarning("[Geo] Timeout waiting for Earth Tracking.");
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void PlaceGeoAnchor(double latitude, double longitude, double altitude = 0.0, float headingDegrees = 0f)
    {
        if (arCoreExtensions == null || earthManager == null ||
            earthManager.EarthTrackingState != TrackingState.Tracking)
        {
            Debug.LogWarning("[Geo] Earth not tracking yet. Call EnsureGeospatialReady and wait.");
            return;
        }

        if (currentAnchor != null)
        {
            Destroy(currentAnchor.gameObject);
            currentAnchor = null;
        }

        Quaternion rotation = Quaternion.Euler(0f, headingDegrees, 0f);

        var created = anchorManager.AddAnchor(latitude, longitude, altitude, rotation);

        if (created == null)
        {
            Debug.LogError("[Geo] Failed to create GeoAnchor (null).");
            return;
        }

        ARGeospatialAnchor geoAnchor = created as ARGeospatialAnchor;
        if (geoAnchor == null)
            geoAnchor = created.GetComponent<ARGeospatialAnchor>();

        if (geoAnchor != null)
        {
            currentAnchor = geoAnchor;
            if (anchorPrefab != null)
                Instantiate(anchorPrefab, currentAnchor.transform);
            Debug.Log($"[Geo] GeoAnchor created at lat:{latitude} lon:{longitude}");
        }
        else
        {
            if (anchorPrefab != null)
                Instantiate(anchorPrefab, created.transform);
            Debug.LogWarning("[Geo] Anchor created but not ARGeospatialAnchor; prefab parented to returned anchor.");
        }
    }

    public void RemoveCurrentAnchor()
    {
        if (currentAnchor != null)
        {
            Destroy(currentAnchor.gameObject);
            currentAnchor = null;
        }
    }
}
