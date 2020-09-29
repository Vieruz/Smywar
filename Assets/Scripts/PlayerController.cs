using Firebase.Auth;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public bool isMainPlayer = false;
    [SerializeField]
    private PlayerData _playerData;
    public PlayerData PlayerData => _playerData;

    public string Name => _playerData.Name;
    public Vector3 targetPosition => _playerData.targetPosition;
    public GameObject targeted;
    public bool targetInRange = false;

    public float speed;
    public float stunDelay;

    public Text label;
    public GameObject playerSprite;
    public SpriteRenderer smileySprite;
    public Slider healthBar;
    private Animator animator;

    public GameObject targetPrefab;
    public Animator healthAnimator;
    public GameObject hitEffect;
    public GameObject missEffect;
    public GameObject deathEffet;
    public GameObject electricBall;
    public GameObject stunEffect;
    public GameObject castHealEffect;
    public GameObject magicHitEffect;
    public GameObject magicMissEffect;
    public GameObject healEffect;
    public GameObject manaEffect;

    public UnityEvent OnPlayerUpdated = new UnityEvent();

    private Coroutine spellCoroutine = null;
    private Coroutine stunCoroutine = null;

    const int BASE_ATTACK_SKILL = 50;
    const int BASE_MAX_DAMAGES = 11;
    const int BASE_MAX_MAGIC_DAMAGES = 30;
    const int BASE_HEALSPELL_SKILL = 100;
    const int BASE_HEAL_SPELL = 50;

    const float KEEP_TOUCH_INTERVAL = .2f; // .2 second;

    void Start()
    {
        OnPlayerUpdated.AddListener(HandlePlayerUpdated);
        animator = GetComponent<Animator>();
        UpdateLabel();
        UpdateHealthBar();
        UpdateSmiley();
    }

    public void SetAsMainPlayer()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null && _playerData.Uid == FirebaseAuth.DefaultInstance.CurrentUser.UserId)
        {
            isMainPlayer = true;
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
            Camera.main.transform.SetParent(transform);
            UIGameController.DefaultInstance.SetMainPlayer(this);
            StartCoroutine("KeepTouch");
        }
    }

    IEnumerator KeepTouch()
    {
        while (true)
        {
            yield return new WaitUntil(() => targeted == null);

            bool touchOverUI = false;
#if !UNITY_EDITOR
            if (isMainPlayer && Input.GetButton("Fire1"))
            {
                touchOverUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }
#endif

            if (Input.GetButton("Fire1")
            && !EventSystem.current.IsPointerOverGameObject()
            && !touchOverUI
            && targeted == null
            && !PlayerData.Stun)
            {
                Vector2 r = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(r, r, 1);


                if (hit && hit.collider.gameObject != gameObject)
                {
                    // Add target UI

                    switch (hit.collider.tag)
                    {
                        case "Monster":
                            Debug.Log("Target monster: " + hit.collider.name);
                            GameObject targetedMonster = hit.collider.gameObject;
                            targeted = Instantiate<GameObject>(targetPrefab, targetedMonster.transform.position, Quaternion.identity, targetedMonster.transform);
                            UIGameController.DefaultInstance.TargetSelected(true, false);
                            SetTargetPosition(transform.position);
                            LookAt(targetedMonster.transform.position.x - transform.position.x < 0);
                            break;
                        case "Player":
                            Debug.Log("Target player: " + hit.collider.name);
                            GameObject targetedPlayer = hit.collider.gameObject;
                            targeted = Instantiate<GameObject>(targetPrefab, targetedPlayer.transform.position, Quaternion.identity, targetedPlayer.transform);
                            UIGameController.DefaultInstance.TargetSelected(true, true);
                            SetTargetPosition(transform.position);
                            LookAt(targetedPlayer.transform.position.x - transform.position.x < 0);
                            break;
                        default:
                            // Move to
                            Vector2 moveTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            SetTargetPosition(moveTarget);
                            break;
                    }
                }
                else
                {
                    // Move to
                    Vector2 moveTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    SetTargetPosition(moveTarget);
                }
            }

            yield return new WaitForSeconds(KEEP_TOUCH_INTERVAL);
        }
    }

    private void Update()
    {
        bool touchOverUI = false;

#if !UNITY_EDITOR
        if (isMainPlayer && Input.GetButtonDown("Fire1"))
        {
            touchOverUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
#endif

        if (isMainPlayer && Input.GetButtonDown("Fire1")
            && !EventSystem.current.IsPointerOverGameObject()
            && !touchOverUI
            && !PlayerData.Stun)
        {
            Vector2 r = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(r, r, 1);

            if (targeted != null)
            {
                // Remove target
                UIGameController.DefaultInstance.TargetSelected(false, false);
                Destroy(targeted);
                targeted = null;
                targetInRange = false;
            }

            // Target
            if (hit && hit.collider.gameObject != gameObject)
            {
                // Add target UI

                switch (hit.collider.tag)
                {
                    case "Monster":
                        Debug.Log("Target monster: " + hit.collider.name);
                        GameObject targetedMonster = hit.collider.gameObject;
                        targeted = Instantiate<GameObject>(targetPrefab, targetedMonster.transform.position, Quaternion.identity, targetedMonster.transform);
                        UIGameController.DefaultInstance.TargetSelected(true, false);
                        SetTargetPosition(transform.position);
                        LookAt(targetedMonster.transform.position.x - transform.position.x < 0);
                        break;
                    case "Player":
                        Debug.Log("Target player: " + hit.collider.name);
                        GameObject targetedPlayer = hit.collider.gameObject;
                        targeted = Instantiate<GameObject>(targetPrefab, targetedPlayer.transform.position, Quaternion.identity, targetedPlayer.transform);
                        UIGameController.DefaultInstance.TargetSelected(true, true);
                        SetTargetPosition(transform.position);
                        LookAt(targetedPlayer.transform.position.x - transform.position.x < 0);
                        break;
                    default:
                        // Move to
                        Vector2 moveTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        SetTargetPosition(moveTarget);
                        break;
                }
            }
            else
            {
                // Move to
                Vector2 moveTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                SetTargetPosition(moveTarget);
            }
        }
    }

    private void FixedUpdate()
    {
        Move();
        if (PlayerData.Stun && stunCoroutine == null)
            stunCoroutine = StartCoroutine("SetStunEffect");
        if (PlayerData.Heal && isMainPlayer)
            SetHealEffect();
    }

    void SetHealEffect()
    {
        Instantiate(healEffect, transform.position, Quaternion.identity, transform);
        PlayerData data = PlayerData;
        data.Heal = false;
        UpdatePlayer(data);
    }

    IEnumerator SetStunEffect()
    {
        GameObject effect = Instantiate(stunEffect, transform.position, Quaternion.identity, transform);
        Destroy(effect, stunDelay);
        if (isMainPlayer)
        {
            if (targeted)
            {
                UIGameController.DefaultInstance.RemoveTarget(targeted.transform.parent);
            }
            UIGameController.DefaultInstance.UpdateInventory();
        }
        yield return new WaitForSeconds(stunDelay);
        PlayerData data = PlayerData;
        data.Stun = false;
        UpdatePlayer(data);

        if (isMainPlayer)
        {
            UIGameController.DefaultInstance.UpdateInventory();
        }
        stunCoroutine = null;
    }

    private void HandlePlayerUpdated()
    {
        PlayerSaveManager.DefaultInstance.SavePlayer(PlayerData);
    }


    private void UpdateLabel()
    {
        gameObject.name = Name;
        label.text = "<" + Name + ">";
    }

    public float GetManaRatio()
    {
        float a = (float) _playerData.MP;
        float b = (float) _playerData.MaxMP;
        return a / b;
    }

    public void Move()
    {
        Vector3Int a = new Vector3Int(Mathf.RoundToInt(transform.position.x),
               Mathf.RoundToInt(transform.position.y), 0);
        Vector3Int b = new Vector3Int(Mathf.RoundToInt(targetPosition.x),
            Mathf.RoundToInt(targetPosition.y), 0);
        if (a != b && !PlayerData.Stun)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime * speed);
            if (!targeted)
            {
                LookAt(targetPosition.x - transform.position.x < 0);
            }
            if (isMainPlayer)
            {
                _playerData.playerPosition = transform.position;
                PlayerSaveManager.DefaultInstance.SavePosition(_playerData);
            }
        }
    }

    public void LookAt(bool left)
    {
        if (left)
        {
            playerSprite.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            playerSprite.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void SetTargetPosition(Vector2 target)
    {
        PlayerData data = PlayerData;
        data.targetPosition = target;
        PlayerSaveManager.DefaultInstance.SaveTargetPosition(data);
    }

    public void UpdatePlayer(PlayerData playerData)
    {
        if (!playerData.Equals(_playerData))
        {
            _playerData = playerData;
            UpdateHealthBar();
            OnPlayerUpdated.Invoke();
        }
    }

    public void UpdateFromDb(PlayerData data)
    {
        if (!data.Equals(_playerData))
        {
            if(_playerData.LP > 0 
                && data.Level == _playerData.Level 
                && data.Stamina == _playerData.Stamina 
                && data.LP > _playerData.LP
                && !isMainPlayer)
            {
                Instantiate(healEffect, transform.position, Quaternion.identity, transform);
            }

            if (data.LP < _playerData.LP)
                Instantiate(hitEffect, transform.position, Quaternion.identity, transform);

            _playerData = data;
            if (!isMainPlayer)
            {
                transform.position = data.playerPosition;
                if(PlayerData.spellToUid != "")
                {
                    GameObject eBall = Instantiate(electricBall, transform.position, Quaternion.identity, transform);
                    ElectricBall eBallscript = eBall.GetComponent<ElectricBall>();
                    eBallscript.mageName = Name;
                    eBallscript.targetUid = PlayerData.spellToUid;
                    eBallscript.damages = 0;
                    _playerData.spellToUid = null;
                }
            }
            UpdateHealthBar();
        }
    }

    public void Charge()
    {
        if(targeted != null && !PlayerData.Stun)
        {
            SetTargetPosition(targeted.transform.position);
            if (targetInRange)
            {
                Attack();
            }
        }
    }

    public void Attack()
    {
        if (!PlayerData.Stun)
        {
            LookAt(targeted.transform.position.x - transform.position.x < 0);
            animator.SetTrigger("attack");

            switch (targeted.transform.parent.tag)
            {
                case "Monster":
                    //Attaquer le monstre
                    MonsterController m = targeted.GetComponentInParent<MonsterController>();
                    Debug.Log("Attack: " + m.name);
                    AttackMonster(m);
                    break;
                case "Player":
                    //Attaquer le joueur
                    PlayerController p = targeted.GetComponentInParent<PlayerController>();
                    Debug.Log("Attack: " + p.name);
                    AttackPlayer(p);
                    break;
                default:
                    Debug.Log("Not implemented target!");
                    break;
            }
        }
    }

    public void Spell()
    {
        if (!PlayerData.Stun && targeted)
        {
            LookAt(targeted.transform.position.x - transform.position.x < 0);
            animator.SetTrigger("attack");

            string uid = "";

            switch (targeted.transform.parent.tag)
            {
                case "Monster":
                    //Attaquer le monstre
                    MonsterController m = targeted.GetComponentInParent<MonsterController>();
                    Debug.Log("Spell to: " + m.name);
                    uid = m.monsterData.Uid;
                    break;
                case "Player":
                    //Attaquer le joueur
                    PlayerController p = targeted.GetComponentInParent<PlayerController>();
                    Debug.Log("Spell to: " + p.name);
                    uid = p.PlayerData.Uid;
                    break;
                default:
                    Debug.Log("Not implemented target!");
                    break;
            }

            SpellToTarget(targeted.transform, uid);
        }
    }

    private void SpellToTarget(Transform target, string uid)
    {
        int xp = 20;
        GameObject eBall = Instantiate(electricBall, transform.position, Quaternion.identity, transform);
        ElectricBall eBallscript = eBall.GetComponent<ElectricBall>();
        eBallscript.mageName = Name;
        eBallscript.targetUid = uid;

        int damages = Random.Range(10, (BASE_MAX_MAGIC_DAMAGES + PlayerData.Intelligence));
        eBallscript.damages = damages;

        PlayerData data = PlayerData;
        data.spellToUid = uid;
        UpdatePlayer(data);
        if(spellCoroutine == null)
            spellCoroutine = StartCoroutine("CastSpell");

        int rm = Random.Range(10, damages / 10);
        UseMana(rm);
        IncreaseXp(xp, 0);
    }

    IEnumerator CastSpell()
    {
        yield return new WaitForSeconds(1);
        PlayerData data = PlayerData;
        data.spellToUid = null;
        UpdatePlayer(data);

        spellCoroutine = null;
    }

    public void Heal()
    {
        LookAt(targeted.transform.position.x - transform.position.x < 0);

        if(PlayerData.MP > 0 && !PlayerData.Stun)
        {
            switch (targeted.transform.parent.tag)
            {
                case "Player":
                    // Soigner le joueur
                    PlayerController p = targeted.GetComponentInParent<PlayerController>();
                    Debug.Log("Heal: " + p.name);
                    animator.SetTrigger("attack");
                    HealPlayer(p);
                    break;
                default:
                    Debug.Log("Not implemented target!");
                    break;
            }
        }
    }

    private void HealPlayer(PlayerController otherPlayer)
    {
        int xp = 10;
        Instantiate(castHealEffect, transform.position, Quaternion.identity, transform);

        // Lancer de dé pour soigner.
        int lpToHeal = otherPlayer.PlayerData.MaxLP - otherPlayer.PlayerData.LP;
        int r = Random.Range(1, otherPlayer.PlayerData.LP);
        int spellSkill = BASE_HEALSPELL_SKILL + PlayerData.Intelligence;

        if (r <= spellSkill && lpToHeal > 0)
        {
            int healValue = Random.Range(1, PlayerData.Wisdom);
            healValue += BASE_HEAL_SPELL;
            Debug.Log(Name + " heal " + otherPlayer.Name + ": " + healValue);
            xp += 10;

            otherPlayer.HealLP(healValue);
        }
        else
        {
            Debug.Log(Name + " miss " + otherPlayer.Name + "!");

            otherPlayer.HealLP(0);
        }
        int rm = Random.Range(10, r/10);
        UseMana(rm);
        IncreaseXp(xp, 0);
    }

    public void AttackMonster(MonsterController monster)
    {
        if (!PlayerData.Stun)
        {
            int xp = 5;
            // Lancer de dé pour toucher.
            int r = Random.Range(1, (101 + (monster.monsterData.Dexterity / 2)));
            int attackSkill = BASE_ATTACK_SKILL + PlayerData.Dexterity;
            int monsterGold = 0;
            if (r <= attackSkill)
            {
                int damages = Random.Range(1, (BASE_MAX_DAMAGES + PlayerData.Strength));
                Debug.Log(Name + " hit " + monster.monsterData.Name + ": " + damages);
                xp += 5;
                monsterGold = Random.Range(0, monster.monsterData.Gold);

                if (monster.TakeDamages(damages, this))
                {
                    int monsterXp = Random.Range(10, 10 * PlayerData.Level);
                    xp += monsterXp;
                    UIGameController.DefaultInstance.TargetSelected(false, false);
                    targeted = null;
                    targetInRange = false;
                } else
                {
                    monsterGold = 0;
                }
            }
            else
            {
                Debug.Log(Name + " miss " + monster.monsterData.Name + "!");
                if (monster.TakeDamages(0, this))
                {
                    UIGameController.DefaultInstance.TargetSelected(false, false);
                    targeted = null;
                    targetInRange = false;
                }
            }
            IncreaseXp(xp, monsterGold);
        }
    }

    public void AttackPlayer(PlayerController otherPlayer, bool isCounterAttack = false)
    {
        if (!PlayerData.Stun)
        {
            int xp = 5;
            // Lancer de dé pour toucher.
            int r = Random.Range(1, (101 + (otherPlayer.PlayerData.Dexterity / 2)));
            int attackSkill = BASE_ATTACK_SKILL + PlayerData.Dexterity;
            int gold = 0;

            if (r <= attackSkill)
            {
                int damages = Random.Range(1, (BASE_MAX_DAMAGES + PlayerData.Strength));
                Debug.Log(Name + " hit " + otherPlayer.Name + ": " + damages);
                xp += 5;
                gold = Random.Range(0, otherPlayer.PlayerData.Gold);

                bool playerKilled;
                if (isCounterAttack)
                {
                    // On ne riposte pas sur une riposte.
                    playerKilled = otherPlayer.TakeDamages(damages, Name);
                }
                else
                {
                    playerKilled = otherPlayer.TakeDamages(damages, Name, this);
                }

                if (playerKilled)
                {
                    int playerXp = Random.Range(10, 10 * otherPlayer.PlayerData.Level);
                    xp += playerXp;
                    UIGameController.DefaultInstance.TargetSelected(false, false);
                    targeted = null;
                    targetInRange = false;
                } else
                {
                    gold = 0;
                }
            }
            else
            {
                Debug.Log(Name + " miss " + otherPlayer.Name + "!");

                bool playerKilled;
                if (isCounterAttack)
                {
                    // On ne riposte pas sur une riposte.
                    playerKilled = otherPlayer.TakeDamages(0, Name);
                }
                else
                {
                    playerKilled = otherPlayer.TakeDamages(0, Name, this);
                }

                if (playerKilled)
                {
                    UIGameController.DefaultInstance.TargetSelected(false, false);
                    targeted = null;
                    targetInRange = false;
                }
            }
            IncreaseXp(xp, gold);
        }
    }

    public bool TakeDamages(int amount, string attackerName, PlayerController otherPlayer = null)
    {
        PlayerData data = PlayerData;

        string damagesText;
        if (amount > 0)
        {
            data.LP -= amount;
            Debug.Log("Damages: " + data.LP);
            UpdatePlayer(data);
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
            OnPlayerUpdated.RemoveAllListeners();
            PlayerSaveManager.DefaultInstance.EraseSave(this, attackerName);
            return true;
        } else
        {
            if(otherPlayer != null && !PlayerData.Stun)
            {
                AttackPlayer(otherPlayer, true);
            }
            return false;
        }
    }

    public bool TakeMagicDamages(int amount, string attackerName)
    {
        Debug.Log(PlayerData.Name + " electrocuted: " + amount);
        PlayerData data = PlayerData;

        amount -= Random.Range(0, data.Wisdom);

        string damagesText;
        if (amount > 0)
        {
            data.LP -= amount;
            data.Stun = true;
            UpdatePlayer(data);
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
            OnPlayerUpdated.RemoveAllListeners();
            PlayerSaveManager.DefaultInstance.EraseSave(this, attackerName);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void HealLP(int amount)
    {
        PlayerData data = PlayerData;

        string healText;
        if (amount > 0)
        {
            data.LP += amount;
            if (data.LP > data.MaxLP)
                data.LP = data.MaxLP;
            data.Heal = true;
            UpdatePlayer(data);
            healText = amount.ToString();
            Instantiate(healEffect, transform.position, Quaternion.identity, transform);
        } else
        {
            healText = "Raté !";
            Instantiate(missEffect, transform.position, Quaternion.identity, transform);
        }

        healthAnimator.GetComponentInChildren<Text>().text = healText;
        healthAnimator.GetComponentInChildren<Text>().color = Color.green;
        healthAnimator.SetTrigger("Damaged");
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        float pv = (float)PlayerData.LP;
        float pvMax = (float)PlayerData.MaxLP;
        healthBar.value = pv / pvMax;
    }

    public void Kill()
    {
        Debug.Log("Player killed: " + name);
        Instantiate(deathEffet, transform.position, Quaternion.identity);
        if (isMainPlayer)
        {
            //Animation de mort
            UIGameController.DefaultInstance.ReloadScene();
        }
        Destroy(gameObject);
    }

    public void UpdateSmiley()
    {
        smileySprite.sprite = PlayerSaveManager.DefaultInstance.GetSmileySprite(PlayerData.Level);
    }

    private void IncreaseXp(int amount, int gold)
    {
        PlayerData data = PlayerData;
        data.Exp += amount;
        data.Gold += gold;
        if (data.Exp >= data.Level * 100)
        {
            healthAnimator.SetTrigger("LevelUp");
            data.Level++;
            data.LevelPoints++;
            data.MaxLP += Random.Range(1, data.Stamina);
            data.LP = data.MaxLP;
            data.MaxMP += Random.Range(0, data.Wisdom);
            data.MP = data.MaxMP;
            data.Exp = 0;
            UIGameController.DefaultInstance.UpdateManaSliders();
            UpdateSmiley();
        }
        UpdatePlayer(data);
    }

    public void ImprovePlayer(string carac)
    {
        PlayerData data = PlayerData;

        if (data.LevelPoints > 0)
        {
            switch (carac)
            {
                case "Strength":
                    data.Strength++;
                    break;
                case "Dexterity":
                    data.Dexterity++;
                    break;
                case "Stamina":
                    data.Stamina++;
                    int bonusLP = Random.Range(1, data.Stamina);
                    data.MaxLP += bonusLP;
                    data.LP += bonusLP;
                    break;
                case "Intelligence":
                    data.Intelligence++;
                    break;
                case "Wisdom":
                    data.Wisdom++;
                    int bonusMP = Random.Range(0, data.Wisdom);
                    data.MaxMP += bonusMP;
                    data.MP += bonusMP;
                    UIGameController.DefaultInstance.UpdateManaSliders();
                    break;
                default:
                    Debug.Log("Not implemented improvement!");
                    break;
            }
            data.LevelPoints--;
            UpdatePlayer(data);
        }

    }

    private void UseMana(int amount)
    {
        PlayerData data = PlayerData;

        data.MP -= amount;
        if (data.MP < 0)
            data.MP = 0;
        UIGameController.DefaultInstance.UpdateManaSliders();
        UpdatePlayer(data);
    }

    public void DrinkMP(int amount)
    {
        PlayerData data = PlayerData;

        string healText;
        if (amount > 0)
        {
            data.MP += amount;
            if (data.MP > data.MaxMP)
                data.MP = data.MaxMP;
            UpdatePlayer(data);
            healText = amount.ToString();
            Instantiate(manaEffect, transform.position, Quaternion.identity, transform);
        }
        else
        {
            healText = "Raté !";
            Instantiate(missEffect, transform.position, Quaternion.identity, transform);
        }

        healthAnimator.GetComponentInChildren<Text>().text = healText;
        healthAnimator.GetComponentInChildren<Text>().color = Color.blue;
        healthAnimator.SetTrigger("Damaged");
        UIGameController.DefaultInstance.UpdateManaSliders();
    }


    public void UseItem(int index)
    {
        PlayerData playerData = PlayerData;

        ItemData itemData = playerData.Inventory[index];
        if(itemData.quantity > 0)
        {
            playerData.Inventory[index].quantity--;
            UpdatePlayer(playerData);
            Item i = UIGameController.DefaultInstance.itemsPrefabs[itemData.type].GetComponent<Item>();
            i.Use(this);
            UIGameController.DefaultInstance.UpdateInventory();
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (targeted != null && collision.gameObject == targeted.transform.parent.gameObject)
        {
            targetInRange = true;
            Attack();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (targeted != null && collision.gameObject == targeted.transform.parent.gameObject)
        {
            targetInRange = false;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (targeted != null && collision.gameObject == targeted.transform.parent.gameObject)
        {
            targetInRange = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Item")
        {
            int type = collision.GetComponent<Item>().data.type;

            ItemData itemData = PlayerData.Inventory[type];
            itemData.type = type;
            itemData.quantity++;
            PlayerData.Inventory[type] = itemData;
            PlayerSaveManager.DefaultInstance.SaveInventory(PlayerData, itemData);

            UIGameController.DefaultInstance.UpdateInventory();
            Item itemTaken = collision.GetComponent<Item>();
            itemTaken.Take();
        }
    }
}
