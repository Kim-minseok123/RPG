using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NPCController : BaseController
{
    public GameObject NameTag;
    public TextMeshProUGUI NpcName;
    [SerializeField]
    protected int templateId = 1;
    public virtual void OpenNpc()
    {
        NpcOpenTrigger();
        CameraSetting();
        OpenNpcUI();
    }
    public virtual void CloseNpc()
    {
        CloseNpcUI();
        NpcCloseTrigger();
    }
    public virtual void NpcOpenTrigger()
    {
        Camera.main.GetComponent<CameraController>().NpcTrigger = true;
        Managers.Object.MyPlayer.NpcTrigger = true;
        Managers.Object.MyPlayer.Body.SetActive(false);
        (Managers.UI.SceneUI as UI_GameScene).NpcTrigger = true;
        (Managers.UI.SceneUI as UI_GameScene).CloseAllUI();
        (Managers.UI.SceneUI as UI_GameScene).CloseInfoAndSlot();
        NameTag.SetActive(false);
    }
    public virtual void CameraSetting()
    {

    }
    public virtual void OpenNpcUI()
    {
        Managers.UI.ShowPopupUI<UI_NpcSell_Popup>().Setting(templateId);
    }
    public virtual void CloseNpcUI()
    {
        Managers.UI.ClosePopupUI();
    }
    public virtual void NpcCloseTrigger()
    {
        Camera.main.GetComponent<CameraController>().NpcToPlayerMove();
        Managers.Object.MyPlayer.NpcTrigger = false;
        Managers.Object.MyPlayer.Body.SetActive(true);
        (Managers.UI.SceneUI as UI_GameScene).NpcTrigger = false;
        (Managers.UI.SceneUI as UI_GameScene).OpenInfoAndSlot();
        NameTag.SetActive(true);
    }

}
