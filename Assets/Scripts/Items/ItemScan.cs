using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScan : MonoBehaviour
{
    private GameObject itemScannable;
    private GameObject powerItem;

    private ObjectGrabbable objectGrabbable;
    private EnemyGrabbable enemyGrabbable;

    private float scanTimer = 0;
    private bool hasScanned = false;

    private void Update()
    {
        if (scanTimer <= 0)
        {
            hasScanned = false;
        }
        scanTimer -= Time.deltaTime;
    }
    private void OnTriggerStay(Collider collider)
    {
        if (!hasScanned && collider.GetComponent<ItemScannable>())
        {
            ScanItem(collider);
        }
    }

    private void ScanItem(Collider collider)
    {
        itemScannable = collider.gameObject;
        powerItem = itemScannable.GetComponent<ItemScannable>().powerItem;

        if (itemScannable.GetComponent<EnemyGrabbable>())
        {
            enemyGrabbable = itemScannable.GetComponent<EnemyGrabbable>();
            if (enemyGrabbable.equipped == false)
            {
                Destroy(itemScannable);
                Instantiate(powerItem, transform.position, transform.rotation);
                hasScanned = true;
            }
        }

        if (itemScannable.GetComponent<ObjectGrabbable>())
        {
            objectGrabbable = itemScannable.GetComponent<ObjectGrabbable>();

            if (objectGrabbable.equipped == false)
            {
                Destroy(itemScannable);
                Instantiate(powerItem, transform.position, transform.rotation);
                hasScanned = true;
            }
        }
    }
}
