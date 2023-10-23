using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Drawer : MonoBehaviour
{
    bool open = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleDrawer()
    {
        open = !open;

        if (open)
        {
            transform.GetChild(0).DOLocalMoveY(140f, 0.5f).SetEase(Ease.Linear);
            transform.GetChild(1).DOLocalMoveX(-140f, 0.5f).SetEase(Ease.Linear);
        }
        else
        {
            transform.GetChild(0).DOLocalMoveY(0f, 0.5f).SetEase(Ease.Linear);
            transform.GetChild(1).DOLocalMoveX(-0f, 0.5f).SetEase(Ease.Linear);
        }
    }
}
