## SNOW TRAILS TESSELLATION


This specifically uses a "distance-based tessellation" method, which can be read more about from https://docs.unity3d.com/Manual/SL-SurfaceShaderTessellation.html

![planesetup](images/planesetup.png)
The tessellation shader operates on a Plane in Unity. I tried getting it to work on tessellating other types of geometry, but there were sometimes odd deformations and vertices weren't always connecting, leaving open views into the skybox. The shader takes in a snow texture, ground texture, two ground colours (tints) that are used just for easier visualizing. Deformations in the tessellation shader will interpolate the texture mapping between the two textures and the tints, and vertices will be offset.

### Surface Shader
First, we should discuss the "StepsSnow" Tessellation shader. It resides in Assets/Shaders. Below is what the vertex displacement looks like. The Tessellation example provides this void disp() function that we modify. That float displacement value should be interpreted as the distance to-vertex from the initial collider.

```cpp
void disp(inout appdata v)
{
	float displacement = tex2Dlod(_DisplacementMap, float4(v.texcoord.xy,0,0)).r * _Displacement;
	if(displacement > 0.1)
	{
		v.vertex.xyz -= v.normal * displacement;
	}

	else
	{
		v.vertex.xyz += v.normal * displacement;
	}

	v.vertex.xyz += v.normal * _Displacement;
}
```

That if(displacement > 0.1) looks weird. The if condition is where the shader decides if it will push out vertices, as if snow's bunching up on the edge of the snow trail, or push the vertices down. The 0.1 value itself was just experimentally chosen as a nice number. While I'm not fond of having branches like this in shaders, I didn't care to optimize this portion... it took long enough to get working as it is.

The next important part of the shader is the surf function.

```cpp
void surf(Input IN, inout SurfaceOutputStandard o)
{
	// Albedo comes from a texture tinted by color
	float displacement_val = tex2Dlod(_DisplacementMap, float4(IN.uv_DisplacementMap, 0, 0)).r; // why aren't .b or .g working?
	float4 c;
	if(displacement_val > 0.5)
	{
		c = lerp(tex2D(_SnowTexture, IN.uv_SnowTexture) * _SnowColour, tex2D(_GroundTexture, IN.uv_GroundTexture) * _GroundColour, displacement_val);
	}

	else
	{
		c = lerp(tex2D(_SnowTexture, IN.uv_SnowTexture) * _SnowColour, tex2D(_GroundTexture, IN.uv_GroundTexture) * _GroundColour2, displacement_val);
	}
	o.Albedo = c.rgb;
	// Metallic and smoothness come from slider variables
	o.Metallic = _Metallic;
	o.Smoothness = _Glossiness;
	o.Alpha = c.a;

	//o.Normal = UnpackNormal(tex2D(_DisplacementMap, IN.uv_DisplacementMap)); // this makes everything darker but ihni y

}
```
This displacement_val should look suspiciously familiar from disp. Again, we have a boolean - this time, checking how we're going to colour the segment. If it's farther than a certain distance, it'll end up being groundcolour1, otherwise groundcolour2. I origiinally wrote this hoping it would colour vertices that got pushed up to a certain value, but that overlooked that this was based on distance from the collider that triggers the deformation, so the direction of the vertex's new position doesn't matter.

The plain may look normal with just a shaded view, but its' static view is actually quite intricate if we look at the wireframe! 
![planewireframe](images/planewireframe.png)

### Unlit Shader
The next important shader is the Assets/Shaders/StepsDraw shader. The only important function here is the fixed4 frag() function. It samples the main texture, computes a clamped strength, and returns the colour. Very simple. The code is below.

```cpp
fixed4 frag(v2f i) : SV_Target
{
	// sample the texture
	fixed4 col = tex2D(_MainTex, i.uv);
	float distance_falloff = 1 - distance(i.uv, _TextureCoordinate.xy);
	float clamped_distance_falloff = saturate(distance_falloff);
	float strength = pow(clamped_distance_falloff, 1024 / _BrushSize);
	fixed4 draw_colour = _DrawColour * (strength * _BrushStrength);
	// apply fog
	UNITY_APPLY_FOG(i.fogCoord, col);
	return saturate(col + draw_colour);
}
```

This shader works hand in hand with the "Steps Draw" scripts. There are two - a NonAnimated one, and an Animated one. Both are in Assets/Scripts, and must be placed onto the actor that triggers the deformations.

[PICTURE of ANIMATED SETUP]

Basically this just, for each foot, raycasts downward 1.5 units and will use the Graphics Blit function and a RenderTexture to draw onto the terrain. This pops up frequently in forum topics about it, and while it seems awkward and I wanted to avoid it, it's very ubiquitously suggested for this kind of thing. One of the biggest issues with getting this to work versus the non animated version was to get it set up with Lara's bone structure. Apparently if you attach colliders to gameobjects, such as a leg, the collider won't stay with the leg when the leg's animated. Colliders have to be attached to *bones* since the body part itself is technically not moving, but it took a lot of forum digging to find that out - for a while, it would just deform at their transform.position, which happened to be the same spot for each leg and foot. And we can't just attach the collider to the root bone, since as shown in the picture, that may not be the one we want. In this case, we need the bone closest to the ground, which is not the hip, but the toe.

