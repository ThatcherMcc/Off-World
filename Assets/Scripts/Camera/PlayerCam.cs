using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] private Transform player;
    public float sensX;
    public float sensY;

    public Transform orientation;

    public float smoothRotationSpeed;
    private float xRotation;
    private float yRotation;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -15f, 90f);
        yRotation += mouseX;

        
        Quaternion targetPlayerRotation = Quaternion.Euler(0, yRotation, 0);
        Quaternion targetOrientationRotation = Quaternion.Euler(xRotation, yRotation, 0);

        player.rotation = Quaternion.Slerp(player.rotation, targetPlayerRotation, smoothRotationSpeed * Time.deltaTime);
        orientation.rotation = Quaternion.Slerp(orientation.rotation, targetOrientationRotation, smoothRotationSpeed * Time.deltaTime);

    }

}
