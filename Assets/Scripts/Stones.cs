using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Stones : MonoBehaviour
{
    private string team;

    // CPU
    public bool CPU = false;
    public GameManager GameManager;

    // 得点計算
    private int total;

    // デュアルカーリング or デュアルカーリングR
    private bool R_;
    public bool R_Property
    {
        get { return R_; }
        set { R_ = value; }
    }
    float team_x;
    float team_z;
    public GameObject team_Camera;

    // ストーン
    public GameObject team_stone;
    public GameObject team_double;
    public GameObject team_gold;
    public List<GameObject> team_stones_1;
    public List<GameObject> team_stones_2;
    public List<GameObject> team_stones_3;
    private int[] stone = new int[3] { 0, 0, 0 }; // ストーンの数を指定 {regular, double, gold}
    public int[] StoneProperty  //public 戻り値 プロパティ名
    {
        get { return stone; } //get {return フィールド名;}
        set { stone = value; } //set {フィールド名 = value;}
    }

    GameObject controrable_stone;
    Rigidbody rb;
    bool ready = false;
    string tag_name;

    // Bonus Pocket
    private int gen;
    public int GenProperty
    {
        get { return gen; }
        set { gen = value; }
    }
    public Sprite[] bonus = new Sprite[4];
    public GameObject BonusDisplay;
    public GameObject BonusResult;
    private readonly List<int> bonusAll = new();
    public List<int> BonusAllProperty { get { return bonusAll; } }

    private GameObject bg;
    private GameObject bgResult;


    // UI 残りストーンを表示
    public Text stone_text_1;
    public Text stone_text_2;
    public Text stone_text_3;
    private readonly string stone_text = "×";

    // 速度
    Vector3 force;
    Vector3 m_stapos;
    Vector3 m_endpos;
    Vector3 m_distance;
    Vector3 force_speeds;
    float st_time = 0;


    // CPU 仮
    int[] shoot_time;
    int tl;

    void Start()
    {
        

        if (gen == 1) BonusDisplay.SetActive(true);

        // 得点をリセット
        total = 0;

        // 残りのストーンの数
        ReWriteStone();

        team = this.name; // オブジェクトネーム　からチームカラーを判別 { "RedTeam" | "BlueTeam" | "GreenTeam" }

        /* チーム数によって
         *  ・ストーンの射出角度
         *  ・ストーン生成のボタン
         * を変更する
         */
        if (team == "RedTeam")
        {
            (team_x, team_z) = !R_ ? (1f, 2.74f) : (1f, 1.2f);
        }
        else if (team == "BlueTeam")
        {
            (team_x, team_z) = !R_ ? (-1f, 2.74f) : (-1f, 1.2f);
        }
        else
        {
            (team_x, team_z) = (0f, 1.54f);
        }

        // BonusPocket
        bg = BonusDisplay.transform.GetChild(0).gameObject;
        bgResult = BonusResult.transform.GetChild(0).gameObject;


        // 仮
        if (CPU) CPU_test();
    }

    void Update()
    {
        if (!CPU)
        {
            if (ready && stone.Sum() > 0)
            {
                // 右クリック長押し
                if (Input.GetMouseButtonDown(1)) m_stapos = Input.mousePosition;
                if (Input.GetMouseButton(1))
                {
                    OnMouseDrag();
                }

                if (!Input.GetMouseButton(1)) Force(); // ストーン射出

                // -------------

                // ストーン変更
                if (Input.GetMouseButtonDown(2)) ChangeStone();
            }
            else if (stone.Sum() > 0)
            {
                // ストーンがない場合、新たに出現させる
                if (Input.GetMouseButtonDown(2))
                {
                    // レギュラーストーン
                    if (stone[0] > 0) controrable_stone = Stone_gen(team_stone);
                    // ダブルストーン
                    else if (stone[1] > 0) controrable_stone = Stone_gen(team_double);
                    // ゴールドストーン
                    else if (stone[2] > 0) controrable_stone = Stone_gen(team_gold);

                    ready = true;
                }
            }
        }
        else
        {
            if (shoot_time.Contains(GameManager.TimeProperty))
            {
                shoot_time = shoot_time.Where(x => x != GameManager.TimeProperty).ToArray();
                ready = true;
            }

            if (ready)
            {
                ready = false;

                if (gen != 1){
                    // レギュラーストーン
                    if (stone[0] > 0) controrable_stone = Stone_gen(team_stone);
                    // ダブルストーン
                    else if (stone[1] > 0) controrable_stone = Stone_gen(team_double);
                    // ゴールドストーン
                    else if (stone[2] > 0) controrable_stone = Stone_gen(team_gold);
                }
                else
                {   
                    // ゴールドストーン
                    if (stone[2] > 0) controrable_stone = Stone_gen(team_gold);
                    // ダブルストーン
                    else if (stone[1] > 0) controrable_stone = Stone_gen(team_double);
                    // レギュラーストーン
                    else if (stone[0] > 0) controrable_stone = Stone_gen(team_stone);
                }

                float y;
                float mp = 0;
                mp = (!R_) ? Random.Range(-1.8f, 1.8f) :
                               Random.Range(-1.5f, 1.5f);
                
                // KeepOutのみ => 初代、Bonus終了後 => Bonus作動中
                if (gen == 2)
                {
                    controrable_stone.transform.position = (!R_) ?
                        new Vector3(-9.7f * team_x + team_z * mp, 2.41f, -8.7f + -1 * team_x * mp) :
                        new Vector3((-18.3f * team_x - 4f) + team_z * 2.3f * mp, 2.41f, -9.6f + (Mathf.Abs(team_x) * 6.7f) + -1 * team_x * 2.3f * mp);
                    int c = team_stones_1.Count() + team_stones_2.Count() + team_stones_3.Count();
                    y = Random.Range(10f + c*0.3f, 16f + c*0.3f);
                }
                else if ((gen == 1 && GameManager.TimeProperty < 15) || gen == 0)
                {
                    controrable_stone.transform.position = (!R_) ?
                        new Vector3(-9.7f * team_x + team_z * mp, 2.41f, -8.7f + -1 * team_x * mp) :
                        new Vector3((-18.3f * team_x - 4f) + team_z * 2.3f * mp, 2.41f, -9.6f + (Mathf.Abs(team_x) * 6.7f) + -1 * team_x * 2.3f * mp);
                    int c = team_stones_1.Count() + team_stones_2.Count() + team_stones_3.Count();
                    y = Random.Range(10f + c * 0.2f, 18f+ c * 0.2f);
                }
                else
                {
                    y = Random.Range(16f, 24f);
                }
                force = new Vector3(team_x * y, 0, team_z * y);
                if (R_) force *= 1.7f;
                rb.AddForce(force, ForceMode.Impulse);

                if (tag_name == "regular") { stone[0]--; stone_text_1.text = stone_text + stone[0]; }
                else if (tag_name == "double") { stone[1]--; stone_text_2.text = stone_text + stone[1]; }
                else if (tag_name == "gold") { stone[2]--; stone_text_3.text = stone_text + stone[2]; }


            }
        }
    }

    void Force()
    {
        // マウスの移動距離 / 時間 = force
        if (Input.GetMouseButton(0))
        {
            st_time += Time.deltaTime;
        }
        if (Input.GetMouseButtonDown(0))
        {
            m_stapos = Input.mousePosition;

            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        if (Input.GetMouseButtonUp(0))
        {
            m_endpos = Input.mousePosition;
            m_distance = m_endpos - m_stapos;

            force_speeds = m_distance / (st_time * 200);
            force_speeds.y = Mathf.Clamp(force_speeds.y, 0f, 30f);

            if (m_distance.y > 400 && st_time <= 1)
            {
                st_time /= 2;
                force = new Vector3(team_x * force_speeds.y + force_speeds.x * team_z, 0, team_z * force_speeds.y - force_speeds.x * team_x); //　チームによって変化
                if (R_) force *= 1.7f;
                rb.AddForce(force, ForceMode.Impulse);

                if (tag_name == "regular") { stone[0]--; stone_text_1.text = stone_text + stone[0]; }
                else if (tag_name == "double") { stone[1]--; stone_text_2.text = stone_text + stone[1]; }
                else if (tag_name == "gold") { stone[2]--; stone_text_3.text = stone_text + stone[2]; }
                ready = false;
            }
            /*else
            {
                StartCoroutine(nameof(Re_ready));
            }*/

            m_endpos = new Vector3(0, 0, 0);
            m_stapos = new Vector3(0, 0, 0);
            st_time = 0;
        }
    }

    void ChangeStone()
    {
        if (tag_name == "regular") // レギュラー　→
        {
            if (stone[1] + stone[2] > 0) ChangeDestroy();
            if (stone[1] > 0) controrable_stone = Stone_gen(team_double);
            else if (stone[2] > 0) controrable_stone = Stone_gen(team_gold);
        }
        else if (tag_name == "double") // ダブル　→
        {
            if (stone[0] + stone[2] > 0) ChangeDestroy();
            if (stone[2] > 0) controrable_stone = Stone_gen(team_gold);
            else if (stone[0] > 0) controrable_stone = Stone_gen(team_stone);
        }
        else if (tag_name == "gold") //  ゴールド →
        {
            if (stone[0] + stone[1] > 0) ChangeDestroy();
            if (stone[0] > 0) controrable_stone = Stone_gen(team_stone);
            else if (stone[1] > 0) controrable_stone = Stone_gen(team_double);
        }
    }

    // ストーン変更の処理（削除）
    void ChangeDestroy()
    {
        if (tag_name == "regular") team_stones_1.RemoveRange(team_stones_1.Count - 1, 1);
        else if (tag_name == "double") team_stones_2.RemoveRange(team_stones_2.Count - 1, 1);
        else if (tag_name == "gold") team_stones_3.RemoveRange(team_stones_3.Count - 1, 1);

        Destroy(controrable_stone);
    }


    // マウスドラッグ中、ストーンを左右移動
    void OnMouseDrag()
    {
        if (ready)
        {
            float mousePos = (Input.mousePosition.x - m_stapos.x) / 300;
            mousePos = (!R_) ? Mathf.Clamp(mousePos, -1.8f, 1.8f) :
                               Mathf.Clamp(mousePos, -1.5f, 1.5f);

            controrable_stone.transform.position = (!R_) ?
                new Vector3(-9.7f * team_x + team_z * mousePos, 2.41f, -8.7f + -1 * team_x * mousePos) :
                new Vector3((-18.3f * team_x - 4f) + team_z * 2.3f * mousePos, 2.41f, -9.6f + (Mathf.Abs(team_x) * 6.7f) + -1 * team_x * 2.3f * mousePos);
        }
    }

    // ストーンを投じず、移動だけした場合
    IEnumerable Re_ready()
    {
        ready = false;
        yield return new WaitForSeconds(0.5f);
        ready = true;
    }

    // ストーン生成
    private GameObject Stone_gen(GameObject stone)
    {
        GameObject gen = (!R_) ?
            Instantiate(stone, new Vector3(-9.7f * team_x, 2.4f, -8.7f), Quaternion.Euler(0, 20 * team_x, 0), transform) :
            Instantiate(stone, new Vector3(-18.3f * team_x - 4f, 2.4f, -9.6f + (Mathf.Abs(team_x) * 6.7f)), Quaternion.Euler(0, 40.5f * team_x, 0), transform);
        tag_name = gen.tag;

        if (tag_name == "regular") team_stones_1.Add(gen);
        else if (tag_name == "double") team_stones_2.Add(gen);
        else if (tag_name == "gold") team_stones_3.Add(gen);

        rb = gen.GetComponent<Rigidbody>();
        return gen;
    }


    // 得点計算
    public int Pts_cal(int[] pts_setting)
    {
        int z;
        int by;

        for (int i = 0; i < team_stones_1.Count + team_stones_2.Count + team_stones_3.Count; i++)
        {
            GameObject stone = transform.GetChild(i).gameObject;


            by = (stone.CompareTag("double")) ? 2 :
                (stone.CompareTag("gold")) ? 3 :
                1;

            z = (int)(stone.transform.position.y * 100);

            if (stone.activeSelf)
            {
                total += (z < 269) ? 0 :
                (z < 327) ? by * pts_setting[0] :
                (z < 377) ? by * pts_setting[1] :
                by * pts_setting[2];
            }
            else
            {
                BonusAdd(by, pts_setting[2]);
            }
        }
        return total;
    }

    public void BonusAdd(int by, int pts_bonus)
    {
        if (gen == 1)
        {
            bonusAll.Add(by * pts_bonus);
        }
    }

    public void BonusIn(GameObject bonusStone)
    {
        GameObject bonus_image = new();
        GameObject bonus_image2 = new();
        int count;
        bonus_image.AddComponent<Image>();
        bonus_image2.AddComponent<Image>();
        RectTransform image2 = bonus_image2.GetComponent<RectTransform>();
        RectTransform bg2 = BonusResult.GetComponent<RectTransform>();
        if (bonusStone.CompareTag("regular"))
        {
            stone[0]++;
            bonus_image.GetComponent<Image>().sprite = bonus[0];
            bonus_image2.GetComponent<Image>().sprite = bonus[0];
        }
        else if (bonusStone.CompareTag("double"))
        {
            stone[1]++;
            bonus_image.GetComponent<Image>().sprite = bonus[1];
            bonus_image2.GetComponent<Image>().sprite = bonus[1];
        }
        else if (bonusStone.CompareTag("gold"))
        {
            stone[2]++;
            bonus_image.GetComponent<Image>().sprite = bonus[2];
            bonus_image2.GetComponent<Image>().sprite = bonus[2];
        }

        count = BonusDisplay.transform.childCount - 1;
        if (count < 1)
        {
            bg.SetActive(true);
            BonusResult.SetActive(true);
        }
        else
        {
            bg.transform.localPosition += new Vector3(140, 0, 0);
            BonusResult.transform.localPosition += new Vector3(0, 210, 0);
            bg2.sizeDelta += new Vector2(0, 140);
        }
        image2.anchorMax = new Vector2(0.5f, 1);
        image2.anchorMin = new Vector2(0.5f, 1);
        bonus_image.transform.SetParent(BonusDisplay.transform);
        bonus_image2.transform.SetParent(BonusResult.transform);
        bonus_image.transform.localPosition = new Vector3(-880 + 140 * count, 440, 0);
        bonus_image2.transform.localPosition = new Vector3(0, -140 * count + -120, 0);
        bonus_image.transform.localScale = new Vector3(1.2f, 1.2f, 0);
        bonus_image2.transform.localScale = new Vector3(1.2f, 1.2f, 0);


        ReWriteStone();
    }

    public void PtsRslt_Effect()
    {
        RectTransform bg2 = BonusResult.GetComponent<RectTransform>();
        BonusResult.transform.localPosition -= new Vector3(0, 210, 0);
        bg2.sizeDelta -= new Vector2(0, 140);

        int last = BonusResult.transform.childCount - 1;
        if (last > 1)
        {
            Destroy(BonusResult.transform.GetChild(last).gameObject);
        }
        else if (last == 1)
        {
            BonusResult.SetActive(false);
        }
    }

    public void ReWriteStone()
    {
        stone_text_1.text = stone_text + stone[0];
        stone_text_2.text = stone_text + stone[1];
        stone_text_3.text = stone_text + stone[2];

        if (CPU) CPU_test();
    }

    private void CPU_test()
    {
        // 仮
        if (gen == 2) tl = GameManager.timeLimit;
        else tl = GameManager.TimeProperty;
        int ss = stone.Sum();
        shoot_time = new int[ss];
        for (int i = 0; i < shoot_time.Length; i++)
        {
            int t = 0;
            int err = 0;
            while (err <= 100)
            {
                err++;
                t = i * (int)(tl / ss) + Random.Range(1, (int)(tl / ss) + 1);
                if (!shoot_time.Contains(t))
                {
                    shoot_time[i] = t;
                    break;
                }
            }
        }
    }
}
