using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotatePlayer : MonoBehaviour
{
    public Transform rotation;
    void Update()
    {
        transform.rotation = rotation.rotation;
    }
}
