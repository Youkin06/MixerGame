using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CutterController : MonoBehaviour
{
    #region Variables
    
    [Header("Movement Settings")]
    [SerializeField] private float duration = 0f;
    
    [SerializeField] private float distance = 0f;
    
    [Header("Easing Settings")]
    [SerializeField] private Ease easeType = Ease.InOutSine;
    
    private Vector3 startPosition;
    private Tween movementTween;
    private Collider objectCollider;
    private Renderer objectRenderer;
    private Material materialInstance;
    private Color originalColor;
    private float previousX;

    private bool isMovingLeft;
    
    #endregion

    #region Unity Methods

    void Start()
    {
        objectCollider = GetComponent<Collider>();
        objectRenderer = GetComponent<Renderer>();
        
        if (objectRenderer != null)
        {
            materialInstance = objectRenderer.material;
            originalColor = materialInstance.color;
        }
        
        startPosition = transform.position;
        previousX = transform.position.x;
        isMovingLeft = false;
        
        StartMovement();
    }

    void OnDestroy()
    {
        if (movementTween != null && movementTween.IsActive())
        {
            movementTween.Kill();
        }
    }
    
    #endregion

    #region Private Methods

    void StartMovement()
    {
        Vector3 targetPosition = startPosition + Vector3.right * distance;
        
        movementTween = transform.DOMove(targetPosition, duration)
            .SetEase(easeType)
            .SetLoops(-1, LoopType.Yoyo)
            .SetAutoKill(false)
            .OnUpdate(CheckMovementDirection);
    }
    
    ＃動く方向を判定
    void CheckMovementDirection()
    {
        float currentX = transform.position.x;
        bool wasMovingLeft = isMovingLeft;
        isMovingLeft = currentX < previousX;
        
        if (wasMovingLeft != isMovingLeft)
        {
            UpdateCollisionAndTransparency();
        }
        
        previousX = currentX;
    }
    
    #左に進むときには当たり判定をなくし、若干透明にする
    void UpdateCollisionAndTransparency()
    {
        if (isMovingLeft)
        {
            if (objectCollider != null)
            {
                objectCollider.enabled = false;
            }
            
            if (materialInstance != null)
            {
                Color newColor = originalColor;
                newColor.a = 0.2f;
                materialInstance.color = newColor;
            }
        }
        else
        {
            if (objectCollider != null)
            {
                objectCollider.enabled = true;
            }
            
            if (materialInstance != null)
            {
                materialInstance.color = originalColor;
            }
        }
    }

    private void RestartMovement()
    {
        if (movementTween != null && movementTween.IsActive())
        {
            movementTween.Kill();
        }
        
        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }
        
        if (materialInstance != null)
        {
            materialInstance.color = originalColor;
        }
        
        startPosition = transform.position;
        previousX = transform.position.x;
        isMovingLeft = false;
        StartMovement();
    }
    
    #endregion
}
