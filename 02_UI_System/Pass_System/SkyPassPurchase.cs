using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SkyPassPurchase : LobbyMenuUIBase
{
    #region type def

    private enum ButtonType
    {
        PurchaseNormalPass,
        PurchaseElitePass,
    }

    private enum TextType
    {
        Desc,

        NormalTime,
        NormalPass,
        NormalRewardNow,
        NormalReward1,
        NormalReward2,
        NormalPrice,

        EliteTime,
        ElitePass,
        EliteRewardNow,
        EliteReward1,
        EliteReward2,
        EliteReward3,
        EliteReward4,
        ElitePrice,

        Reference,
    }

    #endregion // type def





    #region serialized fields

    [SerializeField] private GameObject _questionMark;

    #endregion // serialized fields





    #region private variables

    private List<string> _titles = new List<string>();
    private List<string> _contents = new List<string>();
    private Coroutine _skypassEndTimeCoroutine;

    #endregion // private variables





    #region public funcs

    public override bool HideSubUIsByESCBack() {
        return false;
    }

    public override void SetShow() {
        SetToolTipStrings();

        RefreshUI();

        StartUpdateSkypassEndTime();

        base.SetShow();
    }

    public override void SetHide() {
        ClearToolTipStrings();

        base.SetHide();
    }

    public override void RefreshUI() {
        UpdateTextsUI();
    }

    public override void UpdateTextsUI() {
        SetText((int)TextType.Desc, TStrings.Instance.FindString("HunterPath_25013"));

        TimeSpan restTime = GameDataManager.Instance.SkypassSeasonEndTime;
        if (0 < restTime.Days) {
            SetText((int)TextType.NormalTime, string.Format(TStrings.Instance.FindString("SHOP_9034"), restTime.Days, restTime.Hours));
            SetText((int)TextType.EliteTime, string.Format(TStrings.Instance.FindString("SHOP_9034"), restTime.Days, restTime.Hours));
        }
        else {
            SetText((int)TextType.NormalTime, string.Format(TStrings.Instance.FindString("SHOP_9035"), restTime.Hours, restTime.Minutes));
            SetText((int)TextType.EliteTime, string.Format(TStrings.Instance.FindString("SHOP_9035"), restTime.Hours, restTime.Minutes));
        }

        SetText((int)TextType.NormalPass, TStrings.Instance.FindString("HunterPath_25001"));
        SetText((int)TextType.NormalRewardNow, TStrings.Instance.FindString("HunterPath_25012"));
        SetText((int)TextType.NormalReward1, "TODO");////////////////////////////////////////////////보상생기면 하기
        SetText((int)TextType.NormalReward2, "TODO");////////////////////////////////////////////////보상생기면 하기
        SetText((int)TextType.NormalPrice, "TODO");//////////////////////////////////////////////// 가격 표시

        SetText((int)TextType.ElitePass, TStrings.Instance.FindString("HunterPath_25011"));
        SetText((int)TextType.EliteRewardNow, TStrings.Instance.FindString("HunterPath_25012"));
        SetText((int)TextType.EliteReward1, "TODO");///////////////////////////////////////////////////보상생기면 하기
        SetText((int)TextType.EliteReward2, "TODO");///////////////////////////////////////////////////보상생기면 하기
        SetText((int)TextType.EliteReward3, "TODO");///////////////////////////////////////////////////보상생기면 하기
        SetText((int)TextType.EliteReward4, "TODO");///////////////////////////////////////////////////보상생기면 하기
        SetText((int)TextType.ElitePrice, "TODO");//////////////////////////////////////////////// 가격 표시

        SetText((int)TextType.Reference, TStrings.Instance.FindString("HunterPath_25014"));
    }

    public void OnClick_PurchaseNormalPass(int value) {
        SoundManager.Instance.PlaySFX(FXSound.Click);

        if ((int)ButtonType.PurchaseNormalPass == value) {
            Common.ShowSupportLaterPopup(TStrings.Instance.FindString("SHOP_9042"));
        }
        else if ((int)ButtonType.PurchaseElitePass == value) {
            Common.ShowSupportLaterPopup(TStrings.Instance.FindString("SHOP_9042"));
        }
    }

    public void OnClick_QuestionMark() {
        SoundManager.Instance.PlaySFX(FXSound.Click);

        PopupManager.Instance.ShowTooltip(_questionMark.transform, _titles.ToArray(), _contents.ToArray());
    }

    #endregion // public funcs






    #region private funcs

    private void SetToolTipStrings() {
        _titles.Add(TStrings.Instance.FindString("HunterPath_25015"));
        _titles.Add(TStrings.Instance.FindString("HunterPath_25016"));

        _contents.Add(string.Format("{0}\n\n{1}", TStrings.Instance.FindString("HunterPath_25017"), TStrings.Instance.FindString("HunterPath_25018")));
        _contents.Add(string.Format("{0}\n\n{1}\n\n{2}", TStrings.Instance.FindString("HunterPath_25019"), TStrings.Instance.FindString("HunterPath_25020"), TStrings.Instance.FindString("HunterPath_25014")));
    }

    private void ClearToolTipStrings() {
        _titles.Clear();
        _contents.Clear();
    }

    private void StopUpdateSkypassEndTime() {
        if (null == _skypassEndTimeCoroutine) {
            return;
        }

        StopCoroutine(_skypassEndTimeCoroutine);
        _skypassEndTimeCoroutine = null;
    }

    private void StartUpdateSkypassEndTime() {
        StopUpdateSkypassEndTime();

        _skypassEndTimeCoroutine = StartCoroutine(UpdateSkypassEndTime());
    }

    private IEnumerator UpdateSkypassEndTime() {
        var updateTerm = new WaitForSeconds(1f);

        while (true) {
            GameDataManager.Instance.SetSkyPassTime();

            RefreshUI();

            yield return updateTerm;
        }
    }

    #endregion // private funcs
}
