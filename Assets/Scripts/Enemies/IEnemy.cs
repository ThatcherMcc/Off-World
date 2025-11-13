using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy
{
    Transform player { get; set; }
    void EnableAI(bool enable);
}
