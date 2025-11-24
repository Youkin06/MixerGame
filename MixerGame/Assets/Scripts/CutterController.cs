using UnityEngine;
using DG.Tweening;

public class CutterController : MonoBehaviour
{
    #region Variables
    //グローバル変数定義
    
    [Header("移動関連設定")]
    [SerializeField] private float _duration; //移動時間
    
    [SerializeField] private float _horizontalDistance; //横移動距離
    
    [SerializeField] [Range(0f, 1f)] private float _verticalRatio; //縦移動比率(横移動に対する)
    [SerializeField] private bool VerticalOnOff; //縦移動するかどうか(線移動か楕円移動か)


    [Header("アニメーション関連設定")]
    [SerializeField] private Ease _easeType = Ease.InOutSine;
    [SerializeField] [Range(0f, 1f)] private float _startProgress = 0f; // 0 = 始まり, 1 = 終わり
    
    private Vector3 _startPosition;
    private Collider _objectCollider;
    private Renderer _objectRenderer;
    private Material _materialInstance;
    private Color _originalColor;
    private float _previousX;
    private float _verticalDistance;

    private bool _isMovingRight;

    private const float _delayTime = 0.5f;
    
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
        if (VerticalOnOff)
        {
            StartVerticalMovement();
        }
        StartHorizontalMovement(_duration * _delayTime);
    }
    
    #endregion

    #region Private Methods

    #region Horizontal Movement
    //横移動関連
    void StartHorizontalMovement(float startDelay)
    {
        float targetX = _startPosition.x + _horizontalDistance;
        
        transform.DOMoveX(targetX, _duration)
            .SetEase(_easeType)
            .SetLoops(-1, LoopType.Yoyo)
            .SetAutoKill(false)
            .SetDelay(startDelay)
            .OnStart(() => _previousX = transform.position.x)
            .OnUpdate(CheckHorizontalMovementDirection);
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
        float targetY = _startPosition.y + _verticalDistance;
        
        transform.DOMoveY(targetY, _duration)
            .SetEase(_easeType)
            .SetLoops(-1, LoopType.Yoyo)
            .SetAutoKill(false);
    }

    
    #endregion
    
    #region Others
    
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
