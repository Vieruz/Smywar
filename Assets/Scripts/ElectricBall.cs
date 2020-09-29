using UnityEngine;

public class ElectricBall : MonoBehaviour
{
    public string mageName;
    public string targetUid;
    public int damages;
    public int speed;
    public GameObject explosionPrefab;

    private void Start()
    {
        Destroy(gameObject, 3);
    }

    //Ajouter le déplacement vers target
    private void FixedUpdate()
    {
        if (targetUid != null)
        {
            Transform target = FindTarget();
            if (target == null)
            {
                Destroy(gameObject);
            } else
            {
                transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.fixedDeltaTime);
            }
        }
    }

    Transform FindTarget()
    {
        Transform t = null;
        if(PlayerSaveManager.DefaultInstance.players.Exists((p) => p.PlayerData.Uid == targetUid))
        {
            t = PlayerSaveManager.DefaultInstance.players.Find((p) => p.PlayerData.Uid == targetUid).transform;
        } else if(MonsterManager.DefaultInstance.monsters.Exists((m) => m.monsterData.Uid == targetUid))
        {
            t = MonsterManager.DefaultInstance.monsters.Find((m) => m.monsterData.Uid == targetUid).transform;
        }
        return t;
    }

    //Ajouter l'impact avec la cible
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(damages > 0)
        {
            switch (collision.tag)
            {
                case "Monster":
                    MonsterController monster = collision.GetComponent<MonsterController>();
                    if(monster.monsterData.Uid == targetUid)
                    {
                        if (monster.TakeMagicDamages(damages))
                        {
                            UIGameController.DefaultInstance.RemoveTarget(monster.transform);
                        }
                    }
                    break;
                case "Player":
                    PlayerController player = collision.GetComponent<PlayerController>();
                    if (player.PlayerData.Uid == targetUid)
                    {
                        if (player.TakeMagicDamages(damages, mageName))
                        {
                            UIGameController.DefaultInstance.RemoveTarget(player.transform);
                        }
                    }
                    break;
                default:
                    Debug.Log("Not implemented target!");
                    break;
            }
        }
    }
}
