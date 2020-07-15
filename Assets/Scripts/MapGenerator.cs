using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
	public enum MapDrawMode {NoiseMap, ColorMap};
	public MapDrawMode DrawMode;

	public int MapWidth;
	public int MapHeight;
	public float NoiseScale;

	public int Octaves;
	[Range(0,1)]
	public float Persistance;
	public float Lacunarity;

	public int Seed;
	public Vector2 Offset;

	public bool AutoUpdate;

	public TerrainType[] regions;

	public void GenerateMap() 
	{
		// Generate noise
		float[,] noiseMap = Noise.GenerateNoiseMap(MapWidth, MapHeight, Seed, NoiseScale, Octaves, Persistance, Lacunarity, Offset);

		// Assign colors based on height
		Color[] colorMap = new Color[MapWidth*MapHeight];
		for (int y=0; y<MapHeight; ++y)
		{
			for(int x=0; x<MapWidth; ++x)
			{
				float currentHeight = noiseMap[x,y];
				for (int i=0; i<regions.Length; ++i)
				{
					if(currentHeight <= regions[i].height)
					{
						colorMap[y * MapWidth + x] = regions[i].color;
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
			display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, MapWidth, MapHeight));
		}
		
	}

	void OnValidate() 
	{
		if(MapWidth < 1)
		{
			MapWidth = 1;
		}
		if(MapHeight < 1)
		{
			MapHeight = 1;
		}
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