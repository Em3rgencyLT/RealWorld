# RealWorld

Unity project to create real world locations in a game scene, using OSM (buildings, 
roads, etc) and SRTM (terrain topology) data.

## Usage

* Open the ExampleScene in the Scenes folder.
* Select the MapObjectPlacementManager.
* Enter the Latitude and Logitude of the desired area of the world (recommend a populated area)
* Hit Play and watch your favorite town load into view!
Note: Loading into a new area will take longer for the first time, while all the map data 
downloads.

## Cache

Both OSM and SRTM querries are cached to preserve the resources of the good folks that made 
it available for free. It is unlikely this data will change too often. If you absolutely must 
have the latest version, delete all the files from the 'Assets/OSMCache' and 
'Assets/SRTMCache' folders. 
