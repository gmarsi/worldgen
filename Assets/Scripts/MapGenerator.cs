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

	public GameObject TreePrefab;
	public float TreeRadius = 1;
	public int TreePrecision = 30;
	public float TreeMinHeight;
	public float TreeMaxHeight;
	List<GameObject> trees;
	List<Vector2> treePoints;

	public bool AutoUpdate;

	public TerrainType[] regions;

	float topLeftX = (MapChunkSize-1)/-2f;
	float topLeftZ = (MapChunkSize-1)/2f;

	public void GenerateMap(bool generateTrees) 
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
			GameObject.Find("MapGenerator").transform.Find("Plane").gameObject.SetActive(true);
			GameObject.Find("MapGenerator").transform.Find("Mesh").gameObject.SetActive(false);
		}
		else if (DrawMode == MapDrawMode.ColorMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, MapChunkSize, MapChunkSize));
			GameObject.Find("MapGenerator").transform.Find("Plane").gameObject.SetActive(true);
			GameObject.Find("MapGenerator").transform.Find("Mesh").gameObject.SetActive(false);
		}
		else if (DrawMode == MapDrawMode.Mesh)
		{
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, MeshHeightMultiplier, MeshHeightCurve, LevelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, MapChunkSize, MapChunkSize));
			GameObject.Find("MapGenerator").transform.Find("Plane").gameObject.SetActive(false);
			GameObject.Find("MapGenerator").transform.Find("Mesh").gameObject.SetActive(true);
		}
		
		if(trees == null)
		{
			trees = new List<GameObject>();
		}

		// Generate trees
		foreach(GameObject tree in trees)
		{
			DestroyImmediate(tree);
		}
		if(generateTrees)
		{
			treePoints = PoissonDiscSampling.GeneratePoints(TreeRadius, new Vector2(MapChunkSize, MapChunkSize), TreePrecision, noiseMap, TreeMinHeight, TreeMaxHeight);
			foreach(Vector2 point in treePoints)
			{
				trees.Add(Instantiate(TreePrefab, new Vector3(topLeftX + point.x, MeshHeightCurve.Evaluate(noiseMap[(int)point.x,(int)point.y]) * MeshHeightMultiplier, topLeftZ - point.y), Quaternion.identity));
			}
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