using System.Collections.Generic;
using UnityEngine;


public class SkyPassPopupReward : LobbyUIBase
{
    #region type def

    private enum TextType
    {
        Title,
        Premium,
        Free,
        OK,
    }

    private enum RewardType
    {
        Premium,
        Free,
    }

    #endregion // type def





    #region serialized fields

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private float _smallWidth = 450;
    [SerializeField] private float _bigWidth = 650;

    [SerializeField] private List<GameObject> _contents;

    [SerializeField] private List<CommonItemInfo> _premiumItems;
    [SerializeField] private List<CommonEntityInfo> _premiumCards;

    [SerializeField] private List<CommonItemInfo> _freeItems;
    [SerializeField] private List<CommonEntityInfo> _freeCards;

    #endregion // serialized fields





    #region private variables

    private List<MItem> _freeRewardData;
    private List<MItem> _premiumRewardData;

    private List<MItem> _premiumHeroRewards = new List<MItem>();
    private List<MItem> _premiumItemRewards = new List<MItem>();

    private List<MItem> _freeHeroRewards = new List<MItem>();
    private List<MItem> _freeItemRewards = new List<MItem>();

    #endregion // private variables





    #region public funcs

    public override void SetShow() {
        _freeRewardData = GameDataManager.Instance.ReceivedFreeItems;
        _premiumRewardData = GameDataManager.Instance.ReceivedChargeItems;

        Initialize();

        RefreshUI();

        base.SetShow();
    }

    public override void SetHide() {
        base.SetHide();
    }

    public override void RefreshUI() {
        UpdateTextsUI();
        UpdateContentsUI();

        if (0 != _freeRewardData.Count && 0 != _premiumRewardData.Count) {
            rectTransform.sizeDelta = new Vector2(_bigWidth, rectTransform.sizeDelta.y);
        }
        else {
            rectTransform.sizeDelta = new Vector2(_smallWidth, rectTransform.sizeDelta.y);
        }
    }

    public override void UpdateTextsUI() {
        SetText((int)TextType.Title, TStrings.Instance.FindString("HunterPath_25032"));
        SetText((int)TextType.Premium, TStrings.Instance.FindString("HunterPath_25034"));
        SetText((int)TextType.Free, TStrings.Instance.FindString("HunterPath_25033"));
        SetText((int)TextType.OK, TStrings.Instance.FindString("Mission_22004"));
    }

    public void OnClicked_Close() {
        SoundManager.Instance.PlaySFX(FXSound.Click);

        SetHide();
    }
    public void OnClick_FreeItemReward(int index) {
        SoundManager.Instance.PlaySFX(FXSound.Click);

#if SHOW_LOG
        Debug.LogWarningFormat("SkyPassPopupReward : OnClick_FreeItemReward : [ clicked index : {0} ]\n", index);
#endif // SHOW_LOG

        TItem item = TItems.Instance.Find(_freeItemRewards[index].m_refId);

        PopupManager.Instance.ShowTooltip(_freeItems[index].transform, item);
    }

    public void OnClick_FreeCardReward(int index) {
        SoundManager.Instance.PlaySFX(FXSound.Click);

#if SHOW_LOG
        Debug.LogWarningFormat("SkyPassPopupReward : OnClick_FreeCardReward : [ clicked index : {0} ]\n", index);
#endif // SHOW_LOG

        TCard card = TCards.Instance.Find(_freeHeroRewards[index].m_refId);

        PopupManager.Instance.ShowTooltip(_freeCards[index].transform, card);
    }

    public void OnClick_PremiumItemReward(int index) {
        SoundManager.Instance.PlaySFX(FXSound.Click);

#if SHOW_LOG
        Debug.LogWarningFormat("SkyPassPopupReward : OnClick_PremiumItemReward : [ clicked index : {0} ]\n", index);
#endif // SHOW_LOG

        TItem item = TItems.Instance.Find(_premiumItemRewards[index].m_refId);

        PopupManager.Instance.ShowTooltip(_premiumItems[index].transform, item);
    }

    public void OnClick_PremiumCardReward(int index) {
        SoundManager.Instance.PlaySFX(FXSound.Click);

#if SHOW_LOG
        Debug.LogWarningFormat("SkyPassPopupReward : OnClick_PremiumCardReward : [ clicked index : {0} ]\n", index);
#endif // SHOW_LOG

        TCard card = TCards.Instance.Find(_premiumHeroRewards[index].m_refId);

        PopupManager.Instance.ShowTooltip(_premiumCards[index].transform, card);
    }

