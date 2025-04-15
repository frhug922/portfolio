using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopBannerGoods : LobbyUIBase
{
    #region type def

    private enum TextType
    {
        LimitTime,
        Desc_VIP,
        Desc_Avatar,
        Desc_Pass,
        Price,
        BtnConfirm,

        Desc_Battle,
        CanBuy,
        CannotBuy,
        Amount,
        Buy,
    }

    private enum ButtonType
    {
        Confirm,
        Buy,
        CantBuy,
    }

    private enum ImageType
    {
        Goods,
        Avatar,
    }

    #endregion // type def





    #region serialized fields

    [SerializeField] private ShopUIController.ShopBannerType _bannerType;
    [SerializeField] private Image[] _itemImage;
    [SerializeField] private Button[] _buyButton;
    [SerializeField] private List<GameObject> _gradeStar;
    [SerializeField] private GameObject _clockIcon;

    #endregion // serialized fields





    #region private variables

    private System.Action<ShopUIController.ShopBannerType> _clickCallback;
    private TPrice _tPrice;
    private System.Action<int> _buyCallback;
    private int _leftAmount;
    private int _amount;
    private bool _isLeft = true;

    #endregion // private variables





    #region properties



    #endregion // properties





    #region public funcs

    public void Initialize(System.Action<ShopUIController.ShopBannerType> clickCallback, System.Action<int> buyCallback) {
        _clickCallback = clickCallback;
        _buyCallback = buyCallback;
    }

    public override void SetShow() {
        if (_bannerType == ShopUIController.ShopBannerType.VIP) {
            SetHide();
            return;
        }
        else if (_bannerType == ShopUIController.ShopBannerType.Pass && GameDataManager.Instance.SkypassQuestInfo.m_pathonoff == 1) {
            SetHide();
            return;
        }
        base.SetShow();

        RefreshUI();
    }

    public void SetShow(int id) {
        if (_bannerType == ShopUIController.ShopBannerType.VIP || _bannerType == ShopUIController.ShopBannerType.Avatar) {
            SetHide();
            return;
        }
        else if (_bannerType == ShopUIController.ShopBannerType.Pass && GameDataManager.Instance.SkypassQuestInfo.m_pathonoff == 1) {
            SetHide();
            return;
        }
        _tPrice = TPrices.Instance.Find(id);

        RefreshUI();

        base.SetShow();
    }

    public override void RefreshUI() {
        if (ShopUIController.ShopBannerType.Combat == _bannerType) {
            GetAmount();
            CaculateLeftAmount();
            SetGradeStar();
        }

        UpdateContentsUI();
        UpdateTextsUI();
    }

    public override void UpdateTextsUI() {
        if (ShopUIController.ShopBannerType.Avatar == _bannerType) {
            SetText((int)TextType.Desc_Avatar, TStrings.Instance.FindString("SHOP_9010"));
            SetText((int)TextType.BtnConfirm, TStrings.Instance.FindString("SHOP_9008")); // 구매
        }
        else if (ShopUIController.ShopBannerType.Pass == _bannerType) {
            TimeSpan timespan = GameDataManager.Instance.SkypassSeasonEndTime;
            if (0 > timespan.TotalMilliseconds) {
                SetText((int)TextType.LimitTime, TStrings.Instance.FindString("SHOP_9043"));
            }
            else if (0 < timespan.Days) {
                SetText((int)TextType.LimitTime, string.Format(TStrings.Instance.FindString("SHOP_9034"), timespan.Days, timespan.Hours));
            }
            else {
                SetText((int)TextType.LimitTime, string.Format(TStrings.Instance.FindString("SHOP_9035"), timespan.Hours, timespan.Minutes));
            }
            SetText((int)TextType.Desc_Pass, TStrings.Instance.FindString("SHOP_9014"));
#if UNITY_ANDROID
            if (SystemManager.Instance.StoreType == StoreType.OneStore) {
                SetText((int)TextType.Price, string.Format(TStrings.Instance.FindString("SHOP_9028"), IAPManager.Instance.GetProductPrice(TPrices.Instance.Find(ShopProductType.Path)._productID_IOS)));
            }
            else {
                SetText((int)TextType.Price, string.Format(TStrings.Instance.FindString("SHOP_9028"), IAPManager.Instance.GetProductPrice(TPrices.Instance.Find(ShopProductType.Path)._productID_ANDROID)));
            }
#else
            SetText((int)TextType.Price, string.Format(TStrings.Instance.FindString("SHOP_9028"), IAPManager.Instance.GetProductPrice(TPrices.Instance.Find(ShopProductType.Path)._productID_IOS)));
#endif 
            SetText((int)TextType.BtnConfirm, TStrings.Instance.FindString("SHOP_9008")); // 구매
        }
        else if (ShopUIController.ShopBannerType.VIP == _bannerType) {
            TimeSpan timespan = Common.ConvertJavaMillisecondToDateTimeUTC(GameDataManager.Instance.ShopInfo.m_vipTime) - System.DateTime.UtcNow;
            if (0 > timespan.TotalMilliseconds) {
                SetText((int)TextType.LimitTime, TStrings.Instance.FindString("SHOP_9043"));
            }
            else if (0 < timespan.Days) {
                SetText((int)TextType.LimitTime, string.Format(TStrings.Instance.FindString("SHOP_9034"), timespan.Days, timespan.Hours));
            }
            else {
                SetText((int)TextType.LimitTime, string.Format(TStrings.Instance.FindString("SHOP_9035"), timespan.Hours, timespan.Minutes));
            }

            SetText((int)TextType.Desc_VIP, TStrings.Instance.FindString("SHOP_9013"));
            SetText((int)TextType.BtnConfirm, TStrings.Instance.FindString("SHOP_9012")); // 더보기
        }
        else if (ShopUIController.ShopBannerType.Combat == _bannerType) {
            TimeSpan timespan = GameDataManager.Instance.GetShopGoodsRestTime(_tPrice._id);
            if (0 < timespan.Days) {
                SetText((int)TextType.LimitTime, string.Format(TStrings.Instance.FindString("SHOP_9034"), timespan.Days, timespan.Hours));
            }
            else {
                SetText((int)TextType.LimitTime, string.Format(TStrings.Instance.FindString("SHOP_9035"), timespan.Hours, timespan.Minutes));
            }

            SetText((int)TextType.Buy, _tPrice.LangTypePrice);
            SetText((int)TextType.CannotBuy, TStrings.Instance.FindString("SHOP_9045"));
            SetText((int)TextType.Amount, "X" + _amount.ToString());
            SetText((int)TextType.Desc_Battle, _tPrice.LangTypeName);

            if (_isLeft) {
                SetText((int)TextType.CanBuy, string.Format("{0}:{1}", TStrings.Instance.FindString("SHOP_9007"), _leftAmount.ToString()));
            }
            else {
                SetText((int)TextType.CanBuy, TStrings.Instance.FindString("SHOP_9029"));
            }
        }
    }

    public override void SetHide() {
        base.SetHide();
    }

    public void OnClick_Comfirm() {
        if (null == _clickCallback) {
            return;
        }

        SoundManager.Instance.PlaySFX(FXSound.Click);

        _clickCallback(_bannerType);
    }

    public void OnClick_Buy() {
        if (null == _buyCallback) {
            return;
        }

        SoundManager.Instance.PlaySFX(FXSound.Click);

        _buyCallback(_tPrice._id);
    }

    public void OnClick_Pass() {
        if (_bannerType != ShopUIController.ShopBannerType.VIP) {
            return;
        }

        PopupManager.Instance.ShowTooltip(transform, TStrings.Instance.FindString("SHOP_9042"), TStrings.Instance.FindString("SHOP_9033"));
    }

    public ShopUIController.ShopBannerType BannerType() {
        return _bannerType;
    }

    #endregion //public funcs





    #region private funcs

    private void UpdateContentsUI() {
        if (ShopUIController.ShopBannerType.Combat == _bannerType) {
            SetItemImage(ref _itemImage[(int)ImageType.Goods], _tPrice._goodsImage);
        }
        else if (ShopUIController.ShopBannerType.Avatar == _bannerType) {
            SetItemImage(ref _itemImage[(int)ImageType.Avatar], TPrices.Instance.Find(GameDataManager.Instance.AvatarLists[0].m_priceId)._goodsImage);
        }
        else if (ShopUIController.ShopBannerType.VIP == _bannerType) {
            TimeSpan timespan = Common.ConvertJavaMillisecondToDateTimeUTC(GameDataManager.Instance.ShopInfo.m_vipTime) - System.DateTime.UtcNow;
            _clockIcon.SetActive(0 < timespan.TotalMilliseconds);
        }
        _buyButton[(int)ButtonType.Buy].gameObject.SetActive(_isLeft);
        _buyButton[(int)ButtonType.CantBuy].gameObject.SetActive(!_isLeft);
    }

    private void SetGradeStar() {
        TItem item = TItems.Instance.Find(Int32.Parse(_tPrice._goodsImage));

        if (item._grade > _gradeStar.Count) {
            for (int i = 0, count = item._grade - _gradeStar.Count; i < count; ++i) {
                GameObject gradeStar = Instantiate(_gradeStar[0], _gradeStar[0].transform.parent.transform);
                _gradeStar.Add(gradeStar);
            }
        }
    }

    private void GetAmount() {
        TReward reward = TRewards.Instance.Find(_tPrice._rewardID);
        _amount = reward.RewardItems[0]._minAmount;
    }

    private void CaculateLeftAmount() {
        MShopList _mshopList = TPrices.Instance.GetBannerCombat();

        _leftAmount = _tPrice._buyLimitCount - _mshopList.m_buyCnt;

        if (_leftAmount == 0) {
            _isLeft = false;
        }
    }

    #endregion //private funcs
}
