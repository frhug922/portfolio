using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPackageGoods : LobbyUIBase
{
    #region type def

    private enum TextType
    {
        Title,
        Time,
        Price,
        Ensure,
        BuyAble,
        CantBuy,
    }

    public enum ShopPositionType
    {
        First = 1,
        Second = 2,
        Third = 3,
    }

    private enum ButtonType
    {
        BuyAble,
        CantBuy,
    }

    #endregion // type def





    #region serialized fields

    [SerializeField] private List<CommonItemInfo> _items = new List<CommonItemInfo>();
    [SerializeField] private List<CommonEntityInfo> _heros = new List<CommonEntityInfo>();
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private int _maxCountPerRow = 6;
    [SerializeField] private float _minScale;
    [SerializeField] private ShopPositionType _positionType;
    [SerializeField] private Image _goodsImage;
    [SerializeField] private Button[] _buttons;

    [SerializeField] private GridLayoutGroup _gridLayout;
    [SerializeField] private float _spacingMinX = -21;
    [SerializeField] private float _spacingMaxX = -42;
    [SerializeField] private int _gridConstraintCount = 8;

    #endregion // serialized fields





    #region private variables

    private TPrice _tPrice;
    private List<TRewardItem> _heroRewards = new List<TRewardItem>();
    private List<TRewardItem> _itemRewards = new List<TRewardItem>();
    private int _priceID;
    private int _leftAmount;
    private System.Action<int> _buyCallback;
    private Vector3 _defaultScale = Vector3.zero;

    #endregion // private variables





    #region properties



    #endregion // properties





    #region public funcs

    public void SetShow(System.Action<int> buyCallback) {
        if (Vector3.zero == _defaultScale) {
            _defaultScale = _items[0].transform.localScale;
        }

        _buyCallback = buyCallback;

        Initialize();

        RefreshUI();

        base.SetShow();
    }

    public override void SetHide() {
        base.SetHide();
    }

    public override void RefreshUI() {
        //base.RefreshUI();

        UpdateTextsUI();
        UpdateContentsUI();
    }

    public override void UpdateTextsUI() {
        SetText((int)TextType.Title, _tPrice.LangTypeName);

        if (ShopPositionType.First == _positionType) {
            SetText((int)TextType.Ensure, TStrings.Instance.FindString("SHOP_9031"));
        }
        else {
            SetText((int)TextType.Ensure, string.Format(TStrings.Instance.FindString("SHOP_9032"), _tPrice._minEnsureAmount.ToString()));
        }

        TimeSpan timespan = GameDataManager.Instance.GetShopGoodsRestTime(_priceID);
        if (0 < timespan.Days) {
            SetText((int)TextType.Time, string.Format(TStrings.Instance.FindString("SHOP_9034"), timespan.Days, timespan.Hours));
        }
        else {
            SetText((int)TextType.Time, string.Format(TStrings.Instance.FindString("SHOP_9035"), timespan.Hours, timespan.Minutes));
        }

        if (0 < _leftAmount) {
            SetText((int)TextType.BuyAble, string.Format("{0}:{1}", TStrings.Instance.FindString("SHOP_9007"), _leftAmount.ToString()));
            SetText((int)TextType.Price, _tPrice._cost.ToString());
        }
        else {
            SetText((int)TextType.BuyAble, TStrings.Instance.FindString("SHOP_9029"));
            SetText((int)TextType.CantBuy, TStrings.Instance.FindString("SHOP_9040"));
        }
    }

    public void OnClick_Buy() {
        if (null == _buyCallback) {
            return;
        }

        SoundManager.Instance.PlaySFX(FXSound.Click);

        GameDataManager.Instance.ShopPackageLeftAmount = _leftAmount;

        //if (0 < _leftAmount) {
        _buyCallback(_tPrice._id);
        //}
    }

    #endregion //public funcs





    #region private funcs

    private void Initialize() {
        MShopList mShopList = GameDataManager.Instance.GetPackage((int)_positionType);
        _priceID = mShopList.m_priceId;
        _tPrice = TPrices.Instance.Find(_priceID);
        _leftAmount = _tPrice._buyLimitCount - mShopList.m_buyCnt;
        TReward tReward = TRewards.Instance.Find(_tPrice._rewardID);

        Vector2 gridVector;

        // set scale..
        int rewardCount = tReward.RewardItems.Count;
        if (_maxCountPerRow < rewardCount) {
            for (int j = 0, countJ = _heros.Count; j < countJ; ++j) {
                _heros[j].transform.localScale = _defaultScale * _minScale;
            }
            for (int j = 0, countJ = _items.Count; j < countJ; ++j) {
                _items[j].transform.localScale = _defaultScale * _minScale;
            }
            gridVector = new Vector2(_spacingMaxX, gridLayout.spacing.y);

            _gridLayout.constraintCount = _gridConstraintCount;
            _gridLayout.spacing = gridVector;
        }
        else {
            for (int j = 0, countJ = _heros.Count; j < countJ; ++j) {
                _heros[j].transform.localScale = _defaultScale;
            }
            for (int j = 0, countJ = _items.Count; j < countJ; ++j) {
                _items[j].transform.localScale = _defaultScale;
            }
            gridVector = new Vector2(_spacingMinX, gridLayout.spacing.y);

            _gridLayout.constraintCount = _maxCountPerRow;
            _gridLayout.spacing = gridVector;
        }

        // set item or card info..
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

    private void UpdateContentsUI() {
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
                _items[i].SetShowRewardItem(_itemRewards[i]);
            }
            else {
                _items[i].SetHide();
            }
        }

        _goodsImage.sprite = ObjectPooler.Instance.GetAtlasSprite(AtlasType.ShopResources, _tPrice._goodsImage);

        _buttons[(int)ButtonType.BuyAble].gameObject.SetActive(0 < _leftAmount);
        _buttons[(int)ButtonType.CantBuy].gameObject.SetActive(0 == _leftAmount);
    }

    #endregion //private funcs
}
