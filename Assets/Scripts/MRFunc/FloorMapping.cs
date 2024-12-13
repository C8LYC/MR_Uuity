using Meta.XR.MRUtilityKit;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using static FloorMapping;
using static Meta.XR.MRUtilityKit.FindSpawnPositions;

public class FloorMapping : MonoBehaviour
{
	/// <summary>
	/// Defines possible locations where objects can be spawned.
	/// </summary>
	public enum SpawnLocation
	{
		Floating, // Spawn somewhere floating in the free space within the room
		AnySurface, // Spawn on any surface (i.e. a combination of all 3 options below)
		VerticalSurfaces, // Spawn only on vertical surfaces such as walls, windows, wall art, doors, etc...
		OnTopOfSurfaces, // Spawn on surfaces facing upwards such as ground, top of tables, beds, couches, etc...
		HangingDown // Spawn on surfaces facing downwards such as the ceiling
	}
	[FormerlySerializedAs("selectedSnapOption")]
	[SerializeField, Tooltip("Attach content to scene surfaces.")]
	public SpawnLocation SpawnLocations = SpawnLocation.AnySurface;
	[SerializeField, Tooltip("Maximum number of times to attempt spawning/moving an object before giving up.")]
	public int MaxIterations = 1000;
	[SerializeField, Tooltip("When using surface spawning, use this to filter which anchor labels should be included. Eg, spawn only on TABLE or OTHER.")]
	public MRUKAnchor.SceneLabels Labels = ~(MRUKAnchor.SceneLabels) 0;

	public GameObject PlayerObj;
	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	/// <summary>
	/// Current no use!!!!!!!!!!!!    
	/// Starts the spawning process for all rooms.
	/// </summary>
	public void MapToMRFloor()
	{
		Debug.Log("MapToMRFloor");
		foreach (var room in MRUK.Instance.Rooms)
		{
			MapToMRFloor(room);
		}
	}
	
	/// <summary>
	/// Starts the spawning process for all rooms.
	/// </summary>
	public void MapToTable()
	{
		Debug.Log("MapToTable");
		foreach (var room in MRUK.Instance.Rooms)
		{
			MapToTable(room);
			//room.AnchorCreatedEvent.AddListener(TryMapToTable);
		}
	}

	public void MapToTable(MRUKRoom room)
	{
		MRUKAnchor[] RoomAnchors = room.GetComponentsInChildren<MRUKAnchor>();
		MRUKAnchor Best = null;
		if (RoomAnchors.Length > 0)
		{
			float distance = Mathf.Infinity;
			foreach(var anchor in RoomAnchors)
			{
				if(anchor.Label == Labels)
				{
					float CurDistance = Vector3.Distance(PlayerObj.transform.position, anchor.transform.position);
					if(CurDistance < distance)
					{
						distance = CurDistance;
						Best = anchor;
					}
				}
			}
			if(Best == null)
			{
				Debug.LogError("No table");
				return;
			}
			Vector3 TableSize = new Vector3(Best.PlaneBoundary2D[0].x, 0f, Best.PlaneBoundary2D[0].y);
			gameObject.transform.position = Best.transform.position;
			GetComponent<BoxCollider>().size = TableSize;
		}
	}


	/// <summary>
	/// current no use
	/// </summary>
	/// <param name="room"></param>
	public void MapToMRFloor(MRUKRoom room)
    {
		var prefabBounds = Utilities.GetPrefabBounds(gameObject);
		float minRadius = 0.0f;
		const float clearanceDistance = 0.01f;
		float baseOffset = -prefabBounds?.min.y ?? 0.0f;
		float centerOffset = prefabBounds?.center.y ?? 0.0f;

		if (prefabBounds.HasValue)
		{
			minRadius = Mathf.Min(-prefabBounds.Value.min.x, -prefabBounds.Value.min.z, prefabBounds.Value.max.x, prefabBounds.Value.max.z);
			if (minRadius < 0f)
			{
				minRadius = 0f;
			}

			var min = prefabBounds.Value.min;
			var max = prefabBounds.Value.max;
			min.y += clearanceDistance;
			if (max.y < min.y)
			{
				max.y = min.y;
			}
		}

		bool foundValidSpawnPosition = false;
		Vector3 FloorSize = new Vector3(room.FloorAnchor.PlaneBoundary2D[0].x, 0f, room.FloorAnchor.PlaneBoundary2D[0].y);
		gameObject.transform.position = room.FloorAnchor.transform.position;
		//gameObject.transform.rotation = room.FloorAnchor.transform.rotation;
		GetComponent<BoxCollider>().size = FloorSize;

		return;

		for (int j = 0; j < MaxIterations; ++j)
		{
			Vector3 spawnPosition = Vector3.zero;
			Vector3 spawnNormal = Vector3.zero;
			if (SpawnLocations == SpawnLocation.Floating)
			{
				var randomPos = room.GenerateRandomPositionInRoom(minRadius, true);
				if (!randomPos.HasValue)
				{
					break;
				}

				spawnPosition = randomPos.Value;
			}
			else
			{
				MRUK.SurfaceType surfaceType = 0;
				switch (SpawnLocations)
				{
					case SpawnLocation.AnySurface:
						surfaceType |= MRUK.SurfaceType.FACING_UP;
						surfaceType |= MRUK.SurfaceType.VERTICAL;
						surfaceType |= MRUK.SurfaceType.FACING_DOWN;
						break;
					case SpawnLocation.VerticalSurfaces:
						surfaceType |= MRUK.SurfaceType.VERTICAL;
						break;
					case SpawnLocation.OnTopOfSurfaces:
						surfaceType |= MRUK.SurfaceType.FACING_UP;
						break;
					case SpawnLocation.HangingDown:
						surfaceType |= MRUK.SurfaceType.FACING_DOWN;
						break;
				}

				if (room.GenerateRandomPositionOnSurface(surfaceType, minRadius, new LabelFilter(Labels), out var pos, out var normal))
				{
					spawnPosition = pos + normal * baseOffset;
					spawnNormal = normal;
					var center = spawnPosition + normal * centerOffset;
					// In some cases, surfaces may protrude through walls and end up outside the room
					// check to make sure the center of the prefab will spawn inside the room
					if (!room.IsPositionInRoom(center))
					{
						continue;
					}

					// Ensure the center of the prefab will not spawn inside a scene volume
					if (room.IsPositionInSceneVolume(center))
					{
						continue;
					}

					// Also make sure there is nothing close to the surface that would obstruct it
					/*
					if (room.Raycast(new Ray(pos, normal), SurfaceClearanceDistance, out _))
					{
						continue;
					}
					*/
				}
				room.FloorAnchor.TryGetComponent(out MRUKAnchor anchor);
			}

			Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, spawnNormal);

			foundValidSpawnPosition = true;

			if (gameObject.scene.path == null)
			{
				Instantiate(gameObject, spawnPosition, spawnRotation, transform);
			}
			else
			{
				gameObject.transform.position = spawnPosition;
				gameObject.transform.rotation = spawnRotation;
				GetComponent<BoxCollider>().size = FloorSize;
				return; // ignore SpawnAmount once we have a successful move of existing object in the scene
			}

			break;
		}

		if (!foundValidSpawnPosition)
		{
			Debug.LogWarning($"Failed to find valid spawn position after {MaxIterations} iterations.");
		}
		
	}
}
