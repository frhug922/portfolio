using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanWarUI : LobbyUIBase
{
    #region type def

    private enum TextType
    {
        WarBox,
        NowWar,
        TeamEdit,
        StartTImeToMatch,
        TimeToMatch,
        Matching,
        AttendBefore,
        WarGround,
        SliderL,
        SliderR,
        ClanOur,
        ClanOther,
        TimeToStart,
        WarTime,
        AttendAfter,
    }

    private enum ObjType
    {
        Check,

        BeforeMatching,
        AfterMatching,

        MatchStartThing,
        MatchMakeThing,

        BeforeAttendCheck,
    }

    private enum ToolTipTFType
    {
        WarRule,
        WarBox,
        Slider,
    }

    #endregion // type def





    #region serialized fields

    [SerializeField] private List<GameObject> _onAndOffObjs;
    [SerializeField] private TextSlider _warBoxSlider;
    [SerializeField] private Image _clanwarRuleIcon;
    [SerializeField] private List<Transform> _tooltipTF;
    [SerializeField] private Slider _slider;

    [Header("For Our Clan")]
    [SerializeField] private Image _ourClanFlagImage;
    [SerializeField] private Image _ourClanSimbolImage;
    [Header("For Other Clan")]
    [SerializeField] private Image _otherClanFlagImage;
    [SerializeField] private Image _otherClanSimbolImage;

    [Header("For Clan War Box")]
    [SerializeField] private GameObject _boxReward;
    [SerializeField] private List<CommonItemInfo> _items;
    [SerializeField] private List<CommonEntityInfo> _heros;

    #endregion //serialized fields





    #region private fields

    private ClanData _clanData;
    private MClanInfo _otherClanInfo;
    private MClanInfo _myClanInfo;
    private ClanWarProgressType clanWarProgressType;
    private Coroutine _clanWarRestTimeCoroutine;
    private MWarProgress _clanWarProgress;
    private List<MItem> _heroRewards = new();
    private List<MItem> _itemRewards = new();

    #endregion //private fields





    #region public funcs

    public override void SetShow() {
        base.SetShow();

        _clanData = PlayerManager.Instance.ClanData;
        _otherClanInfo = GameDataManager.Instance.ClanWarInfo.vsClanInfo;
        _myClanInfo = GameDataManager.Instance.ClanWarInfo.clanInfo;
        clanWarProgressType = GameDataManager.Instance.GetClanWarProgress();
        _clanWarProgress = GameDataManager.Instance.ClanWarInfo.warProgress;

        RefreshUI();
        StartUpdateClanWarTime();
    }

    public override void SetHide() {
        SystemManager.Instance.ResetBattleType();
        LobbySceneUIManager.Instance.ClanWarDeckEditType = DeckType.None;

        base.SetHide();
    }

    public override void UpdateTextsUI() {
        SetText((int)TextType.WarBox, TStrings.Instance.FindString("Clan_29122"));
        SetText((int)TextType.NowWar, TStrings.Instance.FindString("Clan_29123"));
        SetText((int)TextType.AttendBefore, TStrings.Instance.FindString("Clan_29120"));
        SetText((int)TextType.WarGround, TStrings.Instance.FindString("Clan_29185"));
        SetText((int)TextType.SliderL, _myClanInfo.m_point.ToString());
        SetText((int)TextType.SliderR, _otherClanInfo.m_point.ToString());
        SetText((int)TextType.ClanOur, _clanData._name);
        SetText((int)TextType.ClanOther, _otherClanInfo.m_clanname);
        SetText((int)TextType.AttendAfter, TStrings.Instance.FindString("Clan_29120"));

        if (ClanWarProgressType.MatchMakingReady == clanWarProgressType) {
            SetText((int)TextType.TeamEdit, TStrings.Instance.FindString("TEAMEDIT_10003"));
            SetText((int)TextType.StartTImeToMatch, TStrings.Instance.FindString("Clan_29124"));
        }
        else if (ClanWarProgressType.MatchMaking == clanWarProgressType) {
            SetText((int)TextType.TeamEdit, TStrings.Instance.FindString("TEAMEDIT_10003"));
            SetText((int)TextType.Matching, TStrings.Instance.FindString("Clan_29125"));
        }
        else if (ClanWarProgressType.ClanMatching == clanWarProgressType) {
            SetText((int)TextType.TeamEdit, TStrings.Instance.FindString("TEAMEDIT_10003"));
            SetText((int)TextType.Matching, TStrings.Instance.FindString("Clan_29125"));
        }
        else if (ClanWarProgressType.ReadyWar == clanWarProgressType) {
            SetText((int)TextType.TimeToStart, TStrings.Instance.FindString("Clan_29186"));
        }
        else if (ClanWarProgressType.DeckCopying == clanWarProgressType) {
            SetText((int)TextType.TimeToStart, TStrings.Instance.FindString("Clan_29247"));
        }
        else if (ClanWarProgressType.War == clanWarProgressType) {
            SetText((int)TextType.TimeToStart, TStrings.Instance.FindString("Clan_29187"));
        }
        else if (ClanWarProgressType.AfterWar == clanWarProgressType) {
            SetText((int)TextType.TimeToStart, TStrings.Instance.FindString("Clan_29182"));
            SetText((int)TextType.WarTime, GameDataManager.Instance.GetClanWarResultToString());
        }
    }

    public override void RefreshUI() {
        UpdateContentsUI();
        UpdateTextsUI();
    }

    public void OnClick_ToggleAttend() {
        if (IsClanWarAttend()) {
            PopupManager.Instance.ShowOKCancelPopup(TStrings.Instance.FindString("Clan_29188"), TStrings.Instance.FindString("Clan_29126"), () => {
                WebHttp.Instance.RequestClanWarUserAttendChange(ClanWarAttendType.NotAttend, () => {
                    _onAndOffObjs[(int)ObjType.Check].SetActive(IsClanWarAttend());
                    _onAndOffObjs[(int)ObjType.BeforeAttendCheck].SetActive(IsClanWarAttend());
                });
            });
        }
        else {
            if (GameDataManager.Instance.ClanWarInfo.deckCards.Count < Common.teamMemberMax) {
                GameDataManager.Instance.IsClanDefenceDeckEditFirst = true;
                EditDefenceDeck(() => {
                    WebHttp.Instance.RequestClanWarUserAttendChange(ClanWarAttendType.Attend, () => {
                        _onAndOffObjs[(int)ObjType.Check].SetActive(IsClanWarAttend());
                        _onAndOffObjs[(int)ObjType.BeforeAttendCheck].SetActive(IsClanWarAttend());
                    });
                });
            }
            else {
                WebHttp.Instance.RequestClanWarUserAttendChange(ClanWarAttendType.Attend, () => {
                    _onAndOffObjs[(int)ObjType.Check].SetActive(IsClanWarAttend());
                    _onAndOffObjs[(int)ObjType.BeforeAttendCheck].SetActive(IsClanWarAttend());
                });
            }
        }
    }

    public void OnClick_WarGround() {
        WebHttp.Instance.RequestClanWarBattleFieldInfo(() => {
            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.ClanWarMap);
        });
    }

    public void OnClick_DefenseTeamEdit() {
        EditDefenceDeck();
    }

    public void OnClick_OurClanFlag() {
        WebHttp.Instance.RequestClanInfo(_myClanInfo.m_clanId, LobbySceneUIManager.Instance.CurrentSubType);
    }

    public void OnClick_EnemyClanFlag() {
        WebHttp.Instance.RequestClanInfo(_otherClanInfo.m_clanId, LobbySceneUIManager.Instance.CurrentSubType);
    }

    public void OnClick_ClanWarRule() {
        PopupManager.Instance.ShowTooltip(_tooltipTF[(int)ToolTipTFType.WarRule],
            TSpecialRuleRotations.Instance.Find((BattleSpecialType)_clanWarProgress.m_war_property).LangTypeName,
            TSpecialRules.Instance.Find((BattleSpecialType)_clanWarProgress.m_war_property).LangTypeDesc);
    }

    public void OnClick_WarBox() {
        if (GameDataManager.Instance.ClanWarInfo.battleBoxRewardCount > 0) {
            WebHttp.Instance.RequestClanWarBoxReward(() => {
                ShowBoxReward();
                RefreshUI();
            });
            return;
        }
        PopupManager.Instance.ShowTooltip(_tooltipTF[(int)ToolTipTFType.WarBox], TStrings.Instance.FindString("Clan_29122"), TStrings.Instance.FindString("Clan_29167"));
    }

    public void OnClick_Slider() {
        PopupManager.Instance.ShowTooltip(_tooltipTF[(int)ToolTipTFType.Slider], TStrings.Instance.FindString("Clan_29248"), TStrings.Instance.FindString("Clan_29249"));
    }

    public void Onclick_Get() {
        _boxReward.gameObject.SetActive(false);
    }

    #endregion // public funcs





    #region private funcs

    private void UpdateContentsUI() {
        if (ClanWarProgressType.MatchMakingReady == clanWarProgressType || ClanWarProgressType.MatchMaking == clanWarProgressType || ClanWarProgressType.ClanMatching == clanWarProgressType) {
            _onAndOffObjs[(int)ObjType.BeforeMatching].SetActive(true);
            _onAndOffObjs[(int)ObjType.AfterMatching].SetActive(false);
            _onAndOffObjs[(int)ObjType.MatchStartThing].SetActive(ClanWarProgressType.MatchMakingReady == clanWarProgressType);
            _onAndOffObjs[(int)ObjType.MatchMakeThing].SetActive(ClanWarProgressType.MatchMakingReady != clanWarProgressType);
        }
        else {
            _onAndOffObjs[(int)ObjType.AfterMatching].SetActive(true);
            _onAndOffObjs[(int)ObjType.BeforeMatching].SetActive(false);

            SetClanIcon(ref _clanwarRuleIcon, string.Format("waricon{0:D2}", _clanWarProgress.m_war_property - 3));

            SetClanFlag();
            SetWarPointSlider();
        }

        _onAndOffObjs[(int)ObjType.Check].SetActive(IsClanWarAttend());
        _onAndOffObjs[(int)ObjType.BeforeAttendCheck].SetActive(IsClanWarAttend());
        SetWarBoxInfo();
    }

    private void SetClanFlag() {
        SetClanPattern(ref _ourClanFlagImage, TClanFlags.Instance.Find(_clanData._pattern)._pattern);
        SetClanSymbol(ref _ourClanSimbolImage, TClanFlags.Instance.Find(_clanData._symbol)._symbol);

        if (0 != _otherClanInfo.m_clanId) {
            SetClanPattern(ref _otherClanFlagImage, TClanFlags.Instance.Find(_otherClanInfo.m_pattern)._pattern);
            SetClanSymbol(ref _otherClanSimbolImage, TClanFlags.Instance.Find(_otherClanInfo.m_mark)._symbol);
        }
    }

    private void SetWarBoxInfo() {
        long nowAmount = GameDataManager.Instance.ClanWarInfo.clanInfo.m_war_box;
        long maxAmount = 25L;

        if (nowAmount > maxAmount) {
            nowAmount = maxAmount;
        }

        _warBoxSlider.SetShow(true, nowAmount, maxAmount, true);
    }

    private bool IsClanWarAttend() {
        ClanData clandata = PlayerManager.Instance.ClanData;

        if (clandata._myClanUserInfo.m_attend_whether == (int)ClanWarAttendType.Attend) {
            return true;
        }
        return false;
    }

    private void SetWarPointSlider() {
        int nowValue = _myClanInfo.m_point;
        int maxValue = _myClanInfo.m_point + _otherClanInfo.m_point;

        if (0 == maxValue) {
            _slider.value = 0.5f;
            return;
        }

        float amount = nowValue / maxValue;

        if (amount < 0.1f) {
            amount = 0.1f;
        }
        else if (amount > 0.9f) {
            amount = 0.9f;
        }

        _slider.value = amount;
    }

    private void StopUpdateClanWarTime() {
        if (null == _clanWarRestTimeCoroutine) {
            return;
        }

        StopCoroutine(_clanWarRestTimeCoroutine);
        _clanWarRestTimeCoroutine = null;
    }

    private void StartUpdateClanWarTime() {
        StopUpdateClanWarTime();

        _clanWarRestTimeCoroutine = StartCoroutine(UpdateClanWarTime());
    }

    private IEnumerator UpdateClanWarTime() {
        var updateTerm = new WaitForSeconds(1f);

        while (true) {
            UpdateTime();

            yield return updateTerm;
        }
    }

    private void UpdateTime() {
        if (ClanWarProgressType.MatchMakingReady == clanWarProgressType) {
            TimeSpan timespan = Common.ConvertJavaMillisecondToDateTimeUTC(GameDataManager.Instance.ClanWarInfo.warProgress.m_open_time) - System.DateTime.UtcNow;
            SetText((int)TextType.TimeToMatch, string.Format(TStrings.Instance.FindString("SHOP_9035"), timespan.Hours, timespan.Minutes));
        }
        else if (ClanWarProgressType.ReadyWar == clanWarProgressType) {
            TimeSpan timespan = Common.ConvertJavaMillisecondToDateTimeUTC(GameDataManager.Instance.ClanWarInfo.warProgress.m_war_ready_time) - System.DateTime.UtcNow;
            SetText((int)TextType.WarTime, string.Format(TStrings.Instance.FindString("SHOP_9035"), timespan.Hours, timespan.Minutes));
        }
        else if (ClanWarProgressType.DeckCopying == clanWarProgressType) {
            TimeSpan timespan = Common.ConvertJavaMillisecondToDateTimeUTC(GameDataManager.Instance.ClanWarInfo.warProgress.m_war_ready_progress_time) - System.DateTime.UtcNow;
            SetText((int)TextType.WarTime, string.Format(TStrings.Instance.FindString("SHOP_9035"), timespan.Hours, timespan.Minutes));
        }
        else if (ClanWarProgressType.War == clanWarProgressType) {
            TimeSpan timespan = Common.ConvertJavaMillisecondToDateTimeUTC(GameDataManager.Instance.ClanWarInfo.warProgress.m_war_end_time) - System.DateTime.UtcNow;
            SetText((int)TextType.WarTime, string.Format(TStrings.Instance.FindString("SHOP_9035"), timespan.Hours, timespan.Minutes));
        }
    }

    private void EditDefenceDeck(System.Action callback = null) {
        SystemManager.Instance.SetBattleType(BattleType.ClanWar);
        LobbySceneUIManager.Instance.ClanWarDeckEditType = DeckType.Defence;

        LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.TeamEdit);

        GameDataManager.Instance.SetCallback(callback);
    }

    private void ShowBoxReward() {
        //Initialize Items
        List<MItem> rewardItems = GameDataManager.Instance.PurchasedItem;

        int rewardCount = rewardItems.Count;

        _heroRewards.Clear();
        _itemRewards.Clear();

        for (int i = 0; i < rewardCount; ++i) {
            if (rewardItems[i].m_type == (int)AssetType.CardType) {
                _heroRewards.Add(rewardItems[i]);
            }
            else if (rewardItems[i].m_type == (int)AssetType.ConsumableItem || rewardItems[i].m_type == (int)AssetType.Jewel) {
                _itemRewards.Add(rewardItems[i]);
            }
        }

        if (_heros.Count < _heroRewards.Count) {
            for (int i = _heros.Count, count = _heroRewards.Count; i < count; ++i) {
                GameObject heroObject = Instantiate(_heros[0].gameObject, _heros[0].transform.parent);
                _heros.Add(heroObject.GetComponent<CommonEntityInfo>());
            }
        }

        if (_items.Count < _itemRewards.Count) {
            for (int i = _items.Count, count = _itemRewards.Count; i < count; ++i) {
                GameObject itemObject = Instantiate(_items[0].gameObject, _items[0].transform.parent);
                _items.Add(itemObject.GetComponent<CommonItemInfo>());
            }
        }

        //Show Items
        for (int i = 0, count = _heros.Count; i < count; ++i) {
            if (i < _heroRewards.Count) {
                _heros[i].SetShowReward(_heroRewards[i]);
            }
            else {
                _heros[i].SetHide();
            }
        }

        for (int i = 0, count = _items.Count; i < count; ++i) {
            if (i < _itemRewards.Count) {
                _items[i].Initialize(i, OnClick_Item, null, null, null);
                if (_itemRewards[i].m_type == (int)AssetType.Jewel) {
                    _items[i].SetShowAsset(_itemRewards[i]);
                }
                else {
                    _items[i].SetShowRewardItem(_itemRewards[i]);
                }
            }
            else {
                _items[i].SetHide();
            }
        }
        _boxReward.SetActive(true);
    }


    private void OnClick_Item(int index) {
        SoundManager.Instance.PlaySFX(FXSound.Click);

#if SHOW_LOG
        Debug.LogWarningFormat("ResultUI : OnClick_Item : [ clicked index : {0} ]\n", index);
#endif // SHOW_LOG

        if (index < _itemRewards.Count) {
            if (_itemRewards[index].m_type == (int)AssetType.Jewel) {
                PopupManager.Instance.ShowTooltip(_items[index].transform, TItems.Instance.Find((int)AssetType.Jewel));
            }
            else {
                PopupManager.Instance.ShowTooltip(_items[index].transform, TItems.Instance.Find(_itemRewards[index].m_id));
            }
        }
    }

    #endregion //private funcs
}
