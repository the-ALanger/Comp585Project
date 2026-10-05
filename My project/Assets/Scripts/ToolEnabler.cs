using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RuntimeTerrainTool))]
public class ToolEnabler : MonoBehaviour
{
    RuntimeTerrainTool terrainTool;

    void Awake()
    {
        terrainTool = GetComponent<RuntimeTerrainTool>();
        terrainTool.enabled = false;
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.tKey.wasPressedThisFrame)
        {
            terrainTool.enabled = !terrainTool.enabled;
        }
    }
}
