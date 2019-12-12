using UnityEngine;

[CreateAssetMenu(menuName = "Spawners / Player Spawner")]
public class PlayerSpawner : ScriptableObject
{
    [SerializeField] private GameObject _playerPrefab;

    public GameObject SpawnPlayer(Vector3 position)
    {
        return Instantiate(_playerPrefab, position, Quaternion.identity);
    }
}
