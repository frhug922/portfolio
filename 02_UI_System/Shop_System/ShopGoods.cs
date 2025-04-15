using System;
using UnityEngine;
using UnityEngine.UI;


public class ShopGoods : LobbyUIBase
{
    #region type def

    private enum TextType
    {
        Name,
        JewelPrice,
        CashPrice,
        Popular,
        Valuable,
        Bonus,
        Full,
    }

    private enum DiscountTagType
    {
        MostPopular,
        BestValuable,
        Discount,
    }

    private enum BonusRates
    {
        None = 0,

        More_4 = 4,
        More_6 = 6,
        More_9 = 9,
        More_12 = 12,
        More_24 = 24,
    }

    private enum BuyBtnTypes
    {
        Jewel,
        Cash,
    }

    #endregion // type def





    #region serialized fields

    [Header("* 이하 ShopGoods 에 선언되어있음 ----------------------------")]
    /// <summary>
    /// 0 = 다이아버튼, 1 = 현금 버튼
    /// </summary>
    [SerializeField] private Button[] _buyTypeBtn;
    [SerializeField] private GameObject _fullImage;
    [SerializeField] private Image _goodsImage;
    [SerializeField] private GameObject[] _tags;
    [SerializeField] private GameObject _questionMark;

    #endregion // serialized fields





    #region private variables

    private TPrice _tPrice;

    private System.Action<int> _clickCallback;

    private int _price;

    #endregion // private variables





    #region properties



    #endregion // properties





    #region public funcs

    public override void UpdateTextsUI() {
        SetText((int)TextType.Name, _tPrice.LangTypeName);

        if (ShopBuyType.Jewel == _tPrice._shopBuyType) {
            SetText((int)TextType.JewelPrice, _tPrice._cost.ToString());
            HideText((int)TextType.CashPrice);
        }
        else if (ShopBuyType.Cash == _tPrice._shopBuyType) {
            HideText((int)TextType.JewelPrice);
#if UNITY_ANDROID
            if (SystemManager.Instance.StoreType == StoreType.OneStore) {
                SetText((int)TextType.CashPrice, IAPManager.Instance.GetProductPrice(_tPrice._productID_IOS));
            }
            else {
                SetText((int)TextType.CashPrice, IAPManager.Instance.GetProductPrice(_tPrice._productID_ANDROID));
            }
#else
            SetText((int)TextType.CashPrice, IAPManager.Instance.GetProductPrice(_tPrice._productID_IOS));
#endif
        }

        if (PriceSubType.None != _tPrice._priceSubType) {
            if (PriceSubType.ExpandTeamCapacity == _tPrice._priceSubType) {
                if (PlayerManager.Instance.TeamLimitExpandBuyCount >= _tPrice._buyLimitCount) {
                    SetText((int)TextType.JewelPrice, 0.ToString());
                }
                else {
                    SetText((int)TextType.JewelPrice, _price.ToString());
                }
                SetText((int)TextType.Full, TStrings.Instance.FindString("SHOP_9030"));
            }
            else {
                SetText((int)TextType.JewelPrice, _price.ToString());
                SetText((int)TextType.Full, TStrings.Instance.FindString("SHOP_9017"));
            }
        }
        else {
            TimeSpan coolTime = GameDataManager.Instance.ShopInvadePreventBuyCoolTime;
            TimeSpan restTime = GameDataManager.Instance.ShopInvadePreventRestTime;

            if (0 > restTime.TotalMilliseconds
                && 0 < coolTime.TotalMilliseconds) {
                if (0 < coolTime.Days) {
                    SetText((int)TextType.Full, string.Format(TStrings.Instance.FindString("SHOP_9018"), string.Format(TStrings.Instance.FindString("SHOP_9034"), coolTime.Days, coolTime.Hours)));
                }
                else {
                    SetText((int)TextType.Full, string.Format(TStrings.Instance.FindString("SHOP_9018"), string.Format(TStrings.Instance.FindString("SHOP_9035"), coolTime.Hours, coolTime.Minutes)));
                }
            }
            else {
                SetText((int)TextType.Full, TStrings.Instance.FindString("SHOP_9040"));
            }
        }
    }

