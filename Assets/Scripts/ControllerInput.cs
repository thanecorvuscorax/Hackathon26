using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerInput : MonoBehaviour
{
    void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        Debug.Log("Device change: " + device + " | " + change);
    }

    void Update()
    {
        foreach (var gamepad in Gamepad.all)
        {
            Debug.Log("Detected Gamepad: " + gamepad.name);
        }

        if (Gamepad.current == null)
        {
            Debug.Log("No Gamepad.current");
            return;
        }

        Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
        Debug.Log("Left Stick: " + leftStick);

        if (Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            Debug.Log("South button pressed");
        }
    }
}