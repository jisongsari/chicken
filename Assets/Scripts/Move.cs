using UnityEngine;
using UnityEngine.InputSystem;

public class Move : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current.leftArrowKey.isPressed)
        {
            input.x = -1f;
        }
        else if (Keyboard.current.rightArrowKey.isPressed)
        {
            input.x = 1f;
        }

        if (Keyboard.current.downArrowKey.isPressed)
        {
            input.y = -1f;
        }
        else if (Keyboard.current.upArrowKey.isPressed)
        {
            input.y = 1f;
        }

        Vector3 direction = new Vector3(input.x, input.y, 0f).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}
