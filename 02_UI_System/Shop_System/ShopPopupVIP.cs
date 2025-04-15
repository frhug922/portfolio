using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPopupVIP : LobbyUIBase
{
    #region type def

    private enum TextType
    {
        Name,
        Active,
    }

    #endregion // type def





    #region serialized fields

    [SerializeField] private List<ShopPopupVIPGoods> _shopPopupVIPGoods;
    [SerializeField] private GameObject _questionMark;

    #endregion // serialized fields





    #region private fields

    private Coroutine _goodsSaleEndTimeCoroutine;

    #endregion // private fields





    #region public funcs

    public override void UpdateTextsUI() {
        SetText((int)TextType.Name, TStrings.Instance.FindString("SHOP_9042"));

        TimeSpan timespan = Common.ConvertJavaMillisecondToDateTimeUTC(GameDataManager.Instance.ShopInfo.m_vipTime) - System.DateTime.UtcNow;
        if (0 > timespan.TotalMilliseconds) {
            SetText((int)TextType.Active, TStrings.Instance.FindString("HunterPath_25030"));
        }
        else {
            SetText((int)TextType.Active, string.Format("{0} {1}", TStrings.Instance.FindString("SHOP_9013"), string.Format(TStrings.Instance.FindString("SHOP_9034"), timespan.Days, timespan.Hours)));
        }
    }

    public override void RefreshUI() {
        //base.RefreshUI();

        UpdateTextsUI();
        UpdateContentsUI();
    }

    public void Initialize(System.Action<int> clickCallback) {
        for (int i = 0, count = _shopPopupVIPGoods.Count; i < count; ++i) {
            _shopPopupVIPGoods[i].Initialize(clickCallback);
        }
    }

    public override void SetShow() {
        RefreshUI();

        base.SetShow();

        StartUpdateGoodsSaleTime();
    }

    public void Onclick_Close() {
        SetHide();
    }

    public void OnClick_QuestionMark() {
        PopupManager.Instance.ShowTooltip(_questionMark.transform, TStrings.Instance.FindString("SHOP_9042"), TStrings.Instance.FindString("SHOP_9033"));
    }

    #endregion //public funcs





    #region private funcs

    private void UpdateContentsUI() {
        TPrice tPrice = TPrices.Instance.Find(ShopProductType.VIP);
        for (int i = 0, count = _shopPopupVIPGoods.Count; i < count; ++i) {
            _shopPopupVIPGoods[i].SetShow(tPrice._id + i);
        }
    }

    private void StopUpdateGoodsSaleTime() {
        if (null == _goodsSaleEndTimeCoroutine) {
            return;
        }

        StopCoroutine(_goodsSaleEndTimeCoroutine);
        _goodsSaleEndTimeCoroutine = null;
    }

    private void StartUpdateGoodsSaleTime() {
        StopUpdateGoodsSaleTime();

        _goodsSaleEndTimeCoroutine = StartCoroutine(UpdateGoodsSaleEndTime());
    }

    private IEnumerator UpdateGoodsSaleEndTime() {
        var updateTerm = new WaitForSeconds(1f);

        while (true) {
            RefreshUI();

            yield return updateTerm;
        }
    }

    #endregion //private funcs
}
