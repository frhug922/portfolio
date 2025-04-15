using System.Collections.Generic;
using UnityEngine;


public class ShopContentsTabs : LobbyUIBase
{
    #region serialize fields

    [SerializeField] private ShopGoodsType _goodsType;
    [SerializeField] private List<ShopGoods> _shopGoodItems;
    [SerializeField] private List<ShopBannerGoods> _shopBannerGoodItems;
    [SerializeField] private List<ShopPackageGoods> _shopPackageGoodItems;

    #endregion





    #region private variables

    private int _goodsStartID;
    private List<int> _goodsDataList;
    private int _initializedGoodsCount = 0;
    private int _initializedBannerCount = 0;

    private System.Action<int> _purchaseCallback;

    #endregion // private variables





    #region public funcs

    public override void SetShow() {
        base.SetShow();

        UpdateContentsUI();
    }

    public void Initialize(System.Action<int> clickCallback, System.Action<ShopUIController.ShopBannerType> bannerClickCallback) {
        _goodsStartID = TPrices.Instance.GoodsStartID(_goodsType);
        _goodsDataList = TPrices.Instance.GetGoodsList(_goodsType);

        _purchaseCallback = clickCallback;

        if (_goodsDataList.Count > _shopGoodItems.Count) {
            for (int i = _shopGoodItems.Count, count = _goodsDataList.Count; i < count; ++i) {
                GameObject shopObj = Instantiate(_shopGoodItems[0].gameObject, _shopGoodItems[0].transform.parent.transform);
                _shopGoodItems.Add(shopObj.GetComponent<ShopGoods>());
            }
        }

        for (int i = _initializedGoodsCount, count = _shopGoodItems.Count; i < count; ++i) {
            _shopGoodItems[i].Initialize(clickCallback);
            ++_initializedGoodsCount;
        }

        for (int i = _initializedBannerCount, count = _shopBannerGoodItems.Count; i < count; ++i) {
            _shopBannerGoodItems[i].Initialize(bannerClickCallback, clickCallback);
            ++_initializedBannerCount;
        }
    }

    public override void RefreshUI() {
        for (int i = 0, count = _shopGoodItems.Count; i < count; ++i) {
            _shopGoodItems[i].RefreshUI();
        }

        for (int i = 0, count = _shopBannerGoodItems.Count; i < count; ++i) {
            _shopBannerGoodItems[i].RefreshUI();
        }

        for (int i = 0, count = _shopPackageGoodItems.Count; i < count; ++i) {
            _shopPackageGoodItems[i].RefreshUI();
        }
    }

    public override void UpdateTextsUI() {
        for (int i = 0, count = _shopGoodItems.Count; i < count; ++i) {
            _shopGoodItems[i].UpdateTextsUI();
        }

        for (int i = 0, count = _shopBannerGoodItems.Count; i < count; ++i) {
            _shopBannerGoodItems[i].UpdateTextsUI();
        }

        for (int i = 0, count = _shopPackageGoodItems.Count; i < count; ++i) {
            _shopPackageGoodItems[i].UpdateTextsUI();
        }

        base.UpdateTextsUI();
    }

    #endregion // public funcs





    #region private funcs

    private void UpdateContentsUI() {
        for (int i = 0, count = _shopGoodItems.Count; i < count; ++i) {
            if (i < _goodsDataList.Count) {
                _shopGoodItems[i].SetShow(_goodsStartID + i);
            }
            else {
                _shopGoodItems[i].SetHide();
            }
        }

        for (int i = 0; i < _shopBannerGoodItems.Count; ++i) {
            if (ShopUIController.ShopBannerType.Combat == _shopBannerGoodItems[i].BannerType()) {
                _shopBannerGoodItems[i].SetShow(TPrices.Instance.GetBannerCombat().m_priceId);
            }
            else {
                _shopBannerGoodItems[i].SetShow();
            }
        }

        for (int i = 0; i < _shopPackageGoodItems.Count; ++i) {
            _shopPackageGoodItems[i].SetShow(_purchaseCallback);
        }
    }

    #endregion // private funcs
}
