using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Generation選択
    public GameObject generations;
    public int gen;
    public int GenProperty
    {
        get { return gen; }
        set { gen = value; }
    }
    private readonly Sprite[] bonus;

    // チーム選択
    public GameObject displays;
    private int team; //{ 0 = "RedTeam", 1 = "BlueTeam", 2 = "GreenTeam" }
    public int TeamProperty
    {
        get { return team; }
        set { team = value; }
    }

    // 照明
    private bool light_setting;
    public bool LightProperty  //public 戻り値 プロパティ名
    {
        get { return light_setting; } //get {return フィールド名;}
        set { light_setting = value; } //set {フィールド名 = value;}
    }
    public GameObject[] Light = new GameObject[2]; // 0 = StudioLight, 1 = LightUP

    // ポイントtest
    private int Red_pts;
    private int Blue_pts;
    private int Green_pts;

    // ポイントゾーン
    public GameObject house_out;
    public GameObject house_middle;
    public GameObject house_in;
    MeshCollider mesh_middle;
    MeshCollider mesh_in;


    public int[] pts_setting = new int[3] { 5, 10, 30 }; // 得点設定 {out, middle, in}
    public int[] PtsProperty  //public 戻り値 プロパティ名
    {
        get { return pts_setting; } //get {return フィールド名;}
        set { pts_setting = value; } //set {フィールド名 = value;}
    }

    // デュアルカーリングR
    private bool R_;
    public bool R_Property
    {
        get { return R_; }
        set { R_ = value; }
    }

    // 得点計算
    public Stones BlueTeam;
    public Stones GreenTeam;
    public Stones RedTeam;
    private List<Stones> stones;
    private int[][][] stone_control; // { B:{1st,2nd,3rd}, G:{...}, R:{...} }
    private int[][] stoneInfo;
    public int[][] StoneInfoProperty
    {
        get { return stoneInfo; }
        set { stoneInfo = value; }
    }

    Pts_script pts_Script;
    public GameObject Red_parent;
    public GameObject Blue_parent;
    public GameObject Green_parent;

    // 制限時間、ゲーム終了
    private int time;
    public int TimeProperty
    {
        get { return time; }
        set { time = value; }
    }
    public int timeLimit;
    DefaltTimer defaltTimer;
    private bool timeup;
    public GameObject[] timers;
    public GameObject[] timeup_enables;
    public GameObject[] timeup_disables;

    // Start is called before the first frame update
    void Start()
    {
        // middle, in のコライダー
        mesh_middle = house_middle.GetComponent<MeshCollider>();
        mesh_in = house_in.GetComponent<MeshCollider>();

        timeLimit = time;
        bonusOn = false;
        bonusOff = false;

        // 制限時間
        timeup = false;
        defaltTimer = this.GetComponent<DefaltTimer>();
        if (gen != 2)
        {
            defaltTimer.Defalt_Timer(timeLimit, timers[team], false, true, new int[] { 0, 15 });
        }
        else
        {
            int repeat = (R_) ? 6 : 5;
            defaltTimer.ShiftTimer(timeLimit, timers[team], true, repeat, 0);
        }

        // チーム設定、display設定
        displays.transform.GetChild(team).gameObject.SetActive(true);
        team_sum = R_ ? 3 : 2;

        // Generation設定
        generations.transform.GetChild(gen).gameObject.SetActive(true);
        if (gen == 1) { BonusPocket(); MeshSetting(); }
        else if (gen == 2) KeepOut();

        // 照明設定
        Light[0].SetActive(light_setting);
        Light[1].SetActive(!light_setting);

        // ハウスの得点 を変更
        House_change();

        // pts_script の取得
        pts_Script = this.GetComponent<Pts_script>();
    }

    void Update()
    {
        // escキーでopening画面に移行
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadSceneAsync("opening", LoadSceneMode.Single);
        }
    }

    // 指定した残り時間になると起動
    public void GetAlert(int restTime, int repeat)
    {
        if (restTime == 15 && gen == 1) bonusOff = true;
        if (restTime == 0)
        {
            if (repeat == 0) timeup = true;
            else if (repeat > 0) keepoutClose = true;
        }
        else if (restTime == timeLimit && gen == 2)
        {
            if (repeat == 0 && R_) keepoutAllOpen = true;
            else if (repeat >= 0) keepoutOpen = true;
        }
    }

    void FixedUpdate() {
        
        // ボーナスポケット
        if (gen == 1)
        {
            if (bonusOn) BonusOn();
            if (bonusOff) BonusOff();
            if (time > 15) BonusWorking();
        }

        // KEEPOUT
        if (gen == 2)
        {
            if (keepoutOpen) KeepOutOpen();
            if (keepoutClose) KeepOutClose();
            if (keepoutAllOpen) KeepOutAllOpen();
        }

        // ゲーム終了後
        if (timeup) TimeUp();
    }

    // ゲーム終了後、ポイントゾーンのセリ上げ、得点表示など
    private void TimeUp()
    {
        mesh_middle.enabled = true;
        mesh_in.enabled = true;
        if (gen == 1) mesh_bonus.enabled = true;

        if (house_out.transform.position.y < 0.5)
        {
            house_out.transform.position += new Vector3(0, 0.0055f, 0);
        }
        if (house_middle.transform.position.y < 1.0)
        {
            house_middle.transform.position += new Vector3(0, 0.005f, 0);
        }
        if (house_in.transform.position.y < 1.5)
        {
            house_in.transform.position += new Vector3(0, 0.005f, 0);
        }
        else
        {
            StartCoroutine(nameof(Pts_display));
            timeup = false;
        }
    }

    // 点数計算、点数表示
    private IEnumerator Pts_display()
    {
        yield return new WaitForSeconds(2);
        foreach (GameObject enabled in timeup_enables)
        {
            enabled.SetActive(true);
        }
        foreach (GameObject disabled in timeup_disables)
        {
            disabled.SetActive(false);
        }
        Dictionary<GameObject, int> point_result;
        Dictionary<GameObject, Stones> bonus_point = new Dictionary<GameObject, Stones>(0);

        Red_pts = RedTeam.Pts_cal(pts_setting);
        Blue_pts = BlueTeam.Pts_cal(pts_setting);
        point_result = new Dictionary<GameObject, int> {
                    { Red_parent , Red_pts  },
                    { Blue_parent, Blue_pts}};

        // 三つ巴対決のみ適用
        if (R_)
        {
            Green_pts = GreenTeam.Pts_cal(pts_setting);
            point_result.Add(Green_parent, Green_pts);
        }

        var sorted = point_result.OrderBy((x) => x.Value);

        if (gen == 1)
        {
            bonus_point = new Dictionary<GameObject, Stones> {
                { Red_parent, RedTeam },
                { Blue_parent, BlueTeam }};

            if (R_) bonus_point.Add(Green_parent, GreenTeam);
        }

        foreach (var team in sorted)
        {
            yield return new WaitForSeconds(3);
            pts_Script.Pts_output_normal(team.Value, team.Key);
            if (gen == 1)
            {
                int t = team.Value;
                List<int> all = bonus_point[team.Key].BonusAllProperty;
                all.Reverse();
                if (all.Count > 0) yield return new WaitForSeconds(3);
                foreach (var bonus in all)
                {
                    bonus_point[team.Key].PtsRslt_Effect();
                    t += bonus;
                    pts_Script.Pts_output_normal(t, team.Key);

                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }

    // 得点パネルを変更
    private void House_change()
    {
        int[] load_panel = new int[3];
        load_panel[0] = (pts_setting[0] == 5) ? 0
            : (pts_setting[0] == 30) ? 2
            : 1;
        load_panel[1] = (pts_setting[1] == 10) ? 0
            : (pts_setting[1] == 50) ? 2
            : 1;
        load_panel[2] = (pts_setting[2] == 30) ? 0
            : (pts_setting[2] == 100) ? 2
            : 1;
        
        house_out.transform.GetChild(load_panel[0]).gameObject.SetActive(true);
        house_middle.transform.GetChild(load_panel[1]).gameObject.SetActive(true);
        house_in.transform.GetChild(load_panel[2]).gameObject.SetActive(true);
    }

    // ボーナスポケット ----------------------------------------
    MeshCollider mesh_bonus;
    MeshCollider mesh_bonus_out;
    MeshCollider mesh_out;

    public Mesh bonusPocket;
    public Mesh bonus_out;
    private bool bonusOn;
    private bool bonusOff;
    private GameObject bonusWall;

    // house_in に mesh の設定
    private void MeshSetting()
    {

        house_in.AddComponent<MeshCollider>();
        house_out.AddComponent<MeshCollider>();
        mesh_bonus = house_in.GetComponents<MeshCollider>()[1]; // mesh_bonus = ボーナスポケット
        mesh_bonus_out = house_out.GetComponents<MeshCollider>()[1]; // mesh_bonus_out = ハウスの外側（ボーナス）
        mesh_out = house_out.GetComponents<MeshCollider>()[0]; // mesh_out = ハウスの外側（通常）

        mesh_bonus.enabled = false;
        mesh_out.enabled = false;
        mesh_bonus.sharedMesh = bonusPocket;
        mesh_bonus_out.sharedMesh = bonus_out;

        mesh_bonus_out.enabled = true;
    }

    private void BonusPocket()
    {
        if (timeLimit >= 20) bonusOn = true;
        bonusWall = generations.transform.GetChild(gen).gameObject;
        house_in.GetComponent<MeshFilter>().mesh = bonusPocket;
        house_out.GetComponent<MeshFilter>().mesh = bonus_out;
    }
    private void BonusOn()
    {
        //mesh_in.enabled = true;
        if (!R_) mesh_bonus.enabled = true;

        if (house_in.transform.position.y < 1.99)
        {
            house_in.transform.position += new Vector3(0, 0.015f, 0);
            bonusWall.transform.position += new Vector3(0, 0.015f, 0);
        }
        else
        {
            bonusOn = false;
        }
    }
    private void BonusOff()
    {
        if (R_) mesh_in.enabled = true;

        if (house_in.transform.position.y > -0.02f)
        {
            house_in.transform.position -= new Vector3(0, 0.015f, 0);
            bonusWall.transform.position -= new Vector3(0, 0.015f, 0);
        }
        else
        {
            mesh_in.enabled = false;
            mesh_bonus_out.enabled = false;
            mesh_out.enabled = true;
            bonusOff = false;
        }
    }
    private void BonusWorking()
    {
        bonusWall.transform.RotateAround(bonusWall.transform.position, Vector3.up, -180 * Time.deltaTime);
    }

    // -----------------------------------------------------
    // KEEPOUTゲート
    private bool firstAttack; // { false: "red" | true: "blue"}
    public bool KeepOutProperty
    {
        get { return firstAttack; }
        set { firstAttack = value;  }
    }
    private GameObject[] keepout;
    private bool keepoutOpen;
    private bool keepoutAllOpen;
    private bool keepoutClose;
    private int team_sum;
    int order;

    // 初期設定
    private void KeepOut()
    {
        round = new int [3]{0,0,0};
        if (R_)
        {
            stones = new List<Stones> { BlueTeam, GreenTeam, RedTeam };
            stone_control = new int[3][][]; // ex) { [ (3, 0, 0), (3, 0, 0), (2, 1, 0)], ..., ... }
            stone_control[0] = new int[3][];
            stone_control[1] = new int[3][];
            stone_control[2] = new int[3][];
        }
        else
        { 
            stones = new List<Stones>() { BlueTeam, RedTeam };
            stone_control = new int[2][][];// ex) { [ (3, 0, 0), (3, 0, 0), (2, 1, 0)], ...}
            stone_control[0] = new int[3][];
            stone_control[1] = new int[3][];
        }

        for (int i = 0; i < team_sum; i++) {
            stones[i].enabled = false;
            StoneDistribution(stoneInfo[i], i);
        }

        if (!firstAttack) order = team_sum -1;
        keepout = new GameObject[team_sum]; // { 0 = b, 1 = r}
        for (int i = 0; i < team_sum; i++)
        {
            keepout[i] = generations.transform.GetChild(gen).GetChild(i).gameObject;
        }
    }

    private void StoneDistribution(int[] stone_Info, int i_team)
    {
        int[] Local_stoneInfo = stone_Info;
        int leng = Local_stoneInfo.Sum();
        int[] Round_Set = new int[3] { leng / 3, leng / 3 , leng / 3 };
        if (leng % 3 == 2) Round_Set[1] += 1;
        if (leng % 3 >= 1) Round_Set[2] += 1;

        for (int i_lound = 2; i_lound >= 0; i_lound--)
        {
            int[] Round = new int[3];
            for (int i_stone = 2; i_stone >= 0; i_stone--)
            {
                int warn = 0;
                while (Local_stoneInfo[i_stone] > 0)
                {
                    warn++;
                    if (Local_stoneInfo[i_stone] >= Round_Set[i_lound])
                    {
                        Round[i_stone] += Round_Set[i_lound];
                        Local_stoneInfo[i_stone] -= Round_Set[i_lound];
                        Round_Set[i_lound] = 0;
                        stone_control[i_team][i_lound] = Round;
                        break;
                    } else if (Local_stoneInfo[i_stone] > 0)
                    {
                        Round[i_stone] += Local_stoneInfo[i_stone];
                        Round_Set[i_lound] -= Local_stoneInfo[i_stone];
                        Local_stoneInfo[i_stone] = 0;
                    }
                    if (warn > 100)
                    {
                        break;
                    }
                }
            }
        }
        
/*        _stoneInfo[0] % 3
        int[] _1st = 
        return _stoneInfo[]*/
    }

    private int[] round;
    private void KeepOutOpen()
    {
        if (keepout[order].transform.localRotation.x > -0.7f)
        {
            keepout[order].transform.Rotate(Vector3.right, -180 * Time.deltaTime);
            
        } else
        {
            stones[order].enabled = true;
            stones[order].StoneProperty = stone_control[order][round[order]];
            stones[order].ReWriteStone();
            round[order]++;
            keepoutOpen = false;
        }
    }

    private void KeepOutClose()
    {
        if (keepout[order].transform.localRotation.x < 0)
        {
            keepout[order].transform.Rotate(Vector3.right, 180 * Time.deltaTime);
        } else
        {
            stones[order].enabled = false;
            order += firstAttack ? 1 : team_sum - 1;
            order %= team_sum;
            keepoutClose = false;
        }
    }

    private void KeepOutAllOpen()
    {
        for (int i = 0; i < team_sum; i++)
        { 
            if (keepout[i].transform.localRotation.x > -0.7f)
            {
                keepout[i].transform.Rotate(Vector3.right, -180 * Time.deltaTime);
            }else
            {
                stones[i].enabled = true;
                stones[i].StoneProperty = stone_control[i][2];
                stones[i].ReWriteStone();
                if(i == team_sum-1) keepoutAllOpen = false;
            }
            
        }
    }
}
