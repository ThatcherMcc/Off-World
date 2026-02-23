using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePlayer : MonoBehaviour
{
    public Transform rotation;
    void Update()
    {
        transform.rotation = rotation.rotation;
    }
}
