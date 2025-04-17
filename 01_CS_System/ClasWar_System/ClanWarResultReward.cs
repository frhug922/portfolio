using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanWarResultReward : LobbyMenuUIBase
{
    #region type def

    private enum TextType
    {
        //Top
        Title,

        //Middle
        Result,
        SliderL,
        SliderR,
        ClanOur,
        ClanEnemy,
        BestPlayer,
        BestPlayerName,
        Level,
        GetPoint,
        Point,
        BestEnemy,
        BestEnemyName,
        LevelE,
        GetPointE,
        PointE,

        //Bottom
        RewardGrade,
        Ok,
    }

    public enum ClanWarResultType
    {
        Draw,
        Win,
        Lose
    }

    #endregion // type def





    #region serialized fields

    [SerializeField] private List<CommonItemInfo> _rewardItems;
    [SerializeField] private Image _ourClanFlagImage;
    [SerializeField] private Image _ourClanSimbolImage;
    [SerializeField] private Image _otherClanFlagImage;
    [SerializeField] private Image _otherClanSimbolImage;
    [SerializeField] private Slider _slider;
    [SerializeField] private ProfileItem _allyProfileItem;
    [SerializeField] private ProfileItem _enemyProfileItem;

    #endregion //serialized fields





    #region private fields

    private MClanWarResult _clanWarResult;
    private ClanWarResultType _resultType;
    private List<MItem> _items;

    #endregion //private fields





    #region public funcs

    public override void SetShow() {
        base.SetShow();

        _clanWarResult = GameDataManager.Instance.ClanWarRewardResponse.ClanWarResult;
        _items = GameDataManager.Instance.ClanWarRewardResponse.Items;
        _resultType = (ClanWarResultType)_clanWarResult.m_result;

        RefreshUI();
    }

    public override void UpdateTextsUI() {
        SetText((int)TextType.Title, TStrings.Instance.FindString("Clan_29151"));

        SetText((int)TextType.SliderL, _clanWarResult.m_items.m_totalPoint.ToString());
        SetText((int)TextType.ClanOur, _clanWarResult.m_items.m_clanname);
        SetText((int)TextType.BestPlayer, TStrings.Instance.FindString("Clan_29154"));
        SetText((int)TextType.BestPlayerName, _clanWarResult.m_items.m_nickname);
        SetText((int)TextType.Level, _clanWarResult.m_items.m_level.ToString());
        SetText((int)TextType.GetPoint, TStrings.Instance.FindString("Clan_29156"));
        SetText((int)TextType.Point, _clanWarResult.m_items.m_getpoint.ToString());

        SetText((int)TextType.SliderR, _clanWarResult.m_vs_items.m_totalPoint.ToString());
        SetText((int)TextType.ClanEnemy, _clanWarResult.m_vs_items.m_clanname);
        SetText((int)TextType.BestEnemy, TStrings.Instance.FindString("Clan_29155"));
        SetText((int)TextType.BestEnemyName, _clanWarResult.m_vs_items.m_nickname);
        SetText((int)TextType.LevelE, _clanWarResult.m_vs_items.m_level.ToString());
        SetText((int)TextType.GetPointE, TStrings.Instance.FindString("Clan_29156"));
        SetText((int)TextType.PointE, _clanWarResult.m_vs_items.m_getpoint.ToString());

        if (ClanWarResultType.Draw == _resultType) {
            SetText((int)TextType.RewardGrade, TStrings.Instance.FindString("Clan_29250"));
            SetText((int)TextType.Result, TStrings.Instance.FindString("Clan_29192"));
        }
        else if (ClanWarResultType.Win == _resultType) {
            SetText((int)TextType.RewardGrade, TStrings.Instance.FindString("Clan_29157"));
            SetText((int)TextType.Result, TStrings.Instance.FindString("Clan_29152"));
        }
        else if (ClanWarResultType.Lose == _resultType) {
            SetText((int)TextType.RewardGrade, TStrings.Instance.FindString("Clan_29158"));
            SetText((int)TextType.Result, TStrings.Instance.FindString("Clan_29153"));
        }
        SetText((int)TextType.Ok, TStrings.Instance.FindString("Clan_29159"));
    }

    public override void RefreshUI() {
        UpdateTextsUI();

        MClanWarClanUserInfo allyInfo = _clanWarResult.m_items;
        MClanWarClanUserInfo enemyInfo = _clanWarResult.m_vs_items;
        _allyProfileItem.SetShow(allyInfo.m_avatar, allyInfo.m_avatarbg, allyInfo.m_avatarpin);
        _allyProfileItem.SetShow(enemyInfo.m_avatar, enemyInfo.m_avatarbg, enemyInfo.m_avatarpin);

        ShowItems();
        SetClanFlag();
        SetWarPointSlider();
    }

    #endregion // public funcs





    #region private funcs

    private void ShowItems() {
        for (int i = 0, count = _rewardItems.Count; i < count; ++i) {
            _rewardItems[i].Initialize(i, OnClick_Item, null, null, null);
        }

        for (int i = 0, count = _items.Count; i < count; ++i) {
            _rewardItems[i].SetShow(_items[i]);
        }
    }

    private void OnClick_Item(int index) {
        if ((int)AssetType.CardType != _items[index].m_type) {
            TItem item = TItems.Instance.Find(_items[index].m_refId);

            if (null == item) {
                item = TItems.Instance.Find(_items[index].m_type);
            }

            PopupManager.Instance.ShowTooltip(_rewardItems[index].transform, item);
        }
        else {
            return;
        }
    }

    private void SetClanFlag() {
        SetClanPattern(ref _ourClanFlagImage, TClanFlags.Instance.Find(_clanWarResult.m_items.m_pattern)._pattern);
        SetClanSymbol(ref _ourClanSimbolImage, TClanFlags.Instance.Find(_clanWarResult.m_items.m_mark)._symbol);

        SetClanPattern(ref _otherClanFlagImage, TClanFlags.Instance.Find(_clanWarResult.m_vs_items.m_pattern)._pattern);
        SetClanSymbol(ref _otherClanSimbolImage, TClanFlags.Instance.Find(_clanWarResult.m_vs_items.m_mark)._symbol);
    }

    private void SetWarPointSlider() {
        int nowValue = _clanWarResult.m_items.m_totalPoint;
        int maxValue = _clanWarResult.m_items.m_totalPoint + _clanWarResult.m_vs_items.m_totalPoint;

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

    #endregion //private funcs
}
