using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct PickUp
    {
        public float m_minHeightOffGround;
        public float m_maxHeightOffGround;
        public float m_minDistancebetween;
        public float m_maxDistancebetween;
        public GameObject m_prefab;
    }
    [SerializeField] private PickUp[] m_pickupPrefabs;
    private ProceduralLandscape m_pl;
    private void Start()
    {
        m_pl = GetComponent<ProceduralLandscape>();
        m_pl.Generate();
        SpawnPickUps();
    }
    private void SpawnPickUps()
    {
        List<Vector2> surface = m_pl.SurfacePoints;
        if (surface == null || surface.Count < 2) return;

        // For each pickup type, do its own pass with its own spacing
        foreach (var pickup in m_pickupPrefabs)
        {
            float minDist = Mathf.Max(0f, pickup.m_minDistancebetween);
            float maxDist = Mathf.Max(minDist, pickup.m_maxDistancebetween);

            // Distance along the path since last spawn
            float accumulated = 0f;
            float nextSpawnDist = Random.Range(minDist, maxDist);

            for (int i = 1; i < surface.Count; i++)
            {
                float segmentLength = Vector2.Distance(surface[i - 1], surface[i]);
                accumulated += segmentLength;

                if (accumulated >= nextSpawnDist)
                {
                    // Spawn at this surface point (or you could interpolate between i-1 and i)
                    Vector2 spawnPos = new(surface[i].x, surface[i].y + Random.Range(pickup.m_minHeightOffGround, pickup.m_maxHeightOffGround));
                    Instantiate(pickup.m_prefab, spawnPos, Quaternion.identity);

                    // Reset for next spawn of this pickup type
                    accumulated = 0f;
                    nextSpawnDist = Random.Range(minDist, maxDist);
                }
            }
        }
    }
}