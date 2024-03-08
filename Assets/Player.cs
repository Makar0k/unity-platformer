using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Model")]
    [SerializeField]
    private CharacterModel playerModel;
    [SerializeField]
    private Vector3 modelLocalPosition;
    [SerializeField]
    private Vector3 modelRotation;
    [Header("Camera Options")]
    [SerializeField]
    private cameraProjection defaultCameraProjection;
    [SerializeField]
    private Camera playerCamera;
    [SerializeField]
    private Vector3 cameraPostion;
    [SerializeField]
    private List<CameraAnchor> camAnchors;
    [SerializeField]
    private float cameraLerpSpeed = 10f;
    [SerializeField]
    private CameraAnchor triggeredAnchor = null;
    // Возможно временный костыль, до добавления новой Input системы
    bool upPressed = false;
    bool downPressed = false;
    bool sprintPressed = false;
    bool climb_rightPressed = false;
    bool climb_leftPressed = false;
    float lastHorizontalAxis = 0f;
    CapsuleCollider mainCollider;
    [Header("Physics Options")]
    [SerializeField]
    private float activeSpeed = 5f;
    [SerializeField]
    private float sprintSpeed = 5f;
    private float lastVelocityMagnitude = 0f;
    Rigidbody rb;
    Transform groundedObjectTransform;
    Vector2 lastGroundedObjectPos;
    // Crouch
    private float defaultHeight;
    [Header("Crouching")]
    [SerializeField]
    private float activeCrouchSpeed = 4f;
    private bool isCrouching = false;
    // Gravity + Jump
    [Header("Jump + Gravity")]
    [SerializeField]
    private float gravity = 9.8f;
    private float fallTime = 0;
    private float currentjumpForce = 0f;
    [SerializeField]
    private float jumpForce = 20f;
    [SerializeField]
    private float jumpWeight = 4f;
    private LayerMask playerLayer;
    // Climbing
    [Header("Climbing")]
    [SerializeField]
    private float climbDistance = 1f;
    private bool isHanging = false;
    private float climbTimer = 0f;
    [SerializeField]
    private float unclimbedLockTime = 0.5f;
    [SerializeField]
    private float addUpperClimbRayDist = 0.5f;
    [SerializeField]
    private float addLowerClimbRayDist = 0.5f;
    [SerializeField]
    private Vector2 climbOffset;
    private Vector2 climbPosition;
    private bool isClimbing = false;
    private Transform climbObjectTransform;
    private Vector2 climbObjectLastPos;
    [SerializeField]
    private float climbSpeed = 1.5f;
    private bool isClimbPossible = true;
    [Header("Sliding")]
    [SerializeField]
    private float slideSpeed = 15f;
    [SerializeField]
    private float slideSlowdown = 1.5f;
    private float currentSlideSpeed = 0f;
    private float slideTime = 0f;
    private bool isSliding = false;
    private bool isManualSliding = false;
    private float manualSlideSpeed = 0f;
    private float manualSlideGravity = 0f;
    private Vector3 manualSlideRotation;
    public enum LookAngle
    {
        Left = 0,
        Right = 1
    }
    public enum cameraProjection
    {
        Perspective = 0,
        Orthographic = 1
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerLayer = LayerMask.GetMask("Player");
        mainCollider = GetComponent<CapsuleCollider>();
        defaultHeight = transform.localScale.y;
    }
    void Update()
    {
        if(isHanging)
        {
            if(Input.GetKeyDown(KeyCode.A))
            {
                climb_leftPressed = true;
            }
            if(Input.GetKeyDown(KeyCode.D))
            {
                climb_rightPressed = true;
            }
        }
        if(Input.GetKeyDown(KeyCode.W))
        {
            upPressed = true;
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            downPressed = true;
        }
        if(Input.GetKey(KeyCode.LeftShift))
        {
            sprintPressed = true;
        }
        else
        {
            sprintPressed = false;
        }
    }
    void FixedUpdate()
    {
        UpdateCharacterModel();
        UpdateCamera();
        UpdateMovement();
    }
    public void UpdateCamera()
    {
        if(triggeredAnchor != null)
        {
            AnchorPlayerCamera(triggeredAnchor);
            return;
        }
        if(camAnchors != null)
        {
            foreach(var anchor in camAnchors)
            {
                if(Vector2.Distance(anchor.GetAnchorPosition(), transform.position) < anchor.distanceToActivate)
                {
                    AnchorPlayerCamera(anchor);
                    return;
                }
            }
        }
        if(defaultCameraProjection == cameraProjection.Orthographic && playerCamera.orthographic == false)
        {
            playerCamera.orthographic = true;
        }
        if(defaultCameraProjection == cameraProjection.Perspective && playerCamera.orthographic == true)
        {
            playerCamera.orthographic = false;
        }
        playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, this.transform.position + cameraPostion, Time.fixedDeltaTime * cameraLerpSpeed);
    }
    public void AnchorPlayerCamera(CameraAnchor anchor)
    {
        playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, anchor.GetAnchorPosition(), Time.fixedDeltaTime * anchor.lerpSpeed);
        if(anchor.cameraProjection == cameraProjection.Orthographic)
        {
            if(playerCamera.orthographic == false)
            {
                playerCamera.orthographic = true;
            }
            playerCamera.orthographicSize = Mathf.Lerp(playerCamera.orthographicSize, anchor.orthographicSize, Time.fixedDeltaTime * anchor.lerpSpeed);
        }
        if(anchor.cameraProjection == cameraProjection.Perspective && playerCamera.orthographic == true)
        {
            playerCamera.orthographic = false;
        }
        return;
    }
    public void UpdateMovement()
    {
        bool grounded = isGrounded();
        if(isHanging)
        {
            PlayerHanging();
            return;
        }
        if(isManualSliding)
        {
            rb.velocity = new Vector3(GetPlayerLookVector() == LookAngle.Right ? manualSlideSpeed : -manualSlideSpeed, grounded ? 0 : -manualSlideGravity, 0);
            return;
        }
        if(isSliding)
        {
            if(currentSlideSpeed <= 0 || !grounded || upPressed)
            {
                if(upPressed && !isOverlaped())
                {
                    CrouchPlayer(false);
                    currentjumpForce = jumpForce;
                    transform.position = transform.position + new Vector3(0, 1, 0);
                }
                PlayerStopSliding();
                ClearPlayerInput();
                return;
            }
            currentSlideSpeed -= (slideTime * slideSlowdown);
            rb.velocity = new Vector2(GetPlayerLookVector() == LookAngle.Right ? currentSlideSpeed : -currentSlideSpeed, 0);
            slideTime += Time.fixedDeltaTime;
            return;
        }
        if(!grounded)
        {
            if(!isCloseToGround())
            {
                playerModel.ChangeAnimatorBool("isFalling", true);
            }
            if(isOverlaped(0f))
            {
                currentjumpForce = 0f;
            }
            fallTime += Time.fixedDeltaTime;
            rb.velocity = new Vector3(rb.velocity.x, -gravity * fallTime + currentjumpForce, 0);
            currentjumpForce -= Time.fixedDeltaTime * jumpWeight;
            CrouchPlayer(false);
            CheckPlayerHang();
            return;
        }
        else
        {
            playerModel.ChangeAnimatorBool("isFalling", false);
        }
        currentjumpForce = 0f;
        fallTime = 0f;

        // Anchor player to moving objects (Making own physics is cool af)
        var groundDelta = (Vector2)groundedObjectTransform.position - lastGroundedObjectPos;
        if(groundDelta != Vector2.zero)
        {
            transform.position = transform.position + new Vector3(groundDelta.x, groundDelta.y, 0);
        }
        lastGroundedObjectPos = (Vector2)groundedObjectTransform.position;
        
        // Movement
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            var resultSpeed = activeSpeed;
            if(sprintPressed)
            {
                resultSpeed = sprintSpeed;
            }
            if(isCrouching)
            {
                resultSpeed = activeCrouchSpeed;
            }
            lastHorizontalAxis = Input.GetAxis("Horizontal");
            rb.velocity = new Vector2(GetPlayerLookVector() == LookAngle.Right ? resultSpeed : -resultSpeed, 0);
        }
        else
        {
            rb.velocity = new Vector2(0, 0);
        }
        if(upPressed)
        {
            upPressed = false;
            if(isCrouching)
            {
                CrouchPlayer(false);
            }
            else
            {
                currentjumpForce = jumpForce;
                rb.velocity = new Vector3(rb.velocity.x, -gravity + currentjumpForce, 0);
            }
        }
        if(downPressed)
        {
            downPressed = false;
            if(sprintPressed)
            {
                PlayerSlide(slideSpeed);
            }
            else if(!isCrouching)
            {
                CrouchPlayer(true);
            }
        }
    }
    public void SetCameraAnchorTrigger(CameraAnchor anchor)
    {
        triggeredAnchor = anchor;
    }
    public LookAngle GetPlayerLookVector()
    {
        if(lastHorizontalAxis > 0)
        {
            return LookAngle.Right;
        }
        else
        {
            return LookAngle.Left;
        }
    }
    public LookAngle GetPlayerLookVector(float axis)
    {
        if(axis > 0)
        {
            return LookAngle.Right;
        }
        else
        {
            return LookAngle.Left;
        }
    }
    public void StopManualSliding()
    {
        playerModel.ChangeAnimatorBool("isSliding", false);
        isManualSliding = false;
    }
    public void PlayerManualSliding(LookAngle direction, float speed, float _gravity, Vector3 rot, bool slideAnim)
    {
        UnhangPlayer();
        isClimbing = false;
        currentjumpForce = 0f;
        manualSlideGravity = _gravity;
        manualSlideRotation = rot;
        manualSlideSpeed = speed;
        isManualSliding = true;
        SetPlayerLookVector(direction);
        CrouchPlayer(true);
        playerModel.ChangeAnimatorBool("isSliding", slideAnim);
    }
    public void SetPlayerLookVector(LookAngle lookAngle)
    {
        if(lookAngle == LookAngle.Left)
        {
            lastHorizontalAxis = -0.1f;
        }
        if(lookAngle == LookAngle.Right)
        {
            lastHorizontalAxis = 0.1f;
        }
    }
    private void PlayerHanging()
    {
        var objDelta = (Vector2)climbObjectTransform.position - climbObjectLastPos;
        if(objDelta != Vector2.zero)
        {
            transform.position = transform.position + new Vector3(objDelta.x, objDelta.y, 0);
            climbPosition += objDelta;
        }
        climbObjectLastPos = (Vector2)climbObjectTransform.position;
        if(isClimbing)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(climbPosition.x, climbPosition.y, transform.position.z), Time.fixedDeltaTime * climbSpeed);
            if(Vector2.Distance(transform.position, climbPosition) < 0.5)
            {
                MovingPlatform climbObjectComponent = climbObjectTransform.GetComponent<MovingPlatform>();
                this.GetComponent<Collider>().enabled = true;
                isClimbing = false;
                UnhangPlayer(objDelta * (climbObjectComponent != null ? climbObjectComponent.moveSpeed : 0));
            }
            return;
        }
        if(climb_leftPressed || climb_rightPressed)
        {
            if(GetPlayerLookVector() == GetPlayerLookVector(Input.GetAxis("Horizontal")) && isClimbPossible)
            {
                PlayerClimb();
                return;
            }
            currentjumpForce = jumpForce;
            lastHorizontalAxis = Input.GetAxis("Horizontal");
            rb.velocity = new Vector3(GetPlayerLookVector() == LookAngle.Right ? activeSpeed : -activeSpeed, rb.velocity.y, rb.velocity.z);
            UnhangPlayer();
            return;
        }
        if(upPressed)
        {
            if(isClimbPossible)
            {
                PlayerClimb();
                return;
            }
            currentjumpForce = jumpForce;
            rb.velocity = new Vector3(GetPlayerLookVector() == LookAngle.Right ? activeSpeed : -activeSpeed, rb.velocity.y, rb.velocity.z);
            UnhangPlayer();
        }
        if(downPressed)
        {
            downPressed = false;
            UnhangPlayer();
        }
    }
    public void PlayerClimb()
    {
        this.GetComponent<Collider>().enabled = false;
        climbPosition = transform.position + new Vector3(0, GetActualHeight(), 0) + new Vector3(GetPlayerLookVector() == LookAngle.Left ? -climbOffset.x : climbOffset.x, climbOffset.y, transform.position.z);
        upPressed = false;
        isClimbing = true;
        playerModel.CallAnimatorTrigger("Climb");
    }
    public void UnhangPlayer()
    {
        climbTimer = unclimbedLockTime;
        isHanging = false;
        fallTime = 0;
        playerModel.ChangeAnimatorBool("isHanging", false);
        ClearPlayerInput();
    }
    public void UnhangPlayer(Vector2 addPosition)
    {
        transform.position = transform.position + (Vector3)addPosition;
        climbTimer = unclimbedLockTime;
        isHanging = false;
        fallTime = 0;
        playerModel.ChangeAnimatorBool("isHanging", false);
        ClearPlayerInput();
    }
    public void ClearPlayerInput()
    {
        climb_leftPressed = false;
        climb_rightPressed = false;
        upPressed = false;
        downPressed = false;
        sprintPressed = false;
    }
    public void PlayerSlide(float slideForce)
    {
        CrouchPlayer(true);
        playerModel.ChangeAnimatorBool("isSliding", true);
        isSliding = true;
        currentSlideSpeed = slideForce;
        slideTime = 0f;
    }
    public void PlayerStopSliding()
    {
        playerModel.ChangeAnimatorBool("isSliding", false);
        isSliding = false;
        currentSlideSpeed = 0f;
        slideTime = 0f;
    }
    public void UpdateCharacterModel()
    {
        playerModel.GetTransform().position = this.transform.position + new Vector3(modelLocalPosition.x * transform.localScale.x, modelLocalPosition.y * transform.localScale.y, modelLocalPosition.z * transform.localScale.z);
        playerModel.GetTransform().rotation = Quaternion.Euler(Quaternion.LookRotation(GetPlayerLookVector() == LookAngle.Right ? Vector3.right : Vector3.left).eulerAngles + modelRotation);      
        playerModel.ChangeAnimatorFloat("moveSpeed", Mathf.Clamp(rb.velocity.magnitude, 1.5f, 50f));
        if(isManualSliding)
        {
            playerModel.GetTransform().rotation = Quaternion.Euler(Quaternion.LookRotation(GetPlayerLookVector() == LookAngle.Right ? Vector3.right : Vector3.left).eulerAngles + manualSlideRotation);      
        }
    }
    public void CheckPlayerHang()
    {
        if(climbTimer > 0)
        {
            climbTimer -= Time.fixedDeltaTime;
            return;
        }
        var colliderScale = GetActualColliderScale();
        var hit1 = Physics.Raycast(transform.position + new Vector3(0, colliderScale.y / 2 + addUpperClimbRayDist, 0), GetPlayerLookVector() == LookAngle.Right ? Vector3.right : Vector3.left, colliderScale.x + climbDistance, ~playerLayer);
        var hit2 = Physics.Raycast(transform.position + new Vector3(0, addLowerClimbRayDist, 0), GetPlayerLookVector() == LookAngle.Right ? Vector3.right : Vector3.left, colliderScale.x + climbDistance, ~playerLayer);
        var hit3 = Physics.Raycast(transform.position, Vector3.up, colliderScale.y/2 + addUpperClimbRayDist, ~playerLayer);
        if(!hit1 && hit2 && !hit3)
        {
            HangPlayer(true);
        }
    }
    public void HangPlayer(bool _isClimbPossible)
    {
        // Get gameObject player attached to
        var colliderScale = GetActualColliderScale();
        RaycastHit hitGameObject;
        var hit2 = Physics.Raycast(transform.position + new Vector3(0, addLowerClimbRayDist, 0), GetPlayerLookVector() == LookAngle.Right ? Vector3.right : Vector3.left, out hitGameObject, colliderScale.x + climbDistance, ~playerLayer);
        climbObjectTransform = hitGameObject.transform;
        climbObjectLastPos = climbObjectTransform.position;
        // -----------------------------
        isClimbPossible = _isClimbPossible;
        upPressed = false;
        isHanging = true;
        fallTime = 0;
        rb.velocity = Vector3.zero;
        currentjumpForce = 0f;
        climbTimer = unclimbedLockTime;
        playerModel.ChangeAnimatorBool("isHanging", true);
        playerModel.CallAnimatorTrigger("Hang");
        playerModel.ChangeAnimatorBool("isFalling", false);
    }
    public void CrouchPlayer(bool state)
    {
        Vector3 scale = transform.localScale;
        if(state)
        {
            scale.y = scale.y/2;
            transform.position = new Vector3(transform.position.x, transform.position.y - GetActualHeight()/2, transform.position.z);
        }
        else
        {   
            if(isOverlaped())
            {
                return;
            }
            scale.y = defaultHeight;
        }
        playerModel.ChangeAnimatorBool("isCrouching", state);
        isCrouching = state;
        transform.localScale = scale;
    }
    private float GetActualHeight()
    {
        return mainCollider.height * transform.localScale.y;
    }
    private Vector3 GetActualColliderScale()
    {
        // x - Radius X | y - Height | z - Radius Z
        return new Vector3(mainCollider.radius * transform.localScale.x, mainCollider.height * transform.localScale.y, mainCollider.radius * transform.localScale.z);
    }
    public bool isOverlaped()
    {
        var colliderScale = GetActualColliderScale();
        var colHeight = colliderScale.y / 2 + defaultHeight / 2;
        if(Physics.Raycast(transform.position, Vector3.up, colHeight, ~playerLayer)) // Center Ray
            return true;
        if(Physics.Raycast(transform.position + new Vector3(colliderScale.x, 0, 0), Vector3.up, colHeight, ~playerLayer)) // Center + X Radius
            return true;
        if(Physics.Raycast(transform.position  + new Vector3(-colliderScale.x, 0, 0), Vector3.up, colHeight, ~playerLayer)) // Center - X Radius
            return true;
        if(Physics.Raycast(transform.position  + new Vector3(0, 0, colliderScale.z), Vector3.up, colHeight, ~playerLayer)) // Center + Z Radius
            return true;
        if(Physics.Raycast(transform.position  + new Vector3(0, 0, -colliderScale.z), Vector3.up, colHeight, ~playerLayer)) // Center - Z Radius
            return true;

        return false;
    }
    public bool isOverlaped(float manualAdditionalDist)
    {
        var colliderScale = GetActualColliderScale();
        var colHeight = colliderScale.y / 2 + manualAdditionalDist;
        if(Physics.Raycast(transform.position, Vector3.up, colHeight, ~playerLayer)) // Center Ray
            return true;
        if(Physics.Raycast(transform.position + new Vector3(colliderScale.x - colliderScale.x / 5, 0, 0), Vector3.up, colHeight, ~playerLayer)) // Center + X Radius
            return true;
        if(Physics.Raycast(transform.position  + new Vector3(-colliderScale.x + colliderScale.x / 5, 0, 0), Vector3.up, colHeight, ~playerLayer)) // Center - X Radius
            return true;
        if(Physics.Raycast(transform.position  + new Vector3(0, 0, colliderScale.z - colliderScale.z/5), Vector3.up, colHeight, ~playerLayer)) // Center + Z Radius
            return true;
        if(Physics.Raycast(transform.position  + new Vector3(0, 0, -colliderScale.z + colliderScale.z/5), Vector3.up, colHeight, ~playerLayer)) // Center - Z Radius
            return true;

        return false;
    }
    public bool isCloseToGround()
    {
        var colliderScale = GetActualColliderScale();
        var colHeight = colliderScale.y/2 + 0.01f;
        if(Physics.Raycast(transform.position, -Vector3.up, colHeight + 0.4f, ~playerLayer)) // Center Ray
            return true;

        return false;    
    }
    public bool isGrounded()
    {
        var colliderScale = GetActualColliderScale();
        var colHeight = colliderScale.y/2 + 0.01f;
        RaycastHit groundHit;
        bool isHitted = false;
        if(Physics.Raycast(transform.position, -Vector3.up, out groundHit, colHeight, ~playerLayer) && !isHitted)
            isHitted = true;
        if(Physics.Raycast(transform.position + new Vector3(colliderScale.x / 2, 0, 0), -Vector3.up, out groundHit, colHeight, ~playerLayer) && !isHitted)
            isHitted = true; // Center + X Radius
        if(Physics.Raycast(transform.position  + new Vector3(-colliderScale.x / 2, 0, 0), -Vector3.up, out groundHit, colHeight, ~playerLayer) && !isHitted)
            isHitted = true;
        if(Physics.Raycast(transform.position  + new Vector3(0, 0, colliderScale.z / 2), -Vector3.up, out groundHit, colHeight, ~playerLayer) && !isHitted)
            isHitted = true; // Center + Z Radius
        if(Physics.Raycast(transform.position  + new Vector3(0, 0, -colliderScale.z / 2), -Vector3.up, out groundHit, colHeight, ~playerLayer) && !isHitted)
            isHitted = true;
        if(isHitted)
        {
            if(groundHit.transform != null)
            {
                if(groundedObjectTransform == null)
                {
                    lastGroundedObjectPos = groundHit.transform.position;
                }
                groundedObjectTransform = groundHit.transform;
            }
            return true;
        }
        groundedObjectTransform = null;
        return false;    
    }
}
