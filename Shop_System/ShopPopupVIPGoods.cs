using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class ShopPopupVIPGoods : LobbyUIBase
{
    #region type def

    private enum TextType
    {
        Title,
        Jewel,
        Emblem,
        Ticket,
        Worker,
        Summon,
        Price,
        Desc,
        CannotBuy,
    }

    private enum ButtonType
    {
        Purchase,
        Purchased,
    }

    #endregion // type def





    #region serialized fields

    [SerializeField] private List<Button> _buttons;

    #endregion // serialized fields





    #region private variables

    private TPrice _tPrice;
    private System.Action<int> _clickCallback;
    private int _limitVIPDays = 395;

    #endregion // private variables





    #region public funcs

    public override void UpdateTextsUI() {
        SetText((int)TextType.Title, _tPrice.LangTypeName);
        SetText((int)TextType.Jewel, TStrings.Instance.FindString("SHOP_9019"));
        SetText((int)TextType.Emblem, TStrings.Instance.FindString("SHOP_9020"));
        SetText((int)TextType.Ticket, TStrings.Instance.FindString("SHOP_9021"));
        SetText((int)TextType.Worker, TStrings.Instance.FindString("SHOP_9022"));
        SetText((int)TextType.Summon, TStrings.Instance.FindString("SHOP_9023"));
        SetText((int)TextType.Price, _tPrice.LangTypePrice);
        if (_tPrice._positionOrder == (int)ShopPositionType.VIP_Top) {
            SetText((int)TextType.Desc, TStrings.Instance.FindString("SHOP_9025"));
        }
        else {
            SetText((int)TextType.Desc, TStrings.Instance.FindString("SHOP_9026"));
        }
        SetText((int)TextType.CannotBuy, TStrings.Instance.FindString("SHOP_9030"));
    }

    public override void RefreshUI() {
        UpdateTextsUI();
        UpdateContentsUI();
    }

    public void Initialize(System.Action<int> clickCallback) {
        _clickCallback = clickCallback;
    }

    public void SetShow(int id) {
        _tPrice = TPrices.Instance.Find(id);

        RefreshUI();

        base.SetShow();
    }

    public void Onclick_Buy() {
        if (null == _clickCallback) {
            return;
        }

        SoundManager.Instance.PlaySFX(FXSound.Click);

        _clickCallback(_tPrice._id);
    }

    #endregion //public funcs





    #region private funcs

    private void UpdateContentsUI() {
        TimeSpan restTimeVIP = Common.ConvertJavaMillisecondToDateTimeUTC(GameDataManager.Instance.ShopInfo.m_vipTime) - System.DateTime.UtcNow;

        _buttons[(int)ButtonType.Purchased].gameObject.SetActive(restTimeVIP.TotalDays + _tPrice.durationDay > _limitVIPDays);
        _buttons[(int)ButtonType.Purchase].gameObject.SetActive(restTimeVIP.TotalDays + _tPrice.durationDay <= _limitVIPDays);
    }

    #endregion //private funcs
}
