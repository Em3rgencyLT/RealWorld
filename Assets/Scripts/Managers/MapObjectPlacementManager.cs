using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Domain;

public class MapObjectPlacementManager : Singleton<MapObjectPlacementManager>
{
    protected MapObjectPlacementManager() { }

    [SerializeField]
    private double startLatitude = 55.243727;
    [SerializeField]
    private double startLongitude = 22.285470;
    private Coordinates origin;
    private Coordinates projectionOrigin;
    [SerializeField]
    private double offset = 0.015;

    private Coordinates bottomCoordinates;
    private Coordinates topCoordinates;
    private Dictionary<MapElement.ID, MapElement> worldData;
    
    public GameObject structurePrefab;
    public GameObject structureParentPrefab;

    public Coordinates GetWorldOrigin() { return projectionOrigin; }
    public Dictionary<MapElement.ID, MapElement> WorldData { get { return worldData; } }

    // Start is called before the first frame update
    void Awake()
    {
        //FIXME: this must be calculated before all other coordinates, or shit breaks
        this.projectionOrigin = Coordinates.of(startLatitude - offset*0.89, startLongitude - offset*1.55);

        this.origin = Coordinates.of(startLatitude, startLongitude);
        this.topCoordinates = Coordinates.of(origin.Latitude + offset*0.89, origin.Longitude + offset*1.55);
        this.bottomCoordinates = projectionOrigin;

        worldData = MapData.GetData(bottomCoordinates, topCoordinates);
    }

    void Start() {
        StartCoroutine("PlaceBuildings");
    }

    private IEnumerator PlaceBuildings() {
        int built = 0;
        GameObject structureParent = Instantiate(structureParentPrefab, Vector3.zero, Quaternion.identity);
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
                    structureObject.transform.parent = structureParent.transform;
                    built++;
                }
                
                if(built % 100 == 0) {
                    yield return null;
                }
            }
        }

        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
