using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Pts_script : MonoBehaviour
{
    public List<Sprite> pts_mtl;

    public void Pts_output_normal(int total, GameObject parent)
    {
        //Vector3 efc_scale = new Vector3(2, 2, 1);
        int count_1 = 0; // 1の数
        int pts = total;

        // 得点がマイナスか判定
        bool minus = false;
        if (pts < 0)
        {
            pts *= -1;
            minus = true;
        }

        // 点数フォント設定
        List<int> figures = Figures_cal(pts);

        GameObject display;

        // {i: 0(1桁), 1(2桁), 2(3桁), 3(4桁)}
        for (int i = 0; i < 4; i++) // i+1 = 桁数
        {
            // i+1桁目 を処理
            display = parent.transform.GetChild(i).gameObject; // i+1桁目のオブジェクト取得
            display.SetActive(true);
            display.GetComponent<Image>().sprite = pts_mtl[figures[i]]; // 得点のi+1桁目を反映
            display.transform.localPosition = new Vector3(-300 * i, 10, 0); // 得点のi+1桁目の位置を設定
            parent.transform.localPosition = new Vector3(0, 0, 0);
            display.transform.localScale = new Vector3(1, 1, 1);
            parent.transform.localScale = new Vector3(1, 1, 1);

            if ((figures[i] == 1) || (count_1 > 0))
            {
                if (figures[i] == 1)
                {
                    display.transform.localPosition += new Vector3(40f * count_1, 0, 0);
                    parent.transform.GetChild(i).gameObject.transform.localScale = new Vector3(1, 1, 1); // 1 のとき、x_scaleを調整
                    count_1++;
                } else
                {
                    display.transform.localPosition += new Vector3(40f * count_1 + -10, 0, 0);
                }
            }
            display.transform.localPosition += new Vector3(40f * count_1, 0, 0);

            // 桁が増えたとき、全体のx_scaleを調整
            if (i > 1) parent.transform.localScale = new Vector3(1 - (i - 1) * 0.2f, 1, 1);

            if (Mathf.Abs(total) >= Mathf.Pow(10, i + 1))
            {
                continue;
            }
            else
            {
                // {i: 0(1桁), 1(2桁), 2(3桁), 3(4桁)}
                for (int j = 0; j <= i; j++) // j = { 0:一の位, 1:十の位, 2:百の位, 3:千の位}
                {
                    // 得点表示を中央に寄せる（childのみ）
                    parent.transform.GetChild(j).gameObject.transform.localPosition += new Vector3(150 * i, 0, 0);
                    // {0, +150, +300, +450} | {-300, -150, 0, +150} | {-600, ---, -300, -150} | {-900, ---, ---, -450}
                }
                break;
            }
        }
        if (count_1 > 0)
        {
            if (pts >= Mathf.Pow(10, count_1))
            {
                switch (count_1)
                {
                    case 1:
                        parent.transform.localPosition = new Vector3(-30, 0, 0);
                        break;
                    case 2:
                        if (pts >= 1000)
                        {
                            parent.transform.localPosition = new Vector3(-50, 0, 0);
                            parent.transform.localScale = new Vector3(0.7f, 1, 1);
                        }
                        else
                        {
                            parent.transform.localPosition = new Vector3(-70, 0, 0);
                            parent.transform.localScale = new Vector3(0.9f, 1, 1);
                        }
                        break;
                    case 3:
                        parent.transform.localPosition = new Vector3(-80, 0, 0);
                        parent.transform.localScale = new Vector3(0.75f, 1, 1);
                        break;
                    case 4:
                        parent.transform.localPosition = new Vector3(-90, 0, 0);
                        parent.transform.localScale = new Vector3(0.7f, 1, 1);
                        break;
                }
            }
            else
            {
                // すべての数字が「1」の場合

                //parent.transform.localPosition = new Vector3(0, 0, 0);
                int d = 0;
                d = ((280 - (count_1 - 1) * 20) * (count_1 - 1)) / 2;

                for (int k = 0; k < count_1; k++)
                {
                    // 全ての数字の位置調整（childのみ）
                    parent.transform.GetChild(k).gameObject.transform.localPosition = new Vector3((280 - (count_1 - 1) * 20) * ((count_1 - 1) - k), 10, 0);
                }

                // count_1 が3以上の時、parent の大きさを調整
                if (count_1 >= 3)
                {
                    parent.transform.localScale = new Vector3(1.2f - (count_1 - 2) * 0.2f, 1, 1);
                }

                // 数字の位置調整（parentのみ）
                float scale_x = parent.transform.localScale.x;
                parent.transform.localPosition = new Vector3(-d * scale_x + 10, 0, 0);

            }
        }

        if (minus) parent.transform.GetChild(4).gameObject.SetActive(true);

        parent.SetActive(true);

        StartCoroutine(Effect(parent));
    }

    // 得点エフェクト
    IEnumerator Effect(GameObject parent)
    {
        float a = 0.15f;
        float limit = 0.3f;

        Vector3 efc_scale = new(2f, 2f, 1);
        for (float x = 2; x < 7; x += limit)
        {
            if (x <= 4)
            {
                efc_scale.x -= a;
                efc_scale.y -= a;
            }
            else if (x <= 4.5)
            {
                efc_scale.x += a;
                efc_scale.y += a;
            }
            else if (x <= 5)
            {
                efc_scale.x -= a;
                efc_scale.y -= a;
            }
            else
            {
                efc_scale.x = 1;
                efc_scale.y = 1;
                parent.transform.parent.localScale = efc_scale;
                break;
            }
            parent.transform.parent.localScale = efc_scale;
            yield return new WaitForSeconds(0.01f);
        }
    }

    private List<int> Figures_cal(int pts)
    {
        List<int> figures = new() { 0, 0, 0, 0 };// (1, 10, 100, 1000)

        do
        {
            if (pts >= 1000)
            {
                figures[3]++;
                pts -= 1000;
            }
            else if (pts >= 100)
            {
                figures[2]++;
                pts -= 100;
            }
            else if (pts >= 10)
            {
                figures[1]++;
                pts -= 10;
            }
            else if (pts >= 1)
            {
                figures[0]++;
                pts -= 1;
            }
        } while (pts != 0);

        return figures;
    }
}
