using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHandler : MonoBehaviour
{
    //Buttons:
    //Activity Timer : Setting time (arrow buttons?), setting on off(restarting). 
    //Focus Mode: On/off - elapsed time maybe?
    //Music: On/off - maybe slider for volume?
    //Spawn Object - not now.

    //Double tap : toggle UI - appears/disappears with fading in out.
    bool doFadeMain = false;
    bool doFadeSetter = false;

    bool activeTimer = false;
    bool activeUI = true;

    bool timerBtnPlay = true; //state of the button if play true, then display play button properties.
    [SerializeField] Sprite timerPlay;
    [SerializeField] Sprite timerPause;

    bool musicToggle = true; //music on at start
    [SerializeField] Sprite musicOn;
    [SerializeField] Sprite musicOff;

    float timerTime = 0; //seconds

    float fadeSpeed = 3.5f;
    CanvasGroup canvasGroupMain;
    CanvasGroup canvasGroupSetter;

    bool runTimer = false;
    string timerDisplayDef = "00:00";
    float remainingTime = 0;

    //references:
    [SerializeField] Image timerButton;
    [SerializeField] Image musicButton;
    [SerializeField] TextMeshProUGUI timerDisplay;

    [SerializeField] Canvas mainCanvas;
    [SerializeField] Canvas setterCanvas;

    [SerializeField] TextMeshProUGUI setterDisplay;

    void Start()
    {
        canvasGroupMain = mainCanvas.GetComponent<CanvasGroup>();
        canvasGroupSetter = setterCanvas.GetComponent<CanvasGroup>();
        //remainingTime = timerTime;

        FadeOutMainCanvas();
        FadeOutSetterCanvas();
    }

    void Update()
    {
        if (Input.touchCount == 2)
        {
            Touch touch = Input.GetTouch(1);

            if (touch.phase == TouchPhase.Ended)
            {
                doFadeMain = !doFadeMain; //toggling
                activeUI = !activeUI;

                if (activeUI) canvasGroupMain.interactable = true;
                else canvasGroupMain.interactable = false;
            }
            
        }

        //if (!doFadeSetter)
        {
            if (!doFadeMain) //alpha up, interactable
            {
                if (canvasGroupMain.alpha < 1)
                {
                    canvasGroupMain.alpha += Time.deltaTime * fadeSpeed;
                }
            }
            else //alpha down, not interactable
            {
                if (canvasGroupMain.alpha > 0)
                {
                    canvasGroupMain.alpha -= Time.deltaTime * fadeSpeed;
                }
            }
        }
        

        if (!doFadeSetter) //alpha up, interactable
        {
            if (canvasGroupSetter.alpha < 1)
            {
                canvasGroupSetter.alpha += Time.deltaTime * fadeSpeed;
            }
        }
        else //alpha down, not interactable
        {
            if (canvasGroupSetter.alpha > 0)
            {
                canvasGroupSetter.alpha -= Time.deltaTime * fadeSpeed;
            }
        }

        if (runTimer)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime > 0)
            {
                UpdateTimerDisplay();
            }
            else
            {
                Debug.Log("Timer ran out!");
                runTimer = false;
                ResetTimer();
                ResetTimerButton();
            }
        }
    }

    public void FadeOutMainCanvas()
    {
        canvasGroupMain.alpha = 0f;
        doFadeMain = true;
        activeUI = false;
        canvasGroupMain.interactable = false;
    }

    public void FadeOutSetterCanvas()
    {
        canvasGroupSetter.alpha = 0f;
        canvasGroupSetter.blocksRaycasts = false;
        doFadeSetter = true;
        canvasGroupSetter.interactable = false;
    }

    public void TimerButton()
    {
        timerBtnPlay = !timerBtnPlay;

        if (timerBtnPlay) //isn't playing yet, in play display state.
        {
            SetTimerButtonSprite(timerBtnPlay);
            PauseTimer();
        }
        else
        {
            SetTimerButtonSprite(timerBtnPlay);
            StartTimer();
        }
    }

    void ResetTimerButton()
    {
        timerBtnPlay = true;
        remainingTime = 0;
        timerTime = 0;
        SetTimerButtonSprite(timerBtnPlay);
    }

    void SetTimerButtonSprite(bool playSprite)
    {
        if(playSprite) timerButton.sprite = timerPlay;
        else timerButton.sprite = timerPause; 
    }

    void StartTimer()
    {
        runTimer = true;
        activeTimer = true;
    }

    void PauseTimer()
    {
        runTimer = false;
    }

    public void ResetTimer()
    {
        runTimer = false;
        activeTimer = false;
        remainingTime = timerTime;
        ResetTimerButton();
        TimerDisplayReset();
    }

    void TimerDisplayReset()
    {
        timerDisplay.text = timerDisplayDef;
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60);

        string timerString = "";

        if (minutes / 10 == 0)
        {
            if (seconds / 10 == 0) timerString = $"0{minutes.ToString()}:0{seconds.ToString()}";
            else timerString = $"0{minutes.ToString()}:{seconds.ToString()}";
        }
        else
        {
            if (seconds / 10 == 0) timerString = $"{minutes.ToString()}:0{seconds.ToString()}";
            else timerString = $"{minutes.ToString()}:{seconds.ToString()}";
        }

        timerDisplay.text = timerString;
    }

    public void ToggleMusic()
    {
        musicToggle = !musicToggle;

        if (musicToggle) //if true, music is on display that it is on.
        {
            musicButton.sprite = musicOn;
        }
        else
        {
            musicButton.sprite = musicOff;
        }
    }

    public void EnableSetterMenu()
    {
        if (!activeTimer)
        {
            canvasGroupSetter.blocksRaycasts = true;
            canvasGroupMain.interactable = false;
            canvasGroupSetter.interactable = true;
            doFadeSetter = false; 
        }
    }

    public void CloseSetter()
    {
        doFadeSetter = true;
        canvasGroupMain.interactable = true;
        canvasGroupSetter.interactable = false;
        canvasGroupSetter.blocksRaycasts = false;
    }

    public void ConfirmSetter()
    {
        CloseSetter();

        int minutes = int.Parse(setterDisplay.text);
        Debug.Log(minutes);
        if (minutes > 0)
        {
            timerTime = minutes * 60f;
            remainingTime = timerTime;
            //StartTimer();
            TimerButton();
        }

        ResetTimerSetDisplay();
    }

    public void EscapeSetter()
    {
        CloseSetter();
        ResetTimerSetDisplay();
    }

    public void ResetTimerSetDisplay()
    {
        setterDisplay.text = "0";
    }

}
