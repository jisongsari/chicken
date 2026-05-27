using UnityEngine;
using UnityEngine.InputSystem;

public class Move : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Camera targetCamera;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void Update()
    {
        Vector2 input = Vector2.zero;
        // 안!지!호! 입니다!
        if (Keyboard.current.leftArrowKey.isPressed||Keyboard.current.aKey.isPressed)
        {
            input.x = -1f;
        }
        else if (Keyboard.current.rightArrowKey.isPressed||Keyboard.current.dKey.isPressed)
        {
            input.x = 1f;
        }

        if (Keyboard.current.downArrowKey.isPressed|| Keyboard.current.sKey.isPressed)
        {
            input.y = -1f;
        }
        else if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
        {
            input.y = 1f;
        }

        Vector3 direction = new Vector3(input.x, input.y, 0f).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }

        Vector3 cameraPosition = targetCamera.transform.position;
        cameraPosition.x = transform.position.x;
        cameraPosition.y = transform.position.y;
        targetCamera.transform.position = cameraPosition;
    }
}
