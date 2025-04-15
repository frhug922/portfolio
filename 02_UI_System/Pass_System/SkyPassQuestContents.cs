using System.Collections.Generic;
using UnityEngine;


public class SkyPassQuestContents : LobbyUIBase
{
    #region serialize fields

    [SerializeField] private SkyPassEventQuest _skyPassEventQuest;
    [SerializeField] private List<SkyPassNormalQuest> _skyPassNormalQuests;
    [SerializeField] private SkyPassContents.QuestTabType _questTabType;

    #endregion





    #region private variables

    private MSeasonQuest _enentQuestData;
    private List<MSeasonQuest> _dailyQuestDatas;
    private List<MSeasonQuest> _seasonQuestDatas;

    #endregion // private variables





    #region public funcs

    public override void SetShow() {
        if (SkyPassContents.QuestTabType.Event == _questTabType) {
            _enentQuestData = GameDataManager.Instance.SkypassEventQuest;
        }
        else if (SkyPassContents.QuestTabType.Daily == _questTabType) {
            _dailyQuestDatas = GameDataManager.Instance.SkypassDailyQuest;
        }
        else if (SkyPassContents.QuestTabType.Challenge == _questTabType) {
            _seasonQuestDatas = GameDataManager.Instance.SkypassSeasonQuest;
        }

        RefreshUI();

        base.SetShow();
    }

    public void Initialize(System.Action<GameObject, int> clickCallback) {
        if (SkyPassContents.QuestTabType.Event == _questTabType) {
            _skyPassEventQuest.Initialize(clickCallback);
        }
        else {
            for (int i = 0, count = _skyPassNormalQuests.Count; i < count; ++i) {
                _skyPassNormalQuests[i].Initialize(clickCallback);
            }
        }
    }

    public override void RefreshUI() {
        UpdateContentsUI();
    }

    #endregion // public funcs





    #region private funcs

    private void UpdateContentsUI() {
        if (SkyPassContents.QuestTabType.Event == _questTabType) {
            if (null != _enentQuestData) {
                _skyPassEventQuest.SetShow();
            }
        }
        else if (SkyPassContents.QuestTabType.Daily == _questTabType) {
            for (int i = 0, count = _skyPassNormalQuests.Count; i < count; ++i) {
                _skyPassNormalQuests[i].SetShow(_dailyQuestDatas[i].m_refid, i);
            }
        }
        else if (SkyPassContents.QuestTabType.Challenge == _questTabType) {
            for (int i = 0, count = _skyPassNormalQuests.Count; i < count; ++i) {
                _skyPassNormalQuests[i].SetShow(_seasonQuestDatas[i].m_refid, i);
            }
        }
    }

    #endregion // private funcs
}
