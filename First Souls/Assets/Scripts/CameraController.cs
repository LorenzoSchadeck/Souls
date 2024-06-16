using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player; // O alvo que a câmera deve seguir (o jogador)
    public Vector3 offset; // A posição relativa da câmera em relação ao jogador
    public Vector3 lockOnOffset; // A posição relativa da câmera em relação ao jogador no estado de lock-on
    public float sensitivity = 10f; // Sensibilidade do mouse
    public float distance = 5.0f; // Distância da câmera até o jogador
    public float maxYAngle = 80f; // Ângulo máximo de rotação no eixo Y
    public float minYAngle = -40f; // Ângulo mínimo de rotação no eixo Y
    public float smoothSpeed = 0.125f; // Velocidade de suavização
    public LayerMask collisionLayers; // Camadas com as quais a câmera deve colidir
    public float lockOnRadius = 10f; // Raio de alcance do lock-on

    private Vector2 currentRotation; // Armazena a rotação atual da câmera
    private float currentDistance; // A distância atual da câmera até o jogador
    private Transform lockTarget; // Alvo atual de lock-on
    private List<Transform> enemies; // Lista de inimigos
    private Vector3 currentOffset; // Offset atual da câmera

    void Start()
    {
        currentRotation = new Vector2(transform.eulerAngles.y, transform.eulerAngles.x);
        currentDistance = distance;
        currentOffset = offset;
        enemies = new List<Transform>();
        FindAllEnemies();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (lockTarget == null)
            {
                LockOnToNearestTarget();
            }
            else
            {
                lockTarget = null;
            }
        }

        if (lockTarget != null && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SwitchLockTarget(-1);
        }
        else if (lockTarget != null && Input.GetKeyDown(KeyCode.RightArrow))
        {
            SwitchLockTarget(1);
        }
    }

    void LateUpdate()
    {
        if (lockTarget == null)
        {
            HandleFreeLook();
        }
        else
        {
            HandleLockOn();
        }

        HandleCameraCollision();
    }

    void HandleFreeLook()
    {
        currentRotation.x += Input.GetAxis("Mouse X") * sensitivity;
        currentRotation.y -= Input.GetAxis("Mouse Y") * sensitivity;
        currentRotation.y = Mathf.Clamp(currentRotation.y, minYAngle, maxYAngle);

        // Calcula a rotação desejada
        Quaternion desiredRotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);

        // Calcula a posição desejada sem colisão
        Vector3 desiredPosition = player.position - (desiredRotation * Vector3.forward * distance + offset);

        // Suaviza a transição de rotação e posição
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, smoothSpeed);
        currentOffset = Vector3.Lerp(currentOffset, offset, smoothSpeed);
    }

    void HandleLockOn()
    {
        // A câmera sempre olha para o alvo lock-on
        Vector3 directionToTarget = lockTarget.position - player.position;
        Quaternion desiredRotation = Quaternion.LookRotation(directionToTarget);

        // Suaviza a transição de rotação
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, smoothSpeed);

        // Manter uma distância fixa do jogador enquanto trava no alvo
        Vector3 desiredPosition = player.position - (transform.rotation * Vector3.forward * distance + lockOnOffset);

        // Suaviza a transição de posição
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        currentOffset = Vector3.Lerp(currentOffset, lockOnOffset, smoothSpeed);
    }

    void HandleCameraCollision()
    {
        // Calcula a rotação desejada com base no lock-on ou free-look
        Quaternion desiredRotation = lockTarget != null ? Quaternion.LookRotation(lockTarget.position - player.position) : Quaternion.Euler(currentRotation.y, currentRotation.x, 0);

        // Calcula a posição desejada sem colisão
        Vector3 desiredPosition = player.position - (desiredRotation * Vector3.forward * distance + currentOffset);

        // Raycast para verificar colisões
        RaycastHit hit;
        if (Physics.Linecast(player.position + currentOffset, desiredPosition, out hit, collisionLayers))
        {
            currentDistance = Mathf.Clamp(hit.distance, 0.5f, distance); // Ajuste a distância com base na colisão
        }
        else
        {
            currentDistance = distance;
        }

        // Ajusta a posição da câmera com base na distância atual
        Vector3 adjustedPosition = player.position - (desiredRotation * Vector3.forward * currentDistance + currentOffset);

        // Suaviza a transição de posição
        transform.position = Vector3.Lerp(transform.position, adjustedPosition, smoothSpeed);
    }

    void FindAllEnemies()
    {
        Enemy[] foundEnemies = FindObjectsOfType<Enemy>();
        enemies.Clear();

        foreach (var enemy in foundEnemies)
        {
            enemies.Add(enemy.transform);
        }
    }

    void LockOnToNearestTarget()
    {
        float minDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        foreach (var enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(player.position, enemy.position);
            if (distanceToEnemy < minDistance && distanceToEnemy <= lockOnRadius)
            {
                minDistance = distanceToEnemy;
                nearestTarget = enemy;
            }
        }

        lockTarget = nearestTarget;
    }

    void SwitchLockTarget(int direction)
    {
        if (enemies.Count == 0) return;

        int currentIndex = enemies.IndexOf(lockTarget);
        int newIndex = (currentIndex + direction + enemies.Count) % enemies.Count;

        lockTarget = enemies[newIndex];
    }
}
