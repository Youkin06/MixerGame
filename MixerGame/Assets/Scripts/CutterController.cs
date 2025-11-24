using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CutterController : MonoBehaviour
{
    #region Variables
    
    [Header("Movement Settings")]
    [SerializeField] private float _duration;
    
    [SerializeField] private float _horizontalDistance;

     [SerializeField] [Range(0f, 1f)] private float verticalRatio;


    [Header("Easing Settings")]
    [SerializeField] private Ease _easeType = Ease.InOutSine;
    
    private Vector3 _startPosition;
    private Tween _movementTween;
    private Collider _objectCollider;
    private Renderer _objectRenderer;
    private Material _materialInstance;
    private Color _originalColor;
    private float _previousX;

    private bool _isMovingRight;
    
    #endregion

    #region Unity Methods

    void Start()
    {
        _objectCollider = GetComponent<Collider>();
        _objectRenderer = GetComponent<Renderer>();
        
        if (_objectRenderer != null)
        {
            _materialInstance = _objectRenderer.material;
            _originalColor = _materialInstance.color;
        }
        
        _startPosition = transform.position;
        _previousX = transform.position.x;
        _isMovingRight = false;
        
        StartHorizontalMovement();
    }

    void OnDestroy()
    {
        if (_movementTween != null && _movementTween.IsActive())
        {
            _movementTween.Kill();
        }
    }
    
    #endregion

    #region Private Methods

    void StartHorizontalMovement()
    {
        Vector3 targetPosition = _startPosition + Vector3.right * _horizontalDistance;
        
        _movementTween = transform.DOMove(targetPosition, _duration)
            .SetEase(_easeType)
            .SetLoops(-1, LoopType.Yoyo)
            .SetAutoKill(false)
            .OnUpdate(CheckMovementDirection);
    }
    
    //動く方向を判定
    void CheckMovementDirection()
    {
        float currentX = transform.position.x;
        bool wasMovingRight = _isMovingRight;
        _isMovingRight = currentX > _previousX;
        
        if (wasMovingRight != _isMovingRight)
        {
            UpdateCollisionAndTransparency();
        }
        
        _previousX = currentX;
    }
    
    //右に進むときには当たり判定をなくし、若干透明にする
    void UpdateCollisionAndTransparency()
    {
        if (_isMovingRight)
        {
            if (_objectCollider != null)
            {
                _objectCollider.enabled = false;
            }
            
            if (_materialInstance != null)
            {
                Color newColor = _originalColor;
                newColor.a = 0.2f;
                _materialInstance.color = newColor;
            }
        }
        else
        {
            if (_objectCollider != null)
            {
                _objectCollider.enabled = true;
            }
            
            if (_materialInstance != null)
            {
                _materialInstance.color = _originalColor;
            }
        }
    }

    private void RestartMovement()
    {
        if (_movementTween != null && _movementTween.IsActive())
        {
            _movementTween.Kill();
        }
        
        if (_objectCollider != null)
        {
            _objectCollider.enabled = true;
        }
        
        if (_materialInstance != null)
        {
            _materialInstance.color = _originalColor;
        }
        
        _startPosition = transform.position;
        _previousX = transform.position.x;
        _isMovingRight = false;
        StartHorizontalMovement();
    }
    
    #endregion
}
