using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;

public class WaveController : MonoBehaviour
{
    // --- コンポーネントと内部変数 ---
    private SpriteShapeController sc; // SpriteShapeを制御するコンポーネント
    private Spline spline;            // 形状（頂点データ）を管理するクラス
    private float scale;              // オブジェクトのX軸スケール
    private Vector2 basePointPos;     // 基準となる左下の頂点位置
    private int n;                    // 生成する水面の頂点の総数
    private float[] vec;              // 各頂点の「上下方向の速度」を記録する配列（バネの動き用）

    // --- 設定パラメータ（インスペクターで調整可能） ---
    [SerializeField] private float interval = 0.45f; // 頂点同士の間隔（狭いほど滑らかだが重くなる）
    [SerializeField] private float maxWave = 0.5f;   // 波の高さの最大制限
    [SerializeField] private float minWave = 0.05f;  // 波として処理する最小の高さ（これ以下なら計算終了）
    [SerializeField] private float k = 0.09f;        // バネ定数（大きいほど素早く元の位置に戻ろうとする）
    [SerializeField] private float waterPow = 0.05f; // 衝突時の衝撃係数（大きいほどドボンと沈む）
    [SerializeField] private float waveRate = 0.65f; // 波が隣へ伝わる際の減衰率（小さいほど波がすぐ消える）
    [SerializeField] private float waveTime = 0.08f; // 隣の頂点へ波が伝わるまでの遅延時間（秒）

    private void Start()
    {
        // コンポーネントの取得
        sc = GetComponent<SpriteShapeController>();
        spline = sc.spline;

        // 形状の基準位置を取得（Index 1は通常、底面の左端などを指す想定）
        basePointPos = spline.GetPosition(1);
        scale = transform.localScale.x;

        // 水面の幅に合わせて、必要な頂点の数(n)を計算
        n = (int)(-basePointPos.x * scale * 2 / interval) + 1;

        // 速度配列の初期化（両端や底面の点は計算しないため n-2）
        vec = new float[n - 2];
        for (int i = 0; i < n - 2; i++){vec[i] = 0;}

        // Splineに頂点を挿入していく処理
        Vector3 p = basePointPos;
        for (int i = 2; i < n; i++) // Index 0,1は既存の点（底面）なので、2から開始
        {
            // 右方向へ interval ずつずらして位置を決定
            p += interval * Vector3.right / scale;
            
            // Splineに新しい点を挿入
            spline.InsertPointAt(i, p);
            // 滑らかな曲線にするための設定（ベジェ曲線）
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
            spline.SetLeftTangent(i, 0.4f * interval * Vector3.left / scale);
            spline.SetRightTangent(i, 0.4f * interval * Vector3.right / scale);  
            
            // 角の丸みやテクスチャの高さ設定（必要に応じて）
            spline.SetHeight(i, 0.1f);
        }
        //底面の点のハンドルを無しにする
        spline.SetTangentMode(0, ShapeTangentMode.Continuous);
        spline.SetLeftTangent(0, Vector3.zero);
        spline.SetRightTangent(0, Vector3.zero);  
        spline.SetTangentMode(1, ShapeTangentMode.Continuous);
        spline.SetLeftTangent(1, Vector3.zero);
        spline.SetRightTangent(1, Vector3.zero);  
        //右上の点のハンドルを無しにする
        spline.SetTangentMode(n, ShapeTangentMode.Continuous);
        spline.SetLeftTangent(n, Vector3.zero);
        spline.SetRightTangent(n, Vector3.zero);  
    }

    // 物理演算のタイミングで実行（バネのシミュレーション）
    private void FixedUpdate()
    {
        // 水面の頂点（2番目以降）すべてに対して計算
        for (int i = 2; i < n; i++)
        {
            Vector3 pos = spline.GetPosition(i);

            // --- 簡易的なバネの計算 (減衰振動) ---
            // フックの法則: 変位(pos.y)と逆向きに力を加える
            vec[i - 2] -= k * pos.y;
            
            // 減衰処理: 毎回速度を90%にして、徐々に揺れを止める（摩擦の表現）
            vec[i - 2] *= 0.9f;
            
            // 計算した速度を現在の位置に加算
            pos += vec[i - 2] * Vector3.up;
            
            // 更新した位置をSplineに適用
            spline.SetPosition(i, pos);
        }
    }

    // 波を発生させ、左右に伝播させる非同期メソッド
    private async void WaveCreate(Collider2D collision)
    {
        // 衝突位置(X)から、Spline上の該当するインデックス(i)を逆算
        int i = (int)((collision.transform.position.x - transform.position.x - basePointPos.x * scale) / interval) + 1;

        // インデックスが範囲外にならないように制限
        if (i < 2) i = 2;
        else if (i > n - 1) i = n - 1;

        // 衝突した物体の落下速度(Y)を取得
        float y_collisionVelocity = collision.GetComponent<Rigidbody2D>().velocity.y;

        // 衝撃を計算して最初の頂点を動かす
        Vector3 pos = spline.GetPosition(i);
        Vector3 dv = pos + waterPow * y_collisionVelocity * Vector3.up;

        // 波の高さが最大値を超えないように制限（Clamp処理）
        if (dv.y > maxWave) dv = new Vector3(pos.x, maxWave, 0);
        else if (dv.y < -maxWave) dv = new Vector3(pos.x, -maxWave, 0);

        // 計算した位置をセット
        spline.SetPosition(i, dv);

        // --- 波の伝播処理 ---
        int l = i; // 左へ広がるインデックス
        int r = i; // 右へ広がるインデックス
        float y = dv.y; // 現在の波の高さ

        // 最初の波が最大でなければ、少しランダム性を入れて減衰させる
        if (y * y != maxWave * maxWave) y *= waveRate * Random.Range(0.8f, 1.2f);

        // 波が消えるまでループ
        while (true)
        {
            // 波の高さが最小値を下回ったら終了
            if (y * y < minWave * minWave) return;

            // 左隣へ伝播
            l--;
            if (l > 1) // インデックス1より大きい範囲で
            {
                pos = spline.GetPosition(l);
                spline.SetPosition(l, new Vector3(pos.x, y, 0));
            }

            // 右隣へ伝播
            r++;
            if (r < n) // インデックスnより小さい範囲で
            {
                pos = spline.GetPosition(r);
                spline.SetPosition(r, new Vector3(pos.x, y, 0));
            }

            // 左右ともに端まで到達していたら終了
            if (l <= 1 && r >= n) return;

            // 次の頂点へ波が伝わるまで少し待つ（これで波が広がるアニメーションになる）
            await UniTask.Delay((int)(waveTime * 1000));

            // 波の高さを減衰させる（ランダム要素を入れて自然に見せる）
            y *= waveRate * Random.Range(0.8f, 1.2f);
        }
    }

    // 物体が水に入った時の判定
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Rigidbody2D（物理挙動）を持たないオブジェクトは無視
        if (!collision.GetComponent<Rigidbody2D>()) return;
        WaveCreate(collision);
    }

    // 物体が水から出た時の判定
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.GetComponent<Rigidbody2D>()) return;
        WaveCreate(collision);
    }
}