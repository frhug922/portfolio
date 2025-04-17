using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanWarMapUI : LobbyMenuUIBase
{
    #region type def

    private enum TextType
    {
        ClanWarKeyAmount,
        SliderL,
        SliderR,
        ClanName,
        EnemyClanName,
        RestTime,
        WarProgress,
        WarInfo,
        TeamCheck,
        FindOn,
        FindOff,
        TeamEdit,
        Chat,
    }

    private enum ButtonType
    {
        FindON,
        FindOFF,
        TeamEdit,
    }

    private enum ToolTipTFType
    {
        WarRule,
        Slider,
    }

    private enum MoveVectorType
    {
        LeftUnder,
        LeftUp,
        CenterUnder,
        CenterUp,
        RightUnder,
        RightUp,
    }

    #endregion // type def





    #region serialized fields

    [Header("For ClanInfo")]
    [SerializeField] private ChargeTooltipUI _asset;
    [SerializeField] private Image _ourClanFlagImage;
    [SerializeField] private Image _ourClanSimbolImage;
    [SerializeField] private Image _otherClanFlagImage;
    [SerializeField] private Image _otherClanSimbolImage;
    [SerializeField] private Slider _slider;

    [Header("클랜전 맵 UI 정보")]
    [SerializeField] private GameObject _settingRoot;
    [SerializeField] private ScrollRect _warMap;
    [SerializeField] private Transform _noticeAnimTF;
    [SerializeField] private List<Text> _miniChatTextList;
    [SerializeField] private GameObject _chatBox;
    [SerializeField] private List<GameObject> _bottomUIButtons;
    [SerializeField] private PopupClanWarAttackInfo _clanWarHistoryPopup;
    [SerializeField] private List<ClanWarAttackHistory> _clanWarHistories;
    [SerializeField] private PopupClanWarInfo _clanWarInfo;
    [SerializeField] private List<Transform> _tooltipTF;
    [SerializeField] private Image _clanWarRuleIcon;
    [SerializeField] private List<Transform> _moveVectors;
    [SerializeField] private List<ClanWarMoveBox> _clanWarMoveBoxes;

    [Header("클랜전 참여 정보 **클랜 배치 순서는 절대로 건들지 말 것(21304)**")]
    [SerializeField] private List<ClanWarProfile> _myClanProfiles;
    [SerializeField] private List<ClanWarProfile> _enemyClanProfiles;

    #endregion //serialized fields





    #region private fields

    private ClanData _clanData;
    private MClanInfo _otherClanInfo;
    private MClanInfo _myClanInfo;
    private ClanWarProgressType _clanWarProgressType;
    private List<MClanWarBattleUser> _myClanWarBattleUsers;
    private List<MClanWarBattleUser> _enemyClanWarBattleUsers;
    private int _initializedMyClanUserCount = 0;
    private int _initializedEnemyClanuserCount = 0;
    private GameObject _rootObject = null;
    private Coroutine _clanWarRestTimeCoroutine;
    private MWarProgress _clanWarProgress;
    private Coroutine _clanWarProfileUpdateCoroutine;
    private List<MClanWarHistory> _clanWarHistoryData = new();
    private Coroutine _clanWarEnemyAliveCoroutine;
    private readonly float _mapContentsMinSize = 0.7f;
    private readonly float _mapContentsMaxSize = 1.4f;

    #endregion //private fields





    #region mono funcs

    private void Update() {
        if (CheckClickOutSide()) {
            _rootObject.SetActive(false);
            _rootObject = null;
        }

        CheckZoom();
    }

    #endregion // mono funcs





    #region public funcs

    public override void SetShow() {
        WebHttp.Instance.RequestClanWarBattleFieldInfo(RefreshUI);
        WebHttp.Instance.RequestClanWarHistory(UpdateHistory);

        _clanData = PlayerManager.Instance.ClanData;
        _otherClanInfo = GameDataManager.Instance.ClanWarInfo.vsClanInfo;
        _myClanInfo = GameDataManager.Instance.ClanWarInfo.clanInfo;
        _clanWarProgressType = GameDataManager.Instance.GetClanWarProgress();
        _myClanWarBattleUsers = GameDataManager.Instance.GetMyClanWarUserList().m_items;
        _enemyClanWarBattleUsers = GameDataManager.Instance.GetEnemyClanWarUserList().m_items;
        _clanWarProgress = GameDataManager.Instance.ClanWarInfo.warProgress;

        SetClanFlag();

        SetButtonUI();

        base.SetShow();
        Initialize();
        InitializeMiniChatList();

        RefreshUI();

        ShowNoticeAnim();

        StartUpdateClanWarTime();
        StartUpdateEnemyAlive();

        TCPSocketManager.Instance.RequestClanWarMapEnter();
        TCPSocketManager.Instance.RequestClanWarEnemyOnline(PlayerManager.Instance.ClanID);
        TCPSocketManager.Instance.RequestClanWarEnemyOnline(_otherClanInfo.m_clanId);
    }

    public override void SetHide() {
        TCPSocketManager.Instance.RequestClanWarMapClose();
        PopupManager.Instance.HideAnimNoticePopup();

        base.SetHide();
    }

    public override void RefreshUI() {
        _myClanWarBattleUsers = GameDataManager.Instance.GetMyClanWarUserList().m_items;

        UpdateTextsUI();
        UpdateContentsUI();
    }

    public override void UpdateTextsUI() {
        SetText((int)TextType.ClanWarKeyAmount, PlayerManager.Instance.GetAssetAmount(AssetType.ClanWarEnergy).ToString());
        SetText((int)TextType.ClanName, _clanData._name);
        SetText((int)TextType.EnemyClanName, _otherClanInfo.m_clanname);
        SetText((int)TextType.WarInfo, TStrings.Instance.FindString("Clan_29130"));
        SetText((int)TextType.TeamCheck, TStrings.Instance.FindString("Clan_29131"));
        SetText((int)TextType.FindOn, TStrings.Instance.FindString("Clan_29141"));
        SetText((int)TextType.FindOff, TStrings.Instance.FindString("Clan_29141"));
        SetText((int)TextType.TeamEdit, TStrings.Instance.FindString("Clan_29132"));
        SetText((int)TextType.Chat, TStrings.Instance.FindString("Clan_29039"));
        SetText((int)TextType.SliderL, GameDataManager.Instance.ClanWarInfo.getPointInfo.m_total_point.ToString());
        SetText((int)TextType.SliderR, GameDataManager.Instance.ClanWarInfo.getPointInfo.m_vs_total_point.ToString());
        SetText((int)TextType.ClanName, _clanData._name);
        SetText((int)TextType.EnemyClanName, _otherClanInfo.m_clanname);

        if (ClanWarProgressType.ReadyWar == _clanWarProgressType || ClanWarProgressType.DeckCopying == _clanWarProgressType) {
            SetText((int)TextType.WarProgress, TStrings.Instance.FindString("Clan_29129"));
        }
        else if (ClanWarProgressType.War == _clanWarProgressType) {
            SetText((int)TextType.WarProgress, TStrings.Instance.FindString("Clan_29181"));
        }
        else if (ClanWarProgressType.AfterWar == _clanWarProgressType) {
            SetText((int)TextType.RestTime, GameDataManager.Instance.GetClanWarResultToString());
            SetText((int)TextType.WarProgress, TStrings.Instance.FindString("Clan_29182"));
        }
    }

    public void UpdateContentsUI() {
        SetClanIcon(ref _clanWarRuleIcon, string.Format("waricon{0:D2}", _clanWarProgress.m_war_property - 3));

        for (int i = 0, count = _initializedMyClanUserCount; i < count; ++i) {
            _myClanProfiles[i].SetShow(_myClanWarBattleUsers[i]);
        }

        if (ClanWarProgressType.War == _clanWarProgressType || ClanWarProgressType.AfterWar == _clanWarProgressType) {
            for (int i = 0, count = _initializedEnemyClanuserCount; i < count; ++i) {
                _enemyClanProfiles[i].SetShow(_enemyClanWarBattleUsers[i]);
            }
        }

        SetWarPointSlider();
    }

    public void UpdateMiniChattingList() {
        _miniChatTextList[0].text = _miniChatTextList[1].text;

        List<MClanChatInfo> chatInfos = PlayerManager.Instance.ClanData._clanChatInfos;
        _miniChatTextList[1].text = GetClanMessage(chatInfos[^1]);
    }

    public void InitializeMiniChatList() {
        List<MClanChatInfo> chatInfos = PlayerManager.Instance.ClanData._clanChatInfos;

        if (chatInfos.Count < _miniChatTextList.Count) {
            for (int i = 0, count = chatInfos.Count; i < count; ++i) {
                _miniChatTextList[i].text = GetClanMessage(chatInfos[i]);
            }
        }
        else {
            for (int i = 0, count = _miniChatTextList.Count; i < count; ++i) {
                _miniChatTextList[i].text = GetClanMessage(chatInfos[^(count - i)]);
            }
        }
    }

    public void OnClick_Chatting() {
        SoundManager.Instance.PlaySFX(FXSound.Click);
#if SHOW_LOG
        Debug.Log("ClanUIContoller : OnClick_Chatting");
#endif

        LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.Chatting);
    }

    public void OnClick_ChatIcon() {
        _chatBox.SetActive(!_chatBox.activeSelf);
    }

    public void OnClick_EditDefenseTeam() {
        SystemManager.Instance.SetBattleType(BattleType.ClanWar);
        LobbySceneUIManager.Instance.ClanWarDeckEditType = DeckType.Defence;

        LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.TeamEdit);
    }

    public void OnClick_Setting() {
        _settingRoot.SetActive(true);
        _rootObject = _settingRoot;
    }

    public void OnClick_WarInfo() {
        _clanWarInfo.SetShow();
    }

    public void OnClick_TeamCheck() {
        WebHttp.Instance.RequestClanWarUsedCardList(() => {
            SystemManager.Instance.SetBattleType(BattleType.ClanWar);
            LobbySceneUIManager.Instance.ClanWarDeckEditType = DeckType.Attack;
            LobbySceneUIManager.Instance.IsClanWarAttack = false;
            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.TeamEdit);
        });
    }

    public void OnClick_History() {
        WebHttp.Instance.RequestClanWarCurrentRanking(() => {
            _clanWarHistoryPopup.SetShow();
        });
    }

    public void OnClick_ClanWarRule() {
        PopupManager.Instance.ShowTooltip(_tooltipTF[(int)ToolTipTFType.WarRule],
            TSpecialRuleRotations.Instance.Find((BattleSpecialType)_clanWarProgress.m_war_property).LangTypeName,
            TSpecialRules.Instance.Find((BattleSpecialType)_clanWarProgress.m_war_property).LangTypeDesc);
    }

    public void UpdateOnlineAndProgress(List<int> attackerUIDs, List<int> defenderUIDs, List<int> progress) {
        for (int i = 0, count = _initializedEnemyClanuserCount; i < count; ++i) {
            _enemyClanProfiles[i].UpdateConnectEnemy(attackerUIDs);
        }

        for (int i = 0, count = _initializedEnemyClanuserCount; i < count; ++i) {
            for (int j = 0, countj = defenderUIDs.Count; j < countj; ++j) {
                if (0 == defenderUIDs[j]) {
                    break;
                }
                else if (defenderUIDs[j] == _enemyClanWarBattleUsers[i].m_userseq) {
                    string attackerNick = string.Empty;

                    for (int k = 0, countk = _initializedMyClanUserCount; k < countk; ++k) {
                        if (_myClanWarBattleUsers[k].m_userseq == attackerUIDs[j]) {
                            attackerNick = _myClanWarBattleUsers[k].m_nickname;
                        }
                    }

                    if (attackerNick == string.Empty) {
                        _enemyClanProfiles[i].UpdateProgress(0, 0);
                    }
                    else {
                        _enemyClanProfiles[i].UpdateProgress(progress[j], attackerUIDs[j], attackerNick);
                    }
                    break;
                }
            }

            if (!attackerUIDs.Contains(_enemyClanProfiles[i].AttackerID)) {
                TCPSocketManager.Instance.RequestClanWarProgress(_otherClanInfo.m_clanId, _enemyClanWarBattleUsers[i].m_userseq, 0);
            }
        }

        for (int i = 0, count = _initializedMyClanUserCount; i < count; ++i) {
            for (int j = 0, countj = defenderUIDs.Count; j < countj; ++j) {
                if (0 == defenderUIDs[j]) {
                    break;
                }
                else if (defenderUIDs[j] == _myClanWarBattleUsers[i].m_userseq) {
                    string attackerNick = string.Empty;

                    for (int k = 0, countk = _initializedEnemyClanuserCount; k < countk; ++k) {
                        if (_enemyClanWarBattleUsers[k].m_userseq == attackerUIDs[j]) {
                            attackerNick = _enemyClanWarBattleUsers[k].m_nickname;
                        }
                    }

                    _myClanProfiles[i].UpdateProgress(progress[j], attackerUIDs[j], attackerNick);
                    break;
                }
            }

            if (!defenderUIDs.Contains(_myClanWarBattleUsers[i].m_userseq)) {
                TCPSocketManager.Instance.RequestClanWarProgress(_myClanInfo.m_clanId, _myClanWarBattleUsers[i].m_userseq, 0);
            }
        }

        UpdateMyClanProgress();
    }

    public void UpdateAttackInfo(int attackerUID, int vsClanID, int vsUserUID, int progress) {
        if ((int)ClanWarProfile.ProgressType.None == progress || (int)ClanWarProfile.ProgressType.Reserve == progress) { // 예약 취소나 공격종료 // 예약이 걸릴 때에는 즉시 반영
            UpdateProgressImmediately(vsClanID, vsUserUID, progress, attackerUID);
        }
        else if ((int)ClanWarProfile.ProgressType.Attack == progress) { //공격이 걸리면 박스가 움직임
            StartUpdateProfileProgress(attackerUID, vsClanID, vsUserUID);
        }
    }

    public void UpdateHistory() {
        _clanWarHistoryData = GameDataManager.Instance.ClanWarHistory;

        int count = _clanWarHistories.Count;
        if (_clanWarHistoryData.Count < _clanWarHistories.Count) {
            count = _clanWarHistoryData.Count;
        }

        for (int i = 0; i < count; ++i) {
            _clanWarHistories[i].SetShow(_clanWarHistoryData[count - (i + 1)].m_nickname,
                _clanWarHistoryData[count - (i + 1)].m_vs_nickname,
                _clanWarHistoryData[count - (i + 1)].m_point,
                _clanWarHistoryData[count - (i + 1)].m_status == 1);
        }
    }

    public void UpdateMap(int vsUserID, List<int> cardList, List<int> hpList) {
        for (int i = 0, count = _myClanWarBattleUsers.Count; i < count; ++i) {
            if (_myClanWarBattleUsers[i].m_userseq == vsUserID) {
                for (int j = 0, countj = _myClanWarBattleUsers[i].m_items.Count; j < countj; ++j) {
                    _myClanWarBattleUsers[i].m_items[j].m_cardHp = hpList[j];
                }
                RefreshUI();
                return;
            }
        }

        for (int i = 0, count = _enemyClanWarBattleUsers.Count; i < count; ++i) {
            if (_enemyClanWarBattleUsers[i].m_userseq == vsUserID) {
                for (int j = 0, countj = _enemyClanWarBattleUsers[i].m_items.Count; j < countj; ++j) {
                    _enemyClanWarBattleUsers[i].m_items[j].m_cardHp = hpList[j];
                }
                RefreshUI();
                return;
            }
        }
    }

    public void OnClick_OurClanFlag() {
        WebHttp.Instance.RequestClanInfo(_myClanInfo.m_clanId, LobbySceneUIManager.Instance.CurrentSubType);
    }

    public void OnClick_EnemyClanFlag() {
        WebHttp.Instance.RequestClanInfo(_otherClanInfo.m_clanId, LobbySceneUIManager.Instance.CurrentSubType);
    }

    public void OnClick_FindEnemy() {
        List<int> attackableList = new();

        for (int i = 0, count = _enemyClanWarBattleUsers.Count; i < count; ++i) {
            if (Common.ConvertJavaMillisecondToDateTimeUTC(_enemyClanWarBattleUsers[i].m_revival_time) < DateTime.UtcNow) {
                attackableList.Add(i);
            }
        }

        System.Random random = new();

        int randomIndex = random.Next(0, attackableList.Count);

        _enemyClanProfiles[attackableList[randomIndex]].OnClick_Profile();
    }

    public void OnClick_Slider() {
        PopupManager.Instance.ShowTooltip(_tooltipTF[(int)ToolTipTFType.Slider], TStrings.Instance.FindString("Clan_29248"), TStrings.Instance.FindString("Clan_29249"));
    }

    public void ReviveAllEnemy(int vsClanID) {
        for (int i = 0, count = _enemyClanWarBattleUsers.Count; i < count; ++i) {
            _enemyClanWarBattleUsers[i].m_revival_time = 0;
        }

        RefreshUI();
    }

    #endregion // public funcs





    #region private funcs

    private void Initialize() {
        _asset.Initialize((int)AssetType.ClanWarEnergy, OnClick_AssetTooltip, null);

        for (int i = _initializedMyClanUserCount, count = _myClanWarBattleUsers.Count; i < count; ++i) {
            _myClanProfiles[i].Initialize(_myClanWarBattleUsers[i], ProfileClicked, _myClanInfo.m_clanId);
            ++_initializedMyClanUserCount;
        }

        if (ClanWarProgressType.ReadyWar == _clanWarProgressType) {
            return;
        }
        for (int i = _initializedEnemyClanuserCount, count = _enemyClanWarBattleUsers.Count; i < count; ++i) {
            _enemyClanProfiles[i].Initialize(_enemyClanWarBattleUsers[i], ProfileClicked, _otherClanInfo.m_clanId);
            ++_initializedEnemyClanuserCount;
        }
    }

    private void UpdateMyClanProgress() {
        for (int i = 0, count = _initializedEnemyClanuserCount; i < count; ++i) {
            if (_enemyClanProfiles[i].Progress != ClanWarProfile.ProgressType.None) {
                ClanData clanData = PlayerManager.Instance.ClanData;
                for (int j = 0, countj = clanData._clanUserInfos.Count; j < countj; ++j) {
                    if (PlayerManager.Instance.UserSeq == _enemyClanProfiles[i].AttackerID) {
                        _enemyClanProfiles[i].UpdateProgress(0, 0);
                        continue;
                    }
                    if (clanData._clanUserInfos[j].m_userseq == _enemyClanProfiles[i].AttackerID) {
                        if (clanData._clanUserInfos[j].m_login_status != (int)LoginStateType.LogIn) {
                            _enemyClanProfiles[j].UpdateProgress(0, 0);
                            continue;
                        }
                    }
                }
            }
        }
    }

    private void OnClick_AssetTooltip(Transform targetTF, int typeIndex) {
        PopupManager.Instance.ShowTooltip(targetTF, TItems.Instance.Find(typeIndex).LangTypeName, TItems.Instance.Find(typeIndex).LangTypeDesc);
    }

    private void ProfileClicked(GameObject rootObject) {
        _rootObject = rootObject;

        float targetX = _rootObject.transform.parent.parent.position.x;
        float targetY = _rootObject.transform.parent.parent.position.y;

        if (targetX > 0) {
            float i = _warMap.horizontalNormalizedPosition;
            while (i < 1 && 0 < _rootObject.transform.parent.parent.position.x) {
                _warMap.horizontalNormalizedPosition = i;
                i += 0.001f;
            }
        }
        else {
            float i = _warMap.horizontalNormalizedPosition;
            while (i > 0 && 0 > _rootObject.transform.parent.parent.position.x) {
                _warMap.horizontalNormalizedPosition = i;
                i -= 0.001f;
            }
        }

        if (targetY > 0) {
            float i = _warMap.verticalNormalizedPosition;
            while (i < 1 && 0 < _rootObject.transform.parent.parent.position.y) {
                _warMap.verticalNormalizedPosition = i;
                i += 0.001f;
            }
        }
        else {
            float i = _warMap.verticalNormalizedPosition;
            while (i > 0 && 0 > _rootObject.transform.parent.parent.position.y) {
                _warMap.verticalNormalizedPosition = i;
                i -= 0.001f;
            }
        }
    }

    private string GetClanMessage(MClanChatInfo chatInfo) {
        string message;

        if (IsProfileTypeMessage(chatInfo.m_type)) {
            message = string.Format("<color=#00ffffff>[{0}]</color> {1}", chatInfo.m_to_nickname, chatInfo.m_message);
        }
        else {
            TClanSystemString systemStr = TClanSystemStrings.Instance.Find(chatInfo.m_type);
            string messageKey = systemStr._messageKey;
            message = TStrings.Instance.FindString(messageKey);

            if (systemStr._bracketNumberType == 0) {
            }
            else if (systemStr._bracketNumberType == 1) {
                message = string.Format(message, chatInfo.m_to_nickname);
            }
            else if (systemStr._bracketNumberType == 2) {
                if (chatInfo.m_condition == 0) {
                    message = string.Format(message, chatInfo.m_from_nickname, chatInfo.m_to_nickname);
                }
                else {
                    message = string.Format(message, chatInfo.m_to_nickname, chatInfo.m_condition);
                }
            }

            message = string.Format("<color=yellow>[{0}]</color> {1}", TStrings.Instance.FindString("Clan_29211"), message);
        }

        return message;
    }

    private bool IsProfileTypeMessage(int type) {
        if (type == 0) {
            return true;
        }

        return false;
    }

    private void ShowNoticeAnim() {
        PopupManager.Instance.ShowAnimNoticePopup(_noticeAnimTF, "asdf");

        if (0 != GameDataManager.Instance.ClanWarPoint) {
            PopupManager.Instance.ShowAnimNoticePopup(_noticeAnimTF, string.Format(TStrings.Instance.FindString("Clan_29177"), GameDataManager.Instance.ClanWarPoint.ToString()));
            GameDataManager.Instance.ClanWarPoint = 0;
            return;
        }

        if (ClanWarProgressType.ReadyWar == _clanWarProgressType || ClanWarProgressType.DeckCopying == _clanWarProgressType) {
            PopupManager.Instance.ShowAnimNoticePopup(_noticeAnimTF, TStrings.Instance.FindString("Clan_29175"));
        }
        else if (ClanWarProgressType.War == _clanWarProgressType) {
            PopupManager.Instance.ShowAnimNoticePopup(_noticeAnimTF, TStrings.Instance.FindString("Clan_29179"));
        }
        else if (ClanWarProgressType.AfterWar == _clanWarProgressType) {
            PopupManager.Instance.ShowAnimNoticePopup(_noticeAnimTF, TStrings.Instance.FindString("Clan_29176"));
        }
    }

    private bool CheckClickOutSide() {
        if (!Input.GetMouseButton(0) || null == _rootObject) {
            return false;
        }

        return (!RectTransformUtility.RectangleContainsScreenPoint(_rootObject.transform.GetComponent<RectTransform>(), Input.mousePosition, Camera.main));
    }

    private void SetClanFlag() {
        SetClanPattern(ref _ourClanFlagImage, TClanFlags.Instance.Find(_clanData._pattern)._pattern);
        SetClanSymbol(ref _ourClanSimbolImage, TClanFlags.Instance.Find(_clanData._symbol)._symbol);

        if (0 != _otherClanInfo.m_clanId) {
            SetClanPattern(ref _otherClanFlagImage, TClanFlags.Instance.Find(_otherClanInfo.m_pattern)._pattern);
            SetClanSymbol(ref _otherClanSimbolImage, TClanFlags.Instance.Find(_otherClanInfo.m_mark)._symbol);
        }
    }

    private void SetWarPointSlider() {
        int nowValue = GameDataManager.Instance.ClanWarInfo.getPointInfo.m_total_point;
        int maxValue = GameDataManager.Instance.ClanWarInfo.getPointInfo.m_total_point + GameDataManager.Instance.ClanWarInfo.getPointInfo.m_vs_total_point;

        if (0 == maxValue) {
            _slider.value = 0.5f;
            return;
        }

        float amount = (float)nowValue / (float)maxValue;

        if (amount < 0.1f) {
            amount = 0.1f;
        }
        else if (amount > 0.9f) {
            amount = 0.9f;
        }

        _slider.value = amount;
    }

    private void SetButtonUI() {
        _bottomUIButtons[(int)ButtonType.TeamEdit].SetActive(ClanWarProgressType.ReadyWar == _clanWarProgressType);
        _bottomUIButtons[(int)ButtonType.FindOFF].SetActive(ClanWarProgressType.AfterWar == _clanWarProgressType || 0 == PlayerManager.Instance.GetAssetAmount(AssetType.ClanWarEnergy) || ClanWarProgressType.DeckCopying == _clanWarProgressType);
    }

    private void StopUpdateClanWarTime() {
        if (null == _clanWarRestTimeCoroutine) {
            return;
        }

        StopCoroutine(_clanWarRestTimeCoroutine);
        _clanWarRestTimeCoroutine = null;
    }

    private void StartUpdateClanWarTime() {
        StopUpdateClanWarTime();

        _clanWarRestTimeCoroutine = StartCoroutine(UpdateClanWarTime());
    }

    private IEnumerator UpdateClanWarTime() {
        var updateTerm = new WaitForSeconds(1f);

        while (true) {
            UpdateTime();

            yield return updateTerm;
        }
    }

    private void UpdateTime() {
        if (ClanWarProgressType.ReadyWar == _clanWarProgressType || ClanWarProgressType.DeckCopying == _clanWarProgressType) {
            TimeSpan timespan = Common.ConvertJavaMillisecondToDateTimeUTC(GameDataManager.Instance.ClanWarInfo.warProgress.m_war_ready_progress_time) - System.DateTime.UtcNow;
            if (timespan.TotalMilliseconds < 0) {
                WebHttp.Instance.RequestClanWarBattleFieldInfo(RefreshUI);
            }
            SetText((int)TextType.RestTime, string.Format("{0}:{1}:{2}", timespan.Hours, timespan.Minutes, timespan.Seconds));
        }
        else if (ClanWarProgressType.War == _clanWarProgressType) {
            TimeSpan timespan = Common.ConvertJavaMillisecondToDateTimeUTC(GameDataManager.Instance.ClanWarInfo.warProgress.m_war_end_time) - System.DateTime.UtcNow;
            if (timespan.TotalMilliseconds < 0) {
                WebHttp.Instance.RequestClanWarBattleFieldInfo(RefreshUI);
            }
            SetText((int)TextType.RestTime, string.Format("{0}:{1}:{2}", timespan.Hours, timespan.Minutes, timespan.Seconds));
        }
        else {
            return;
        }
    }

    private void UpdateProgressImmediately(int vsClanID, int vsUserUID, int progress, int attackerUID) {
        if (vsClanID == PlayerManager.Instance.ClanID) { // 아군이 공격받음
            for (int i = 0, count = _myClanWarBattleUsers.Count; i < count; ++i) {
                if (_myClanWarBattleUsers[i].m_userseq == vsUserUID) {
                    string attackerNick = string.Empty;

                    for (int k = 0, countk = _initializedEnemyClanuserCount; k < countk; ++k) {
                        if (_enemyClanWarBattleUsers[k].m_userseq == attackerUID) {
                            attackerNick = _enemyClanWarBattleUsers[k].m_nickname;
                        }
                    }

                    _myClanProfiles[i].UpdateProgress(progress, attackerUID, attackerNick);
                }
            }
        }
        else { // 적군이 공격받음
            for (int i = 0, count = _enemyClanWarBattleUsers.Count; i < count; ++i) {
                if (_enemyClanWarBattleUsers[i].m_userseq == vsUserUID) {
                    string attackerNick = string.Empty;

                    for (int k = 0, countk = _initializedMyClanUserCount; k < countk; ++k) {
                        if (_myClanWarBattleUsers[k].m_userseq == attackerUID) {
                            attackerNick = _myClanWarBattleUsers[k].m_nickname;
                        }
                    }

                    _enemyClanProfiles[i].UpdateProgress(progress, attackerUID, attackerNick);
                }
            }
        }
    }

    private void StartUpdateProfileProgress(int attackerUID, int vsClanID, int vsUserUID) {
        StopUpdateProfileProgress();

        _clanWarProfileUpdateCoroutine = StartCoroutine(UpdateProfileProgress(attackerUID, vsClanID, vsUserUID));
    }

    private void StopUpdateProfileProgress() {
        if (null == _clanWarProfileUpdateCoroutine) {
            return;
        }

        StopCoroutine(_clanWarProfileUpdateCoroutine);
        _clanWarProfileUpdateCoroutine = null;
    }

    private IEnumerator UpdateProfileProgress(int attackerUID, int vsClanID, int vsUserUID) {
        Transform attackerTF = transform;
        Transform defenderTF = transform;
        List<Transform> moveVectors = new();

        if (vsClanID == _myClanInfo.m_clanId) { // 공격받을 때
            for (int i = 0, count = _enemyClanWarBattleUsers.Count; i < count; ++i) {
                if (_enemyClanWarBattleUsers[i].m_userseq == attackerUID) {
                    if (i - 4 % 5 == 0) { // 맨 우측라인
                        moveVectors.Add(_moveVectors[(int)MoveVectorType.RightUp].transform);
                        moveVectors.Add(_moveVectors[(int)MoveVectorType.RightUnder].transform);
                    }
                    else if (i - 3 % 5 == 0) { // 맨 좌측라인
                        moveVectors.Add(_moveVectors[(int)MoveVectorType.LeftUp].transform);
                        moveVectors.Add(_moveVectors[(int)MoveVectorType.LeftUnder].transform);
                    }
                    else { // 나머지 가운데
                        moveVectors.Add(_moveVectors[(int)MoveVectorType.CenterUp].transform);
                        moveVectors.Add(_moveVectors[(int)MoveVectorType.CenterUnder].transform);
                    }
                    attackerTF = _enemyClanProfiles[i].transform;
                    string attackernick = _enemyClanWarBattleUsers[i].m_nickname;

                    _clanWarMoveBoxes[0].SetBox(vsClanID == _myClanInfo.m_clanId, _enemyClanWarBattleUsers[i].m_nickname,
                                                _enemyClanWarBattleUsers[i].m_avater, _enemyClanWarBattleUsers[i].m_avaterbg, _enemyClanWarBattleUsers[i].m_avaterpin);

                    for (int j = 0, countj = _myClanWarBattleUsers.Count; j < countj; ++j) {
                        if (_myClanWarBattleUsers[j].m_userseq == vsUserUID) {
                            _myClanProfiles[j].UpdateProgress(2, attackerUID, attackernick);
                            defenderTF = _myClanProfiles[j].transform;
                        }
                    }
                }
            }
        }
        else { // 공격할 때 
            for (int i = 0, count = _myClanWarBattleUsers.Count; i < count; ++i) {
                if (_myClanWarBattleUsers[i].m_userseq == attackerUID) {
                    if (i - 4 % 5 == 0) { // 맨 우측라인
                        moveVectors.Add(_moveVectors[(int)MoveVectorType.RightUnder].transform);
                        moveVectors.Add(_moveVectors[(int)MoveVectorType.RightUp].transform);
                    }
                    else if (i - 3 % 5 == 0) { // 맨 좌측라인
                        moveVectors.Add(_moveVectors[(int)MoveVectorType.LeftUnder].transform);
                        moveVectors.Add(_moveVectors[(int)MoveVectorType.LeftUp].transform);
                    }
                    else { // 나머지 가운데
                        moveVectors.Add(_moveVectors[(int)MoveVectorType.CenterUnder].transform);
                        moveVectors.Add(_moveVectors[(int)MoveVectorType.CenterUp].transform);
                    }
                    attackerTF = _myClanProfiles[i].transform;
                    string attackernick = _myClanWarBattleUsers[i].m_nickname;

                    _clanWarMoveBoxes[0].SetBox(vsClanID == _myClanInfo.m_clanId, _myClanWarBattleUsers[i].m_nickname,
                                                _myClanWarBattleUsers[i].m_avater, _myClanWarBattleUsers[i].m_avaterbg, _myClanWarBattleUsers[i].m_avaterpin);

                    for (int j = 0, countj = _enemyClanWarBattleUsers.Count; j < countj; ++j) {
                        if (_enemyClanWarBattleUsers[j].m_userseq == vsUserUID) {
                            _enemyClanProfiles[j].UpdateProgress(2, attackerUID, attackernick);
                            defenderTF = _enemyClanProfiles[j].transform;
                        }
                    }
                }
            }
        }

        _clanWarMoveBoxes[0].Move(attackerTF, moveVectors[0], moveVectors[1].transform, defenderTF);

        while (_clanWarMoveBoxes[0].gameObject.activeSelf == true && attackerUID == PlayerManager.Instance.UserSeq) {
            float targetX = _clanWarMoveBoxes[0].transform.position.x;
            float targetY = _clanWarMoveBoxes[0].transform.position.y;

            if (targetX > 0) {
                float i = _warMap.horizontalNormalizedPosition;
                while (i < 1 && 0 < _clanWarMoveBoxes[0].transform.position.x) {
                    _warMap.horizontalNormalizedPosition = i;
                    i += 0.0001f;
                }
            }
            else {
                float i = _warMap.horizontalNormalizedPosition;
                while (i > 0 && 0 > _clanWarMoveBoxes[0].transform.position.x) {
                    _warMap.horizontalNormalizedPosition = i;
                    i -= 0.0001f;
                }
            }

            if (targetY > 0) {
                float i = _warMap.verticalNormalizedPosition;
                while (i < 1 && 0 < _clanWarMoveBoxes[0].transform.position.y) {
                    _warMap.verticalNormalizedPosition = i;
                    i += 0.0001f;
                }
            }
            else {
                float i = _warMap.verticalNormalizedPosition;
                while (i > 0 && 0 > _clanWarMoveBoxes[0].transform.position.y) {
                    _warMap.verticalNormalizedPosition = i;
                    i -= 0.0001f;
                }
            }
            yield return null;
        }

        yield return null;
    }

    private void StartUpdateEnemyAlive() {
        StopUpdateEnemyAlive();

        _clanWarEnemyAliveCoroutine = StartCoroutine(UpdateEnemyAlive());
    }

    private void StopUpdateEnemyAlive() {
        if (null == _clanWarEnemyAliveCoroutine) {
            return;
        }

        StopCoroutine(_clanWarEnemyAliveCoroutine);
        _clanWarEnemyAliveCoroutine = null;
    }

    private IEnumerator UpdateEnemyAlive() {
        float requesttime = 20f;

        while (true) {
            TCPSocketManager.Instance.RequestClanWarEnemyOnline(_otherClanInfo.m_clanId);

            if (IsAllEnemyDead()) {
                TCPSocketManager.Instance.RequestClanWarRevival(_otherClanInfo.m_clanId);
            }
            else if (IsAllAllyDead()) {
                TCPSocketManager.Instance.RequestClanWarRevival(_myClanInfo.m_clanId);
            }

            yield return new WaitForSeconds(requesttime);
        }
    }

    private bool IsAllEnemyDead() {
        for (int i = 0, count = _enemyClanWarBattleUsers.Count; i < count; ++i) {
            if (Common.ConvertJavaMillisecondToDateTimeUTC(_enemyClanWarBattleUsers[i].m_revival_time) < DateTime.UtcNow) {
                return false;
            }
        }
        return true;
    }

    private bool IsAllAllyDead() {
        for (int i = 0, count = _myClanWarBattleUsers.Count; i < count; ++i) {
            if (Common.ConvertJavaMillisecondToDateTimeUTC(_myClanWarBattleUsers[i].m_revival_time) < DateTime.UtcNow) {
                return false;
            }
        }
        return true;
    }

    private void CheckZoom() {
        if (Input.touchCount == 2) {
            Vector2 vecPreTouchPos0 = Input.touches[0].position - Input.touches[0].deltaPosition;
            Vector2 vecPreTouchPos1 = Input.touches[1].position - Input.touches[1].deltaPosition;

            // 이전 두 터치의 차이 
            float fPreDis = (vecPreTouchPos0 - vecPreTouchPos1).magnitude;
            // 현재 두 터치의 차이
            float fToucDis = (Input.touches[0].position - Input.touches[1].position).magnitude;


            // 이전 두 터치의 거리와 지금 두 터치의 거리의 차이
            float fDis = fPreDis - fToucDis;


            if (fDis < 0) {
                if (_warMap.content.localScale.x > _mapContentsMaxSize) {
                    return;
                }
            }
            else {
                if (_warMap.content.localScale.x < _mapContentsMinSize) {
                    return;
                }
            }
            _warMap.content.localScale -= new Vector3(fDis * 0.001f, fDis * 0.001f);
        }
    }

    #endregion //private funcs
}
