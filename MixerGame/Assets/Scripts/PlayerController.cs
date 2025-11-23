using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("基本的な移動設定")]
    [SerializeField] private float moveSpeed = 5f;       // 地上での移動速度
    [SerializeField] private float jumpForce = 7f;       // 地上でのジャンプ力

    [Header("水中の設定")]
    [SerializeField] private float waterGravity = 0.1f;  // 水中の重力（小さいほどゆっくり沈む）
    [SerializeField] private float waterDrag = 3f;       // 水の抵抗（動きの鈍さ）
    [SerializeField] private float swimPower = 5f;       // 水中で上昇する力（泳ぐ力）
    [SerializeField] private float swayForce = 2f;       // 水流で流される力
    [SerializeField] private float swaySpeed = 1f;       // 水流の速さ

    // 内部変数
    private Rigidbody2D rb;
    private float defaultGravity;
    private float defaultDrag;
    private bool isInWater = false;
    
    private float horizontalInput;
    private float verticalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 初期の重力と抵抗値を保存
        defaultGravity = rb.gravityScale;
        defaultDrag = rb.drag;
    }

    void Update()
    {
        // 入力の取得
        horizontalInput = Input.GetAxis("Horizontal"); // 左右 (A/D, 矢印左右)
        verticalInput = Input.GetAxis("Vertical");     // 上下 (W/S, 矢印上下)

        // 水中でのジャンプボタンによる泳ぎ（ホップする動き）
        if (Input.GetButtonDown("Jump") && isInWater)
        {
            rb.AddForce(Vector2.up * swimPower, ForceMode2D.Impulse);
        }
    }

    void FixedUpdate()
    {
        Move();

        if (isInWater)
        {
            ApplyWaterPhysics();
        }
    }

    // 移動処理
    void Move()
    {
        if (isInWater)
        {
            // 水中：力を加えて移動（ふんわりした動き）
            
            // 横移動
            rb.AddForce(new Vector2(horizontalInput * moveSpeed * 2f, 0));

            // 【追加】縦移動（矢印キー上下で浮上・潜行）
            // 上キーを押している間、上に力を加える
            if (verticalInput != 0)
            {
                rb.AddForce(new Vector2(0, verticalInput * swimPower * 2f));
            }
        }
        else
        {
            // 地上：速度を直接操作（キビキビした動き）
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        }
    }

    // 水中特有の物理挙動（ゆらゆら・抵抗）
    void ApplyWaterPhysics()
    {
        // 自動で左右にゆらゆら流される処理
        float sway = Mathf.Sin(Time.time * swaySpeed);
        rb.AddForce(new Vector2(sway * swayForce, 0));
    }

    // --- 衝突判定 ---

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Water"))
        {
            EnterWater();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Water"))
        {
            ExitWater();
        }
    }

    void EnterWater()
    {
        isInWater = true;
        rb.gravityScale = waterGravity; // 重力を弱く
        rb.drag = waterDrag;            // 抵抗を強く
        
        // 水に入った瞬間、勢いを少し殺す（バシャッという抵抗感）
        rb.velocity = rb.velocity * 0.5f;
    }

    void ExitWater()
    {
        isInWater = false;
        rb.gravityScale = defaultGravity; // 重力を戻す
        rb.drag = defaultDrag;            // 抵抗を戻す
    }
}