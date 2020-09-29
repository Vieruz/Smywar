using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MonsterController : MonoBehaviour
{
    [SerializeField]
    private MonsterData _monsterData;
    public MonsterData monsterData => _monsterData;

    public float stunDelay;

    public UnityEvent OnMonsterUpdated = new UnityEvent();

    public Slider healthBar;
    public Animator healthAnimator;
    public GameObject hitEffect;
    public GameObject missEffect;
    public GameObject deathEffet;
    public GameObject magicHitEffect;
    public GameObject magicMissEffect;
    public GameObject stunEffect;

    public GameObject[] droppableItems;

    private Coroutine stunCoroutine = null;

    const int BASE_ATTACK_SKILL = 30;
    const int BASE_MAX_DAMAGES = 6;

    private void Awake()
    {
        OnMonsterUpdated.AddListener(HandleMonsterUpdated);
    }

    private void Start()
    {
        transform.position = _monsterData.monsterPosition;
        UpdateHealthBar();
    }

    private void FixedUpdate()
    {
        if (monsterData.Stun && stunCoroutine == null)
            stunCoroutine = StartCoroutine("SetStunEffect");
    }

    IEnumerator SetStunEffect()
    {
        GameObject effect = Instantiate(stunEffect, transform.position, Quaternion.identity, transform);
        Destroy(effect, stunDelay);
        yield return new WaitForSeconds(stunDelay);

        MonsterData data = monsterData;
        data.Stun = false;
        UpdateMonster(data);

        stunCoroutine = null;
    }

    public void CreateMonster(string uid, int type, Vector3Int randomPosition)
    {
        MonsterData data = monsterData;
        data.Uid = uid;
        data.monsterType = type;
        data.monsterPosition = randomPosition;

        _monsterData = data;
        OnMonsterUpdated.Invoke();
    }

    public void UpdateMonster(MonsterData monsterData)
    {
        if (!monsterData.Equals(_monsterData))
        {
            _monsterData = monsterData;
            UpdateHealthBar();
            OnMonsterUpdated.Invoke();
        }
    }

    public void UpdateFromDb(MonsterData data)
    {
        if (!data.Equals(_monsterData))
        {
            if (data.LP < _monsterData.LP)
                Instantiate(hitEffect, transform.position, Quaternion.identity, transform);

            _monsterData = data;
            UpdateHealthBar();
        }
    }

    public void HandleMonsterUpdated()
    {
        MonsterManager.DefaultInstance.SaveMonster(monsterData);
    }

    void UpdateHealthBar()
    {
        float pv = (float)monsterData.LP;
        float pvMax = (float)monsterData.MaxLP;
        healthBar.value = pv / pvMax;
    }

    public void AttackPlayer(PlayerController otherPlayer)
    {
        // Lancer de dé pour toucher.
        int r = Random.Range(1, (101 + (otherPlayer.PlayerData.Dexterity / 2)));
        int attackSkill = BASE_ATTACK_SKILL + (monsterData.Dexterity + otherPlayer.PlayerData.Level);
        if (r <= attackSkill)
        {
            int damages = Random.Range(1, (BASE_MAX_DAMAGES + (monsterData.Strength + otherPlayer.PlayerData.Level)));
            Debug.Log(monsterData.Name + " hit " + otherPlayer.Name + ": " + damages);
            Debug.Log("Hit: " + otherPlayer.PlayerData.LP);
            otherPlayer.TakeDamages(damages, monsterData.Name);
        }
        else
        {
            Debug.Log(monsterData.Name + " miss " + otherPlayer.Name);
            otherPlayer.TakeDamages(0, monsterData.Name);
        }
    }

    public bool TakeDamages(int amount, PlayerController otherPlayer)
    {
        MonsterData data = monsterData;
        amount -= Random.Range(0, otherPlayer.PlayerData.Level);

        string damagesText;
        if (amount > 0)
        {
            data.LP -= amount;
            UpdateMonster(data);
            damagesText = amount.ToString();
            Instantiate(hitEffect, transform.position, Quaternion.identity, transform);
        }
        else
        {
            damagesText = "Raté !";
            Instantiate(missEffect, transform.position, Quaternion.identity, transform);
        }

        healthAnimator.GetComponentInChildren<Text>().text = damagesText;
        healthAnimator.GetComponentInChildren<Text>().color = Color.red;
        healthAnimator.SetTrigger("Damaged");

        if (data.LP <= 0)
        {
            MonsterManager.DefaultInstance.DeleteMonster(this);
            return true;
        }
        else
        {
            if(!monsterData.Stun)
                AttackPlayer(otherPlayer);
            return false;
        }
    }

    public bool TakeMagicDamages(int amount)
    {
        Debug.Log(monsterData.Name + " electrocuted: " + amount);
        MonsterData data = monsterData;

        amount -= Random.Range(0, data.Wisdom);

        string damagesText;
        if (amount > 0)
        {
            data.LP -= amount;
            data.Stun = true;
            UpdateMonster(data);
            damagesText = amount.ToString();
            Instantiate(magicHitEffect, transform.position, Quaternion.identity, transform);
        }
        else
        {
            damagesText = "Résiste !";
            Instantiate(magicMissEffect, transform.position, Quaternion.identity, transform);
        }

        healthAnimator.GetComponentInChildren<Text>().text = damagesText;
        healthAnimator.GetComponentInChildren<Text>().color = Color.red;
        healthAnimator.SetTrigger("Damaged");

        if (data.LP <= 0)
        {
            MonsterManager.DefaultInstance.DeleteMonster(this);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Kill()
    {
        Instantiate(deathEffet, transform.position, Quaternion.identity);
        int r = Random.Range(0, droppableItems.Length);
        Item dropItem = droppableItems[r].GetComponent<Item>();
        float rd = Random.Range(0f, 100f);
        if(rd < dropItem.dropRate)
            Instantiate(droppableItems[r], transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
