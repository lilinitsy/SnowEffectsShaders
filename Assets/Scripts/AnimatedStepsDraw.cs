﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedStepsDraw : MonoBehaviour
{
	public Shader steps_shader;

	public GameObject terrain;
	public GameObject[] legs; // legs check in front, behind, right, left
	public GameObject[] feet; // feet check in front, behind, right, left, and below.

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
		/*if(Physics.Raycast(transform.position, -Vector3.up, out hit, 1.0f, layer_mask))
		{
			draw_material.SetVector("_TextureCoordinate", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));

			RenderTexture tmp = RenderTexture.GetTemporary(displacement_map.width, displacement_map.height, 0, RenderTextureFormat.ARGBFloat);
			Graphics.Blit(displacement_map, tmp);
			Graphics.Blit(tmp, displacement_map, draw_material);
			RenderTexture.ReleaseTemporary(tmp);
		}*/

		for(int i = 0; i < legs.Length; i++)
		{
			// A raycast won't work cause a plane is 2D... fuck.
			Debug.Log("Leg " + i + " position: " + legs[i].transform.position.ToString("F4"));
			if(	Physics.Raycast(legs[i].transform.position, Vector3.right, out hit, 1.0f, layer_mask) 	|| 
				Physics.Raycast(legs[i].transform.position, -Vector3.right, out hit, 1.0f, layer_mask) 	||
				Physics.Raycast(legs[i].transform.position, Vector3.forward, out hit, 1.0f, layer_mask) ||
				Physics.Raycast(legs[i].transform.position, -Vector3.forward, out hit, 1.0f, layer_mask))
			{
				Debug.Log("LEG HIT");
				draw_material.SetVector("_TextureCoordinate", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));

				RenderTexture tmp = RenderTexture.GetTemporary(displacement_map.width, displacement_map.height, 0, RenderTextureFormat.ARGBFloat);
				Graphics.Blit(displacement_map, tmp);
				Graphics.Blit(tmp, displacement_map, draw_material);
				RenderTexture.ReleaseTemporary(tmp);
			}
		}
    }
}
