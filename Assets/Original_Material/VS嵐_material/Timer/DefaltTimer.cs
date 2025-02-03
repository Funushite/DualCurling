using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DefaltTimer : MonoBehaviour
{
    /// <summary>
    /// 1�`199�b�܂ŃJ�E���g�\
    /// </summary>

    // �t�H���_�\��
    // canvas
    //      ->  timer(empty GameObject)
    //          ->  timer_1, timer_10, timer_100
    readonly int[] index = new int[2]; // {_1, _10}
    readonly Image[] images = new Image[3]; // {_1, _10, _100}

    // GameManager�Ɠ����I�u�W�F�N�g�ɕt�^
    // GameManager��TimeProperty�������
    private GameManager manager;
    private int restTime;
    private int timeLimit;
    private int[] alerts;

    // �ڍאݒ�
    private bool _00; // �c��0�b��\�����邩
    private bool red; // �c��10�b�ŐԐF�ɂ��邩

    // ��㐧
    private bool yellow;
    private int repeat;
    private int interval;

    private Sprite[] material;

    void Start()
    {
        _00 = false;
        red = false;

        yellow = false;
        repeat = 0;
        interval = 0;

        // �g�p����摜
        material = Resources.LoadAll<Sprite>("Timer");

        // GameManager�̎擾
        manager = this.GetComponent<GameManager>();
    }

    // �ʏ�̃^�C�}�[
    public void Defalt_Timer(int timeLimit_, GameObject parent, bool mode_00, bool mode_red, int[] alert)
    {
        // �������Ԃ��擾
        restTime = timeLimit_;

        // image���擾
        for (int i = 0; i < 3; i++)
        {
            images[i] = parent.transform.GetChild(i).GetComponent<Image>();
        }

        // �ڍאݒ���擾
        _00 = mode_00;
        red = mode_red;

        // �Ԃ����Ԃ��擾
        alerts = alert;

        StartCoroutine(nameof(Activation));
        DefaltDisplay();
    }

    // ��㐧�^�C�}�[
    public void ShiftTimer(int timeLimit_, GameObject parent, bool mode_yellow, int repeat_, int interval_)
    {
        // �������Ԃ��擾
        restTime = timeLimit_;
        timeLimit = timeLimit_;

        // image���擾
        for (int i = 0; i < 3; i++)
        {
            images[i] = parent.transform.GetChild(i).GetComponent<Image>();
        }

        // �ڍאݒ���擾
        _00 = true;
        yellow = mode_yellow;
        red = false;
        repeat = repeat_;
        interval = interval_;

        alerts = new int[]{0, timeLimit};

        StartCoroutine(nameof(Activation));
        DefaltDisplay();
        if (alerts.Contains(restTime)) manager.GetAlert(restTime, repeat);
    }

    // �o���{�^�C�}�[�N��
    private IEnumerator Activation()
    {
        DefaltDisplay();
        while (true)
        {
            if (images[0].color.a < 1)
            {
                for (int i = 0; i < 3; i++)
                {
                    images[i].color += new Color(0, 0, 0, 0.05f);
                }
                yield return new WaitForSeconds(0.001f * Time.deltaTime);
                continue;
            }
            else
            {
                StartCoroutine(nameof(CountDown));
                break;
            }
        }
    }

    // �J�E���g�_�E��
    private IEnumerator CountDown()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            restTime--;
            manager.TimeProperty = restTime;
            if (alerts.Contains(restTime))
            {
                manager.GetAlert(restTime, repeat);
            }

            DefaltDisplay();

            if (repeat == 0 && restTime == 0) break;
            else if (repeat > 0 && restTime == 0)
            {
                StartCoroutine(nameof(Interval));
                break;
            }
        }
    }

    // �\��
    private void DefaltDisplay()
    {
        index[0] = (restTime % 100) % 10;
        index[1] = (restTime % 100) / 10;

        // spriteSetting
        if (restTime <= 10 && red == true)
        {
            index[0] += 10;
            index[1] += 10;
        }
        if (yellow)
        {
            if(restTime <= 3)
            {
                index[0] += 30;
                index[1] += 30;
            } else
            {
                index[0] += 20;
                index[1] += 20;
            }
        }

        if (restTime >= 0)
        {
            images[0].sprite = material[index[0]];
            images[1].sprite = material[index[1]];
        }

        // unactivation
        if (restTime == 0 && _00 == false)
        {
            images[0].enabled = false;
            images[1].enabled = false;
        }
        if (restTime >= 100) images[2].sprite = material[1];
        else images[2].enabled = false;
    }

    // ��㐧�\��
    private IEnumerator Interval()
    {
        yield return new WaitForSeconds(1); // 1 -> 0
        StartCoroutine(nameof(Fadeout));
    }

    // �^�C�}�[����
    private IEnumerator Fadeout()
    {
        while (true)
        {
            if (images[0].color.a > 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    images[i].color -= new Color(0, 0, 0, 0.05f);
                }
                yield return new WaitForSeconds(0.001f * Time.deltaTime);
                continue;
            }
            else
            {
                yield return new WaitForSeconds(interval);
                restTime = timeLimit;
                repeat--;
                StartCoroutine(nameof(Activation));
                if (alerts.Contains(restTime)) manager.GetAlert(restTime, repeat);
                break;
            }
        }
    }
}