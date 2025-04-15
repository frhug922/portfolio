using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIController : LobbyMenuUIBase
{
    #region type def

    public enum TabType
    {
        Recommend,
        Jewel,
        Resource,
        BattleItem,

        Boundary,
    }

    public enum ShopPopupType
    {
        PurchaseResource,
        PurchaseBattleItem,
        PurchaseConfirm,
        PurchaseComplete,
        PurchaseDefense,
        PurchasePackage,
        PurchasePackageDone,

        Boundary,
    }

    public enum ShopBannerType
    {
        Avatar,
        Pass,
        VIP,
        Combat,
    }

    private enum TextType
    {
        Title,
        Recommend,
        Jewel,
        Resource,
        BattleItem,

        AssetText_Jewel,
    }

    #endregion





    #region serialize fields

    [SerializeField] private ChargeTooltipUI _jewelAsset;
    [SerializeField] private List<TextSlider> _assetTextSlider;
    [SerializeField] private Button _probabilityInfoBtn;

    [SerializeField] private SimpleTab _mainTabs;
    [SerializeField] private TabType _initMainTabType = TabType.Jewel;

    [SerializeField] private ShopPopupBuy _shopPopupBuy;
    [SerializeField] private ShopPopupAvatar _shopPopupAvatar;
    [SerializeField] private ShopPopupVIP _shopPopupVIP;

    [SerializeField] private ShopContentsTabs[] _shopContentsTabs;
    [SerializeField] private GameObject[] _rootTabs;
    [SerializeField] private Transform _noticeAnimTF;

    #endregion





    #region private fields

    private TabType _mainType = TabType.Boundary;

    private Coroutine _goodsSaleEndTimeCoroutine;
    private TProbabilityInfoUrl _tProbaInfoUrl;

    #endregion





    #region public funcs

    public override void UpdateTextsUI() {
        UpdateAssetInfos();

        SetText((int)TextType.Title, TStrings.Instance.FindString("SHOP_9000"));

        SetText((int)TextType.Recommend, TStrings.Instance.FindString("SHOP_9001"));
        SetText((int)TextType.Jewel, TStrings.Instance.FindString("SHOP_9002"));
        SetText((int)TextType.Resource, TStrings.Instance.FindString("SHOP_9003"));
        SetText((int)TextType.BattleItem, TStrings.Instance.FindString("SHOP_9004"));

        SetText((int)TextType.AssetText_Jewel, PlayerManager.Instance.GetAssetAmount(AssetType.Jewel).ToString());
    }

    public void SetMainType(AssetType assetType) {
        if (AssetType.Jewel == assetType) {
            _mainType = TabType.Jewel;
        }
        else if (AssetType.ConsumableItem == assetType) {
            _mainType = TabType.BattleItem;
        }
        else if (AssetType.Avatar == assetType) {
            _mainType = TabType.Recommend;
        }
        else {
            _mainType = TabType.Resource;
        }
    }

    public override void SetShow() {
        SoundManager.Instance.PlayBGM("BGM_Shop");

        Initialize();

        _tProbaInfoUrl = TProbabilityInfoUrls.Instance.Find(ProbabilityInfoUrlType.RecommendShop);

        if (_tProbaInfoUrl != null) {
            _probabilityInfoBtn.gameObject.SetActive(!string.IsNullOrEmpty(_tProbaInfoUrl._url));
        }
        else {
            Debug.LogFormat("TProbabilityInfoUrls table error.!! not founded id : {0}", (int)ProbabilityInfoUrlType.RecommendShop);
        }

        base.SetShow();

        if (TabType.Boundary != _mainType) {
            ToggleMain(_mainType);
        }
        else {
            ToggleMain(_initMainTabType);
            _mainType = _initMainTabType;
        }

        StartUpdateGoodsSaleTime();
    }

    public override void SetHide() {
        SoundManager.Instance.PlayBGM(SceneType.Main);

        _mainType = TabType.Boundary;

        base.SetHide();
    }

    public override void RefreshUI() {
        //base.RefreshUI();

        UpdateTextsUI();

        UpdateContentsUI();
    }

    public void ShowShopInsidePopup(ShopBannerType shopBannerType) {
        if (shopBannerType == ShopBannerType.Avatar) {
            _shopPopupAvatar.SetShow();
        }
        else if (shopBannerType == ShopBannerType.VIP) {
            _shopPopupVIP.SetShow();
        }
    }

    public void OpenUrl_probabilityinfo() {
        Application.OpenURL(_tProbaInfoUrl._url);
    }

    #endregion





    #region private funcs

    private void Initialize() {
        _mainTabs.Initialize(ClickedMainTab);

        _shopPopupBuy.Initialize(Onclick_Buy);
        _shopPopupAvatar.Initialize(OnClick_Goods);
        _shopPopupVIP.Initialize(OnClick_Goods);

        for (int i = 0, count = _shopContentsTabs.Length; i < count; ++i) {
            _shopContentsTabs[i].Initialize(OnClick_Goods, Onclick_More);
        }
    }

    private void UpdateContentsUI() {
        for (int i = 0; i < _shopContentsTabs.Length; ++i) {
            if (i == (int)_mainType) {
                _shopContentsTabs[i].SetShow();
            }
            else {
                _shopContentsTabs[i].SetHide();
            }
        }

        _shopPopupAvatar.RefreshUI();
    }

    private void ClickedMainTab(int index) {
        TabType mainTabType = (TabType)index;

        Debug.LogFormat("ShopUIController : ClickedMainTab : [ index : {0} \t/ MainTabType : {1} ]\n", index, mainTabType);

        SoundManager.Instance.PlaySFX(FXSound.Click);

        ToggleMain(mainTabType);

        RefreshUI();
    }

    private void ToggleMain(TabType type) {
        _mainType = type;

        _mainTabs.SetActiveTab((int)_mainType);

        RefreshUI();
    }

    private void OnClick_Goods(int id) {
        TPrice tprice = TPrices.Instance.Find(id);

        if (!CheckAssetAmount(id) && ShopGoodsType.Jewel != tprice._shopGoodsType && ShopGoodsType.Recommend != tprice._shopGoodsType) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ALERT_7002"), TStrings.Instance.FindString("ALERT_7003"), null);
            return;
        }

        if (TabType.Recommend == _mainType) {
            if (tprice._shopGoodsType == ShopGoodsType.Avatar) {
                _shopPopupBuy.SetShow(ShopPopupType.PurchaseConfirm, id);
            }
            else {
                _shopPopupBuy.SetShow(ShopPopupType.PurchasePackage, id);
            }
        }
        else if (TabType.Jewel == _mainType) {
            GameDataManager.Instance.SetPurchaseType(PurchaseType.Normal);
            TPrice price = TPrices.Instance.Find(id);
#if UNITY_ANDROID
            WebHttp.Instance.RequestGooglePurchaseKey(() => {
                IAPManager.Instance.BuyProductID(SystemManager.Instance.StoreType == StoreType.OneStore ? price._productID_IOS : price._productID_ANDROID, () => {
                    _shopPopupBuy.SetShow(ShopPopupType.PurchaseComplete, id);
                    RefreshUI();
                });
            });
#else
            WebHttp.Instance.RequestGooglePurchaseKey(() => {
                IAPManager.Instance.BuyProductID(price._productID_IOS, () => {
                    _shopPopupBuy.SetShow(ShopPopupType.PurchaseComplete, id);
                    RefreshUI();
                });
            });
#endif
        }
        else if (TabType.Resource == _mainType) {
            _shopPopupBuy.SetShow(ShopPopupType.PurchaseResource, id);
        }
        else if (TabType.BattleItem == _mainType) {
            if (TPrices.Instance.GetBannerCombat().m_priceId == id) {
                _shopPopupBuy.SetShow(ShopPopupType.PurchaseConfirm, id);
            }
            else {
                _shopPopupBuy.SetShow(ShopPopupType.PurchaseBattleItem, id);
            }
        }
    }

    private void Onclick_More(ShopBannerType shopBannerType) {
        if (shopBannerType == ShopBannerType.Avatar) {
            _shopPopupAvatar.SetShow();
        }
        else if (shopBannerType == ShopBannerType.Pass) {
            GameDataManager.Instance.SetPurchaseType(PurchaseType.Normal);
            TPrice price = TPrices.Instance.Find(ShopProductType.Path);
#if UNITY_ANDROID
            WebHttp.Instance.RequestGooglePurchaseKey(() => {
                IAPManager.Instance.BuyProductID(SystemManager.Instance.StoreType == StoreType.OneStore ? price._productID_IOS : price._productID_ANDROID, () => {
                    _shopPopupBuy.SetShow(ShopPopupType.PurchaseComplete, price._id);
                });
            });
#else
            WebHttp.Instance.RequestGooglePurchaseKey(() => {
                IAPManager.Instance.BuyProductID(price._productID_IOS, () => {
                });
            });
#endif
        }
        else if (shopBannerType == ShopBannerType.VIP) {
            _shopPopupVIP.SetShow();
        }
    }

    private void Onclick_Buy(int buyCount, int id) {
        if (CheckAssetAmount(id)) {
            WebHttp.Instance.RequestPurchase(buyCount, id, PurchaseComplete);
        }
        else {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ALERT_7002"), TStrings.Instance.FindString("ALERT_7003"), null);
        }
    }

    private void PurchaseComplete(int id) {
        RefreshUI();

        if (TabType.BattleItem == _mainType) {
            if (TPrices.Instance.GetBannerCombat().m_priceId == id) {
                _shopPopupBuy.SetShow(ShopPopupType.PurchasePackageDone, id);
            }
            else {
                _shopPopupBuy.SetShow(ShopPopupType.PurchaseComplete, id);
            }
        }
        else if (TabType.Recommend == _mainType) {
            if (TPrices.Instance.Find(id)._shopGoodsType != ShopGoodsType.Avatar) {
                _shopPopupBuy.SetShow(ShopPopupType.PurchasePackageDone, id);
            }
        }
        else if (TabType.Resource == _mainType) {
            TPrice tp = TPrices.Instance.Find(id);
            if (tp._priceSubType == PriceSubType.ExpandTeamCapacity) {
                PopupManager.Instance.ShowAnimNoticePopup(_noticeAnimTF, string.Format("{0}{1}->{2}", TStrings.Instance.FindString("SHOP_9044"), PlayerManager.Instance.RetainableMaxTeamCount - 1, PlayerManager.Instance.RetainableMaxTeamCount));
            }
            else if (tp._priceSubType == PriceSubType.ExpandCardCapacity
                || tp._priceSubType == PriceSubType.ExpandWeaponCapacity) {
                _shopPopupBuy.SetShow(ShopPopupType.PurchaseComplete, id);
            }
        }
    }

    private bool CheckAssetAmount(int id) {
        int retainedJewelAmount = PlayerManager.Instance.GetAssetAmount(AssetType.Jewel);
        TPrice tprice = TPrices.Instance.Find(id);

        if (retainedJewelAmount < tprice._cost) {
            return false;
        }
        return true;
    }

    private void UpdateAssetInfos() {
        _jewelAsset.SetAmount(PlayerManager.Instance.GetAssetAmount(AssetType.Jewel));

        for (int i = 0, count = _assetTextSlider.Count; i < count; ++i) {
            if (null == _assetTextSlider[i]) {
                continue;
            }

            _assetTextSlider[i].SetShow(true, PlayerManager.Instance.GetAssetAmount((AssetType)i), PlayerManager.Instance.GetAssetMaxAmount((AssetType)i), false);
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
            GameDataManager.Instance.SetShopPreventbuyCoolTime();
            GameDataManager.Instance.SetShopGoodsPurchaseEndTime();
            GameDataManager.Instance.SetSkypassSeasonEndTime();

            _shopContentsTabs[(int)_mainType].UpdateTextsUI();

            yield return updateTerm;
        }
    }

    #endregion

}
