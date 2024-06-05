using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Stat : UI_Base
{
    MyPlayerController myPlayer;
    bool isInit = false;
    enum Texts
    {
        PlayerStatNameText,
        AttackText,
        RemainPointText,
        StrText,
        DexText,
        LukText,
        IntText,   
        HpText,
        MpText,
    }
    enum Buttons
    {
        ExitButton,
        StrUpButton,
        DexUpButton,
        LukUpButton,
        IntUpButton
    }
    public override void Init()
    {
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.ExitButton).gameObject.BindEvent((e) => { var ui = Managers.UI.SceneUI as UI_GameScene; ui.CloseUI("UI_Stat"); });

        myPlayer = Managers.Object.MyPlayer;

        isInit = true;

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (isInit == false) return;
        GetText((int)Texts.PlayerStatNameText).text = $"닉네임 : <color=#F3E3AE>{myPlayer.objectInfo.Name}</color>";
        GetText((int)Texts.HpText).text = $"HP\t: <color=#F3E3AE>{myPlayer.Hp}</color> / <color=#F3E3AE>{myPlayer.MaxHp}</color>";
        GetText((int)Texts.MpText).text = $"MP\t: <color=#F3E3AE>{myPlayer.Mp}</color> / <color=#F3E3AE>{myPlayer.MaxMp}</color>";
        GetText((int)Texts.AttackText).text = $"공격력\t: <color=#F3E3AE>{myPlayer.MinAttack}</color> ~ <color=#F3E3AE>{myPlayer.MaxAttack}</color>";
        GetText((int)Texts.RemainPointText).text = $"남은 스텟 포인트 : <color=#F3E3AE>{myPlayer.Stat.StatPoint}</color>";
        GetText((int)Texts.StrText).text = $"Str\t: <color=#F3E3AE>{myPlayer.Stat.Str}</color>";
        GetText((int)Texts.DexText).text = $"Dex\t: <color=#F3E3AE>{myPlayer.Stat.Dex}</color>";
        GetText((int)Texts.LukText).text = $"Luk\t: <color=#F3E3AE>{myPlayer.Stat.Luk}</color>";
        GetText((int)Texts.IntText).text = $"Int\t: <color=#F3E3AE>{myPlayer.Stat.Int}</color>";

    }
}
