using UnityEngine;
using Cinemachine;

public class LockOnSystem : MonoBehaviour
{
    public CinemachineFreeLook freeLookCamera;
    public CinemachineVirtualCamera lockOnCamera;
    private Transform currentTarget;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (currentTarget == null)
            {
                FindAndLockTarget();
            }
            else
            {
                UnlockTarget();
            }
        }
    }

    void FindAndLockTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length > 0)
        {
            currentTarget = enemies[0].transform;
            lockOnCamera.LookAt = currentTarget;
            lockOnCamera.Priority = 20;
            freeLookCamera.Priority = 10;
        }
    }

    void UnlockTarget()
    {
        currentTarget = null;
        lockOnCamera.LookAt = null;
        lockOnCamera.Priority = 10; 
        freeLookCamera.Priority = 20;
    }
}
