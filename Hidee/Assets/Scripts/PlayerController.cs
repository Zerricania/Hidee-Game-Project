using UnityEngine;
using FishNet.Object;

public class PlayerController : NetworkBehaviour
{
    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    [Header("Camera Settings")]
    public Transform cameraPivot;
    public float cameraDistance = 5.0f;
    public float cameraHeight = 2.0f;
    public float sensitivity = 3.0f;
    public float minY = -20f;
    public float maxY = 60f;
    public float smoothTime = 0.1f;

    private float _rotationY;
    private Camera _playerCamera;
    private CharacterController _characterController;
    private Vector3 _moveDirection = Vector3.zero;
    private bool _canMove = true;
    private bool _cursorVisible = false;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            _playerCamera = Camera.main;
            if (_playerCamera != null) _playerCamera.transform.SetParent(null);

            if (cameraPivot == null)
            {
                var pivot = new GameObject("CameraPivot");
                pivot.transform.SetParent(transform);
                pivot.transform.localPosition = new Vector3(0, cameraHeight, 0);
                cameraPivot = pivot.transform;
            }
        }
        else
        {
            gameObject.GetComponent<PlayerController>().enabled = false;
        }
    }

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        LockCursor(true);
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleCursor();
        if (!_cursorVisible)
        {
            HandleMovement();
            HandleCamera();
        }
    }

    private void HandleMovement()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = _canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = _canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = _moveDirection.y;

        _moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && _canMove && _characterController.isGrounded)
        {
            _moveDirection.y = jumpSpeed;
        }
        else
        {
            _moveDirection.y = movementDirectionY;
        }

        if (!_characterController.isGrounded)
        {
            _moveDirection.y -= gravity * Time.deltaTime;
        }

        _characterController.Move(_moveDirection * Time.deltaTime);
    }

    void HandleCamera()
    {
        if (_playerCamera == null || cameraPivot == null) return;

        // Поворот персонажа по оси X
        float rotationX = transform.eulerAngles.y + Input.GetAxis("Mouse X") * sensitivity;
        transform.rotation = Quaternion.Euler(0, rotationX, 0);

        // Наклон камеры по оси Y
        _rotationY -= Input.GetAxis("Mouse Y") * sensitivity;
        _rotationY = Mathf.Clamp(_rotationY, minY, maxY);
        cameraPivot.localRotation = Quaternion.Euler(_rotationY, 0, 0);

        // Позиция камеры
        Vector3 targetPosition = cameraPivot.position - cameraPivot.forward * cameraDistance;
        _playerCamera.transform.position = Vector3.Lerp(_playerCamera.transform.position, targetPosition, smoothTime);
        _playerCamera.transform.LookAt(cameraPivot);
    }

    void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            _cursorVisible = !_cursorVisible;
            LockCursor(!_cursorVisible);
        }
    }

    void LockCursor(bool lockState)
    {
        Cursor.lockState = lockState ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockState;
    }
}
