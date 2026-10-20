
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// Hopper Studios [AC, GW, JC, JG, JP]

// Deals with button press for each player character for other scripts to use

public class PlayerInput : MonoBehaviour
{
    // Other scripts can listen for button activity
    public static event Action<int> OnPressed; // Pressin
    public static event Action<int> OnReleased; // Releasin

    void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        CheckButton(keyboard.digit1Key, 0); // Activates Player 1 (Martin1)
        CheckButton(keyboard.digit2Key, 1); // Activates Player 2 (Martin2)
        CheckButton(keyboard.digit3Key, 2); // Activates Player 3 (Dragon1)
        CheckButton(keyboard.digit4Key, 3); // Activates Player 4 (Dragon2)
    }

    void CheckButton(KeyControl key, int playerIndex)
    {
        if (key.wasPressedThisFrame)
            OnPressed?.Invoke(playerIndex);

        if (key.wasReleasedThisFrame)
            OnReleased?.Invoke(playerIndex);
    }
}
