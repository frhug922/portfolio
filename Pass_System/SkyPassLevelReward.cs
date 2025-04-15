using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkyPassLevelReward : LobbyUIBase
{

    #region type def

    private enum TextType
    {
        Level,
        Active,
    }

    private enum ObjectType
    {
        Active,
        GainButton,

        VIPLock,

        VIPCheck,
        FreeCheck,

        VIPItem,
        VIPHero,

        FreeItem,
        FreeHero,

        ActiveButton,

        VIPBox,
        FreeBox,
    }

    private enum RewardType
    {
        VIP,
        Free,
    }

    #endregion





    #region serialize fields

    [SerializeField] private List<GameObject> _activeObjects;
    [SerializeField] private List<CommonEntityInfo> _rewardHero;
    [SerializeField] private List<CommonItemInfo> _rewardItem;

    [SerializeField] private Image _vipBoxImage;
    [SerializeField] private Image _freeBoxImage;

    #endregion





    #region private fields

    private System.Action<int> _clickCallback;
    private TSeasonQuestLevel _seasonQuestLevel;
    private List<MSeasonQuestReward> _skypassRewardInfo;

    private TReward _normalReward;
    private TReward _vipReward;

    private bool _isActivated = false;

    #endregion





    #region properties

    public bool IsActivated { get { return _isActivated; } }

    #endregion // properties





    #region public funcs

    public void SetShow(int level) {
        base.SetShow();

        _skypassRewardInfo = GameDataManager.Instance.SkypassInfo.m_seasonQuestReward;
        _seasonQuestLevel = TSeasonQuestLevels.Instance.Find(GameDataManager.Instance.SkypassInfo.m_seasonQuestInfo.m_season, level);

        for (int i = 0, count = _rewardHero.Count; i < count; ++i) {
            _rewardHero[i].Initialize(i, EntityType.None, OnClick_Card);
        }

        for (int i = 0, count = _rewardItem.Count; i < count; ++i) {
            _rewardItem[i].Initialize(i, OnClick_Item, null, null, null);
        }

        RefreshUI();
    }

    public void Initialize(System.Action<int> gainCallback) {
        _clickCallback = gainCallback;
    }

    public void OnClick_ActiveButton() {
        SoundManager.Instance.PlaySFX(FXSound.Click);

        LobbySceneUIManager.Instance.ShowSkyPassPurchasePopup();
    }

    public void OnClick_GainButton() {
        if (null != _clickCallback) {
            SoundManager.Instance.PlaySFX(FXSound.Click);

            _clickCallback(_seasonQuestLevel._level);
        }
    }

    public override void RefreshUI() {
        UpdateTextsUI();
        UpdateContentsUI();
    }

    public override void UpdateTextsUI() {
        SetText((int)TextType.Level, _seasonQuestLevel._level.ToString());
        SetText((int)TextType.Active, TStrings.Instance.FindString("HunterPath_25002"));
    }

    #endregion





    #region private funcs

    private void UpdateContentsUI() {
        _normalReward = TRewards.Instance.Find(_seasonQuestLevel._normalRewardID);
        _vipReward = TRewards.Instance.Find(_seasonQuestLevel._vipRewardID);

        if (_normalReward != null) {
            _activeObjects[(int)ObjectType.FreeBox].SetActive(1 < _normalReward._rowRewards.Count);
            if (1 < _normalReward._rowRewards.Count) {
                SetSkyPathIcon(ref _vipBoxImage, "Path_GeneralBox");
            }
            else {
                TRewardItem reward = _normalReward._rowRewards[0];

                _activeObjects[(int)ObjectType.FreeHero].SetActive((int)AssetType.CardType == reward._itemType);
                _activeObjects[(int)ObjectType.FreeItem].SetActive((int)AssetType.ConsumableItem == reward._itemType);

                if (reward._itemType == (int)AssetType.CardType) {
                    _rewardHero[(int)RewardType.Free].SetShowReward(reward);
                }
                else if (reward._itemType == (int)AssetType.ConsumableItem) {
                    _rewardItem[(int)RewardType.Free].SetShowRewardItem(reward);
                }
                else if (reward._itemType == (int)AssetType.Jewel) {
                    _rewardItem[(int)RewardType.Free].SetShowAsset(reward);
                }
            }
        }

        _activeObjects[(int)ObjectType.VIPBox].SetActive(1 < _vipReward._rowRewards.Count);
        if (1 < _vipReward._rowRewards.Count) {
            SetSkyPathIcon(ref _freeBoxImage, "Path_PremiumBox");
        }
        else {
            TRewardItem reward = _vipReward._rowRewards[0];

            _activeObjects[(int)ObjectType.VIPHero].SetActive(reward._itemType == (int)AssetType.CardType);
            _activeObjects[(int)ObjectType.VIPItem].SetActive(reward._itemType == (int)AssetType.ConsumableItem);

            if (reward._itemType == (int)AssetType.CardType) {
                _rewardHero[(int)RewardType.VIP].SetShowReward(reward);
            }
            else if (reward._itemType == (int)AssetType.ConsumableItem) {
                _rewardItem[(int)RewardType.VIP].SetShowRewardItem(reward);
            }
            else if (reward._itemType == (int)AssetType.Jewel) {
                _rewardItem[(int)RewardType.VIP].SetShowAsset(reward);
            }
        }

        MSeasonQuestInfo questInfo = GameDataManager.Instance.SkypassQuestInfo;

        _activeObjects[(int)ObjectType.VIPLock].SetActive(0 == questInfo.m_pathonoff);
        _activeObjects[(int)ObjectType.Active].SetActive(_seasonQuestLevel._level <= questInfo.m_level);
        _activeObjects[(int)ObjectType.ActiveButton].SetActive(_seasonQuestLevel._level == questInfo.m_level + 1 && GameDataManager.Instance.SkypassQuestInfo.m_pathonoff == 0);

        _activeObjects[(int)ObjectType.VIPCheck].SetActive((int)SkyPassRewardType.Acquired == _skypassRewardInfo[_seasonQuestLevel._level - 1].m_chargeReward);
        _activeObjects[(int)ObjectType.FreeCheck].SetActive((int)SkyPassRewardType.Acquired == _skypassRewardInfo[_seasonQuestLevel._level - 1].m_normalReward
            && null != _normalReward);

        _activeObjects[(int)ObjectType.GainButton].SetActive(IsObtainable());
    }

    private bool IsObtainable() {
        MSeasonQuestInfo questInfo = GameDataManager.Instance.SkypassQuestInfo;
        if (_seasonQuestLevel._level <= questInfo.m_level) {
            if (null == _normalReward) {
                if (questInfo.m_pathonoff == 1) {
                    if ((int)SkyPassRewardType.Unacquired == GameDataManager.Instance.SkypassInfo.m_seasonQuestReward[_seasonQuestLevel._level - 1].m_chargeReward) {
                        _isActivated = true;
                        return true;
                    }
                }
            }
            else {
                if (questInfo.m_pathonoff == 1) {
                    if ((int)SkyPassRewardType.Unacquired == GameDataManager.Instance.SkypassInfo.m_seasonQuestReward[_seasonQuestLevel._level - 1].m_chargeReward) {
                        _isActivated = true;
                        return true;
                    }
                }
                else {
                    if ((int)SkyPassRewardType.Unacquired == GameDataManager.Instance.SkypassInfo.m_seasonQuestReward[_seasonQuestLevel._level - 1].m_normalReward) {
                        _isActivated = true;
                        return true;
                    }
                }

            }
        }
        _isActivated = false;
        return false;
    }

    private void OnClick_Item(int index) {
        SoundManager.Instance.PlaySFX(FXSound.Click);

#if SHOW_LOG
        Debug.LogWarningFormat("SkyPassLevelReward : OnClick_Item : [ clicked index : {0} ]\n", index);
#endif // SHOW_LOG

        TItem item = null;

        if (index == (int)RewardType.Free) {
            if (_normalReward._rowRewards[0]._itemType == (int)AssetType.ConsumableItem) {
                item = TItems.Instance.Find(_normalReward._rowRewards[0]._kind);
            }
            else if (_normalReward._rowRewards[0]._itemType == (int)AssetType.Jewel) {
                item = TItems.Instance.Find((int)AssetType.Jewel);
            }
        }
        else if (index == (int)RewardType.VIP) {
            if (_vipReward._rowRewards[0]._itemType == (int)AssetType.ConsumableItem) {
                item = TItems.Instance.Find(_vipReward._rowRewards[0]._kind);
            }
            else if (_vipReward._rowRewards[0]._itemType == (int)AssetType.Jewel) {
                item = TItems.Instance.Find((int)AssetType.Jewel);
            }
        }

        if (item != null) {
            PopupManager.Instance.ShowTooltip(_rewardItem[index].transform, item);
        }
    }

    public void OnClick_Card(int index) {
        SoundManager.Instance.PlaySFX(FXSound.Click);

#if SHOW_LOG
        Debug.LogWarningFormat("SkyPassLevelReward : OnClick_Card : [ clicked index : {0} ]\n", index);
#endif // SHOW_LOG

        TCard card = null;

        if (index == (int)RewardType.VIP) {
            card = TCards.Instance.Find(_vipReward._rowRewards[0]._kind);
        }
        else if (index == (int)RewardType.Free) {
            card = TCards.Instance.Find(_normalReward._rowRewards[0]._kind);
        }

        if (card != null) {
            PopupManager.Instance.ShowTooltip(_rewardHero[index].transform, card);
        }
    }

    #endregion
}


