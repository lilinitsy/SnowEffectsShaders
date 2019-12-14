using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonAnimatedTrailDraw : MonoBehaviour
{
	public Shader steps_shader;

	public GameObject terrain;
	public Texture2D trail_texture;

	public float brush_size;
	public float brush_strength;

	private Material draw_material;
	private Material terrain_material;
	private RenderTexture displacement_map;
    // Start is called before the first frame update
    void Start()
    {
		draw_material = new Material(steps_shader);
		terrain_material = terrain.GetComponent<MeshRenderer>().material;
		displacement_map = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGBFloat);
		terrain_material.SetTexture("_DisplacementMap", displacement_map);
		draw_material.SetFloat("_BrushStrength", brush_strength);
		draw_material.SetFloat("_BrushSize", brush_size);

    }

    // Update is called once per frame
    void Update()
    {
        int layer_mask = 1 << 8;
		RaycastHit hit;
		if(Physics.Raycast(transform.position, -Vector3.up, out hit, 1.0f, layer_mask))
		{
			draw_material.SetVector("_TextureCoordinate", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));

			RenderTexture tmp = RenderTexture.GetTemporary(displacement_map.width, displacement_map.height, 0, RenderTextureFormat.ARGBFloat);
			Graphics.Blit(displacement_map, tmp);

			float x = hit.textureCoord.x;
			float y = hit.textureCoord.y;

			Rect r = new Rect();
			r.x = trail_texture.width * 0.5f;
			r.y = tmp.height - hit.textureCoord.y - trail_texture.height * 0.5f;
			r.width = trail_texture.width;
			r.height = trail_texture.height;
			Graphics.DrawTexture(r, trail_texture, draw_material);

			//Graphics.Blit(tmp, displacement_map, draw_material);
			RenderTexture.ReleaseTemporary(tmp);
		}
    }
}
