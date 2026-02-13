using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class ThicknessBackFaceDepth : MonoBehaviour
{
    public Material backFaceDepthMaterial;
    public string globalTextureName = "_ThicknessDepthTexture";
    public RenderTextureFormat format = RenderTextureFormat.RHalf;

    private RenderTexture rt;
    private Camera cam;
    public MeshFilter meshFilter;

    void OnEnable()
    {
        cam = GetComponent<Camera>();

        if (cam != null) cam.depthTextureMode |= DepthTextureMode.Depth;

        rt = new RenderTexture(
            cam.pixelWidth,
            cam.pixelHeight,
            24,
            format
        )
        {
            name = globalTextureName,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        rt.Create();
    }

    void OnDisable()
    {
        if (rt != null) rt.Release();
    }

    void Update()
    {
        if (cam == null || meshFilter == null) return;

        Graphics.SetRenderTarget(rt);
        GL.Clear(true, true, Color.black);

        backFaceDepthMaterial.SetPass(0);
        Graphics.DrawMeshNow(meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix);

        Shader.SetGlobalTexture(globalTextureName, rt);
    }
}