[PICTURE OF LARA'S LEG + SETUP]

```cpp
for(int i = 0; i < feet.Length; i++)
{
	// A raycast won't work cause a plane is 2D... fuck.
	if(Physics.Raycast(feet[i].transform.position, -Vector3.up, out hit, 1.5f, layer_mask))
	{
		Debug.Log("LEG HIT AT (" + hit.textureCoord.x + " " + hit.textureCoord.y + ")");
		draw_material.SetVector("_TextureCoordinate", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));

		RenderTexture tmp = RenderTexture.GetTemporary(displacement_map.width, displacement_map.height, 0, RenderTextureFormat.ARGBFloat);
		Graphics.Blit(displacement_map, tmp);
		Graphics.Blit(tmp, displacement_map, draw_material);
		RenderTexture.ReleaseTemporary(tmp);
	}
}
```

### Animation Challenges
So, there are some big issues with importing someone elses animated models. There are definitely advantages, such as me not having to go out and learn how to do animations from scratch. Or worse, modeling from scratch. But some of the challenges are that when importing the model, we have this massive animation state machine I don't know how to work with, and a set of long scripts that are meant to handle it while I just want a couple animations.

[PICTURE OF URAIDERS'S STATE MACHINE]

So, I got rid of a lot of things. Here's my state machine.

[PICTURE OF MY STATE MACHINE FOR WHATEVER]

Didn't bother using the scripts. I just made a new Animator for each (2) of the animations I cared to use for demos, and placed them on different characters in different scenes. iT'S A fEaTuRE!! As such, the animations are going to look choppy and sucky. The original animations from the repo linked at the end are phenomenally well done and thorough, I just desecrated them for my purpose.

The only two I bothered to end up using are one that has Lara turning around, albeit in a bit of a jittery fashion - didn't want to go down the rabbithole of figuring out why. I chose this one specifically cause it was one that had her feet moving in places that don't overlap much, so we could see the deformation from each of them clearly. It also has a bit of a sliding motion, as shown in these videos.

This is a slowmo of the sliding motion and the resultant tessellation effects.
https://drive.google.com/file/d/1iIGYc18dlZBmChBBBlgvsYw64775AkHN/view?usp=sharing

Here is the same kind of thing, but with shaded wireframe on.
https://drive.google.com/file/d/1Vf4dqXJ28tHGzmYbgT8Cy_udyjImBzI3/view?usp=sharing

The above two can be replicated by running the LARA-moving scene.

The second one is just a lying-down pose. I picked this one, again, because the feet were far enough apart where it is easy to see the difference in their deformations. This video should show that, and also goes in a little closer to show the raised edges.
https://drive.google.com/file/d/1gf6ocsxUY9VyMkmpIIIvg87zOJDbvqh8/view?usp=sharing
This can be run by opening the LARA-static scene.

This next one shows a cube with a larger brush size - I believe it was 10, and can be seen in the Cube scene.
https://drive.google.com/file/d/14qaTTo4eLoLInnBsypAFlSl4519dfS2x/view?usp=sharing

All the above were using a StepsSnow tessellation value of 4. We can see that the vertices are a bit choppy and jaggedy. Upping the tessellation can alleviate that. This video maxes out the tessellation at 32 (it can go higher, but I have it hardcoded to a cap of 32. It lags a bit to start at 32, so upping it might increase that, although the runtime after the initial lag is fine. It's important to note that there's basically no other game logic going on besides Unity's boilerplate stuff).
https://drive.google.com/file/d/1KIJxNhukKo6-ErNqMRUB2ka-81FeOJ_g/view?usp=sharing

The difference is quite immense.

### Failures
Adding a rigidbody to the Lara model didn't mess with the tessellation, but did produce a nice blooper clip.
https://drive.google.com/file/d/17O_YjMuSYDjbefUZq-MYLPlIy-T_D2VS/view?usp=sharing

Other obvious failings include that the snow getting pushed out if d > 0.1 or whatever is a *total* hack that shouldn't really be valid.
This also doesn't interact with a particle system, which would be something that would *vastly* improve the overall look, probably without being too intensive of an addition in work or compute time. Being able to kick up snow particles would also add a lot without being a massive amount of work, and filling in snowtrails with snow as it falls could be nice too.


### Resources
The Lara model was extracted from here: https://github.com/TiernanWatson/uraider
The snow tessellation done here served as a great example, although they do it in a different way, the results are more pleasant than the way I attempted: https://github.com/wacki/Unity-IndentShader They're drawing the indentation a lot more smoothly. If you look at the DentableSurface.shader, you can see that one of the smart things they did was only offset the vertex.y, and also change the normals. I experimented with how the normals were modified, and the normal offset is how the snow gets kicked up on the edges in this one, as opposed to the way I do it with vertex positions. 
I tried to make some modifications and import them in, but was unsuccessful. The primary edits were in Assets/Shaders/External/DentableSurface.shader, and the fantastic resulting bug is shown here:
https://drive.google.com/file/d/1n-RIRwd_jxdygBQO6qfELt3iHhp0mxe-/view?usp=sharing 

