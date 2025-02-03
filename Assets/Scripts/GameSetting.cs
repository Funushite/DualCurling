using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System.Linq;
using System;
using UnityEngine.SearchService;

public class GameSetting : MonoBehaviour
{
    private int mode; // 0 = Normal, 1 = R
    readonly string[] mode_name = new string[2] { "DualCurling", "DualCurlingR" };
    private int gen = 0; // {0 = 1st, 1 = Bonus, 2 = Keepout}
    public Sprite[] sprites = new Sprite[2];
    public GameObject title;
    public GameObject mode_button;

    public GameObject[] Canvas_GO = new GameObject[5];

    // -------------------------------
    Scene DualCurling;
    AsyncOperation pre_scene;
    GameObject eventsystem;

    private void Start()
    {
        gen = 0;
        firstAttack = true; // KEEPOUT
        mode = 0; // R
        eventsystem = GameObject.Find("EventSystem"); // EventSystemの重複を防ぐ
        stone[0] = new int[3] {8,1,0 };
        stone[1] = new int[3] {8,1,0 };
        stone[2] = new int[3] {8,1,0 };
        pts_dic["pts_out"] = new int[] { 5, 10, 30, 0 };
        pts_dic["pts_middle"] = new int[] { 10, 30, 50, 1 };
        pts_dic["pts_in"] = new int[] { 30, 50, 100, 2 };
        light_ = true;
        time = 60;
        time_value = 10;
    }

