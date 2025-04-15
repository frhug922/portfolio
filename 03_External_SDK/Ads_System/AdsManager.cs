using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    #region private field

    private static AdsManager instance = null;

#if UNITY_ANDROID
    string appKey = "TESTKEYANDROID";
#elif UNITY_IPHONE
    string appKey = "TESTKEYIOS";
#else 
    string appKey = "unexpected_platform";
#endif

    private string _hash = null;
    private System.Action _callback = null;
    private int _type = 0;
    private int _serverCheckCount = 0;

    #endregion // private field





    #region properties

    public static AdsManager Instance { get { return instance; } }
    public int ServerCheckCount { get { return _serverCheckCount; } }

    #endregion // properties





    #region mono funcs

    private void Awake() {
        instance = this;
    }

    private void OnEnable() {
        IronSourceEvents.onSdkInitializationCompletedEvent += SDKInitialized;

        ////Add AdInfo Banner Events
        //IronSourceBannerEvents.onAdLoadedEvent += BannerOnAdLoadedEvent;
        //IronSourceBannerEvents.onAdLoadFailedEvent += BannerOnAdLoadFailedEvent;
        //IronSourceBannerEvents.onAdClickedEvent += BannerOnAdClickedEvent;
        //IronSourceBannerEvents.onAdScreenPresentedEvent += BannerOnAdScreenPresentedEvent;
        //IronSourceBannerEvents.onAdScreenDismissedEvent += BannerOnAdScreenDismissedEvent;
        //IronSourceBannerEvents.onAdLeftApplicationEvent += BannerOnAdLeftApplicationEvent;

        ////Add AdInfo Interstitial Events
        //IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
        //IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
        //IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
        //IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent;
        //IronSourceInterstitialEvents.onAdShowSucceededEvent += InterstitialOnAdShowSucceededEvent;
        //IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
        //IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;

        //Add AdInfo Rewarded Video Events
        IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
        IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;
    }

    private void SDKInitialized() {
#if SHOW_LOG
        Debug.LogWarning("AdsManager : SDKInitialized Complete");
#endif // SHOW_LOG
    }

    private void OnApplicationPause(bool pause) {
        IronSource.Agent.onApplicationPause(pause);
    }

    #endregion // mono funcs





    #region public funcs

    public void Init() {
        IronSource.Agent.validateIntegration();
        IronSource.Agent.setUserId(PlayerManager.Instance.Userkey);
        IronSource.Agent.init(appKey);
    }

    /// <summary>
    /// 하단 광고 배너 보여주기 
    /// </summary>
    public void LoadBottomBanner() {
        IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
    }

    /// <summary>
    /// 상단 배너 광고 보여주기
    /// </summary>
    public void LoadTopBanner() {
        IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.TOP);
    }

    /// <summary>
    /// 배너 광고 삭제하기
    /// </summary>
    public void DestroyBanner() {
        IronSource.Agent.destroyBanner();
    }

    /// <summary>
    /// 전면 광고 로드하기
    /// </summary>
    public void LoadInterstitial() {
        IronSource.Agent.loadInterstitial();
    }

    /// <summary>
    /// 전면 광고 보여주기
    /// </summary>
    public void ShowInterstitial() {
        if (IronSource.Agent.isInterstitialReady()) {
            IronSource.Agent.showInterstitial();
        }
        else {
            //보여줄 수 있는 전면 광고 없음
        }
    }

    /// <summary>
    /// 보상형 광고 로드하기
    /// </summary>
    public void LoadRewarded() {
#if SHOW_LOG
        Debug.LogWarning("보상 광고 로드");
#endif
        IronSource.Agent.clearRewardedVideoServerParams();
        IronSource.Agent.loadRewardedVideo();
    }

    /// <summary>
    /// 보상형 광고 보여주기
    /// </summary>
    public void ShowRewarded(int type, System.Action callback = null) {
        _type = type;
        _callback = callback;

        if (IronSource.Agent.isRewardedVideoAvailable()) {
#if SHOW_LOG
            Debug.LogWarning("광고 보여주기");
#endif
            var mdHash = MD5.Create();

            _hash = mdHash.GetHashCode().ToString();

            IronSource.Agent.setDynamicUserId(GetMd5Hash(mdHash, string.Format("{0}{1}", PlayerManager.Instance.Userkey, _hash)));
            IronSource.Agent.showRewardedVideo();
        }
        else {
#if SHOW_LOG
            Debug.LogWarning("보여줄 광고 없음");
#endif
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("APPAD_36004"), TStrings.Instance.FindString("APPAD_36008"), () => {
                if (null != _callback) {
                    _callback();

                    _callback = null;
                }
            });
        }
    }

    public void StartRequest() {
        StartCoroutine(RequestAfterOneSecond());
    }

    public IEnumerator RequestAfterOneSecond() {
        float returnTime = 1f;

        yield return new WaitForSeconds(returnTime);

        if (_serverCheckCount > Common.adsServerCheckMaxCount) {
            PopupManager.Instance.HideRewardProgress();
            PopupManager.Instance.HideRewardPopup();
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("APPAD_36004"), TStrings.Instance.FindString("APPAD_36009"), null);
            yield break;
        }

        WebHttp.Instance.RequestAdReward(_hash, _type, _callback);
        ++_serverCheckCount;
    }

    public void ResetServerCheckCount() {
        _serverCheckCount = 0;
    }

    #endregion // public funcs





    #region private funcs

    static string GetMd5Hash(MD5 md5Hash, string input) {
        // Convert the input string to a byte array and compute the hash.
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        StringBuilder sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data 
        // and format each one as a hexadecimal string.
        for (int i = 0; i < data.Length; i++) {
            sBuilder.Append(data[i].ToString("x2"));
        }

        // Return the hexadecimal string.
        return sBuilder.ToString();
    }

    #endregion // private funcs






    //#region Banner

    ///************* Banner AdInfo Delegates *************/
    ////Invoked once the banner has loaded
    //void BannerOnAdLoadedEvent(IronSourceAdInfo adInfo) {
    //}
    ////Invoked when the banner loading process has failed.
    //void BannerOnAdLoadFailedEvent(IronSourceError ironSourceError) {
    //}
    //// Invoked when end user clicks on the banner ad
    //void BannerOnAdClickedEvent(IronSourceAdInfo adInfo) {
    //}
    ////Notifies the presentation of a full screen content following user click
    //void BannerOnAdScreenPresentedEvent(IronSourceAdInfo adInfo) {
    //}
    ////Notifies the presented screen has been dismissed
    //void BannerOnAdScreenDismissedEvent(IronSourceAdInfo adInfo) {
    //}
    ////Invoked when the user leaves the app
    //void BannerOnAdLeftApplicationEvent(IronSourceAdInfo adInfo) {
    //}

    //#endregion // Banner





    //#region Interstitial

    ///************* Interstitial AdInfo Delegates *************/
    //// Invoked when the interstitial ad was loaded succesfully.
    //void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo) {
    //}
    //// Invoked when the initialization process has failed.
    //void InterstitialOnAdLoadFailed(IronSourceError ironSourceError) {
    //}
    //// Invoked when the Interstitial Ad Unit has opened. This is the impression indication. 
    //void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo) {
    //}
    //// Invoked when end user clicked on the interstitial ad
    //void InterstitialOnAdClickedEvent(IronSourceAdInfo adInfo) {
    //}
    //// Invoked when the ad failed to show.
    //void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo) {
    //}
    //// Invoked when the interstitial ad closed and the user went back to the application screen.
    //void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo) {
    //}
    //// Invoked before the interstitial ad was opened, and before the InterstitialOnAdOpenedEvent is reported.
    //// This callback is not supported by all networks, and we recommend using it only if  
    //// it's supported by all networks you included in your build. 
    //void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo adInfo) {
    //}

    //#endregion // Interstitial





    #region Rewarded

    /************* RewardedVideo AdInfo Delegates *************/
    // Indicates that there’s an available ad.
    // The adInfo object includes information about the ad that was loaded successfully
    // This replaces the RewardedVideoAvailabilityChangedEvent(true) event
    void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo) {
        Debug.Log("RewardedVideoOnAdAvailable");
    }
    // Indicates that no ads are available to be displayed
    // This replaces the RewardedVideoAvailabilityChangedEvent(false) event
    void RewardedVideoOnAdUnavailable() {
        Debug.Log("RewardedVideoOnAdUnavailable");
    }
    // The Rewarded Video ad view has opened. Your activity will loose focus.
    void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo) {
        Debug.Log("RewardedVideoOnAdOpenedEvent");
    }
    // The Rewarded Video ad view is about to be closed. Your activity will regain its focus.
    void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo) {
        WebHttp.Instance.RequestAdReward(_hash, _type, _callback);
        ResetServerCheckCount();
        Debug.Log("RewardedVideoOnAdClosedEvent");
    }
    // The user completed to watch the video, and should be rewarded.
    // The placement parameter will include the reward data.
    // When using server-to-server callbacks, you may ignore this event and wait for the ironSource server callback.
    void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo) {
        Debug.Log("RewardedVideoOnAdRewardedEvent");
        //WebHttp.Instance.RequestAdReward(_hash, _type, _callback);
    }
    // The rewarded video ad was failed to show.
    void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo) {
        Debug.Log("RewardedVideoOnAdShowFailedEvent");
        PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("APPAD_36004"), TStrings.Instance.FindString("APPAD_36008"), () => {
            if (null != _callback) {
                _callback();

                _callback = null;
            }
        });
    }
    // Invoked when the video ad was clicked.
    // This callback is not supported by all networks, and we recommend using it only if
    // it’s supported by all networks you included in your build.
    void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo) {
        Debug.Log("RewardedVideoOnAdClickedEvent");
    }

    #endregion // Rewarded
}
