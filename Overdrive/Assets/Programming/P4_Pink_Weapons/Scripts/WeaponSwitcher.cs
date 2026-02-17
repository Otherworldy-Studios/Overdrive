using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSwitcher : MonoBehaviour
{
    
    private int currentWeapon=0;
    private Transform[] weaponList;
    private InputSystem_Actions inputActions;

    void Start()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        
        weaponList = new Transform[transform.childCount];

        int i =0;
        foreach (Transform child in transform)
        {
            weaponList[i] = child;
            i++;
        }

        UpdateWeapon();
    }

    
    void Update()
    {
        float scrollDirectionY = inputActions.UI.ScrollWheel.ReadValue<Vector2>().y;
        if (scrollDirectionY > 0) //scroll up
        {
            if (currentWeapon >= weaponList.Length-1)
            {
                currentWeapon = 0;
            } else
            {
                currentWeapon++;
            }
            UpdateWeapon();
        }

        if (scrollDirectionY < 0) //scroll down
        {
            if (currentWeapon <= 0)
            {
                currentWeapon = weaponList.Length-1;
            }
            else
            {
                currentWeapon--;
            }
            UpdateWeapon();
        }
    }

    void UpdateWeapon()
    {
        for (int i = 0; i < weaponList.Length; i++)
        {
            if (i == currentWeapon)
            {
                weaponList[i].gameObject.SetActive(true);
            }
            else
            {
                weaponList[i].gameObject.SetActive(false);
            }
        }
    }
}
