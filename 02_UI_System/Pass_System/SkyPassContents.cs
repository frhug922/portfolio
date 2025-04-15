using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkyPassContents : LobbyUIBase
{
    #region type def

    public enum QuestTabType
    {
        Event,
        Daily,
        Challenge,
    }

    private enum SliderValueType
    {
        Top = 3,

        Bottom = 28,
    }

    #endregion





    #region serialize fields

    [SerializeField] private SkyPassUIController.TabType _tabType;
    [SerializeField] private List<SkyPassLevelReward> _skyPassLevelRewards;
    [SerializeField] private List<SkyPassQuestContents> _skyPassQuestContents;
    [SerializeField] private SkyPassFinalReward _skyPassFinalReward;

    [SerializeField] private Scrollbar _scrollBar;

    #endregion





    #region private variables

    private int _initializedRewards = 0;
    private List<TSeasonQuestLevel> _levelRewardsDatas;

    #endregion // private variables





    #region public funcs

    public override void SetShow() {
        base.SetShow();

        RefreshUI();
    }

    public void Initialize(System.Action<GameObject, int> clickCallback, System.Action<int> gainCallback, System.Action finalrewardCallback) {
        if (SkyPassUIController.TabType.Reward == _tabType) {
            _levelRewardsDatas = TSeasonQuestLevels.Instance.FindSeason(GameDataManager.Instance.SkypassInfo.m_seasonQuestInfo.m_season);

            if (_levelRewardsDatas.Count > _skyPassLevelRewards.Count) {
                for (int i = _skyPassLevelRewards.Count, count = _levelRewardsDatas.Count; i < count; ++i) {
                    GameObject rewardObj = Instantiate(_skyPassLevelRewards[0].gameObject, _skyPassLevelRewards[0].transform.parent.transform);
                    _skyPassLevelRewards.Add(rewardObj.GetComponent<SkyPassLevelReward>());
                }
            }

            for (int i = 0, count = _levelRewardsDatas.Count; _initializedRewards < count; ++i) {
                _skyPassLevelRewards[i].Initialize(gainCallback);
                ++_initializedRewards;
            }

            _skyPassFinalReward.Initialize(finalrewardCallback);
        }
        else {
            for (int i = 0, count = _skyPassQuestContents.Count; i < count; ++i) {
                _skyPassQuestContents[i].Initialize(clickCallback);
            }
        }
    }

    public override void RefreshUI() {
        UpdateContentsUI();
    }

    public void SetSlider() {
        if (SkyPassUIController.TabType.Reward == _tabType) {
            _scrollBar.value = SliderValue();
        }
    }

    #endregion // public funcs





    #region private funcs

    private void UpdateContentsUI() {
        if (SkyPassUIController.TabType.Reward == _tabType) {
            for (int i = 0, count = _skyPassLevelRewards.Count; i < count; ++i) {
                _skyPassLevelRewards[i].SetShow(i + 1);
            }
            _skyPassFinalReward.SetShow();
        }
        else {
            if (GameDataManager.Instance.SkypassEventQuest != null) {
                _skyPassQuestContents[(int)QuestTabType.Event].SetShow();
            }
            _skyPassQuestContents[(int)QuestTabType.Daily].SetShow();
            _skyPassQuestContents[(int)QuestTabType.Challenge].SetShow();
        }
    }

    private float SliderValue() {
        for (int i = 0, count = _skyPassLevelRewards.Count; i < count; ++i) {
            if (_skyPassLevelRewards[i].IsActivated == true) {
                if (i < (int)SliderValueType.Top) {
                    return 1f;
                }
                else if (i >= (int)SliderValueType.Top && i < (int)SliderValueType.Bottom) {
                    return GenerateSliderValue(0.03f, 0.99f, (int)SliderValueType.Bottom - (int)SliderValueType.Top)[i - (int)SliderValueType.Top];
                }
                else if (i >= (int)SliderValueType.Bottom) {
                    return 0f;
                }
            }
        }

        if (30 <= GameDataManager.Instance.SkypassQuestInfo.m_level) {
            return 0f;
        }
        return 1f;
    }

    public float[] GenerateSliderValue(float startValue, float endValue, int numberOfValues) {
        float difference = (endValue - startValue) / (numberOfValues - 1);

        float[] sequence = new float[numberOfValues];
        for (int i = 0; i < numberOfValues; ++i) {
            sequence[i] = endValue - i * difference;
        }

        return sequence;
    }

    #endregion // private funcs
}
