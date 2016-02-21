using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
public class DecalController : MonoBehaviour {
    Camera mDepthCamera;
    private int cameraRightId;
    private int cameraUpId;
    private int cameraWorldPosId;
    private int cameraNearLeftBottomCornerPointId;
    private int cameraNearSizeId;
    private int cameraToWorldMatrix;

    public GameObject               DebugDrawSphere;
    private CommandBuffer           commandBuf;
    // Use this for initialization
    void Awake()
    {
        mDepthCamera = Camera.main;
        mDepthCamera.depthTextureMode |= DepthTextureMode.Depth;
    }
    void Start()
    {
        CreateMaterialsIfNeeded();
        InitShaderProperties();
        commandBuf = new CommandBuffer();
        commandBuf.name = "draw decal";
        mDepthCamera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuf);
    }
    // Update is called once per frame
    void Update()
    {
        CreateMaterialsIfNeeded();
        DrawDecal();
    }
    private void CreateMaterialsIfNeeded()
    {
        Renderer renderer = GetRenderer();
        if (renderer == null)
        {
            return;
        }
        if (renderer.materials == null)
        {
            renderer.materials = new Material[1];
            renderer.materials[0] = LoadDecalMaterial();
            return;
        }
        bool found = false;
        for (int i = 0; i < renderer.materials.Length; i++)
        {
            if (renderer.materials[i] == null)
            {
                found = true;
                renderer.materials[i] = LoadDecalMaterial();
                break;
            }
            if (renderer.materials[i].name.StartsWith("BranchDemo/Decal"))
            {
                found = true;
                break;
            }
        }
        if (found)
        {
            return;
        }
        Material[] newMats = new Material[renderer.materials.Length + 1];
        renderer.materials.CopyTo(newMats, 0);
        newMats[newMats.Length - 1] = LoadDecalMaterial();
        renderer.materials = newMats;
    }
    private void DrawDecal()
    {
        Renderer renderer = GetRenderer();
        if (renderer == null)
        {
            return;
        }
        if (renderer.sharedMaterials != null)
        {
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                if (renderer.materials[i] != null)
                {
                    if (renderer.materials[i].name.StartsWith("BranchDemo/Decal"))
                    {
                        UpdateShaderProperties(renderer.materials[i]);
                        commandBuf.Clear();
                        commandBuf.DrawRenderer(renderer, renderer.materials[i]);
                        break;
                    }
                }
            }
        }
    }
    private Renderer GetRenderer()
    {
        Renderer renderer = GetComponent<SkinnedMeshRenderer>();
        if (renderer == null)
        {
            renderer = GetComponent<Renderer>();
        }
        return renderer;
    }
    private Material LoadDecalMaterial()
    {
        return new Material(Shader.Find("BranchDemo/Decal"));
    }
    void InitShaderProperties()
    {
        cameraRightId = Shader.PropertyToID("CameraRight");
        cameraUpId = Shader.PropertyToID("CameraUp");
        cameraWorldPosId = Shader.PropertyToID("CameraWorldPos");
        cameraNearLeftBottomCornerPointId = Shader.PropertyToID("CameraNearLeftBottomCornerPoint");
        cameraNearSizeId = Shader.PropertyToID("CameraNearSize");
        cameraToWorldMatrix = Shader.PropertyToID("CameraToWorld");
    }
    void UpdateShaderProperties(Material decalMat)
    {
        Camera camera = Camera.main;
        Vector3 cameraPos = camera.transform.position;
        float fov = camera.fieldOfView;
        //local space xyz
        //          y
        //          |              
        //          |              
        //   camPos ------z
        //          \
        //           \
        //            x
        float z = camera.nearClipPlane;
        float y = z * Mathf.Tan(fov * Mathf.Deg2Rad);
        float x = y * ((float)Screen.width / (float)Screen.height);
        Vector3 cameraNearLeftBottomCornerPoint = Vector3.zero;
        cameraNearLeftBottomCornerPoint = cameraPos + z * camera.transform.forward - x * camera.transform.right - y * camera.transform.up;
        decalMat.SetVector(cameraRightId, camera.transform.right);
        decalMat.SetVector(cameraUpId, camera.transform.up);
        decalMat.SetVector(cameraWorldPosId, camera.transform.position);
        decalMat.SetVector(cameraNearLeftBottomCornerPointId, cameraNearLeftBottomCornerPoint);
        decalMat.SetVector(cameraNearSizeId, new Vector2(x*2,y*2));
        /*
                float2 offset = depthUV * CameraNearSize.xy;
				float3 nearPlanePos = CameraNearLeftBottomCornerPoint.xyz + offset.x * CameraRight.xyz + offset.y * CameraUp.xyz;
        */
        Vector2 offset = new Vector2(0.5f, 0.5f);
        decalMat.SetMatrix(cameraToWorldMatrix, camera.cameraToWorldMatrix);
        //DebugDrawSphere.transform.position = cameraNearLeftBottomCornerPoint+ offset.x * camera.transform.right+ offset.y * camera.transform.up;
    }
}
