using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : SPPlaneControl
{
    public GameObject player = GameObject.Find("Ship");

    [SerializeField]
    public GameObject enemy;

    new protected void UpdateInputs()
    {
        joystickX = Mathf.Sign(CalculateTurn());
    }

    float CalculateTurn()
    {
        Vector3 enemyToOrigin = -enemy.transform.position;
        Vector3 enemyToPlayer = player.transform.position - enemy.transform.position;
        Vector3 path = Vector3.Cross(enemyToOrigin, Vector3.Cross(enemyToPlayer, enemyToOrigin)).normalized;
        Vector3 currPath = enemy.transform.forward;
        float sign = Mathf.Sign(Vector3.Dot(enemy.transform.right, path));
        return sign * Mathf.Acos(Vector3.Dot(currPath, path));
    }
}
