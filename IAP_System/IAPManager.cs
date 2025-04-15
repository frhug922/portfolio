using OneStore.Purchasing;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour, IDetailedStoreListener
#if UNITY_ANDROID
, IPurchaseCallback
#endif
{
    #region static

    private static IStoreController storeController;
    private static IExtensionProvider extensionProvider;
    private static IAPManager _instance = null;

    #endregion // static





    #region Instance

    public static IAPManager Instance { get { return _instance; } }

    #endregion // Instance





    #region serialized field

    [SerializeField] private bool _isiOSBuild = false;

    #endregion // serialized field





    #region properties

    public bool IsiOSBuild { get { return _isiOSBuild; } }

    #endregion // properties





    #region private field

    private System.Action _callback;
#if UNITY_ANDROID
    private PurchaseClientImpl _purchaseClient;
#endif
    List<ProductDetail> _onestoreProducts = new();
    private string environment = "production";
    private int _serverCheckCount = 0;
    private PurchaseData _purchaseData = null;
    private Coroutine _serverCheckCoroutine;

    #endregion // private field





    #region mono funcs

    private void Awake() {
        _instance = this;
    }

    async void Start() {
        try {
            var options = new InitializationOptions()
                .SetEnvironmentName(environment);

            await UnityServices.InitializeAsync(options);
        }
        catch (Exception exception) {
            Debug.LogException(exception);
        }
    }

    #endregion // mono funcs





    #region public funcs

    public void InitializePurchasing() {
#if UNITY_ANDROID
        if (StoreType.OneStore == SystemManager.Instance.StoreType) {
            _purchaseClient = new PurchaseClientImpl("EXAMPLE_PUBLIC_KEY");
            _purchaseClient?.Initialize(gameObject.GetComponent<IAPManager>());
            string storeCode = _purchaseClient?.StoreCode;
            List<TPrice> onestoreItem = TPrices.Instance._tableData;
            List<string> itemsString = new();
            for (int i = 0, count = onestoreItem.Count; i < count; ++i) {

                if (onestoreItem[i]._productID_IOS != string.Empty) {
                    itemsString.Add(onestoreItem[i]._productID_IOS);
                }
#if SHOW_LOG
                Debug.LogFormat("OneStore Item : Item : {0}", onestoreItem[i]._productID_IOS);
#endif
            }

            _purchaseClient?.QueryProductDetails(itemsString.AsReadOnly(), OneStore.Purchasing.ProductType.INAPP);

            return;
        }
#endif

        if (IsInitialized())
            return;

        var module = StandardPurchasingModule.Instance();

        ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);

        List<TPrice> inappItem = TPrices.Instance._tableData;
        for (int i = 0, count = inappItem.Count; i < count; ++i) {
#if UNITY_ANDROID
            if (inappItem[i]._productID_ANDROID != string.Empty) {
                builder.AddProduct(inappItem[i]._productID_ANDROID, UnityEngine.Purchasing.ProductType.Consumable);
            }
#elif UNITY_IOS
            if (inappItem[i]._productID_IOS != string.Empty) {
                builder.AddProduct(inappItem[i]._productID_IOS, UnityEngine.Purchasing.ProductType.Consumable);
            }
#endif
        }

        UnityPurchasing.Initialize(this, builder);
    }

    public void BuyProductID(string productId, System.Action callback) { // 구매 요청
        _callback = callback;
        PopupManager.Instance.ShowWaitPopup(TStrings.Instance.FindString("SHOP_9047"), TStrings.Instance.FindString("SHOP_9048"));

        if (StoreType.OneStore == SystemManager.Instance.StoreType) {
#if UNITY_ANDROID
            try {
                if (productId == null) {
                    return;
                }

                ProductDetail product = FindProduct(productId);
                if (product == null) {
                    PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("SHOP_9008"), TStrings.Instance.FindString("SHOP_9049"), null);
                    return;
                }
                OneStore.Purchasing.ProductType productType = OneStore.Purchasing.ProductType.Get(product.type);

                var purchaseFlowParams = new PurchaseFlowParams.Builder().SetProductId(product.productId).SetProductType(productType).Build();

                _purchaseClient?.Purchase(purchaseFlowParams);
            }
            catch (Exception e) {
                Debug.Log("BuyProductID: FAIL. Exception during purchase. " + e);
                PopupManager.Instance.HideNormalPopup();
            }
#endif
        }
        else {
            try {
                if (IsInitialized()) {
                    Product p = storeController.products.WithID(productId);

                    if (p != null && p.availableToPurchase) {
                        Debug.Log(string.Format("Purchasing product asychronously: '{0}'", p.definition.id));
                        storeController.InitiatePurchase(p);
                    }
                    else {
                        Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                    }
                }
                else {
                    Debug.Log("BuyProductID FAIL. Not initialized.");
                }
            }
            catch (Exception e) {
                Debug.Log("BuyProductID: FAIL. Exception during purchase. " + e);
                PopupManager.Instance.HideNormalPopup();
            }
        }
    }

    public void RestorePurchase() { // 구매 복원 요청
        if (StoreType.OneStore == SystemManager.Instance.StoreType) {
#if UNITY_ANDROID
            try {
                _purchaseClient.QueryPurchases(OneStore.Purchasing.ProductType.INAPP);
            }
            catch (Exception e) {
                Debug.LogWarning(e);
            }
#endif
        }
        else {
            if (!IsInitialized()) {
                //PopupManager.Instance.ShowOKPopup("등록된 상품 없음", "등록된 상품이 없습니다. 게임을 다시 시작해주세요", null);
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer) {
                Debug.Log("RestorePurchases started ...");
            }
            else {
                Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }
    }

    public string GetProductPrice(string productId) {
        if (SystemManager.Instance.StoreType == StoreType.OneStore) {
            string productPrice = string.Empty;

            ProductDetail product = FindProduct(productId);

            if (product != null) {
                productPrice = product.price;
            }

            return string.Format("{0}₩", productPrice);
        }
        else {
            return storeController.products.WithID(productId).metadata.localizedPriceString;
        }
    }

    public void OnestorePurchaseComplete() { //원스토어 결제 완료 후 호출
#if UNITY_ANDROID
        if (null != _purchaseData) {
            _purchaseClient?.ConsumePurchase(_purchaseData);
        }
#endif
    }

    public void StartCheckPurchase() {
        StartRequestServerCheck();
    }

    public void StopCheckPurchase() {
        _serverCheckCount = 0;
        StopRequestServerCheck();
    }

    #endregion // public funcs





    #region UnityPurchase

    public void OnInitialized(IStoreController sc, IExtensionProvider ep) { // 상품 등록 완료
        Debug.Log("IAPManager : OnInitialized : PASS");
#if SHOW_LOG
        Debug.Log("\t<color=white>IAPManager : OnInitialized..!!</color>\n");
#endif // SHOW_LOG

        storeController = sc;
        extensionProvider = ep;
    }

    public void OnInitializeFailed(InitializationFailureReason reason) {
        Debug.LogError("OnInitializeFailed InitializationFailureReason:" + reason);
    }

    public void OnInitializeFailed(InitializationFailureReason reason, string message) {
        Debug.LogError("OnInitializeFailed InitializationFailureReason:" + reason + message);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) { //구매 성공 시 호출
#if SHOW_LOG
        Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
#endif
        char[] charsToTrim = { '\\', '"', ':', ',' };
        string orderId = Common.GetBetween(args.purchasedProduct.receipt, "orderId", "packageName").Trim(charsToTrim);
        string purchaseToken = Common.GetBetween(args.purchasedProduct.receipt, "purchaseToken", "quantity").Trim(charsToTrim);

        string receiptString = args.purchasedProduct.receipt;
        var receiptDict = (Dictionary<string, object>)MiniJson.JsonDecode(receiptString);
        string payloadString = (string)receiptDict["Payload"];

        char[] charsToTrimInProductName = { '(', ')' };

#if UNITY_ANDROID
        PlayerPrefsManager.Instance.SavePurchaseData(GameDataManager.Instance.GetPurchaseType()
            , args.purchasedProduct.metadata.localizedTitle.Trim(charsToTrimInProductName)
            , args.purchasedProduct.definition.id
            , orderId
            , purchaseToken);
#else
        PlayerPrefsManager.Instance.SavePurchaseData(GameDataManager.Instance.GetPurchaseType()
            , args.purchasedProduct.metadata.localizedTitle.Trim(charsToTrimInProductName)
            , args.purchasedProduct.definition.id
            ,args.purchasedProduct.transactionID
            ,payloadString);
#endif

        WebHttp.Instance.RequestCashPurchaseCheck(_callback);

        PopupManager.Instance.HideNormalPopup();

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) { // 구매 실패 시 호출
        Debug.LogError(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription desc) {
        PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("Eventstore_32012"), string.Format(TStrings.Instance.FindString("Eventstore_32013"), desc.reason), null);
    }

    #endregion //UnityPurchase




#if UNITY_ANDROID
    #region OnestorePurchase

    public void OnSetupFailed(IapResult iapResult) {
#if SHOW_LOG
        Debug.LogWarningFormat("Onestore SetupFailed : OnSetupFailed : {0}", iapResult.Message);
#endif 
    }

    public void OnProductDetailsSucceeded(List<ProductDetail> productDetails) { // 상품정보 조회 성공
#if SHOW_LOG
        Debug.LogWarning("Onestore Initialize Succeeded : OnProductDetailsSucceeded");
#endif

        _onestoreProducts = productDetails;
    }

    public void OnProductDetailsFailed(IapResult iapResult) { // 상품정보 조회 실패
#if SHOW_LOG
        Debug.LogWarningFormat("Onestore Initialize Failed : OnProductDetailsFailed : {0}", iapResult.Message);
#endif
    }

    public void OnPurchaseSucceeded(List<PurchaseData> purchases) { // 구매 성공 시 호출
#if SHOW_LOG
        Debug.Log(string.Format("ProcessPurchase: PASS."));
#endif
        _purchaseData = purchases[0];

        PlayerPrefsManager.Instance.SavePurchaseData(GameDataManager.Instance.GetPurchaseType()
            , _purchaseData.PackageName
            , _purchaseData.ProductId
            , _purchaseData.OrderId
            , _purchaseData.PurchaseToken);

        WebHttp.Instance.RequestCashPurchaseCheck(_callback);

        PopupManager.Instance.HideNormalPopup();
    }

    public void OnPurchaseFailed(IapResult iapResult) { // 구매 실패 시 호출
        PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("Eventstore_32012"), string.Format(TStrings.Instance.FindString("Eventstore_32013"), iapResult.Message), null);
    }

    public void OnConsumeSucceeded(PurchaseData purchase) {

    }

    public void OnConsumeFailed(IapResult iapResult) {

    }

    public void OnAcknowledgeSucceeded(PurchaseData purchase, OneStore.Purchasing.ProductType type) {

    }

    public void OnAcknowledgeFailed(IapResult iapResult) {

    }

    public void OnManageRecurringProduct(IapResult iapResult, PurchaseData purchase, RecurringAction action) {

    }

    public void OnNeedUpdate() {

    }

    public void OnNeedLogin() {

    }

    #endregion // OnestorePurchase
