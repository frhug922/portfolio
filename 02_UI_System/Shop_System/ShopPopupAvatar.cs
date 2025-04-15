using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShopPopupAvatar : LobbyUIBase
{
    #region type def

    private enum TextType
    {
        Name,
        Time,
    }

    #endregion // type def





    #region serialized fields

    [SerializeField] private List<ShopPopupAvatarGoods> _shopAvatarGoods;

    #endregion // serialized fields





    #region private fields

    private List<MAvatarList> _avatarLists;
    private int _initializedItems = 0;

    private Coroutine _goodsSaleEndTimeCoroutine;

    #endregion // private fields





    #region public funcs

    public void Initialize(System.Action<int> clickCallback) {
        _avatarLists = GameDataManager.Instance.AvatarLists;

        if (_avatarLists.Count > _shopAvatarGoods.Count) {
            for (int i = _shopAvatarGoods.Count, count = _avatarLists.Count; i < count; ++i) {
                GameObject shopObj = Instantiate(_shopAvatarGoods[0].gameObject, _shopAvatarGoods[0].transform.parent.transform);
                _shopAvatarGoods.Add(shopObj.GetComponent<ShopPopupAvatarGoods>());
            }
        }

        for (int i = _initializedItems, count = _shopAvatarGoods.Count; i < count; ++i) {
            _shopAvatarGoods[i].Initialize(clickCallback);
            ++_initializedItems;
        }
    }

    public override void SetShow() {
        RefreshUI();

        base.SetShow();

        StartUpdateGoodsSaleTime();
    }

    public override void UpdateTextsUI() {
        SetText((int)TextType.Name, string.Format("{0} {1}", TStrings.Instance.FindString("SHOP_9010"), TStrings.Instance.FindString("SHOP_9000")));
        TimeSpan restTime = GameDataManager.Instance.ShopAvatarRestTime;
        if (0 < restTime.Days) {
            SetText((int)TextType.Time, string.Format(TStrings.Instance.FindString("SHOP_9034"), restTime.Days, restTime.Hours));
        }
        else {
            SetText((int)TextType.Time, string.Format(TStrings.Instance.FindString("SHOP_9035"), restTime.Hours, restTime.Minutes));
        }
    }

    public override void RefreshUI() {
        UpdateTextsUI();
        UpdateContentsUI();
    }

    public override void SetHide() {
        base.SetHide();
    }

    public void Onclick_Close() {
        SetHide();
    }

    #endregion //public funcs





    #region private funcs

    private void UpdateContentsUI() {
        for (int i = 0, count = _shopAvatarGoods.Count; i < count; ++i) {
            if (i < _avatarLists.Count) {
                _shopAvatarGoods[i].SetShow(_avatarLists[i].m_priceId);
            }
            else {
                _shopAvatarGoods[i].SetHide();
            }
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
