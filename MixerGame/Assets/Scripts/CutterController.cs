using UnityEngine;
using DG.Tweening;

public class CutterController : MonoBehaviour
{
    #region Variables
    //グローバル変数定義
    
    [Header("移動関連設定")]
    [SerializeField] private float _duration = 2f; //移動時間
    
    [SerializeField] private float _horizontalDistance = 6f; //横移動距離
    
    [SerializeField] [Range(0f, 2f)] private float _verticalRatio = 0.25f; //縦移動比率(横移動に対する)
    [SerializeField] private bool VerticalOnOff = true; //縦移動するかどうか(線移動か楕円移動か)


    [Header("アニメーション関連設定")]
    [SerializeField] private Ease _easeType = Ease.InOutSine;
    [SerializeField] [Range(0f, 1f)] private float _startProgress; // 0 = 始まり, 1 = 終わり
    
    private Vector3 _startPosition;
    private Collider _objectCollider;
    private Renderer _objectRenderer;
    private Material _materialInstance;
    private Color _originalColor;
    private float _previousX;
    private float _verticalDistance;
    private Tween _horizontalTween;
    private Tween _verticalTween;

    private bool _isMovingRight;
    private float _phaseOffset = 0.5f; // 縦横のズレる時間 _durationの1/4
    
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
        
        //初期状態を記憶
        _startPosition = transform.position;
        _previousX = transform.position.x;
        _verticalDistance = _horizontalDistance * _verticalRatio;
        _isMovingRight = false;
        
        //アニメーション開始
        StartHorizontalMovement();
        if (VerticalOnOff)
        {
            StartVerticalMovement();
        }
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
    //横移動関連
    void StartHorizontalMovement()
    {
        float targetX = _startPosition.x + _horizontalDistance;
        
        _horizontalTween = transform.DOMoveX(targetX, _duration)
            .SetEase(_easeType)
            .SetLoops(-1, LoopType.Yoyo)
            .SetAutoKill(false)
            .OnStart(() => _previousX = transform.position.x)
            .OnUpdate(CheckHorizontalMovementDirection);

        ApplyStartProgress(_horizontalTween, _phaseOffset);        
    }
    
    //左右どちらに動いているか判定
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
    //縦移動関連
    void StartVerticalMovement()
    {
        float targetY = _startPosition.y - _verticalDistance;

        _verticalTween = transform.DOMoveY(targetY, _duration)
            .SetEase(_easeType)
            .SetLoops(-1, LoopType.Yoyo)
            .SetAutoKill(false);

        ApplyStartProgress(_verticalTween, 0f);
    }

    
    #endregion
    
    #region Others
    
    void ApplyStartProgress(Tween tween, float baseProgress)
    {
        if (tween == null)
        {
            return;
        }
        
        float clampedProgress = Mathf.Clamp01(_startProgress);
        float normalizedBase = Mathf.Repeat(baseProgress, _duration);
        float targetProgress = Mathf.Repeat(normalizedBase + clampedProgress, _duration);

        if (Mathf.Approximately(targetProgress, 0f))
        {
            return;
        }
        
        DOTween.To(() => 0f, _ => { }, _duration, 0f).OnComplete(() =>
        {
            tween.Goto(tween.Duration(false) * targetProgress, true);
        });
    }
    
    void UpdateCollisionAndTransparency()
    {
        //右に動いているときは当たり判定をなくし、若干透明にする
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
