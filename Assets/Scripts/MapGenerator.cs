using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
	public enum MapDrawMode {NoiseMap, ColorMap, Mesh};
	public MapDrawMode DrawMode;

	const int MapChunkSize = 241;
	[Range(0,6)]
	public int LevelOfDetail;
	public float NoiseScale;

	[Range(1,15)]
	public int Octaves;
	[Range(0,1)]
	public float Persistance;
	public float Lacunarity;

	public int Seed;
	public Vector2 Offset;

	public float MeshHeightMultiplier;
	public AnimationCurve MeshHeightCurve;

	public bool AutoUpdate;

	public TerrainType[] regions;

	public void GenerateMap() 
	{
		// Generate noise
		float[,] noiseMap = Noise.GenerateNoiseMap(MapChunkSize, MapChunkSize, Seed, NoiseScale, Octaves, Persistance, Lacunarity, Offset);

		// Assign colors based on height
		Color[] colorMap = new Color[MapChunkSize*MapChunkSize];
		for (int y=0; y<MapChunkSize; ++y)
		{
			for(int x=0; x<MapChunkSize; ++x)
			{
				float currentHeight = noiseMap[x,y];
				for (int i=0; i<regions.Length; ++i)
				{
					if(currentHeight <= regions[i].height)
					{
						colorMap[y * MapChunkSize + x] = regions[i].color;
						break;
					}
				}
			}
		}

		// Render!
		MapDisplay display = FindObjectOfType<MapDisplay>();
		if(DrawMode == MapDrawMode.NoiseMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));	
		}
		else if (DrawMode == MapDrawMode.ColorMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, MapChunkSize, MapChunkSize));
		}
		else if (DrawMode == MapDrawMode.Mesh)
		{
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, MeshHeightMultiplier, MeshHeightCurve, LevelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, MapChunkSize, MapChunkSize));
		}
		
	}

	void OnValidate() 
	{
		if(Lacunarity < 1)
		{
			Lacunarity = 1;
		}
		if(Octaves < 0)
		{
			Octaves = 0;
		}
	}
}

[System.Serializable]
public struct TerrainType 
{
	public string name;
	public float height;
	public Color color;
}