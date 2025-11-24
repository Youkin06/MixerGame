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
    
    [SerializeField] [Range(0f, 1f)] private float _verticalRatio;
    [SerializeField] private bool VerticalOnOff;


    [Header("Easing Settings")]
    [SerializeField] private Ease _easeType = Ease.InOutSine;
    
    private Vector3 _startPosition;
    private Tween _horizontalTween;
    private Tween _verticalTween;
    private Collider _objectCollider;
    private Renderer _objectRenderer;
    private Material _materialInstance;
    private Color _originalColor;
    private float _previousX;
    private float _previousY;
    private float _verticalDistance;

    private bool _isMovingRight;
    private bool _isMovingUp;
    
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
        _previousY = transform.position.y;
        _verticalDistance = _horizontalDistance * _verticalRatio;
        _isMovingRight = false;
        _isMovingUp = false;
        
        if (VerticalOnOff)
        {
            StartVerticalMovement();
        }
        
        StartHorizontalMovement(_duration * 0.5f);
    }

    void OnDestroy()
    {
        if (_horizontalTween != null && _horizontalTween.IsActive())
        {
            _horizontalTween.Kill();
        }
        
        if (_verticalTween != null && _verticalTween.IsActive())
        {
            _verticalTween.Kill();
        }
    }
    
    #endregion

    #region Private Methods
    
    #region Horizontal Movement
    
    void StartHorizontalMovement(float startDelay = 0f)
    {
        float targetX = _startPosition.x + _horizontalDistance;
        
        _horizontalTween = transform.DOMoveX(targetX, _duration)
            .SetEase(_easeType)
            .SetLoops(-1, LoopType.Yoyo)
            .SetAutoKill(false)
            .SetDelay(startDelay)
            .OnStart(() => _previousX = transform.position.x)
            .OnUpdate(CheckHorizontalMovementDirection);
    }
    
    void CheckHorizontalMovementDirection()
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
    
    #endregion
    
    #region Vertical Movement
    
    void StartVerticalMovement()
    {
        float targetY = _startPosition.y + _verticalDistance;
        
        _verticalTween = transform.DOMoveY(targetY, _duration)
            .SetEase(_easeType)
            .SetLoops(-1, LoopType.Yoyo)
            .SetAutoKill(false)
            .OnStart(() => _previousY = transform.position.y)
            .OnUpdate(CheckVerticalMovementDirection);
    }
    
    void CheckVerticalMovementDirection()
    {
        float currentY = transform.position.y;
        _isMovingUp = currentY > _previousY;
        _previousY = currentY;
    }
    
    #endregion
    
    #region Others
    
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
    
    #endregion
    
    #endregion
}