    void Update()
    {

        // escキーでリセット
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadSceneAsync("opening", LoadSceneMode.Single);

        }
    }

    // --------------------------------------------------
    // スタート画面
    public void START()
    {
        Canvas_GO[1].SetActive(true);
        Canvas_GO[0].SetActive(false);
    }
    public void ChangeMode() // デュアルカーリング or R の選択｛0 = Normal, 1 = R｝
    {
        mode = (mode == 0) ? 1 : 0;
        mode_button.GetComponent<Image>().sprite = sprites[mode];
        title.GetComponent<Text>().text = (mode == 0) ? "デュアルカーリング" : "デュアルカーリング R";
    }

    // --------------------------------------------------
    // Generation選択
    // {gen: 0 = 1st, 1 = Bonus, 2 = Keepout}

    public void Generation()
    {
        switch (Self().name){
            case "Button_1st":
                gen = 0;
                break;
            case "Button_2nd":
                gen = 1;
                break;
            case "Button_3rd":
                gen = 2;
                time = 10;
                time_value = 1;
                break;
        }
    }
    public void GenSeleceted()
    {
        Canvas_GO[1].SetActive(false);
        if(gen != 2) Canvas_GO[2].SetActive(true);
        else Canvas_GO[5].SetActive(true);
    }

    // --------------------------
    // GameSetting
    GameObject grandparent;
    GameObject parent;
    GameObject self;
    int index = 0;
    int time_value;
    Text text;
    bool firstAttack;

    Slider slider;
    int time;
    bool light_ = true;
    readonly int[] pts_setting = new int[3] {5, 10, 30};

    GameObject GrandParent()
    {
        return EventSystem.current.currentSelectedGameObject.transform.parent.parent.gameObject;
    }
    GameObject Parent()
    {
        return EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
    }
    GameObject Self()
    {
        return EventSystem.current.currentSelectedGameObject;
    }


    public void Timer() // 制限時間
    {
        self = Self();
        slider = self.GetComponent<Slider>(); // slider
        text = Parent().GetComponent<Text>(); // text
        time = Mathf.FloorToInt(slider.value) * time_value;
        text.text = time.ToString();
    }
    public void Light() // 照明設定
    {
        if (Self().GetComponent<Toggle>().isOn) light_ = true;
        else light_ = false;
    }

    public void FirstAttack() // 先攻後攻
    {
        self = Self();
        slider = self.GetComponent<Slider>();
        firstAttack = (Mathf.FloorToInt(slider.value) == 1);
    }

    // 得点設定
    readonly Dictionary<string, int[]> pts_dic = new() {
        { "pts_out", null },
        { "pts_middle", null },
        { "pts_in", null },
    };
    public void Pts() 
    {
        self = Self(); // slider(子)
        slider = self.GetComponent<Slider>(); // slider
        index = Mathf.FloorToInt(slider.value);
        parent = Parent();// pts(親)
        text = parent.GetComponent<Text>();
        pts_setting[pts_dic[parent.name][3]] = pts_dic[parent.name][index];

        text.text = pts_setting[pts_dic[parent.name][3]].ToString();
    }

    public void Setted()
    {
        Canvas_GO[2].SetActive(false);
        Canvas_GO[5].SetActive(false);
        if (mode == 0) Canvas_GO[3].SetActive(true);
        else if (mode == 1) Canvas_GO[4].SetActive(true);
    }

    // --------------------------
    // Team設定
    private readonly Stones stones;
    private readonly int[][] stone = new int[3][]; //{ 0 = "RedTeam", 1 = "BlueTeam", 2 = "GreenTeam" }
    int t_stone = 0;

    int team = 0; //{ 0 = "RedTeam", 1 = "BlueTeam", 2 = "GreenTeam" }

    public void Count()
    {
        grandparent = GrandParent();
        parent = grandparent.transform.GetChild(0).gameObject;
        text = parent.GetComponent<Text>();

        if (grandparent.CompareTag("red")) index = 0;
        else if (grandparent.CompareTag("blue")) index = 1;
        else if (grandparent.CompareTag("green")) index = 2;
    }
    public void CountUP() // ストーンの数（増加）
    {
        int limit = (gen != 2) ? 15 : 12;
        if (stone[index].Sum() < limit)
        {
            if (parent.CompareTag("regular")) { stone[index][0]++; t_stone = stone[index][0]; }
            else if (parent.CompareTag("double")) { stone[index][1]++; t_stone = stone[index][1]; }
            else if (parent.CompareTag("gold")) { stone[index][2]++; t_stone = stone[index][2]; }

            text.text = "×" + t_stone;
        }
    }
    public void CountDOWN() // ストーンの数（減少）
    {
        int limit = (gen != 2) ? 1 : 3;
        if (stone[index].Sum() > limit)
        {
            if (parent.CompareTag("regular") && stone[index][0] > 0) { stone[index][0]--; t_stone = stone[index][0]; }
            else if (parent.CompareTag("double") && stone[index][1] > 0) { stone[index][1]--; t_stone = stone[index][1]; }
            else if (parent.CompareTag("gold") && stone[index][2] > 0) { stone[index][2]--; t_stone = stone[index][2]; }

            text.text = "×" + t_stone;
        }
    }
    public void Team() // チーム選択
    {
        self = Self();
        team = (self.CompareTag("red")) ? 0 :
            (self.CompareTag("blue")) ? 1 :
            (self.CompareTag("green")) ? 2 :
            99;
        GameStart();
    }

    // --------------------------------------
    // ゲームスタート
    private void GameStart() 
    {
        pre_scene = SceneManager.LoadSceneAsync(mode_name[mode], LoadSceneMode.Additive);
        Destroy(eventsystem);       
        pre_scene.allowSceneActivation = true;
        DualCurling = SceneManager.GetSceneByName(mode_name[mode]);
        StartCoroutine(nameof(NowRoading));
    }
    private IEnumerator NowRoading()
    {
        int cnt = 0;
        while (true)
        {
            yield return new WaitForNextFrameUnit();

            if (DualCurling.GetRootGameObjects().Length >= (10 + mode))
            {
                for (int i = 0; i < DualCurling.GetRootGameObjects().Length; i++)
                {
                    GameObject gameobject = DualCurling.GetRootGameObjects()[i];
                    GameManager gameManager = gameobject.GetComponent<GameManager>();

                    if (gameobject.name == "RedTeam")
                    {
                        if (gen != 2)gameobject.GetComponent<Stones>().StoneProperty = stone[0];
                        gameobject.GetComponent<Stones>().R_Property = (mode == 1);
                        gameobject.GetComponent<Stones>().GenProperty = gen;
                        if (team != 0) gameobject.GetComponent<Stones>().CPU = true;
                    }
                    else if (gameobject.name == "BlueTeam")
                    {
                        if (gen != 2)gameobject.GetComponent<Stones>().StoneProperty = stone[1];
                        gameobject.GetComponent<Stones>().R_Property = (mode == 1);
                        gameobject.GetComponent<Stones>().GenProperty = gen;
                        if (team != 1) gameobject.GetComponent<Stones>().CPU = true;
                    }
                    else if (gameobject.name == "GreenTeam")
                    {
                        if (gen != 2)gameobject.GetComponent<Stones>().StoneProperty = stone[2];
                        gameobject.GetComponent<Stones>().R_Property = (mode == 1);
                        gameobject.GetComponent<Stones>().GenProperty = gen;
                        if (team != 2) gameobject.GetComponent<Stones>().CPU = true;
                    }
                    else if (gameobject.name == "GameManager")
                    {
                        gameManager.GenProperty = gen;
                        if (gen == 2)
                        {
                            gameManager.KeepOutProperty = firstAttack;
                            gameManager.StoneInfoProperty = 
                                (mode ==1) ? new int[3][]{ stone[1],stone[2],stone[0] }: // {B, G, R}
                                            new int[2][]{ stone[1],stone[0]}; // {B, R}
                        }
                        gameManager.R_Property = (mode == 1);
                        gameManager.TeamProperty = team;
                        gameManager.LightProperty = light_;
                        gameManager.TimeProperty = time;
                        gameManager.PtsProperty = pts_setting;
                    }

                    if (gameobject.name != "DontActive")
                    {
                        DualCurling.GetRootGameObjects()[i].SetActive(true);
                    }
                }
                SceneManager.UnloadSceneAsync("opening");
                break;
            }
            cnt++;

            if (cnt >= 100) {
                break;

            }
        }
    }
}
