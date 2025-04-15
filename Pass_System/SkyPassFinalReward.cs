using System.Collections.Generic;
using UnityEngine;

public class SkyPassFinalReward : LobbyUIBase
{

    #region type def

    private enum TextType
    {
        Desc,
        Gain,
        Gain_Button,
        Slider,
    }

    private enum ObjectType
    {
        GainButton,
    }

    private enum RewardLevel
    {
        FinalReward = 30,
    }

    #endregion





    #region serialize fields

    [SerializeField] List<GameObject> _activeObjects;
    [SerializeField] GameObject _questionMark;
    [SerializeField] TextSlider _xpSlider;

    #endregion





    #region private fields

    private System.Action _clickCallback;
    private MSeasonQuestInfo _questInfo;

    #endregion





    #region public funcs

    public override void SetShow() {
        _questInfo = GameDataManager.Instance.SkypassQuestInfo;

        base.SetShow();

        RefreshUI();
    }

    public void Initialize(System.Action gainCallback) {
        _clickCallback = gainCallback;
    }

    public override void RefreshUI() {
        UpdateTextsUI();
        UpdateContentsUI();
    }

    public override void UpdateTextsUI() {
        SetText((int)TextType.Desc, TStrings.Instance.FindString("HunterPath_25036"));
        SetText((int)TextType.Gain, TStrings.Instance.FindString("Mission_22004"));
        SetText((int)TextType.Gain_Button, TStrings.Instance.FindString("Mission_22004"));

        if ((int)RewardLevel.FinalReward <= _questInfo.m_level) {
            SetText((int)TextType.Slider, string.Format("{0} / {1}", _questInfo.m_point, TSeasonQuestSets.Instance.Find(_questInfo.m_season)._point));
        }
        else {
            SetText((int)TextType.Slider, string.Format("0 / {0}", TSeasonQuestSets.Instance.Find(_questInfo.m_season)._point));
        }
    }

    public void OnClick_GainButton() {
        if (null != _clickCallback) {
            SoundManager.Instance.PlaySFX(FXSound.Click);
            _clickCallback();
        }
    }

    public void OnClick_QuestionMark() {
        PopupManager.Instance.ShowTooltip(_questionMark.transform, TStrings.Instance.FindString("HunterPath_25038"), string.Format("{0}\n\n{1}\n\n{2}", TStrings.Instance.FindString("HunterPath_25039"), TStrings.Instance.FindString("HunterPath_25040"), TStrings.Instance.FindString("HunterPath_25041")));
    }

    #endregion





    #region private funcs

    private void UpdateContentsUI() {
        _activeObjects[(int)ObjectType.GainButton].SetActive((int)RewardLevel.FinalReward <= _questInfo.m_level && TSeasonQuestSets.Instance.Find(_questInfo.m_season)._point <= _questInfo.m_point);
        if ((int)RewardLevel.FinalReward <= _questInfo.m_level) {
            SetSlider();
        }
    }

    private void SetSlider() {
        long currAmount = _questInfo.m_point;
        long maxAmount = TSeasonQuestSets.Instance.Find(_questInfo.m_season)._point;

        if (currAmount >= maxAmount) {
            currAmount = maxAmount;
        }

        _xpSlider.SetShow(true, currAmount, maxAmount, true);
    }

    #endregion
}


