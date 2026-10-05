using UnityEngine;
using UnityEngine.InputSystem;

public class RuntimeTerrainTool : MonoBehaviour
{
    [SerializeField] Camera targetCamera;
    [SerializeField, Min(0.1f)] float brushRadius = 10f;
    [SerializeField, Min(0.1f)] float brushStrength = 10f;
    [SerializeField, Min(1f)] float maxRayDistance = 10000f;

    Terrain pendingTerrain;

    void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        if (!mouse.leftButton.isPressed)
        {
            FinishSculpting();
            return;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            return;
        }

        Ray ray = targetCamera.ScreenPointToRay(mouse.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            return;
        }

        Terrain terrain = hit.collider.GetComponent<Terrain>();
        if (terrain == null || terrain.terrainData == null)
        {
            return;
        }

        if (pendingTerrain != terrain)
        {
            FinishSculpting();
            pendingTerrain = terrain;
        }

        Keyboard keyboard = Keyboard.current;
        bool lowerTerrain = keyboard != null &&
            (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        Sculpt(terrain, hit.point, lowerTerrain);
    }

    void OnDisable()
    {
        FinishSculpting();
    }

    void Sculpt(Terrain terrain, Vector3 hitPoint, bool lowerTerrain)
    {
        TerrainData data = terrain.terrainData;
        int resolution = data.heightmapResolution;
        Vector3 localHit = terrain.transform.InverseTransformPoint(hitPoint);
        Vector3 terrainSize = data.size;
        float radius = Mathf.Max(0.1f, brushRadius);
        int centerX = Mathf.RoundToInt(localHit.x / terrainSize.x * (resolution - 1));
        int centerZ = Mathf.RoundToInt(localHit.z / terrainSize.z * (resolution - 1));
        Vector3 scale = terrain.transform.lossyScale;
        int radiusX = Mathf.CeilToInt(radius / (terrainSize.x * Mathf.Abs(scale.x)) * (resolution - 1));
        int radiusZ = Mathf.CeilToInt(radius / (terrainSize.z * Mathf.Abs(scale.z)) * (resolution - 1));
        int xBase = Mathf.Clamp(centerX - radiusX, 0, resolution - 1);
        int zBase = Mathf.Clamp(centerZ - radiusZ, 0, resolution - 1);
        int xMax = Mathf.Clamp(centerX + radiusX, 0, resolution - 1);
        int zMax = Mathf.Clamp(centerZ + radiusZ, 0, resolution - 1);
        int width = xMax - xBase + 1;
        int height = zMax - zBase + 1;
        float[,] heights = data.GetHeights(xBase, zBase, width, height);
        float direction = lowerTerrain ? -1f : 1f;
        float heightChange = brushStrength * Time.deltaTime / terrainSize.y * direction;

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float sampleX = (xBase + x) / (float)(resolution - 1) * terrainSize.x;
                float sampleZ = (zBase + z) / (float)(resolution - 1) * terrainSize.z;
                Vector3 sampleWorld = terrain.transform.TransformPoint(new Vector3(sampleX, 0f, sampleZ));
                float distance = Vector2.Distance(
                    new Vector2(sampleWorld.x, sampleWorld.z),
                    new Vector2(hitPoint.x, hitPoint.z));
                if (distance > radius)
                {
                    continue;
                }

                float falloff = 1f - distance / radius;
                falloff *= falloff * (3f - 2f * falloff);
                heights[z, x] = Mathf.Clamp01(heights[z, x] + heightChange * falloff);
            }
        }

        data.SetHeightsDelayLOD(xBase, zBase, heights);
        pendingTerrain = terrain;
    }

    void FinishSculpting()
    {
        if (pendingTerrain != null)
        {
            pendingTerrain.terrainData.SyncHeightmap();
            pendingTerrain = null;
        }
    }
}
