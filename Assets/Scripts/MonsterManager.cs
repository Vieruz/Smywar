using Firebase.Database;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public const string MONSTER_KEY = "Monsters";

    public static MonsterManager DefaultInstance;

    public int max;
    public float frequency;
    public int probability;
    public List<MonsterController> monsters;

    public GameObject[] monsterPrefabs;
    public MapManager map;

    private FirebaseDatabase _database;
    private Coroutine _spawnCoroutine;

    private void Start()
    {
        if (DefaultInstance == null)
        {
            DefaultInstance = this;
        }

        _database = FirebaseDatabase.DefaultInstance;

        monsters = new List<MonsterController>();
        _database.GetReference(MONSTER_KEY).ChildRemoved += HandleChildRemoved;
        _database.GetReference(MONSTER_KEY).ChildAdded += HandleChildAdd;
        _database.GetReference(MONSTER_KEY).ChildChanged += HandleChildChange;
        StartCoroutine("SpawnFirstMonster");
    }

    private void OnApplicationQuit()
    {
        _database.GetReference(MONSTER_KEY).ChildRemoved -= HandleChildRemoved;
        _database.GetReference(MONSTER_KEY).ChildAdded -= HandleChildAdd;
        _database.GetReference(MONSTER_KEY).ChildChanged -= HandleChildChange;
    }

    int GetMaxMonsters()
    {
        int nbPlayers = PlayerSaveManager.DefaultInstance.players.Count;
        return max * nbPlayers;
    }

    public IEnumerator SpawnFirstMonster()
    {
        Task<DataSnapshot> checkMonstertask = _database.GetReference(MONSTER_KEY).GetValueAsync();
        yield return new WaitUntil(() => checkMonstertask.IsCompleted);
        if (checkMonstertask.Result.Exists || checkMonstertask.Result.ChildrenCount < GetMaxMonsters())
        {
            _spawnCoroutine = StartCoroutine("SpawnMonsters");
        }
    }

    public IEnumerator SpawnMonsters()
    {
        int maxMonsters = GetMaxMonsters();
        while (monsters.Count < maxMonsters)
        {
            int r = Random.Range(0, 100);
            if(r < probability)
            {
                CreateMonster();
            }
            yield return new WaitForSeconds(frequency);
        }

        _spawnCoroutine = null;
    }

    public void CreateMonster()
    {
        string monsterUid = System.Guid.NewGuid().ToString();

        //Position du monstre
        Vector3Int randomPosition = new Vector3Int(Random.Range(1, map.width), Random.Range(1, map.height), 0);
        while (map.maps[1].GetTile(randomPosition) != null)
        {
            randomPosition = new Vector3Int(Random.Range(1, map.width), Random.Range(1, map.height), 0);
        }

        //Création du perso
        int t = Random.Range(0, monsterPrefabs.Length);
        GameObject monster = Instantiate<GameObject>(monsterPrefabs[t], randomPosition, Quaternion.identity);
        MonsterController monsterScript = monster.GetComponent<MonsterController>();
        monsterScript.CreateMonster(monsterUid, t, randomPosition);
        monsters.Add(monsterScript);
        Debug.Log("Monster spawned @" + monster.transform.position);
    }

    public void SaveMonster(MonsterData monster)
    {
        _database.GetReference(MONSTER_KEY).Child(monster.Uid).SetRawJsonValueAsync(JsonUtility.ToJson(monster));
    }

    public void AddMonster(MonsterData data)
    {
        GameObject newMonster = Instantiate<GameObject>(monsterPrefabs[data.monsterType], data.monsterPosition, Quaternion.identity);
        MonsterController monsterScript = newMonster.GetComponent<MonsterController>();
        monsterScript.UpdateMonster(data);
        monsters.Add(monsterScript);
    }

    void HandleChildAdd(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        var monsterToAdd = JObject.Parse(args.Snapshot.GetRawJsonValue());
        MonsterData data = monsterToAdd.ToObject<MonsterData>();
        if(!monsters.Find((m) => m.monsterData.Uid == data.Uid))
        {
            AddMonster(data);
        }
    }

    void HandleChildChange(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        var monsterToChange = JObject.Parse(args.Snapshot.GetRawJsonValue());
        MonsterData data = monsterToChange.ToObject<MonsterData>();
        UpdateMonster(data);
    }

    void HandleChildRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        var monsterRemoved = JObject.Parse(args.Snapshot.GetRawJsonValue());
        MonsterData data = monsterRemoved.ToObject<MonsterData>();
        MonsterController monsterToKill = monsters.Find((m) => m.monsterData.Uid == data.Uid);
        monsters.Remove(monsterToKill);
        monsterToKill.Kill();
        if (_spawnCoroutine == null && monsters.Count < GetMaxMonsters())
        {
            _spawnCoroutine = StartCoroutine("SpawnMonsters");
        }
    }

    public void UpdateMonster(MonsterData data)
    {
        MonsterController monsterToUpdate = monsters.Find((monster) => monster.monsterData.Uid == data.Uid);
        monsterToUpdate.UpdateFromDb(data);
    }


    public void DeleteMonster(MonsterController monster)
    {
        _database.GetReference(MONSTER_KEY).Child(monster.monsterData.Uid).RemoveValueAsync();
    }
}
