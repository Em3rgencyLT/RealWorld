using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Domain;

public class MapObjectPlacementManager : Singleton<MapObjectPlacementManager>
{
    protected MapObjectPlacementManager() { }

    [SerializeField]
    private double originLatitude = 55.237884;
    [SerializeField]
    private double originLongitude = 22.276198;
    private Coordinates projectionOrigin;
    [SerializeField]
    private double offset = 0.03;

    private Coordinates bottomCoordinates;
    private Coordinates topCoordinates;
    private Dictionary<MapElement.ID, MapElement> worldData;
    
    public GameObject structurePrefab;
    private GameObject structureParentObject;

    public double OriginLongitude { get { return originLongitude; } }
    public double OriginLatitude { get { return originLatitude; } }
    public Coordinates ProjectionOrigin { 
        get { return projectionOrigin; } 
        //TODO: setting the projection origin to something else should also wipe and redraw the entire world
        set { this.projectionOrigin = value; }
    }

    public Dictionary<MapElement.ID, MapElement> WorldData { get { return worldData; } }

    void Start() {
        this.topCoordinates = Coordinates.of(OriginLatitude + offset*0.89, OriginLongitude + offset*1.55);
        this.bottomCoordinates = ProjectionOrigin;

        this.worldData = MapData.GetData(bottomCoordinates, topCoordinates);

        StartCoroutine("PlaceBuildings");
    }

    private IEnumerator PlaceBuildings() {
        int built = 0;
        this.structureParentObject = new GameObject("Structures");
        foreach (MapElement mapElement in worldData.Values)
        {
            if(mapElement.Data.ContainsKey(MapNodeKey.KeyType.Building)) {
                List<Vector3> verticePositions = new List<Vector3>();
                mapElement.References.ForEach(reference => {
                    verticePositions.Add(worldData[reference].Coordinates.Position);
                });
                if(verticePositions.Count > 3) {
                    verticePositions.RemoveAt(verticePositions.Count - 1);
                    GameObject structureObject = Instantiate(structurePrefab, Vector3.zero, Quaternion.identity);
                    structureObject.name = string.IsNullOrWhiteSpace(mapElement.GetAddress()) ? "Building" : mapElement.GetAddress();
                    Structure structureScript = structureObject.GetComponent<Structure>();
                    structureScript.Build(mapElement, verticePositions);
                    structureObject.transform.parent = structureParentObject.transform;
                    built++;
                }
                
                if(built % 100 == 0) {
                    yield return null;
                }
            }
        }

        yield return null;
    }
}
