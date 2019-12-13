using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDraw : MonoBehaviour
{
	public Camera camera;
	public Shader draw_shader;

	public float brush_size;
	public float brush_strength;

	private Material snow_material;
	private Material draw_material;
	private RenderTexture displacement_map;
    // Start is called before the first frame update
    void Start()
    {
        draw_material = new Material(draw_shader);
		draw_material.SetVector("_DrawColour", Color.red);

		// This should be the tesselation shader's material
		snow_material = GetComponent<MeshRenderer>().material;
		displacement_map = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGBFloat);
		snow_material.SetTexture("_DisplacementMap", displacement_map);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Mouse0))
		{
			RaycastHit hit;
			if(Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit));
			{
				draw_material.SetVector("_TextureCoordinate", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));
				draw_material.SetFloat("_BrushStrength", brush_strength);
				draw_material.SetFloat("_BrushSize", brush_size);
				RenderTexture tmp = RenderTexture.GetTemporary(displacement_map.width, displacement_map.height, 0, RenderTextureFormat.ARGBFloat);
				Graphics.Blit(displacement_map, tmp);
				Graphics.Blit(tmp, displacement_map, draw_material);
				RenderTexture.ReleaseTemporary(tmp);
			}
		}
    }

	private void OnGUI()
	{
		GUI.DrawTexture(new Rect(0, 0, 256, 256), displacement_map, ScaleMode.ScaleToFit, false, 1);
	}
}
