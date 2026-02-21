using UnityEngine;

public class Door : MonoBehaviour
{
    private bool locked;
    
    public void UnlockDoor()
    {
        //TODO: Play anim
        locked = false;
    }
    
}
