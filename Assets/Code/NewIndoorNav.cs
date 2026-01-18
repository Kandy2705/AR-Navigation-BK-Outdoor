#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class NewIndoorNav : MonoBehaviour {
    [SerializeField] private Transform player;
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private GameObject trackedImagePrefab;
    [SerializeField] private LineRenderer line;

    private List<NavigationTarget> navigationTargets = new List<NavigationTarget>();
    private NavMeshSurface navMeshSurface;
    private NavMeshPath navMeshPath;
    private GameObject navigationBase;

    private void Start() {
        navMeshPath = new NavMeshPath();
        Debug.Log("when play, activated");

#if UNITY_EDITOR
        // Giả lập ARTrackedImage được "phát hiện"
        SimulateImageDetected();
#endif

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

  private void Update() {
    if (navigationBase != null && navigationTargets.Count > 0 && navMeshSurface != null)
    {
        // Luôn snap lại player và target mỗi frame
        Vector3 playerPos = GetNavMeshPosition(player.position);
        Vector3 targetPos = GetNavMeshPosition(navigationTargets[0].transform.position);
        player.position = GetNavMeshPosition(player.position);

        // Tính path
        NavMesh.CalculatePath(playerPos, targetPos, NavMesh.AllAreas, navMeshPath);

        if (navMeshPath.status == NavMeshPathStatus.PathComplete)
        {
            line.positionCount = navMeshPath.corners.Length;
            line.SetPositions(navMeshPath.corners);
        }
        else
        {
            line.positionCount = 0;
        }

        // Debug.Log($"Path status={navMeshPath.status}, corners={navMeshPath.corners.Length}");

    
        // DebugTargetOnNavMesh(navigationTargets[0].transform);
        // DebugTargetOnNavMesh(player.transform);
    }
}

    private void OnEnable() {
        if (m_TrackedImageManager != null) 
            m_TrackedImageManager.trackedImagesChanged += OnChanged;
    }

    private void OnDisable() {
        if (m_TrackedImageManager != null) 
            m_TrackedImageManager.trackedImagesChanged -= OnChanged;
    }

    private void OnChanged(ARTrackedImagesChangedEventArgs eventArgs) {
        Debug.Log("if on changed activated, this will appeared");
        foreach (var newImage in eventArgs.added) {
            CreateNavigationBase(newImage.transform.position, newImage.transform.rotation);
        }
    }

    private void CreateNavigationBase(Vector3 pos, Quaternion rot) {
        navigationBase = GameObject.Instantiate(trackedImagePrefab, pos, rot);
        navigationTargets.Clear();
        navigationTargets = navigationBase.transform.GetComponentsInChildren<NavigationTarget>().ToList();
        navMeshSurface = navigationBase.transform.GetComponentInChildren<NavMeshSurface>();

        // Build navmesh ngay sau khi spawn
        if (navMeshSurface != null)
            navMeshSurface.BuildNavMesh();

        // Snap tất cả target vào NavMesh
        foreach (var target in navigationTargets)
        {
            Vector3 snapPos = GetNavMeshPosition(target.transform.position);
            target.transform.position = snapPos;
        }
    }


    /// <summary>
    /// Trả về vị trí gần nhất trên NavMesh (nếu không thì giữ nguyên pos)
    /// </summary>
    private Vector3 GetNavMeshPosition(Vector3 pos, float maxDistance = 2f)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(pos, out hit, maxDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return pos;
    }

    private void DebugTargetOnNavMesh(Transform target)
    {
        Vector3 targetPos = target.position;
        NavMeshHit hit;

        bool onNavMesh = NavMesh.SamplePosition(targetPos, out hit, 2.0f, NavMesh.AllAreas);

        if (onNavMesh)
        {
            Debug.Log($"✅ Target {target.name} nằm trên NavMesh. " +
                    $"Target pos: {targetPos}, Closest NavMesh pos: {hit.position}, Distance: {hit.distance}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Target {target.name} KHÔNG nằm trên NavMesh! Pos: {targetPos}");
        }
    }

#if UNITY_EDITOR
    private void SimulateImageDetected()
    {
        Debug.Log("Simulating image detection in Editor...");

        // Fake position trong Editor (vd: cách origin 2 đơn vị)
        Vector3 fakePos = new Vector3(2, 0, 0);
        Quaternion fakeRot = Quaternion.identity;

        // Gọi hàm tạo navigationBase trực tiếp
        CreateNavigationBase(fakePos, fakeRot);
    }
#endif
}
