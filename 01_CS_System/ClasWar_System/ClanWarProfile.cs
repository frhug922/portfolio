using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanWarProfile : LobbyUIBase
{
    #region type def

    public enum ProgressType
    {
        None = 0,
        Reserve = 1,
        Attack = 2,
    }

    private enum TextType
    {
        NickName,
        AllianceAmount,
        TeamCombatPower,
        ResurrectionTime,
        AttackerNick,
        Reserve,
        ReserveNick,
        Attack,
        Edit,
        Info,
    }

    private enum ProfileType
    {
        Mine,
        MyClan,
        Enemy,
    }

    private enum ButtonType
    {
        Attack,
        Edit,
        Info,
    }

    private enum SpriteType
    {
        Online,
        Offline,
    }

    #endregion // type def





    #region serialized fields

    [SerializeField] private ProfileType _profileType;
    [SerializeField] private GameObject _myProfileBox;
    [SerializeField] private List<GameObject> _buttons;
    [SerializeField] private GameObject _choiceThings;
    [SerializeField] private Image _connectIcon;
    [SerializeField] private List<Sprite> _connectIcons;
    [SerializeField] private GameObject _reserveRoot;
    [SerializeField] private GameObject _battleRoot;
    [SerializeField] private List<GameObject> _enemyObjects;

    [Header("For Hero")]
    [SerializeField] private List<SimpleHeroInfo> _heros;

    #endregion //serialized fields





    #region private fields

    private MClanWarBattleUser _myData;
    private System.Action<GameObject> _clickCallback;
    private int _clanUID;
    private int _teamCombatPower;
    private Coroutine _profileResurrectionTimeCoroutine;
    private ProgressType _progress = ProgressType.None;
    private int _attackerID;

    #endregion //private fields




    #region 

    public ProgressType Progress { get { return _progress; } }
    public int AttackerID { get { return _attackerID; } }

    #endregion





    #region public funcs

    public void Initialize(MClanWarBattleUser userdata, System.Action<GameObject> clickCallback, int clanUID) {
        _myData = userdata;
        _clickCallback = clickCallback;
        _clanUID = clanUID;

        if (userdata.m_userseq == PlayerManager.Instance.UserSeq) {
            _myProfileBox.SetActive(true);
            _profileType = ProfileType.Mine;
        }

        if (ProfileType.Enemy == _profileType) {
            for (int i = 0, count = _enemyObjects.Count; i < count; ++i) {
                _enemyObjects[i].SetActive(true);
            }
        }

        SetTeamCombatPower();
    }

    public void SetShow(MClanWarBattleUser userdata) {
        _myData = userdata;

        base.SetShow();

        RefreshUI();
        StartUpdateRessurectionTime();
    }

    public override void RefreshUI() {
        UpdateTextsUI();
        UpdateContentsUI();
    }

    public override void UpdateTextsUI() {
        SetText((int)TextType.NickName, _myData.m_nickname);
        SetText((int)TextType.AllianceAmount, _myData.m_war_energy.ToString());
        SetText((int)TextType.TeamCombatPower, string.Format("{0} : {1}", TStrings.Instance.FindString("Clan_29149"), _teamCombatPower.ToString()));
        SetText((int)TextType.Reserve, TStrings.Instance.FindString("Clan_29146"));
        SetText((int)TextType.Attack, TStrings.Instance.FindString("PVP_6030"));
        SetText((int)TextType.Edit, TStrings.Instance.FindString("Clan_29133"));
        SetText((int)TextType.Info, TStrings.Instance.FindString("Clan_29134"));
    }

    public void OnClick_Profile() {
        _choiceThings.SetActive(true);

        List<MClanWarBattleUser> attackers = GameDataManager.Instance.GetMyClanWarUserList().m_items;
        bool isClanWarAttend = false;
        for (int i = 0, count = attackers.Count; i < count; ++i) {
            if (attackers[i].m_userseq == PlayerManager.Instance.UserSeq) {//전쟁에 참가중인지 확인
                isClanWarAttend = true;
                break;
            }
        }

        if (!isClanWarAttend) {
            if (_profileType == ProfileType.MyClan) {
                _buttons[(int)ButtonType.Info].SetActive(true);
            }
            else if (_profileType == ProfileType.Enemy) {
                _buttons[(int)ButtonType.Info].SetActive(true);
                _buttons[(int)ButtonType.Attack].SetActive(false);
            }
            return;
        }

        if (_profileType == ProfileType.MyClan) {
            _buttons[(int)ButtonType.Info].SetActive(true);
        }
        else if (_profileType == ProfileType.Enemy) {
            _buttons[(int)ButtonType.Info].SetActive(true);
            _buttons[(int)ButtonType.Attack].SetActive(ClanWarProgressType.War == GameDataManager.Instance.GetClanWarProgress() && _progress == ProgressType.None && Common.ConvertJavaMillisecondToDateTimeUTC(_myData.m_revival_time) < DateTime.UtcNow);
        }
        else if (_profileType == ProfileType.Mine) {
            _buttons[(int)ButtonType.Info].SetActive(true);
            _buttons[(int)ButtonType.Edit].SetActive(ClanWarProgressType.ReadyWar == GameDataManager.Instance.GetClanWarProgress());
        }

        if (_clickCallback == null) {
            return;
        }
        _clickCallback(_choiceThings);
    }

    public void OnClick_Info() {
        WebHttp.Instance.RequestUserInfo(_myData.m_userseq, () => {
            GameDataManager.Instance.SetOpponentPlayerInfo(_myData);
            GameDataManager.Instance.SetClanVsUserData(_myData);
            GameDataManager.Instance.InfoType = PlayerInfoType.ClanWar;
            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.OpponentPlayerInfo);
        });
    }

    public void OnClick_Attack() {
        if (_progress != ProgressType.None) {
            ClickCanceled();
            return;
        }

        TCPSocketManager.Instance.RequestClanWarProgress(_clanUID, _myData.m_userseq, (int)ProgressType.Reserve);
        WebHttp.Instance.RequestClanWarUsedCardList(() => {
            SystemManager.Instance.SetBattleType(BattleType.ClanWar);
            LobbySceneUIManager.Instance.ClanWarDeckEditType = DeckType.Attack;
            GameDataManager.Instance.SetClanVsUserData(_myData);
            GameDataManager.Instance.SetClanVsDeck(_myData.m_items);
            LobbySceneUIManager.Instance.IsClanWarAttack = true;
            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.TeamEdit);
        });
    }

    public void OnClick_Edit() {
        SystemManager.Instance.SetBattleType(BattleType.ClanWar);
        LobbySceneUIManager.Instance.ClanWarDeckEditType = DeckType.Defence;
        GameDataManager.Instance.SetClanVsUserData(_myData);

        LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.TeamEdit);
    }

    public void ClickCanceled() {
        _choiceThings.SetActive(false);
        for (int i = 0, count = _buttons.Count; i < count; ++i) {
            _buttons[i].SetActive(false);
        }
    }

    public void UpdateConnectEnemy(List<int> onlineUIDs) {
        if (onlineUIDs.Contains(_myData.m_userseq)) {
            _connectIcon.sprite = _connectIcons[(int)SpriteType.Online];
        }
        else {
            _connectIcon.sprite = _connectIcons[(int)SpriteType.Offline];
        }
    }

    public void UpdateProgress(int progress, int attackerUID, string attackerNick = null) {
        _progress = (ProgressType)progress;
        _attackerID = attackerUID;
        if ((int)ProgressType.None == progress) {
            _reserveRoot.SetActive(false);
            _battleRoot.SetActive(false);
        }
        else if ((int)ProgressType.Reserve == progress) {
            _reserveRoot.SetActive(true);
            _battleRoot.SetActive(false);
            if (null != attackerNick) {
                SetText((int)TextType.ReserveNick, attackerNick);
            }
        }
        else if ((int)ProgressType.Attack == progress) {
            _battleRoot.SetActive(true);
            _reserveRoot.SetActive(false);
            if (null != attackerNick) {
                SetText((int)TextType.AttackerNick, attackerNick);
            }
        }
    }

    #endregion // public funcs





    #region private funcs

    private void UpdateContentsUI() {
        UpdateConnect();
        SetHeroDatas();
    }

    private void UpdateConnect() {
        if (ProfileType.Mine == _profileType) {
            _connectIcon.sprite = _connectIcons[(int)SpriteType.Online];
        }
        else if (ProfileType.MyClan == _profileType) {
            ClanData clanData = PlayerManager.Instance.ClanData;

            for (int i = 0, count = clanData._clanUserInfos.Count; i < count; ++i) {
                if (clanData._clanUserInfos[i].m_userseq == _myData.m_userseq) {
                    if (clanData._clanUserInfos[i].m_login_status == (int)LoginStateType.LogIn) {
                        _connectIcon.sprite = _connectIcons[(int)SpriteType.Online];
                    }
                    else {
                        _connectIcon.sprite = _connectIcons[(int)SpriteType.Offline];
                    }
                }
            }
        }
        return;
    }

    private void SetHeroDatas() {
        if (Common.ConvertJavaMillisecondToDateTimeUTC(_myData.m_revival_time) > DateTime.UtcNow) {
            for (int i = 0, count = _myData.vsCards.Count; i < count; ++i) {
                _heros[i].SetHide();
            }
            return;
        }

        for (int i = 0, count = _myData.vsCards.Count; i < count; ++i) {
            for (int j = 0, countj = _myData.m_items.Count; j < countj; ++j) {
                if (_myData.vsCards[i].m_id == _myData.m_items[j].m_cardId && i + 1 == _myData.m_items[j].m_x) {
                    HeroData heroData = new(_myData.vsCards[i]);
                    _heros[_myData.m_items[j].m_x - 1].SetShow(heroData, _myData.m_items[i].m_cardHp);
                }
            }
        }
    }

    private void SetTeamCombatPower() {
        PvPOpponentData pvPOpponentData = new PvPOpponentData(_myData);
        _teamCombatPower = pvPOpponentData._teamCombatPower;
    }

    private void StartUpdateRessurectionTime() {
        StopUpdateRessurectionTime();

        _profileResurrectionTimeCoroutine = StartCoroutine(UpdateProfileProgress());
    }

    private void StopUpdateRessurectionTime() {
        if (null == _profileResurrectionTimeCoroutine) {
            return;
        }

        StopCoroutine(_profileResurrectionTimeCoroutine);
        _profileResurrectionTimeCoroutine = null;
    }

    private IEnumerator UpdateProfileProgress() {
        var updateTerm = new WaitForSeconds(1f);

        while (Common.ConvertJavaMillisecondToDateTimeUTC(_myData.m_revival_time) > DateTime.UtcNow) {
            UpdateTime();

            yield return updateTerm;
        }

        HideText((int)TextType.ResurrectionTime, true);
        UpdateContentsUI();
        yield break;
    }

    private void UpdateTime() {
        TimeSpan timespan = Common.ConvertJavaMillisecondToDateTimeUTC(_myData.m_revival_time) - System.DateTime.UtcNow;
        SetText((int)TextType.ResurrectionTime, string.Format(TStrings.Instance.FindString("SHOP_9035"), timespan.Hours, timespan.Minutes), true);
    }

    #endregion //private funcs
}
