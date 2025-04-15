using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ShopPopupBuy : LobbyUIBase
{
    #region type def

    private enum TextType
    {
        /// <summary>
        /// TopUI
        /// </summary>
        Title,

        /// <summary>
        /// MiddleUI
        /// </summary>
        AssetAmount_Before,
        AssetAmount_After,
        Desc_Resource,

        Desc_BattleItem,

        Confirm,

        Amount,

        Desc_PreventInvade,

        Time,
        Name,

        /// <summary>
        /// BottomUI
        /// </summary>
        AmountBuy,
        AmountStock,

        AmountNow,
        AmountJewel,

        Desc_Package,

        Purchase,
        Price,

        /// <summary>
        /// ADD Things
        /// </summary>
        CantBuy,
    }

    private enum ImageType
    {
        Resource,
        BattleItem,
        Done,
        Defense,
        Package,
    }

    private enum UIType
    {
        /// <summary>
        /// TopUI
        /// </summary>
        TitleBarImage_ForPackageDone,

        /// <summary>
        /// MiddleUI
        /// </summary>
        Resource,
        BattleItem,
        ContinuePurchase,
        Done,
        PreventInvade,
        Package,
        PackageDone,

        /// <summary>
        /// BottomUI
        /// </summary>
        ItemSlider,
        AmountThings,
        DescForPackage,
        RewardItem,

        ButtonPurchase,
        ButtonConfirm,
        ImageJewel_CenterOfButton,

        /// <summary>
        /// Image
        /// </summary>
        ImageGoods,
        ImageArrow,

        /// <summary>
        /// ADD Things
        /// </summary>
        ButtonOff,
    }

    private enum BuyCountType
    {
        None,

        BattleItemMax = 99,
    }

    #endregion // type def





    #region serialized fields

    [SerializeField] private List<RectTransform> _rectWidths;
    [SerializeField] private float _smallWidth = 500;
    [SerializeField] private float _middleWidth = 540;
    [SerializeField] private float _bigWidth = 590;

    [SerializeField] private List<Transform> _imagePosition;

    [SerializeField] private List<GameObject> _UITypes;

    [SerializeField] private Image _goodsImage;
    [SerializeField] private GameObject _imageHalo;
    [SerializeField] private List<CommonItemInfo> _items;
    [SerializeField] private List<CommonEntityInfo> _heros;

    [SerializeField] private List<CommonItemInfo> _recievedItems;
    [SerializeField] private List<CommonEntityInfo> _recievedHeros;

    [SerializeField] private List<RectTransform> _rebuildRect;

    [SerializeField] private InputField _inputField;

    #endregion // serialized fields





    #region private variables

    private ShopUIController.ShopPopupType _shopPopupType;
    private TPrice _tPrice;

    private System.Action<int, int> _buyCallback;

    private int _storagedAmount;
    private int _maxAmount;
    private int _afterCharge;
    private int _buyCount = 1;
    private int _price;

    private List<TRewardItem> _heroRewards = new List<TRewardItem>();
    private List<TRewardItem> _itemRewards = new List<TRewardItem>();

    private List<MItem> _recievedItemData = new List<MItem>();
    private List<MItem> _recievedHeroData = new List<MItem>();

    private Coroutine _goodsSaleEndTimeCoroutine;

    #endregion // private variables





    #region properties

    private int RequiredCost { get { return (_tPrice._cost * _buyCount); } }

    #endregion // properties





    #region public funcs

    public void Initialize(System.Action<int, int> buyCallback) {
        _buyCallback = buyCallback;
    }

    public void SetShow(ShopUIController.ShopPopupType shopPopupType, int id) {
        base.SetShow();

        _tPrice = TPrices.Instance.Find(id);

        if (shopPopupType != ShopUIController.ShopPopupType.PurchaseComplete) {
            ClearAmount();
            CalculateChargeAmount();
            CaculatePrice();
        }


        if (ShopProductType.PreventInvade == _tPrice._shopProductType) {
            _shopPopupType = ShopUIController.ShopPopupType.PurchaseDefense;
        }
        else {
            _shopPopupType = shopPopupType;
        }

        if (ShopUIController.ShopPopupType.PurchaseDefense == shopPopupType
            || ShopUIController.ShopPopupType.PurchasePackage == shopPopupType) {
            for (int i = 0, count = _rectWidths.Count; i < count; ++i) {
                _rectWidths[i].sizeDelta = new Vector2(_bigWidth, _rectWidths[i].sizeDelta.y);
            }
        }
        else if (ShopUIController.ShopPopupType.PurchasePackageDone == shopPopupType) {
            for (int i = 0, count = _rectWidths.Count; i < count; ++i) {
                _rectWidths[i].sizeDelta = new Vector2(_middleWidth, _rectWidths[i].sizeDelta.y);
            }
        }
        else {
            for (int i = 0, count = _rectWidths.Count; i < count; ++i) {
                _rectWidths[i].sizeDelta = new Vector2(_smallWidth, _rectWidths[i].sizeDelta.y);
            }
        }

        if (shopPopupType == ShopUIController.ShopPopupType.PurchasePackage
            || shopPopupType == ShopUIController.ShopPopupType.PurchasePackageDone) {
            Initialize();
        }

        RefreshUI();

        StartUpdateGoodsSaleTime();
    }

    public override void SetHide() {
        _inputField.gameObject.SetActive(false);
        base.SetHide();
    }

    public override void RefreshUI() {
        //base.RefreshUI();

        UpdateTextsUI();
        UpdateContentsUI();


        for (int i = 0, count = _rebuildRect.Count; i < count; ++i) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rebuildRect[i]);
        }
    }

    public override void UpdateTextsUI() {
        if (ShopUIController.ShopPopupType.PurchaseResource == _shopPopupType) { //Title , Before, After, Desc, Price
            SetText((int)TextType.Title, _tPrice.LangTypeName);
            SetText((int)TextType.AssetAmount_Before, _storagedAmount.ToString());
            SetText((int)TextType.AssetAmount_After, _afterCharge.ToString());
            SetText((int)TextType.Desc_Resource, _tPrice.LangTypeDesc);
            SetText((int)TextType.Purchase, string.Empty);
            if (PriceSubType.ExpandCardCapacity == _tPrice._priceSubType) {
                SetText((int)TextType.Price, _price.ToString());
            }
            else if (PriceSubType.ExpandWeaponCapacity == _tPrice._priceSubType) {
                SetText((int)TextType.Price, _price.ToString());
            }
            else if (PriceSubType.ExpandTeamCapacity == _tPrice._priceSubType) {
                SetText((int)TextType.Price, _price.ToString());
            }
            else {
                SetText((int)TextType.Price, _tPrice._cost.ToString());
            }
        }
        else if (ShopUIController.ShopPopupType.PurchaseBattleItem == _shopPopupType) { // Title, Desc, AmountBuy, AmountStock, Purchase, Price
            SetText((int)TextType.Title, _tPrice.LangTypeName);
            SetText((int)TextType.Desc_BattleItem, _tPrice.LangTypeDesc);
            SetText((int)TextType.AmountBuy, _buyCount.ToString());
            SetText((int)TextType.AmountStock, string.Format("{0} : {1}", TStrings.Instance.FindString("SHOP_9009"), PlayerManager.Instance.GetItemAmount(int.Parse(_tPrice._goodsImage))));
            SetText((int)TextType.Purchase, TStrings.Instance.FindString("SHOP_9008"));
            SetText((int)TextType.Price, (_tPrice._cost * _buyCount).ToString());
        }
        else if (ShopUIController.ShopPopupType.PurchaseConfirm == _shopPopupType) { // Title, Confirm, AmountNow, AmountJewel, Price 
            SetText((int)TextType.Title, TStrings.Instance.FindString("ALERT_7005"));
            SetText((int)TextType.Confirm, TStrings.Instance.FindString("ALERT_7006"));
            SetText((int)TextType.AmountNow, TStrings.Instance.FindString("ALERT_7007"));
            SetText((int)TextType.AmountJewel, PlayerManager.Instance.GetAssetAmount(AssetType.Jewel).ToString());
            SetText((int)TextType.Price, _tPrice.LangTypePrice);
            SetText((int)TextType.Purchase, string.Empty);
        }
        else if (ShopUIController.ShopPopupType.PurchaseComplete == _shopPopupType) { // Title, Amount, Purchase
            SetText((int)TextType.Title, _tPrice.LangTypeName);

            if (_tPrice._priceSubType == PriceSubType.ExpandCardCapacity || _tPrice._priceSubType == PriceSubType.ExpandWeaponCapacity) {
                SetText((int)TextType.Amount, string.Format("{0}\t\t\t{1}", _storagedAmount, _afterCharge));
            }
            else if (_tPrice._shopBuyType == ShopBuyType.Cash) {
                if (_tPrice._shopProductType == ShopProductType.Path) {
                    SetText((int)TextType.Title, TStrings.Instance.FindString("SHOP_9046"));
                    HideText((int)TextType.Amount);
                }
                else {
                    TReward reward = TRewards.Instance.Find(_tPrice._rewardID);
                    SetText((int)TextType.Amount, string.Format("X{0}", reward.RewardItems[0]._minAmount));
                }
            }
            else {
                SetText((int)TextType.Amount, string.Format("X{0}", _buyCount.ToString()));
            }

            SetText((int)TextType.Purchase, TStrings.Instance.FindString("MAIL_13003"));
            SetText((int)TextType.Price, string.Empty);
        }
        else if (ShopUIController.ShopPopupType.PurchaseDefense == _shopPopupType) {// Title, Desc, Price
            SetText((int)TextType.Title, _tPrice.LangTypeName);
            SetText((int)TextType.Desc_PreventInvade, _tPrice.LangTypeDesc);
            SetText((int)TextType.Purchase, string.Empty);
            SetText((int)TextType.Price, _tPrice._cost.ToString());
        }
        else if (ShopUIController.ShopPopupType.PurchasePackage == _shopPopupType) { // Title, Name, Time, Desc, Purchase, Price
            if ((int)ShopPackageGoods.ShopPositionType.First == _tPrice._positionOrder) {
                SetText((int)TextType.Desc_Package, TStrings.Instance.FindString("SHOP_9006"));
            }
            else {
                SetText((int)TextType.Desc_Package, string.Format(TStrings.Instance.FindString("SHOP_9032"), _tPrice._minEnsureAmount.ToString()));
            }

            TimeSpan timespan = GameDataManager.Instance.GetShopGoodsRestTime(_tPrice._id);
            if (0 < timespan.Days) {
                SetText((int)TextType.Time, string.Format(TStrings.Instance.FindString("SHOP_9034"), timespan.Days, timespan.Hours));
            }
            else {
                SetText((int)TextType.Time, string.Format(TStrings.Instance.FindString("SHOP_9035"), timespan.Hours, timespan.Minutes));
            }

            SetText((int)TextType.Purchase, TStrings.Instance.FindString("SHOP_9008"));
            SetText((int)TextType.Name, _tPrice.LangTypeName);
            SetText((int)TextType.Title, TStrings.Instance.FindString("SHOP_9005"));
            SetText((int)TextType.Price, _tPrice._cost.ToString());
            SetText((int)TextType.CantBuy, TStrings.Instance.FindString("SHOP_9030"));
        }
        else if (ShopUIController.ShopPopupType.PurchasePackageDone == _shopPopupType) { // Title, Confirm
            SetText((int)TextType.Title, _tPrice.LangTypeName);
            SetText((int)TextType.Purchase, TStrings.Instance.FindString("MAIL_13003"));
            SetText((int)TextType.Price, string.Empty);
        }
    }

    public void OnClicked_Close() {
        SoundManager.Instance.PlaySFX(FXSound.Click);

        SetHide();
    }

    public void OnClicked_Buy() {
        SoundManager.Instance.PlaySFX(FXSound.Click);

        if (null != _buyCallback
            && ShopUIController.ShopPopupType.PurchaseComplete != _shopPopupType) {
            _buyCallback(_buyCount, _tPrice._id);
        }

        SetHide();
    }

    public void OnClick_Plus() {
        int jewelAmount = PlayerManager.Instance.GetAssetAmount(AssetType.Jewel);
        if (jewelAmount <= RequiredCost + _tPrice._cost || (int)BuyCountType.BattleItemMax == _buyCount) {
            return;
        }

        ++_buyCount;

        UpdateTextsUI();
    }

    public void Onclick_Minus() {
        if (1 == _buyCount) {
            return;
        }

        --_buyCount;

        UpdateTextsUI();
    }

    public void OnClick_Item(int index) {
        SoundManager.Instance.PlaySFX(FXSound.Click);

#if SHOW_LOG
        Debug.LogWarningFormat("ShopPopupBuy : OnClick_Item : [ clicked index : {0} ]\n", index);
#endif // SHOW_LOG

        TItem item = TItems.Instance.Find(_itemRewards[index]._kind);

        PopupManager.Instance.ShowTooltip(_items[index].transform, item);
    }

    public void OnClick_Card(int index) {
        SoundManager.Instance.PlaySFX(FXSound.Click);

#if SHOW_LOG
        Debug.LogWarningFormat("ShopPopupBuy : OnClick_Card : [ clicked index : {0} ]\n", index);
#endif // SHOW_LOG

        TCard card = TCards.Instance.Find(_heroRewards[index]._kind);

        LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.HeroDetail);
        CharacterDetailUIController charDetailUICtrlr = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.HeroDetail) as CharacterDetailUIController;

        HeroData heroData = GetMaxLevelHeroData(card._id);
        charDetailUICtrlr.SetShow(heroData);
    }

    private HeroData GetMaxLevelHeroData(int id) {
        HeroData heroData = new HeroData(0, id);
        TCard card = TCards.Instance.Find(id);

        int grade = card._grade;
        heroData._awakening = grade;
        heroData._level = TCardLevels.Instance.GetMaxLevel(grade, grade);
        heroData._skillLevel = heroData.GetMaxSkillLevel();

        return heroData;
    }

    public void OnClick_ItemReward(int index) {
        SoundManager.Instance.PlaySFX(FXSound.Click);

#if SHOW_LOG
        Debug.LogWarningFormat("ShopPopupBuy : OnClick_ItemReward : [ clicked index : {0} ]\n", index);
#endif // SHOW_LOG

        TItem item = TItems.Instance.Find(_recievedItemData[index].m_refId);

        PopupManager.Instance.ShowTooltip(_recievedItems[index].transform, item);
    }

    public void OnClick_CardReward(int index) {
        SoundManager.Instance.PlaySFX(FXSound.Click);

#if SHOW_LOG
        Debug.LogWarningFormat("ShopPopupBuy : OnClick_CardReward : [ clicked index : {0} ]\n", index);
#endif // SHOW_LOG

        TCard card = TCards.Instance.Find(_recievedHeroData[index].m_refId);

        PopupManager.Instance.ShowTooltip(_recievedHeros[index].transform, card);
    }

    public void OnClick_AmountThings() {
        _inputField.text = _buyCount.ToString();

        _inputField.gameObject.SetActive(true);
    }

    public void OnSubmit_InputField() {
        _buyCount = int.Parse(_inputField.text);

        int jewelAmount = PlayerManager.Instance.GetAssetAmount(AssetType.Jewel);
        if (jewelAmount < RequiredCost) {
            _buyCount = jewelAmount / _tPrice._cost;
        }

        _inputField.gameObject.SetActive(false);

        UpdateTextsUI();
    }

    public void OnValueChanged_InputField() {
        if (_inputField.text.Length >= 3) {
            _inputField.text = ((int)BuyCountType.BattleItemMax).ToString();
        }
    }

    #endregion //public funcs





    #region private funcs

    private void Initialize() {
        if (ShopUIController.ShopPopupType.PurchasePackage == _shopPopupType) {
            TReward tReward = TRewards.Instance.Find(_tPrice._rewardID);
            int rewardCount = tReward.RewardItems.Count;

            _heroRewards.Clear();
            _itemRewards.Clear();
            for (int i = 0; i < rewardCount; ++i) {
                if (tReward.RewardItems[i]._itemType == (int)AssetType.CardType) {
                    _heroRewards.Add(tReward.RewardItems[i]);
                }
                else if (tReward.RewardItems[i]._itemType == (int)AssetType.ConsumableItem) {
                    _itemRewards.Add(tReward.RewardItems[i]);
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
        }
        else if (ShopUIController.ShopPopupType.PurchasePackageDone == _shopPopupType) {
            List<MItem> _purchasedItems = GameDataManager.Instance.PurchasedItem;

            _recievedItemData.Clear();
            _recievedHeroData.Clear();

            for (int i = 0; i < _purchasedItems.Count; ++i) {
                if ((int)AssetType.CardType == _purchasedItems[i].m_type) {
                    _recievedHeroData.Add(_purchasedItems[i]);
                }
                else if ((int)AssetType.ConsumableItem == _purchasedItems[i].m_type) {
                    _recievedItemData.Add(_purchasedItems[i]);
                }
            }

            if (_recievedHeros.Count < _recievedHeroData.Count) {
                for (int i = _recievedHeros.Count, count = _recievedHeroData.Count; i < count; ++i) {
                    GameObject obj = Instantiate(_recievedHeros[0].gameObject, _recievedHeros[0].transform.parent);
                    _recievedHeros.Add(obj.GetComponent<CommonEntityInfo>());
                }
            }

            if (_recievedItems.Count < _recievedItemData.Count) {
                for (int i = _recievedItems.Count, count = _recievedItemData.Count; i < count; ++i) {
                    GameObject obj = Instantiate(_recievedItems[0].gameObject, _recievedItems[0].transform.parent);
                    _recievedItems.Add(obj.GetComponent<CommonItemInfo>());
                }
            }
        }
    }

    private void UpdateContentsUI() {
        _imageHalo.GetComponent<RectTransform>();

        if (ShopUIController.ShopPopupType.PurchaseResource == _shopPopupType) { // Small / Resource  / On / Image / ButtonPurchase
            SetItemImage(ref _goodsImage, _tPrice._goodsImage);

            _imageHalo.transform.SetParent(_imagePosition[(int)ImageType.Resource]);
            _imageHalo.transform.position = _imagePosition[(int)ImageType.Resource].position;

            for (int i = 0, count = _UITypes.Count; i < count; ++i) {
                _UITypes[i].SetActive(i == (int)UIType.Resource
                    || i == (int)UIType.ImageJewel_CenterOfButton
                    || i == (int)UIType.ImageGoods
                    || i == (int)UIType.ButtonPurchase);
            }
        }
        else if (ShopUIController.ShopPopupType.PurchaseBattleItem == _shopPopupType) { //Small / BattleItem / ItemSlider  / On / Image / ButtonPurchase
            SetItemImage(ref _goodsImage, _tPrice._goodsImage);

            _imageHalo.transform.SetParent(_imagePosition[(int)ImageType.BattleItem]);
            _imageHalo.transform.position = _imagePosition[(int)ImageType.BattleItem].position;

            for (int i = 0, count = _UITypes.Count; i < count; ++i) {
                _UITypes[i].SetActive(i == (int)UIType.BattleItem
                    || i == (int)UIType.ItemSlider
                    || i == (int)UIType.ImageJewel_CenterOfButton
                    || i == (int)UIType.ImageGoods
                    || i == (int)UIType.ButtonPurchase);
            }
        }
        else if (ShopUIController.ShopPopupType.PurchaseConfirm == _shopPopupType) { // Small / Confirm / AmountThings  / On / ButtonPurchase
            for (int i = 0, count = _UITypes.Count; i < count; ++i) {
                _UITypes[i].SetActive(i == (int)UIType.ContinuePurchase
                    || i == (int)UIType.AmountThings
                    || i == (int)UIType.ImageJewel_CenterOfButton
                    || i == (int)UIType.ButtonPurchase);
            }
        }
        else if (ShopUIController.ShopPopupType.PurchaseComplete == _shopPopupType) { // Small / Done  / Off / Image / ButtonPurchase
            SetItemImage(ref _goodsImage, _tPrice._goodsImage);

            _imageHalo.transform.SetParent(_imagePosition[(int)ImageType.Done]);
            _imageHalo.transform.position = _imagePosition[(int)ImageType.Done].position;

            if (_tPrice._priceSubType == PriceSubType.ExpandCardCapacity || _tPrice._priceSubType == PriceSubType.ExpandWeaponCapacity) {
                for (int i = 0, count = _UITypes.Count; i < count; ++i) {
                    _UITypes[i].SetActive(i == (int)UIType.Done
                        || i == (int)UIType.ImageGoods
                        || i == (int)UIType.ButtonPurchase
                        || i == (int)UIType.ImageArrow);
                }
            }
            else if (_tPrice._shopProductType == ShopProductType.Path) {
                for (int i = 0, count = _UITypes.Count; i < count; ++i) {
                    _UITypes[i].SetActive(i == (int)UIType.Done
                        || i == (int)UIType.ButtonPurchase);
                }
            }
            else {
                for (int i = 0, count = _UITypes.Count; i < count; ++i) {
                    _UITypes[i].SetActive(i == (int)UIType.Done
                        || i == (int)UIType.ImageGoods
                        || i == (int)UIType.ButtonPurchase);
                }
            }
        }
        else if (ShopUIController.ShopPopupType.PurchaseDefense == _shopPopupType) { // Big / PreventInvade / ON / Image / ButtonPurchase
            SetItemImage(ref _goodsImage, _tPrice._goodsImage);

            _imageHalo.transform.SetParent(_imagePosition[(int)ImageType.Defense]);
            _imageHalo.transform.position = _imagePosition[(int)ImageType.Defense].position;

            for (int i = 0, count = _UITypes.Count; i < count; ++i) {
                _UITypes[i].SetActive(i == (int)UIType.PreventInvade
                    || i == (int)UIType.ImageJewel_CenterOfButton
                    || i == (int)UIType.ImageGoods
                    || i == (int)UIType.ButtonPurchase);
            }
        }
        else if (ShopUIController.ShopPopupType.PurchasePackage == _shopPopupType) { // Big / Package / Desc / Item / ON / Image / ButtonPuchase
            _goodsImage.sprite = ObjectPooler.Instance.GetAtlasSprite(AtlasType.ShopResources, _tPrice._goodsImage);

            _imageHalo.transform.SetParent(_imagePosition[(int)ImageType.Package]);
            _imageHalo.transform.position = _imagePosition[(int)ImageType.Package].position;

            if (0 < GameDataManager.Instance.ShopPackageLeftAmount) {
                for (int i = 0, count = _UITypes.Count; i < count; ++i) {
                    _UITypes[i].SetActive(i == (int)UIType.Package
                        || i == (int)UIType.DescForPackage
                        || i == (int)UIType.RewardItem
                        || i == (int)UIType.ImageJewel_CenterOfButton
                        || i == (int)UIType.ImageGoods
                        || i == (int)UIType.ButtonPurchase);
                }
            }
            else {
                for (int i = 0, count = _UITypes.Count; i < count; ++i) {
                    _UITypes[i].SetActive(i == (int)UIType.Package
                        || i == (int)UIType.DescForPackage
                        || i == (int)UIType.RewardItem
                        || i == (int)UIType.ImageJewel_CenterOfButton
                        || i == (int)UIType.ImageGoods
                        || i == (int)UIType.ButtonOff);
                }
            }

            for (int i = 0, count = _heros.Count; i < count; ++i) {
                if (i < _heroRewards.Count) {
                    _heros[i].Initialize(i, EntityType.None, OnClick_Card);
                    _heros[i].SetShowReward(_heroRewards[i]);
                }
                else {
                    _heros[i].SetHide();
                }
            }

            for (int i = 0, count = _items.Count; i < count; ++i) {
                if (i < _itemRewards.Count) {
                    _items[i].Initialize(i, OnClick_Item, null, null, null);
                    _items[i].SetShowRewardItem(_itemRewards[i]);
                }
                else {
                    _items[i].SetHide();
                }
            }
        }
        else if (ShopUIController.ShopPopupType.PurchasePackageDone == _shopPopupType) { // Titlebar / PackageDone / ButtonConfirm
            for (int i = 0, count = _UITypes.Count; i < count; ++i) {
                _UITypes[i].SetActive(i == (int)UIType.TitleBarImage_ForPackageDone
                    || i == (int)UIType.PackageDone
                    || i == (int)UIType.ButtonConfirm);
            }

            for (int i = 0, count = _recievedHeros.Count; i < count; ++i) {
                if (i < _recievedHeroData.Count) {
                    _recievedHeros[i].Initialize(i, EntityType.None, OnClick_CardReward);
                    _recievedHeros[i].SetShowReward(_recievedHeroData[i]);
                }
                else {
                    _recievedHeros[i].SetHide();
                }
            }

            for (int i = 0, count = _recievedItems.Count; i < count; ++i) {
                if (i < _recievedItemData.Count) {
                    _recievedItems[i].Initialize(i, OnClick_ItemReward, null, null, null);
                    _recievedItems[i].SetShowRewardItem(_recievedItemData[i]);
                }
                else {
                    _recievedItems[i].SetHide();
                }
            }
        }
    }

    private void CalculateChargeAmount() {
        if (null == _tPrice) {
            return;
        }

        _storagedAmount = PlayerManager.Instance.GetAssetAmount(_tPrice._getableAssetType);
        _maxAmount = PlayerManager.Instance.GetAssetMaxAmount(_tPrice._getableAssetType);

        int amount = 0;

        if (PriceSubType.HalfCharge == _tPrice._priceSubType) {
            amount = Mathf.CeilToInt((_maxAmount - _storagedAmount) / 2f);
            _afterCharge = _storagedAmount + amount;
        }
        else if (PriceSubType.ExpandCardCapacity == _tPrice._priceSubType) {
            _storagedAmount = PlayerManager.Instance.RetainableMaxCardCount;
            _afterCharge = _storagedAmount + _tPrice._buyNumber;
        }
        else if (PriceSubType.ExpandWeaponCapacity == _tPrice._priceSubType) {
            _storagedAmount = PlayerManager.Instance.RetainableMaxWeaponCount;
            _afterCharge = _storagedAmount + _tPrice._buyNumber;
        }
        else if (PriceSubType.ExpandTeamCapacity == _tPrice._priceSubType) {
            _storagedAmount = PlayerManager.Instance.RetainableMaxTeamCount;
            _afterCharge = _storagedAmount + _tPrice._buyNumber;
        }
        else {
            _afterCharge = _maxAmount;
        }
    }

    private void CaculatePrice() {
        if (null == _tPrice) {
            return;
        }

        MShopInfo mshopInfo = GameDataManager.Instance.ShopInfo;

        _price = _tPrice._cost;

        if (PriceSubType.ExpandCardCapacity == _tPrice._priceSubType) {
            if (0 != mshopInfo.m_cardCnt) {
                for (int i = 1; i <= mshopInfo.m_cardCnt; ++i) {
                    if (0 == i % _tPrice._buyNumber) {
                        _price += _tPrice._priceUpCount;
                    }
                }
            }
        }
        else if (PriceSubType.ExpandWeaponCapacity == _tPrice._priceSubType) {
            if (0 != mshopInfo.m_weaponCnt) {
                for (int i = 1; i <= mshopInfo.m_weaponCnt; ++i) {
                    if (0 == i % _tPrice._buyNumber) {
                        _price += _tPrice._priceUpCount;
                    }
                }
            }
        }
        else if (PriceSubType.ExpandTeamCapacity == _tPrice._priceSubType) {
            if (0 != mshopInfo.m_teamCnt) {
                for (int i = 1; i <= mshopInfo.m_teamCnt; ++i) {
                    if (0 == i % _tPrice._buyNumber) {
                        _price += _tPrice._priceUpCount;
                    }
                }
            }
        }
    }

    private void ClearAmount() {
        _storagedAmount = 0;
        _maxAmount = 0;
        _afterCharge = 0;
        _buyCount = 1;
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
