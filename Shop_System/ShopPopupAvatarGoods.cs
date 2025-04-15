using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ShopPopupAvatarGoods : LobbyUIBase
{
    #region type def

    private enum TextType
    {
        Daily,
        Name,
        Price,
        Purchased,
    }

    private enum ButtonType
    {
        Purchase,
        Purchased,
    }

    #endregion // type def





    #region serialized fields

    [SerializeField] private Image _avatarImage;
    [SerializeField] private List<Button> _buttons;

    #endregion // serialized fields





    #region private variables

    private TPrice _tPrice;

    private System.Action<int> _clickCallback;
    private int _leftAmount;

    #endregion // private variables





    #region properties



    #endregion // properties





    #region public funcs

    public override void UpdateTextsUI() {
        SetText((int)TextType.Daily, string.Format(TStrings.Instance.FindString("SHOP_9036"), _tPrice._positionOrder.ToString()));
        SetText((int)TextType.Name, _tPrice.LangTypeName);
        if (0 >= _leftAmount) {
            SetText((int)TextType.Purchased, TStrings.Instance.FindString("SHOP_9011"));
        }
        SetText((int)TextType.Price, _tPrice._cost.ToString());
    }

    public override void RefreshUI() {
        MAvatarList mAvatarList = GameDataManager.Instance.GetAvatarList(_tPrice._id);
        _leftAmount = _tPrice._buyLimitCount - mAvatarList.m_buyCnt;

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
        SetItemImage(ref _avatarImage, _tPrice._goodsImage);
        _buttons[(int)ButtonType.Purchase].gameObject.SetActive(0 < _leftAmount);
        _buttons[(int)ButtonType.Purchased].gameObject.SetActive(0 == _leftAmount);
    }

    #endregion //private funcs
}
