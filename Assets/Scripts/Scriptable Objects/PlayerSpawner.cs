using UnityEngine;

[CreateAssetMenu(menuName = "Spawners / Player Spawner")]
public class PlayerSpawner : ScriptableObject
{
    [SerializeField] private GameObject _playerPrefab;

    public void SpawnPlayer(Vector3 position)
    {
        Instantiate(_playerPrefab, position, Quaternion.identity);
    }
}
