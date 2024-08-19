using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_SceneConfirm_Popup : UI_Popup
{
    public GameObject MovingGo;
    public TextMeshProUGUI Content;

    public void Setting(string str)
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.sortingOrder = Managers.UI.ConfirmOrder++;
        Content.text = str;
        Sequence sq = DOTween.Sequence();
        sq.Append(MovingGo.transform.DOLocalMoveX(755f, 0.35f).SetEase(Ease.OutExpo))
            .AppendInterval(2.5f)
            .Append(MovingGo.transform.DOLocalMoveX(1165f, 0.35f).SetEase(Ease.OutExpo).OnComplete(() => { Destroy(gameObject); }));
    }
}