    #endregion //public funcs





    #region private funcs

    private void Initialize() {
        _freeHeroRewards.Clear();
        _freeItemRewards.Clear();
        _premiumHeroRewards.Clear();
        _premiumItemRewards.Clear();

        for (int i = 0, count = _freeRewardData.Count; i < count; ++i) {
            if ((int)AssetType.CardType == _freeRewardData[i].m_type) {
                _freeHeroRewards.Add(_freeRewardData[i]);
            }
            else if ((int)AssetType.ConsumableItem == _freeRewardData[i].m_type || (int)AssetType.Jewel == _freeRewardData[i].m_type) {
                _freeItemRewards.Add(_freeRewardData[i]);
            }
        }

        if (_freeCards.Count < _freeRewardData.Count) {
            for (int i = _freeCards.Count, count = _freeRewardData.Count; i < count; ++i) {
                GameObject heroObject = Instantiate(_freeCards[0].gameObject, _freeCards[0].transform.parent);
                _freeCards.Add(heroObject.GetComponent<CommonEntityInfo>());
            }
        }

        if (_freeItems.Count < _freeItemRewards.Count) {
            for (int i = _freeItems.Count, count = _freeItemRewards.Count; i < count; ++i) {
                GameObject heroObject = Instantiate(_freeItems[0].gameObject, _freeItems[0].transform.parent);
                _freeItems.Add(heroObject.GetComponent<CommonItemInfo>());
            }
        }

        for (int i = 0, count = _premiumRewardData.Count; i < count; ++i) {
            if ((int)AssetType.CardType == _premiumRewardData[i].m_type) {
                _premiumHeroRewards.Add(_premiumRewardData[i]);
            }
            else if ((int)AssetType.ConsumableItem == _premiumRewardData[i].m_type || (int)AssetType.Jewel == _premiumRewardData[i].m_type) {
                _premiumItemRewards.Add(_premiumRewardData[i]);
            }
        }

        if (_premiumCards.Count < _premiumRewardData.Count) {
            for (int i = _premiumCards.Count, count = _premiumRewardData.Count; i < count; ++i) {
                GameObject heroObject = Instantiate(_premiumCards[0].gameObject, _premiumCards[0].transform.parent);
                _premiumCards.Add(heroObject.GetComponent<CommonEntityInfo>());
            }
        }

        if (_premiumItems.Count < _premiumItemRewards.Count) {
            for (int i = _premiumItems.Count, count = _premiumItemRewards.Count; i < count; ++i) {
                GameObject heroObject = Instantiate(_premiumItems[0].gameObject, _premiumItems[0].transform.parent);
                _premiumItems.Add(heroObject.GetComponent<CommonItemInfo>());
            }
        }
    }

    private void UpdateContentsUI() {
        _contents[(int)RewardType.Free].SetActive(0 != _freeRewardData.Count);
        _contents[(int)RewardType.Premium].SetActive(0 != _premiumRewardData.Count);


        for (int i = 0, count = _freeCards.Count; i < count; ++i) {
            if (i < _freeHeroRewards.Count) {
                _freeCards[i].Initialize(i, EntityType.None, OnClick_FreeCardReward);
                _freeCards[i].SetShowReward(_freeHeroRewards[i]);
            }
            else {
                _freeCards[i].SetHide();
            }
        }

        for (int i = 0, count = _freeItems.Count; i < count; ++i) {
            if (i < _freeItemRewards.Count) {
                _freeItems[i].Initialize(i, OnClick_FreeItemReward, null, null, null);
                _freeItems[i].SetShowRewardItem(_freeItemRewards[i]);
            }
            else {
                _freeItems[i].SetHide();
            }
        }

        for (int i = 0, count = _premiumCards.Count; i < count; ++i) {
            if (i < _premiumHeroRewards.Count) {
                _premiumCards[i].Initialize(i, EntityType.None, OnClick_PremiumCardReward);
                _premiumCards[i].SetShowReward(_premiumHeroRewards[i]);
            }
            else {
                _premiumCards[i].SetHide();
            }
        }

        for (int i = 0, count = _premiumItems.Count; i < count; ++i) {
            if (i < _premiumItemRewards.Count) {
                _premiumItems[i].Initialize(i, OnClick_PremiumItemReward, null, null, null);
                _premiumItems[i].SetShowRewardItem(_premiumItemRewards[i]);
            }
            else {
                _premiumItems[i].SetHide();
            }
        }
    }

    #endregion //private funcs
}
