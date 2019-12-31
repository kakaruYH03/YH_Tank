using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTank : MonoBehaviour
{
    public Transform target;

    private float speed;
    private float rotSpeed;

    private void Start()
    {
        speed = 10.0f;
        rotSpeed = 2.0f;
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, target.position) < 1.0f)
        {
            return;
        }
        Vector3 targetPos = target.position;
        Vector3 dirRot = targetPos - transform.position;

        Quaternion targetRot = Quaternion.LookRotation(dirRot);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSpeed * Time.deltaTime);
        transform.Translate(new Vector3(0, 0, speed * Time.deltaTime));
    }
}