    public override void RefreshUI() {
        //base.RefreshUI();

        CalculatePriceandAmount();

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

    public void OnClick_Goods() {
        if (null == _clickCallback) {
            return;
        }

        SoundManager.Instance.PlaySFX(FXSound.Click);

        _clickCallback(_tPrice._id);
    }

    public void OnClick_QuestionMark() {
        PopupManager.Instance.ShowTooltip(gameObject.transform, _tPrice.LangTypeName, _tPrice.LangTypeDesc);
    }

    #endregion //public funcs





    #region private funcs

    private void UpdateContentsUI() {
        _fullImage?.SetActive(IsFull(_tPrice._getableAssetType));

        if (ShopProductType.PreventInvade == _tPrice._shopProductType) {
            if (0 < GameDataManager.Instance.ShopInvadePreventRestTime.TotalMilliseconds) {
                _fullImage?.SetActive(0 < GameDataManager.Instance.ShopInvadePreventRestTime.TotalMilliseconds);
            }
            else {
                _fullImage?.SetActive(0 < GameDataManager.Instance.ShopInvadePreventBuyCoolTime.TotalMilliseconds);
            }
        }

        if (ShopProductType.IncLimit == _tPrice._shopProductType) {
            if (PriceSubType.ExpandTeamCapacity == _tPrice._priceSubType) {
                _fullImage?.SetActive(PlayerManager.Instance.TeamLimitExpandBuyCount >= _tPrice._buyLimitCount);
            }
        }

        CheckCashOrJewel();

        SetDiscountTags();
        SetImage();
        SetQuestionMark();
    }

    private void CheckCashOrJewel() {
        if (ShopBuyType.Jewel == _tPrice._shopBuyType) {
            _buyTypeBtn[(int)BuyBtnTypes.Jewel].gameObject.SetActive(true);
        }
        else if (ShopBuyType.Cash == _tPrice._shopBuyType) {
            _buyTypeBtn[(int)BuyBtnTypes.Cash].gameObject.SetActive(true);
        }
    }

    private void CalculatePriceandAmount() {
        if (PriceSubType.None == _tPrice._priceSubType) {
            return;
        }

        MShopInfo mshopInfo = GameDataManager.Instance.ShopInfo;

        int storagedAmount = PlayerManager.Instance.GetAssetAmount(_tPrice._getableAssetType);
        int maxAmount = PlayerManager.Instance.GetAssetMaxAmount(_tPrice._getableAssetType);

        _price = _tPrice._cost;
        int amount;

        if (PriceSubType.HalfCharge == _tPrice._priceSubType) {
            amount = Mathf.CeilToInt((maxAmount - storagedAmount) / 2f);
            _price = Mathf.CeilToInt(amount / (float)_tPrice._amount);
            if (_price < 0) {
                _price = 0;
            }
            _tPrice._cost = _price;
        }
        else if (PriceSubType.FullCharge == _tPrice._priceSubType) {
            amount = maxAmount - storagedAmount;
            _price = Mathf.CeilToInt(amount / (float)_tPrice._amount);
            if (_price < 0) {
                _price = 0;
            }
            _tPrice._cost = _price;
        }
        else if (PriceSubType.ChargePvEForce == _tPrice._priceSubType) {
        }
        else if (PriceSubType.ChargePvPForce == _tPrice._priceSubType) {
        }
        else if (PriceSubType.ExpandCardCapacity == _tPrice._priceSubType) {
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

    private bool IsFull(AssetType assetType) {
        if (AssetType.Boundary == assetType) {
            return false;
        }

        AssetType _getableAssetType = _tPrice._getableAssetType;
        int storagedAmount = PlayerManager.Instance.GetAssetAmount(_getableAssetType);
        int maxAmount = PlayerManager.Instance.GetAssetMaxAmount(_getableAssetType);

        if (storagedAmount >= maxAmount) {
            return true;
        }

        return false;
    }

    private void SetDiscountTags() {
        if (ShopGoodsType.Jewel == _tPrice._shopGoodsType) {
            if ((int)BonusRates.More_9 == _tPrice._bonusRate) {
                _tags[(int)DiscountTagType.MostPopular].SetActive(true);
                SetText((int)TextType.Popular, (_tPrice.LangTypeRecommend));

            }
            else if ((int)BonusRates.More_24 == _tPrice._bonusRate) {
                _tags[(int)DiscountTagType.BestValuable].SetActive(true);
                SetText((int)TextType.Valuable, (_tPrice.LangTypeRecommend));
            }
        }

        if ((int)BonusRates.None != _tPrice._bonusRate) {
            _tags[(int)DiscountTagType.Discount].SetActive(true);
            SetText((int)TextType.Bonus, string.Format(TStrings.Instance.FindString("SHOP_9024"), _tPrice._bonusRate));
        }
    }

    private void SetImage() {
        if (null == _goodsImage) {
            return;
        }

        SetItemImage(ref _goodsImage, _tPrice._goodsImage);
    }

    private void SetQuestionMark() {
        if (null == _questionMark) {
            return;
        }

        _questionMark.SetActive(ShopGoodsType.Resource == _tPrice._shopGoodsType || ShopGoodsType.BattleItem == _tPrice._shopGoodsType);
    }


    #endregion //private funcs
}
