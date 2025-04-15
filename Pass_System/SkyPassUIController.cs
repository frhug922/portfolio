using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SkyPassUIController : LobbyMenuUIBase
{
    #region type def

    public enum TabType
    {
        Challenge,
        Reward,
    }

    private enum TextType
    {
        //TopUI
        Title,
        Activate,
        Time,
        Challenge,
        Reward,
        PassLevel,
        Level,

        // Challenge Tab
        Price,
        ActiveButton,
        DescBanner,
        Pass,
        Event,
        DaillyChallenge,
        Renewal,
        PassChallenge,

        // Reward Tab
        Free,
        Skypass,
        RewardCount,
    }

    #endregion





    #region serialize fields

    [SerializeField] private SimpleTab _mainTabs;
    [SerializeField] private List<SkyPassContents> _mainContents;
    [SerializeField] private GameObject _questionMark;
    [SerializeField] private GameObject _pointEndIcon;

    [SerializeField] private Transform _pointBar;
    [SerializeField] private Transform _nextLevel;

    [SerializeField] private TextSlider _textSlider;

    [SerializeField] private SkyPassPopupReward _rewardPopup;

    [SerializeField] private GameObject _passActiveTab;

    #endregion





    #region private fields

    private TabType _mainType = TabType.Challenge;
    private Coroutine _skypassEndTimeCoroutine;
    private MSeasonQuestInfo _questInfo;

    #endregion





    #region public funcs

    public override bool HideSubUIsByESCBack() {
        return true;
    }

    public override void SetShow() {
        Initialize();

        ToggleMain(_mainType);

        RefreshUI();

        base.SetShow();

        CheckRemainReward();
        StartUpdateSkypassEndTime();
    }

    public override void SetHide() {
        _mainType = TabType.Challenge;

        base.SetHide();
    }

    public override void RefreshUI() {
        _questInfo = GameDataManager.Instance.SkypassQuestInfo;

        UpdateTextsUI();
        UpdateContentsUI();
    }

    public override void UpdateTextsUI() {
        SetText((int)TextType.Title, TStrings.Instance.FindString("HunterPath_25001"));
        SetText((int)TextType.Activate, TStrings.Instance.FindString("HunterPath_25002"));

        SetText((int)TextType.Challenge, TStrings.Instance.FindString("HunterPath_25004"));
        SetText((int)TextType.Reward, TStrings.Instance.FindString("HunterPath_25005"));

        TSeasonQuestLevel seasonQuestLevel = TSeasonQuestLevels.Instance.Find(GameDataManager.Instance.SkypassInfo.m_seasonQuestInfo.m_season, _questInfo.m_level + 1);
        if (null == seasonQuestLevel) {
            SetText((int)TextType.PassLevel, string.Format("{0} / {1}", _questInfo.m_point, TSeasonQuestSets.Instance.Find(_questInfo.m_season)._point));
            SetText((int)TextType.Level, _questInfo.m_level.ToString());
        }
        else {
            SetText((int)TextType.PassLevel, string.Format("{0} / {1}", _questInfo.m_point, seasonQuestLevel._exp));
            SetText((int)TextType.Level, seasonQuestLevel._level.ToString());
        }

        if (_mainType == TabType.Challenge) {
            SetText((int)TextType.Price, string.Format(TStrings.Instance.FindString("SHOP_9028"), TPrices.Instance.Find(ShopProductType.Path).LangTypePrice));
            SetText((int)TextType.ActiveButton, TStrings.Instance.FindString("HunterPath_25002"));
            SetText((int)TextType.DescBanner, TStrings.Instance.FindString("HunterPath_25006"));
            SetText((int)TextType.Pass, TStrings.Instance.FindString("HunterPath_25001"));
            SetText((int)TextType.Event, TStrings.Instance.FindString("HunterPath_25029"));
            SetText((int)TextType.DaillyChallenge, TStrings.Instance.FindString("HunterPath_25008"));
            SetText((int)TextType.PassChallenge, TStrings.Instance.FindString("HunterPath_25010"));
        }
        else if (_mainType == TabType.Reward) {
            SetText((int)TextType.Free, TStrings.Instance.FindString("HunterPath_25031"));
            SetText((int)TextType.Skypass, TStrings.Instance.FindString("HunterPath_25001"));
        }

        int rewardCount = GameDataManager.Instance.SkypassQuestInfo.m_rewardCnt;
        if (rewardCount > 0) {
            SetText((int)TextType.RewardCount, rewardCount.ToString(), true);
        }
        else {
            HideText((int)TextType.RewardCount, true);
        }
    }

    public void OnClick_ActivePass() {
        SoundManager.Instance.PlaySFX(FXSound.Click);

        LobbySceneUIManager.Instance.ShowSkyPassPurchasePopup();
    }

    public void OnClick_QuestionMark() {
        SoundManager.Instance.PlaySFX(FXSound.Click);

        PopupManager.Instance.ShowTooltip(_questionMark.transform, TStrings.Instance.FindString("HunterPath_25001"),
            string.Format("{0}\n\n<color=#85FFFF>{1}</color>\n{2}\n\n<color=#85FFFF>{3}</color>\n{4}\n\n<color=#85FFFF>{5}</color>\n{6}\n\n<color=#85FFFF>{7}</color>\n{8}",
            TStrings.Instance.FindString("HunterPath_25021"),
            TStrings.Instance.FindString("HunterPath_25008"),
            TStrings.Instance.FindString("HunterPath_25023"),
            TStrings.Instance.FindString("HunterPath_25010"),
            TStrings.Instance.FindString("HunterPath_25024"),
            TStrings.Instance.FindString("HunterPath_25022"),
            TStrings.Instance.FindString("HunterPath_25025"),
            TStrings.Instance.FindString("HunterPath_25026"),
            TStrings.Instance.FindString("HunterPath_25027")));
    }

    public void OnClick_PointBar() {
        SoundManager.Instance.PlaySFX(FXSound.Click);

        PopupManager.Instance.ShowTooltip(_pointBar, TStrings.Instance.FindString("HunterPath_25022"), TStrings.Instance.FindString("HunterPath_25025"));
    }

    public void OnClick_NextLevel() {
        SoundManager.Instance.PlaySFX(FXSound.Click);

        TSeasonQuestLevel seasonQuestLevel = TSeasonQuestLevels.Instance.Find(GameDataManager.Instance.SkypassInfo.m_seasonQuestInfo.m_season, _questInfo.m_level + 1);
        if (seasonQuestLevel == null) {
            int point = TSeasonQuestSets.Instance.Find(_questInfo.m_season)._point - _questInfo.m_point;
            if (0 > point) {
                point = 0;
            }
            PopupManager.Instance.ShowTooltip(_nextLevel, TStrings.Instance.FindString("HunterPath_25026"), string.Format("{0}\n\n{1}", TStrings.Instance.FindString("HunterPath_25027"), string.Format(TStrings.Instance.FindString("HunterPath_25028"), point)));
        }
        else {
            PopupManager.Instance.ShowTooltip(_nextLevel, TStrings.Instance.FindString("HunterPath_25026"), string.Format("{0}\n\n{1}", TStrings.Instance.FindString("HunterPath_25027"), string.Format(TStrings.Instance.FindString("HunterPath_25028"), seasonQuestLevel._exp - _questInfo.m_point)));
        }
    }

    #endregion





    #region private funcs

    private void Initialize() {
        _mainTabs.Initialize(ClickedMainTab);

        for (int i = 0, count = _mainContents.Count; i < count; ++i) {
            _mainContents[i].Initialize(OnClick_Complete, OnClick_GainButton, OnClick_FinalReward);
        }
    }

    private void ClickedMainTab(int index) {
        TabType mainTabType = (TabType)index;

        Debug.LogFormat("PassUIController : ClickedMainTab : [ index : {0} \t/ MainTabType : {1} ]\n", index, mainTabType);

        SoundManager.Instance.PlaySFX(FXSound.Click);

        ToggleMain(mainTabType);

        RefreshUI();

        if (index == (int)TabType.Reward) {
            _mainContents[index].SetSlider();
        }
    }

    private void ToggleMain(TabType type) {
        _mainType = type;

        _mainTabs.SetActiveTab((int)_mainType);

        RefreshUI();
    }

    private void UpdateContentsUI() {
        SetSlider();

        _passActiveTab.SetActive(0 == GameDataManager.Instance.SkypassQuestInfo.m_pathonoff);

        for (int i = 0; i < _mainContents.Count; ++i) {
            if (i == (int)_mainType) {
                _mainContents[i].SetShow();
            }
            else {
                _mainContents[i].SetHide();
            }
        }
    }

    private void SetSlider() {
        long currAmount = _questInfo.m_point;
        long maxAmount = 0;

        TSeasonQuestLevel questLevel = TSeasonQuestLevels.Instance.Find(GameDataManager.Instance.SkypassInfo.m_seasonQuestInfo.m_season, _questInfo.m_level + 1);
        if (questLevel == null) {
            maxAmount = TSeasonQuestSets.Instance.Find(_questInfo.m_season)._point;
        }
        else {
            maxAmount = questLevel._exp;
        }

        _textSlider.SetShow(true, currAmount, maxAmount, true);
    }

    private void OnClick_Complete(GameObject startObj, int id) {
        AssetPathMoveManager.Instance.SetShowSkyPassPoint(startObj, _pointEndIcon);

        WebHttp.Instance.RequestSkypassComplete(id, () => {
            RefreshUI();
        });
    }

    private void OnClick_GainButton(int level) {
        WebHttp.Instance.RequestSkypassReward(level, () => {
            _rewardPopup.SetShow();
            RefreshUI();
        });
    }

    private void OnClick_FinalReward() {
        WebHttp.Instance.RequestSkypassFinalReward(() => {
            _rewardPopup.SetShow();
            RefreshUI();
        });
    }

    private void CheckRemainReward() {
        int point = GameDataManager.Instance.SkypassInfo.clearpoint;
        if (0 < point) {
            PopupManager.Instance.ShowSkypassRewardPopup(TStrings.Instance.FindString("HunterPath_25042"), string.Format(TStrings.Instance.FindString("HunterPath_25043"), point), point);
        }
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

        while (0 < GameDataManager.Instance.SkypassDailyEndTime.TotalMilliseconds) {
            GameDataManager.Instance.SetSkyPassTime();

            _questInfo = GameDataManager.Instance.SkypassQuestInfo;

            TimeSpan restTime = GameDataManager.Instance.SkypassSeasonEndTime;
            if (0 < restTime.Days) {
                SetText((int)TextType.Time, string.Format("{0} {1}", TStrings.Instance.FindString("HunterPath_25003"), string.Format(TStrings.Instance.FindString("SHOP_9034"), restTime.Days, restTime.Hours)));
            }
            else {
                SetText((int)TextType.Time, string.Format("{0} {1}", TStrings.Instance.FindString("HunterPath_25003"), string.Format(TStrings.Instance.FindString("SHOP_9035"), restTime.Hours, restTime.Minutes)));
            }

            if (_mainType == TabType.Challenge) {
                TimeSpan restDailyTime = GameDataManager.Instance.SkypassDailyEndTime;
                SetText((int)TextType.Renewal, string.Format("{0} {1}", TStrings.Instance.FindString("HunterPath_25009"), string.Format(TStrings.Instance.FindString("SHOP_9035"), restDailyTime.Hours, restDailyTime.Minutes)));
            }

            yield return updateTerm;
        }
    }

    #endregion

}
