using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	enum Status
    {
        IDLE,
        MOVE
    }

    private PlayerInput playerInput;
    private InputAction inputAction;
    private Vector3 playerDirection;
    private Status playerStatus = Status.IDLE;

    [SerializeField] float m_playerSpeed = 4f;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        inputAction = playerInput.actions["MOVE"];
        inputAction.performed += OnMOVEAction_performed;
    }
    private void Update()
    {
        if (playerStatus == Status.MOVE)
        {
            transform.rotation = Quaternion.LookRotation(playerDirection);
            transform.Translate(Vector3.forward * m_playerSpeed * Time.deltaTime);
        }
    }
    private void OnMOVEAction_performed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        playerStatus = input != Vector2.zero ? Status.MOVE : Status.IDLE;
            playerDirection = new Vector3(input.x, 0f, input.y);
            Debug.Log("OnMOVEAction_performed");
    }
}
