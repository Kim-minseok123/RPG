using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_DamageInfo_Item : UI_Base
{
    enum Texts
    {
        DamageText,
    }
    public override void Init()
    {
        Canvas canvas = GetComponentInChildren<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        transform.localPosition = Vector3.zero;
        BindText(typeof(Texts));
    }
    public void Update()
    {
        if (Camera.main == null || gameObject == null) return;
        transform.LookAt(Camera.main.transform);
    }
    public void Setting(int damage, Transform obj)
    {
        TextMeshProUGUI text = GetText((int)Texts.DamageText);
        if (damage <= 0)
            text.text = "MISS";
        else
            text.text = damage.ToString();
        transform.position = obj.position;
        text.transform.DOMoveY(text.transform.position.y + 0.5f, 2f).SetEase(Ease.OutCirc);
        text.DOFade(0, 2).SetEase(Ease.OutCirc).OnComplete(() => { Managers.Resource.Destroy(gameObject); });
    }
}
