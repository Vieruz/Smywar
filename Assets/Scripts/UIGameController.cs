using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIGameController : MonoBehaviour
{
    public static UIGameController DefaultInstance;

    public PlayerController mainPlayer;

    public Animator UIAnimator;
    public Button healButton;
    public Button spellButton;

    public Text nameText;
    public Text classText;
    public Text levelText;
    public Text experienceText;

    public Text strengthText;
    public Text dexterityText;
    public Text staminaText;
    public Text intelligenceText;
    public Text wisdomText;

    public Text lpText;
    public Text manaText;
    public Text goldText;

    public GameObject improvePanel;

    public GameObject[] itemsPrefabs;
    public Image[] inventoryLocation;
    public Text[] inventoryQuantity;
    public Sprite hideImage;

    private bool isInventoryOpen = false;

    private void Start()
    {
        if (DefaultInstance == null)
        {
            DefaultInstance = this;
        }
    }

    public void Logout()
    {
        FirebaseAuth.DefaultInstance.SignOut();
        ReloadScene();
    }

    public void ReloadScene()
    {
        FirebaseInit._app.Dispose();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TargetSelected(bool value, bool healable)
    {
        UIAnimator.SetBool("TargetSelected", value);
        UpdateManaSliders();
        if (healButton.gameObject.activeSelf)
        {
            healButton.interactable = healable;
        }
    }

    public void Attack()
    {
        mainPlayer.Charge();
    }

    public void Heal()
    {
        mainPlayer.Heal();
    }

    public void Spell()
    {
        mainPlayer.Spell();
    }

    public void UpdatePlayerProfilePanel()
    {
        string classeName = "";
        switch (mainPlayer.PlayerData.PlayerClass)
        {
            case PlayerClass.Knight:
                classeName = "Chevalier";
                break;
            case PlayerClass.Mage:
                classeName = "Mage";
                break;
            case PlayerClass.Priest:
                classeName = "Prêtre";
                break;
        }

        // Mettre à jour la fiche de perso.
        nameText.text = "Pseudo : " + mainPlayer.PlayerData.Name;
        classText.text = "Classe : " + classeName;
        levelText.text = "Niveau : " + mainPlayer.PlayerData.Level.ToString();
        int maxXp = mainPlayer.PlayerData.Level * 100;
        experienceText.text = "Exp : " + mainPlayer.PlayerData.Exp.ToString()
            + "/" + maxXp.ToString();
        strengthText.text = "Force : " + mainPlayer.PlayerData.Strength.ToString();
        dexterityText.text = "Dextérité : " + mainPlayer.PlayerData.Dexterity.ToString();
        staminaText.text = "Endurance : " + mainPlayer.PlayerData.Stamina.ToString();
        intelligenceText.text = "Intelligence : " + mainPlayer.PlayerData.Intelligence.ToString();
        wisdomText.text = "Sagesse : " + mainPlayer.PlayerData.Wisdom.ToString();
        lpText.text = "PV : " + mainPlayer.PlayerData.LP.ToString() 
            + "/" + mainPlayer.PlayerData.MaxLP.ToString();
        manaText.text = "PM : " + mainPlayer.PlayerData.MP.ToString() 
            + "/" + mainPlayer.PlayerData.MaxMP.ToString();
        goldText.text = "Or : " + mainPlayer.PlayerData.Gold.ToString();
        improvePanel.SetActive(mainPlayer.PlayerData.LevelPoints > 0);
    }

    public void ImprovePlayer(string carac)
    {
        mainPlayer.ImprovePlayer(carac);
        UpdatePlayerProfilePanel();
    }

    public void OpenInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (isInventoryOpen)
        {
            UpdateInventory();
        }
        UIAnimator.SetBool("OpenInventory", isInventoryOpen);
    }

    public void UpdateInventory()
    {
        for (int i = 0; i < inventoryLocation.Length; i++)
        {
            ItemData playerItem = mainPlayer.PlayerData.Inventory[i];
            if (playerItem.quantity > 0)
            {
                inventoryLocation[i].sprite = itemsPrefabs[playerItem.type].GetComponent<Item>().image;
                inventoryQuantity[i].text = playerItem.quantity.ToString();
                if (!mainPlayer.PlayerData.Stun)
                {
                    inventoryLocation[i].transform.parent.GetComponent<Button>().interactable = true;
                } else
                {
                    inventoryLocation[i].transform.parent.GetComponent<Button>().interactable = false;
                }
            }
            else
            {
                // Effacer l'image d'inventaire
                inventoryLocation[i].sprite = hideImage;
                inventoryQuantity[i].text = "";
                inventoryLocation[i].transform.parent.GetComponent<Button>().interactable = false;
            }
        }
    }

    public void UseItem(int index)
    {
        mainPlayer.UseItem(index);
    }

    public void SetMainPlayer(PlayerController main)
    {
        mainPlayer = main;
        switch (main.PlayerData.PlayerClass)
        {
            case PlayerClass.Priest:
                // Afficher le heal
                healButton.gameObject.SetActive(true);
                break;
            case PlayerClass.Mage:
                // Afficher le sort
                spellButton.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void EnableHeal(bool value)
    {
        healButton.interactable = value;
    }

    public void UpdateManaSliders()
    {
        if (healButton.gameObject.activeSelf)
        {
            healButton.GetComponentInChildren<Slider>().value = mainPlayer.GetManaRatio();
            if (mainPlayer.PlayerData.MP < 10)
            {
                healButton.interactable = false;
            } else
            {
                healButton.interactable = true;
            }
        } else if (spellButton.gameObject.activeSelf)
        {
            spellButton.GetComponentInChildren<Slider>().value = mainPlayer.GetManaRatio();
            if (mainPlayer.PlayerData.MP < 10)
            {
                spellButton.interactable = false;
            }
            else
            {
                spellButton.interactable = true;
            }
        }
    }

    public void RemoveTarget(Transform target)
    {
        if(mainPlayer.targeted != null
            && mainPlayer.targeted.transform.parent == target)
        {
            UIGameController.DefaultInstance.TargetSelected(false, false);
            mainPlayer.targeted = null;
            mainPlayer.targetInRange = false;
        }
    }
}
