# 상점 코드 설명

**상점 시스템 (Unity UI 및 서버 연동)**

---

## 📌 담당한 업무

- 게임 내 **상점 UI 구성 및 기능 개발**
    
- 상품 구매 처리 (서버 연동 및 구매 팝업 처리)
    
- 탭별 상품 구성 및 UI 흐름 관리
    

---

## 🔨 사용 기술 및 환경

|항목|상세 내용|
|---|---|
|사용 언어|C#|
|사용 엔진 및 툴|Unity|
|주요 기술|Unity UI, Coroutine, 서버 통신 처리|

---

## 📌 주요 코드 설명 (`ShopUIController.cs`)

### ① 탭 UI 관리

- 상점의 각 탭(추천, 보석, 자원, 전투 아이템 등)을 관리
    
- 탭 전환 및 상품 목록 업데이트
    

```csharp

private void ToggleMain(TabType type) {
    _mainType = type;
    _mainTabs.SetActiveTab((int)_mainType);
    RefreshUI();
}

```


---

### ② 상품 구매 흐름 처리

- 상품 선택 시 서버와의 연동을 통해 구매 처리
    
- 구매 결과에 따라 적절한 팝업을 노출
    

```csharp

private void OnClick_Goods(int id) {
    TPrice tprice = TPrices.Instance.Find(id);

    if (!CheckAssetAmount(id) && ShopGoodsType.Jewel != tprice._shopGoodsType) {
        PopupManager.Instance.ShowOKPopup("알림", "재화가 부족합니다.", null);
        return;
    }

    if (_mainType == TabType.Jewel) {
        GameDataManager.Instance.SetPurchaseType(PurchaseType.Normal);
        WebHttp.Instance.RequestGooglePurchaseKey(() => {
            IAPManager.Instance.BuyProductID(tprice._productID_ANDROID, () => {
                _shopPopupBuy.SetShow(ShopPopupType.PurchaseComplete, id);
                RefreshUI();
            });
        });
    }
}

```


---

### ③ 상품 구매 완료 후 처리

- 구매 완료 후 아이템 추가 등 후속 처리
    
- 상품 유형에 따른 추가 처리 로직 포함
    

```csharp

private void PurchaseComplete(int id) {
    RefreshUI();

    if (_mainType == TabType.Resource) {
        TPrice tp = TPrices.Instance.Find(id);
        if (tp._priceSubType == PriceSubType.ExpandTeamCapacity) {
            PopupManager.Instance.ShowAnimNoticePopup(_noticeAnimTF, string.Format("팀 최대 보유량 증가: {0}->{1}", PlayerManager.Instance.RetainableMaxTeamCount - 1, PlayerManager.Instance.RetainableMaxTeamCount));
        }
    }
}

```


---