#endif





    #region private funcs

    private bool IsInitialized() {
        return (storeController != null && extensionProvider != null);
    }

    private ProductDetail FindProduct(string productId) {
        for (int i = 0, count = _onestoreProducts.Count; i < count; ++i) {
            if (productId == _onestoreProducts[i].productId) {
                return _onestoreProducts[i];
            }
        }

        return null;
    }

    private void StartRequestServerCheck() {
        StopRequestServerCheck();

        _serverCheckCoroutine = StartCoroutine(UpdateServerPurchaseCheck());
    }

    private void StopRequestServerCheck() {
        if (null == _serverCheckCoroutine) {
            return;
        }

        StopCoroutine(_serverCheckCoroutine);
        _serverCheckCoroutine = null;
    }

    private IEnumerator UpdateServerPurchaseCheck() {
        float returnTime = 2f;

        yield return new WaitForSeconds(returnTime);

        if (_serverCheckCount > Common.iapServerCheckMaxCount) {
            PopupManager.Instance.HideNormalPopup();
            _serverCheckCount = 0;
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("SHOP_9008"), TStrings.Instance.FindString("SHOP_9049"), null);
            yield break;
        }

        WebHttp.Instance.RequestCashPurchaseCheck(_callback);
        ++_serverCheckCount;

        yield return null;
    }

    #endregion //private funcs
}
