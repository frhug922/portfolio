using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


public class WebHttp : MonoBehaviour
{
    #region private variables

    private static WebHttp _instance = null;

    private string _url;

#if SHOW_LOG
    private DateTime _sendStartTime;
#endif // SHOW_LOG

    #endregion // private variables




    #region properties

    public static WebHttp Instance { get { return _instance; } }

    #endregion // properties




    #region mono funcs

    private void Awake() {
        _instance = this;
    }

    private void Start() {
        SystemManager.Instance.MakeDontDestroy(this.gameObject);

#if SHOW_LOG
        Debug.Log("WebHttp : Start!\n");
#endif // SHOW_LOG
    }

    //// Update is called once per frame
    //void Update() {

    //}

    #endregion // mono funcs




    #region public funcs

    // Start is called before the first frame update
    public void Init(string addr, string port) {
        _url = string.Format("{0}:{1}", addr, port);

#if SHOW_LOG
        Debug.LogFormat("\t<color=white>WebHttp : Init..!!</color>\t[ {0} ]\n", _url);
#endif // SHOW_LOG

        SystemManager.Instance.HttpStatus = HttpStatus.Initialized;
    }

    public void RequestServerCheck(Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestServerCheck..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ServerCheckRequest() {
        },
        (string response) => {

            //messagepack test............
            //  byte[] byteArray = Encoding.UTF8.GetBytes(response);
            //  var stream = new MemoryStream(byteArray);
            // var obj = MsgPack.Deserialize<ServerCheckResponse>(stream);

            var obj = JsonConvert.DeserializeObject<ServerCheckResponse>(response);
            ResponseServerCheck(obj, completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestNewRegister(string nickName, LoginPlatformType platformType, string account, Action<string> completeCallback) {
#if SHOW_LOG
        Debug.LogFormat("\t<color=white>WebHttp : RequestNewRegister..!! nickName : {0}\t platformType {1}</color>\n", nickName, (int)platformType);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new RegisterNewRequest() {
            account = account, // server 에서 google 계정 등록시 사용되어서 필요하다고 함.
            nickName = nickName,
            type = (int)platformType,
            //language = 0,
        },
        (string response) => {
            ResponseNewRegister(JsonConvert.DeserializeObject<RegisterNewResponse>(response), completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestNewLogin(LoginPlatformType type, string accountName, Action registerCallback, Action<string, int, int> completeCallback) {
#if SHOW_LOG
        Debug.LogFormat("\t<color=white>WebHttp : RequestNewLogin..!! accountName : {0}\t type : {1}</color>\n", accountName, type);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new LoginNewRequest() {
            type = (int)type,
            account = accountName,
        },
        (string response) => {
            ResponseNewLogin(JsonConvert.DeserializeObject<LoginNewResponse>(response), registerCallback, completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestLoadGame() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestLoadGame..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new LoadgameRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseLoadGame(JsonConvert.DeserializeObject<LoadgameResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestTutorialInfos() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestTutorialInfos..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TutorialInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseTutorialInfos(JsonConvert.DeserializeObject<TutorialInfoResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestTutorialSave(TutorialSaveType saveType, int tutorialID, Action completeCallback) {
#if SHOW_LOG
        if (TutorialSaveType.Chapter == saveType) {
            Debug.LogFormat("\t<color=white>WebHttp : RequestTutorialSave..!! \t [ tutorialChapter : {0}({1}) ]</color>\n",
                tutorialID, (TutorialChapterType)tutorialID);
        }
        else if (TutorialSaveType.LobbyBuilding == saveType) {
            Debug.LogFormat("\t<color=white>WebHttp : RequestTutorialSave..!! \t [ tutorialBuilding : {0}({1}) ]</color>\n",
                tutorialID, (BuildingType)tutorialID);
        }
        else if (TutorialSaveType.Skip == saveType) {
            Debug.Log("\t<color=white>WebHttp : RequestTutorialSave..!! \t [ skip ]</color>\n");
        }
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TutorialConfirmRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            tutorialId = (int)tutorialID,
            type = (int)saveType,
        },
        (string response) => {
            ResponseTutorialSave(JsonConvert.DeserializeObject<TutorialConfirmResponse>(response), completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestLobbyInfo() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestLobbyInfo..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new LobbyInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseLobbyInfo(JsonConvert.DeserializeObject<LobbyInfoResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestStageList(int themaID, bool waitLobbyInfo) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestStageList..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new StageListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            areaId = themaID,
        },
        (string response) => {
            ResponseStageList(JsonConvert.DeserializeObject<StageListResponse>(response), waitLobbyInfo);
            //LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestPvEStart(int stageID) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestPvEStart..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new PveStartRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            stageId = stageID,
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponsePveStart(JsonConvert.DeserializeObject<PveStartResponse>(response));
        }));
    }

    public void RequestPveEnd(ResultType resultType) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestPveEnd..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new PveEndRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            stageId = PlayerManager.Instance.CurrPlayingStageID,
            gameResult = (int)resultType,       //0:lose, 1:win, 2:draw
            playTime = (int)(DateTime.Now - BattleManager.Instance.StartTime).TotalSeconds,  //플레이 시간(초)
            myKillCnt = BattleManager.Instance.AlliesDiedCount,  //나의 병사 죽은 갯수
            mobBossKillCnt = BattleManager.Instance.KilledBossCount,  //대장 죽인 갯수
            mobKillCnt = BattleManager.Instance.KilledEnemyCount,  // 졸병 죽인 갯수
            comboMaxCnt = BattleManager.Instance.MaxComboCount,  //최대 콤보 갯수 
            turnCnt = BattleManager.Instance.TurnCount,  //턴 횟수
            blockBrokenCnt = BattleManager.Instance.MatchedBlockCount,  //블록 파괴된 갯수
            enemyKillCounts = GameDataManager.Instance.EnemyKillCounts,   //스테이지에 나온 적군의 정보(적군 이미지 파일명, 적군 속성, 적군 처치수)
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponsePveEnd(JsonConvert.DeserializeObject<PveEndResponse>(response));
        }));
    }

    public void RequestPvERestart(int stageID) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestPvERestart..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new PveReStartRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            stageId = stageID,
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponsePvERestart(JsonConvert.DeserializeObject<PveReStartResponse>(response));
        }));
    }

    public void RequestPvETicket(int stageID, int useTicketCount, List<MEnemyKillCount> enemyInfo, int bossKillCount, Action<PveTicketResponse> completeCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestPvETicket..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new PveTicketRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            stageId = stageID,
            mobBossKillCnt = bossKillCount,  //대장 죽인 갯수
            enemyKillCounts = enemyInfo, //스테이지에 나온 적군의 정보(적군 이미지 파일명, 적군 속성, 적군 처치수)
            useTicketCount = useTicketCount, // 전투품 티켓 사용 갯수		
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponsePvETicket(JsonConvert.DeserializeObject<PveTicketResponse>(response), completeCallback);
        }));
    }

    public void RequestBattleItemUse(int deckNumber, int itemID, Action<int> completeCallback) {
#if SHOW_LOG
        Debug.LogFormat("\t<color=white>WebHttp : RequestBattleItemUse..!! \t [ deckNum : {0} / itemID : {1} ]</color>\n", deckNumber, itemID);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new BattleItemUseRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            deckNum = deckNumber,
            itemId = itemID,
        },
        (string response) => {
            ResponseBattleItemUse(JsonConvert.DeserializeObject<BattleItemUseResponse>(response), completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestCardChange(int deckNUM, List<MDeckCard> cards, Action completeCallback, bool updateTeamEditUI) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestCardChange..!!</color>\n");
#endif // SHOW_LOG

        //LoadAnimManager.Instance.SetAnimType(LoadAnimType.CircleType);

        StartCoroutine(SendRequest(new CardChangeRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            deckNum = deckNUM,
            deckCards = cards,
        },
        (string response) => {
            ResponseCardChange(JsonConvert.DeserializeObject<CardChangeResponse>(response), updateTeamEditUI);
            //LoadAnimManager.Instance.Hide(LoadAnimType.CircleType);
            if (null != completeCallback) {
                completeCallback();
            }
        }));
    }

    public void RequestBattleItemChange(int deckNum, List<MDeckItem> items, Action completeCallback, bool updateTeamEditUI) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestBattleItemChange..!!</color>\n");
#endif // SHOW_LOG

        //LoadAnimManager.Instance.SetAnimType(LoadAnimType.CircleType);

        StartCoroutine(SendRequest(new BattleItemChangeRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            deckNum = deckNum,
            deckItems = items,
        },
        (string response) => {
            ResponseBattleItemChange(JsonConvert.DeserializeObject<BattleItemChangeResponse>(response), updateTeamEditUI);
            //LoadAnimManager.Instance.Hide(LoadAnimType.CircleType);
            if (null != completeCallback) {
                completeCallback();
            }
        }));
    }

    public void RequestDeckChange(DeckType deckType, int deckNumber, Action completeCallback, bool updateTeamEditUI) {
#if SHOW_LOG
        Debug.LogFormat("\t<color=white>WebHttp : RequestDeckChange..!!</color>[ {0} ]\n", deckType);
#endif // SHOW_LOG

        //LoadAnimManager.Instance.SetAnimType(LoadAnimType.CircleType);

        StartCoroutine(SendRequest(new DeckChangeRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            type = (int)deckType - 1,  //0:공격덱, 1:방어덱
            deckNum = deckNumber,
        },
        (string response) => {
            ResponseDeckChange(JsonConvert.DeserializeObject<DeckChangeResponse>(response), deckType, updateTeamEditUI);
            //LoadAnimManager.Instance.Hide(LoadAnimType.CircleType);
            if (null != completeCallback) {
                completeCallback();
            }
        }));
    }

    public void RequestChangeDefenceDeckFormation(int deckNum, WavePositionType type, Action completeCallback, bool updateTeamEditUI) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestChangeDefenceDeckFormation..!!</color>\n");
#endif // SHOW_LOG

        //LoadAnimManager.Instance.SetAnimType(LoadAnimType.CircleType);

        StartCoroutine(SendRequest(new DeckFormationRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            defenseDeck = deckNum,  //방어덱 번호
            formationType = (int)type,
        },
        (string response) => {
            ResponseChangeDefenceDeckFormation(JsonConvert.DeserializeObject<DeckFormationResponse>(response), updateTeamEditUI);
            //LoadAnimManager.Instance.Hide(LoadAnimType.CircleType);
            if (null != completeCallback) {
                completeCallback();
            }
        }));
    }

    public void RequestCardUpgrade(CardUpgradeType upgradeType, int tarCardID, List<int> resCardIDs) {
#if SHOW_LOG
        HeroData heroData = PlayerManager.Instance.GetHeroDataByUniqueID(tarCardID);
        Debug.LogFormat("\t<color=white>[ SEND ] WebHttp : RequestCardUpgrade..!!\t[ Request {0}\t/ target Card UID : {1}\t/ Grade : {2}\t/ Awaken : {3}\t/ level : {4}\t/ exp : {5}\t SkillLevel : {6} ]</color>\n",
            upgradeType, tarCardID, heroData.CardData._grade, heroData._awakening, heroData._level, heroData._exp, heroData._skillLevel);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new CardUpgradeRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            upgradeType = (int)upgradeType,  // 0:레벨업, 1:각성 레벨업	2:스킬 레벨업
            tarCardId = tarCardID,
            resCardIds = resCardIDs,
        },
        (string response) => {
            ResponseCardUpgrade(JsonConvert.DeserializeObject<CardUpgradeResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestCardFavorite(int cardUID, bool targetFavorite, Action completeCallback) {
#if SHOW_LOG
        Debug.LogFormat("\t<color=white>WebHttp : RequestCardFavorite..!!\t\t[ cardID : {0} / targetFavorite : {1} ]</color>\n", cardUID, targetFavorite);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new CardLockRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            cardId = cardUID,
            locked = targetFavorite ? 1 : 0,  //0: 잠금 해제, 1: 잠금
        },
        (string response) => {
            ResponseCardFavorite(JsonConvert.DeserializeObject<CardLockResponse>(response), completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestDeckName(int deckNumber, string deckName, System.Action<string> completeCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestDeckName..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new DeckNameRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            deckNum = deckNumber,  //덱 번호
            teamName = deckName,
        },
        (string response) => {
            ResponseDeckName(JsonConvert.DeserializeObject<DeckNameResponse>(response), completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestMailList() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestMailList..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new MailListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseMailList(JsonConvert.DeserializeObject<MailListResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestMailConfirm(int mailID) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestMailConfirm..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new MailConfirmRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            mailId = mailID,
        },
        (string response) => {
            ResponseMailConfirm(JsonConvert.DeserializeObject<MailConfirmResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestMailDelete(int mailID) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestMailDelete..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new MailDeleteRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            mailId = mailID,
        },
        (string response) => {
            ResponseMailDelete(JsonConvert.DeserializeObject<MailDeleteResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestBoxOpen(int boxID, BoxOpenType openType) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestBoxOpen..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new BoxOpenRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            openType = (int)openType,  // 0:타이머시작, 1:시간완료, 2:보석오픈
            boxId = boxID,
        },
        (string response) => {
            ResponseBoxOpen(JsonConvert.DeserializeObject<BoxOpenResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestPvPLobbyInfo(Action afterCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestPvPLobbyInfo..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new PvpLobbyInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponsePvPLobbyInfo(JsonConvert.DeserializeObject<PvpLobbyInfoResponse>(response), afterCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestSeasonRank() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestSeasonRank..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new SeasonRankRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            season = PlayerManager.Instance.PvPSeasonNum,   // 요청 시즌 번호 
            page = 0,     // 1page, 2page ..
            listCnt = 100, //page 필드 갯수
        },
        (string response) => {
            ResponseSeasonRank(JsonConvert.DeserializeObject<SeasonRankResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestPvPSearch(PvPSearchType searchType) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestPvPSearch..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new PvpSearchRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            searchType = (int)searchType,
        },
        (string response) => {
            ResponsePvPSearch(JsonConvert.DeserializeObject<PvpSearchResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestPvPStart(PvPMatchType matchType) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestPvPStart() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new PvpStartRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            gameType = (int)matchType,
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponsePvPStart(JsonConvert.DeserializeObject<PvpStartResponse>(response));
        }));
    }

    public void RequestPvPEnd(ResultType resultType) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestPvPEnd() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new PvpEndRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            gameResult = (int)resultType, //0:lose, 1:win, 2:draw
            playTime = (int)(DateTime.Now - BattleManager.Instance.StartTime).TotalSeconds,  //플레이 시간(초)
            mobBossKillCnt = BattleManager.Instance.KilledBossCount,  //대장 죽인 갯수
            mobKillCnt = BattleManager.Instance.KilledEnemyCount,  // 졸병 죽인 갯수
            comboMaxCnt = BattleManager.Instance.MaxComboCount,  //최대 콤보 갯수 
            turnCnt = BattleManager.Instance.TurnCount,  //턴 횟수
            blockBrokenCnt = BattleManager.Instance.MatchedBlockCount,  //블록 파괴된 갯수 
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponsePvPEnd(JsonConvert.DeserializeObject<PvpEndResponse>(response));
        }));
    }

    public void RequestPvPSeasonReward() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestPvPSeasonReward() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new SeasonRewardRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponsePvPSeasonReward(JsonConvert.DeserializeObject<SeasonRewardResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestPVPRecordList(Action afterCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestResponsePVPRecord..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new PvpRecordListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponsePVPRecordList(JsonConvert.DeserializeObject<PvpRecordListResponse>(response), afterCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestPvpRevenge(string strRecordID) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestPvpRevenge..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new PvpRevengeRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            recordID = strRecordID,
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponsePvpRevenge(JsonConvert.DeserializeObject<PvpRevengeResponse>(response), strRecordID);
        }));
    }

    public void RequestRevengeStart(string strRecordID) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestPvpRevengeStart..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new PvpRevengeStartRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            recordID = strRecordID,
        },
        (string response) => {
            ResponseRevengeStart(JsonConvert.DeserializeObject<PvpRevengeStartResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestPvpRecordInfo(string strRecordID) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestPvpRecordInfo..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new PvpRecordInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            recordID = strRecordID,
        },
        (string response) => {
            ResponsePvpRecordInfo(JsonConvert.DeserializeObject<PvpRecordInfoResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestStructuresZoneList() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestStructuresZoneList..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new StrucZoneListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseStructuresZoneList(JsonConvert.DeserializeObject<StrucZoneListResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    /// <summary>
    /// 거점 지역에 최초 건물을 지을때 사용 
    /// </summary>
    /// <param name="zoneID"></param>
    /// <param name="structureID"></param>
    public void RequestStartBuildStructure(int zoneID, int structID, int structIndex) {
#if SHOW_LOG
        Debug.LogFormat("[ {0} ]\t<color=white>WebHttp : RequestStartBuildStructure..!!</color>\t\t[ BuildingZoneID : {1}({2}) \t/ structID : {3} \t/ index : {4} ]\n", DateTime.UtcNow, zoneID, (BuildingType)zoneID, structID, structIndex);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new StrucBuildRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            zoneId = zoneID,
            structIndex = structIndex,
            strucRefId = zoneID, //건축물 테이블(t_structInfo) 참조 ID = zoneID 와 같다.
        },
        (string response) => {
            ResponseStartBuildStructure(JsonConvert.DeserializeObject<StrucBuildResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    /// <summary>
    /// 건물 actionType 빌드,업그레이드, 레벨업  완료 여부 확인에 사용
    /// </summary>
    /// <param name="zoneID"></param>
    /// <param name="structureID"></param>
    /// <param name="useSkipAsset"></param>
    public void RequestConfirmStructureActionState(int zoneID, int structureID, bool useSkipAsset, bool hidePrevPopups, System.Action afterCallback = null) {
#if SHOW_LOG
        Debug.LogFormat("[ {0} ]\t<color=white>WebHttp : RequestConfirmStructureActionState..!!</color>\t\t[ zoneID : {1} \t/ structID : {2} ]\n", DateTime.UtcNow, zoneID, structureID);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new StrucConfirmRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            zoneId = zoneID,
            strucId = structureID,
            resUse = useSkipAsset ? 1 : 0,    // 0:재화없이 확인, 1:재화로 시간 스킵
        },
        (string response) => {
            ResponseConfirmStructureActionState(JsonConvert.DeserializeObject<StrucConfirmResponse>(response), hidePrevPopups);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);

            if (null != afterCallback) {
                afterCallback();
            }
        }));
    }

    /// <summary>
    /// 건물의 건설 / 업그레이드 (레벨업) 넘기기 서버 요청 시 사용
    /// </summary>
    /// <param name="zoneID"></param>
    /// <param name="structureID"></param>
    /// <param name="hidePrevPopups"></param>
    /// <param name="afterCallback"></param>
    public void RequestStructSkipByGem(int zoneID, int structureID, bool hidePrevPopups, System.Action afterCallback = null) {
#if SHOW_LOG
        Debug.LogFormat("[ {0} ]\t<color=white>WebHttp : RequestStructSkipByGem..!!</color>\t\t[ zoneID : {1}({2}) \t/ structID : {3} ]\n", DateTime.UtcNow, zoneID, (BuildingType)zoneID, structureID);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new StrucSkipGemRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            zoneId = zoneID,
            strucId = structureID,
        },
        (string response) => {
            ResponseStructSkipByGem(JsonConvert.DeserializeObject<StrucSkipGemResponse>(response), hidePrevPopups);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);

            if (null != afterCallback) {
                afterCallback();
            }
        }));
    }

    /// <summary>
    /// 건물에서 생산한 식량,철광석 수확물 회수 할 때 사용
    /// </summary>
    /// <param name="zoneID"></param>
    /// <param name="structureID"></param>
    public void RequestHarvest(int zoneID, int structureID, HarvestType harvestType, System.Action<List<MItem>> procHarvestItemsCallback, System.Action lastCallback = null) {
#if SHOW_LOG
        Debug.LogFormat("[ {0} ]\t<color=white>WebHttp : RequestHarvest..!!</color>\t\t[ ZoneID : {1}({2}) \t/ strucID : {3} \t/ harvestType : {4} ]\n",
            DateTime.UtcNow, zoneID, (BuildingType)zoneID, structureID, harvestType);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new StrucHarvestRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            zoneId = zoneID,
            strucId = structureID,
            harvestFlag = HarvestType.Normal == harvestType ? 0 : 1,
        },
        (string response) => {
            ResponseHarvest(JsonConvert.DeserializeObject<StrucHarvestResponse>(response), procHarvestItemsCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);

            if (null != lastCallback) {
                lastCallback();
            }
        }));
    }

    public void RequestHarvestAll(int zoneID, System.Action<List<MItem>> procHarvestItemsCallback) {
#if SHOW_LOG
        Debug.LogFormat("[ {0} ]\t<color=white>WebHttp : RequestHarvestAll..!!</color>\t\t[ zoneID : {1}({2}) ]\n", DateTime.UtcNow, zoneID, (BuildingType)zoneID);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new StrucAllHarvestRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            zoneId = zoneID,
        },
        (string response) => {
            ResponseHarvestAll(JsonConvert.DeserializeObject<StrucAllHarvestResponse>(response), procHarvestItemsCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    /// <summary>
    /// 건물 레벨업 시작 사용
    /// </summary>
    /// <param name="zoneID"></param>
    /// <param name="structureID"></param>
    public void RequestLevelUpStructure(int zoneID, int structureID) {
#if SHOW_LOG
        Debug.LogFormat("[ {0} ]\t<color=white>WebHttp : RequestLevelUpStructure..!!</color>\t\t[ zoneID : {1}({2}) \t/ structID : {3} ]\n", DateTime.UtcNow, zoneID, (BuildingType)zoneID, structureID);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new StrucLevelupRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            zoneId = zoneID,
            strucId = structureID,
        },
        (string response) => {
            ResponseLevelUpStructure(JsonConvert.DeserializeObject<StrucLevelupResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public IEnumerator RequestStructResearchListCoroutine(int zoneID, int structID) {
#if SHOW_LOG
        Debug.LogFormat("\t<color=white>WebHttp : RequestStructResearchListCoroutine..!!</color>\t\t[ zoneID : {0} \t/ structID : {1} ]\n", zoneID, structID);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        yield return StartCoroutine(SendRequest(new StrucResearchListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            strucId = structID,          // 건물 ID
        },
        (string response) => {
            ResponseStructResearchList(JsonConvert.DeserializeObject<StrucResearchListResponse>(response), (BuildingType)zoneID);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestStructResearchList(BuildingType buildingType, int structID, bool moveToFactoryListTypeMenu) {
#if SHOW_LOG
        Debug.LogFormat("\t<color=white>WebHttp : RequestStructResearchList..!!</color>\t\t[ zoneID : {0}({1}) \t/ structID : {2} ]\n", (int)buildingType, buildingType, structID);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new StrucResearchListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            strucId = structID,          // 건물 ID
        },
        (string response) => {
            ResponseStructResearchList(JsonConvert.DeserializeObject<StrucResearchListResponse>(response), buildingType, moveToFactoryListTypeMenu);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestStartResearchOrTraining(StructResearchTypes structResearchType, BuildingType buildingType, int strucID, int researchID,
        List<MMaterial> materials, List<MMaterial> cardIDs, List<MMaterial> weaponIDs) {
#if SHOW_LOG
        Debug.LogFormat("[ {0} ]\t<color=white>WebHttp : RequestStartResearchOrTraining..!!</color>\t\t[ type : {1} \t/ zoneID : {2}({3}) \t/ structID : {4} \t/ researchID : {5} ]\n",
            DateTime.UtcNow, structResearchType, (int)buildingType, buildingType, strucID, researchID);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

#if SHOW_LOG
        if (0 < materials.Count) {
            string materialLogs = "Materials : ";
            for (int i = 0, count = materials.Count; i < count; ++i) {
                materialLogs = string.Format("{0}Items ( ItemID : {1} \t/ amount : {2} ) \t/ ", materialLogs, materials[i].m_Id, materials[i].m_amount);
            }
            Debug.LogFormat("\t{0}", materialLogs);
        }

        if (0 < cardIDs.Count) {
            string materialLogs = "CardUIDs : ";
            for (int i = 0, count = cardIDs.Count; i < count; ++i) {
                materialLogs = string.Format("{0}Items ( ItemID : {1} \t/ amount : {2} ) \t/ ", materialLogs, cardIDs[i].m_Id, cardIDs[i].m_amount);
            }
            Debug.LogFormat("\t{0}", materialLogs);
        }

        if (0 < weaponIDs.Count) {
            string materialLogs = "WeaponUIDs : ";
            for (int i = 0, count = weaponIDs.Count; i < count; ++i) {
                materialLogs = string.Format("{0}Items ( ItemID : {1} \t/ amount : {2} ) \t/ ", materialLogs, weaponIDs[i].m_Id, weaponIDs[i].m_amount);
            }
            Debug.LogFormat("\t{0}", materialLogs);
        }
#endif // SHOW_LOG

        TResearch tResearch = TResearchs.Instance.Find(researchID);

        StartCoroutine(SendRequest(new StrucResearchOkRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            strucId = strucID,          // 건물 ID
            reserRefId = researchID,    // 연구 테이블 refID
            actionType = (int)structResearchType,     // 1:연구, 2:훈련		
            resourceIds = materials,    // 재료 각성 사용 할 재료 refID
            weaponIds = weaponIDs,      // 무기
            cardIds = cardIDs,          // 카드
        },
        (string response) => {
            ResponseStartResearchOrTraining(JsonConvert.DeserializeObject<StrucResearchOkResponse>(response), buildingType, structResearchType);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestHavestTraining(BuildingType buildingType, int structID, int researchID, int retrainingRefID, System.Action<int, int, List<MItem>> procHarvestItemsCallback, System.Action afterCallback) {
#if SHOW_LOG
        Debug.LogFormat("[ {0} ]\t<color=white>WebHttp : RequestHavestResearch..!!</color>\t\t[ structID : {1} \t/ researchID : {2} ]\n",
            DateTime.UtcNow, structID, researchID);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new StrucResearchHavestRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            strucId = structID,         // 건물 ID
            reserRefId = researchID,    // 연구 테이블 refID
        },
        (string response) => {
            ResponseHavestTraining(JsonConvert.DeserializeObject<StrucResearchHavestResponse>(response), buildingType, structID, researchID, retrainingRefID, procHarvestItemsCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);

            if (null != afterCallback) {
                afterCallback();
            }
        }));
    }

    public void RequestHarvestTrainingAll(int zoneID, System.Action<List<MItem>> afterCallback) {
#if SHOW_LOG
        Debug.LogFormat("[ {0} ]\t<color=white>WebHttp : RequestHarvestTrainingAll..!!</color>\t\t[ zoneID : {1} ]\n",
            DateTime.UtcNow, zoneID);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new StrucResearchAllHavestRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            zoneId = zoneID,         // 건물 ID
        },
        (string response) => {
            ResponseHarvestTrainingAll(JsonConvert.DeserializeObject<StrucResearchAllHavestResponse>(response), zoneID, afterCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    /// <summary>
    /// 연구 또는 훈련의 넘기기 서버 요청 시 사용
    /// </summary>
    /// <param name="structResearchType"></param>
    /// <param name="buildingType"></param>
    /// <param name="structID"></param>
    /// <param name="researchID"></param>
    /// <param name="retrainingTargetID"></param>
    /// <param name="procHarvestItemsCallback"></param>
    public void RequestSkipResearchOrTrainingByGem(StructResearchTypes structResearchType, BuildingType buildingType, int structID, int researchID, int retrainingTargetID, System.Action<int, int, List<MItem>> procHarvestItemsCallback) {
#if SHOW_LOG
        Debug.LogFormat("[ {0} ]\t<color=white>WebHttp : RequestSkipStructResearchByGem..!!</color>\t\t[ structID : {1} \t/ researchID : {2} ]\n",
            DateTime.UtcNow, structID, researchID);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new StrucResearchSkipGemRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            strucId = structID,          // 건물 ID
            reserRefId = researchID,    // 연구 테이블 refID
        },
        (string response) => {
            ResponseSkipResearchOrTrainingByGem(JsonConvert.DeserializeObject<StrucResearchSkipGemResponse>(response), structResearchType, buildingType, structID, researchID, retrainingTargetID, procHarvestItemsCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestChangeStructTraining(BuildingType buildingType, int strucID, int researchID, StructResearchTypes type, int changeCounts, List<MMaterial> materials) {
#if SHOW_LOG
        Debug.LogFormat("[ {0} ]\t<color=white>WebHttp : RequestChangeStructTraining..!!</color>\t\t[ structID : {1} \t/ researchID : {2} ]\n",
            DateTime.UtcNow, strucID, researchID);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new StrucResearchAddRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            strucId = strucID,          // 건물 ID
            reserRefId = researchID,    // 연구 테이블 refID
            add = changeCounts,         // 예약 건 지정  -:차감, +:증가
            resourceIds = materials,    //재료 각성 사용할 재료 ID
            weaponIds = new List<MMaterial>(),  //무기 ID             // 무기랑 카드의 경우에는 훈련의 변경이 없다..!!
            cardIds = new List<MMaterial>(),  //카드 ID
        },
        (string response) => {
            ResponseChangeStructTraining(JsonConvert.DeserializeObject<StrucResearchAddResponse>(response), buildingType);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestWeaponChange() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestWeaponChange..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new WeaponChangeRequest() {
        },
        (string response) => {
            ResponseWeaponChange(JsonConvert.DeserializeObject<WeaponChangeResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestWeaponUpgrade(bool isAwakening, int tarWeaponID, List<int> resWeaponIDs) {
#if SHOW_LOG
        WeaponData weaponData = PlayerManager.Instance.GetWeaponDataByUniqueID(tarWeaponID);
        Debug.LogFormat("\t<color=white>[ SEND ] WebHttp : RequestWeaponUpgrade..!!\t[ Request {0}\t/ target Weapon UID : {1}\t/ Grade : {2}\t/ Awaken : {3}\t/ level : {4}\t/ exp : {5} ]</color>\n",
            isAwakening ? "각성" : "레벨업", tarWeaponID, weaponData.WeaponCardData._grade, weaponData._awakening, weaponData._level, weaponData._exp);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new WeaponUpgradeRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            upgradeType = isAwakening ? 1 : 0,  // 0:레벨업, 1:각성 레벨업	
            tarWeaponId = tarWeaponID,
            resIds = resWeaponIDs,
        },
        (string response) => {
            ResponseWeaponUpgrade(JsonConvert.DeserializeObject<WeaponUpgradeResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestUserDetailInfo() {
        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new UserDetailInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            account = PlayerPrefsManager.Instance.GetAccountName(),
        },
        (string response) => {
            ResponseUserDetailInfo(JsonConvert.DeserializeObject<UserDetailInfoResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestOutpostPvPSearch(PvPSearchType searchType, LobbySubType targetLobbySubType) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestOutpostPvPSearch..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new OutpostPvpSearchRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            searchType = (int)searchType,
        },
        (string response) => {
            ResponseOutpostPvPSearch(JsonConvert.DeserializeObject<OutpostPvpSearchResponse>(response), targetLobbySubType);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestOutpostPvPStart(PvPMatchType matchType) {
#if SHOW_LOG
        Debug.LogFormat("\t<color=white>WebHttp : RequestOutpostPvPStart..!!\t\t[ MatchType : {0} ]</color>\n", matchType);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new OutpostPvpStartRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            gameType = (int)matchType,
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponseOutpostPvPStart(JsonConvert.DeserializeObject<OutpostPvpStartResponse>(response));
        }));
    }

    public void RequestOutpostPvPEnd(ResultType resultType) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestOutpostPvPEnd() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new OutpostPvpEndRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            stageId = SystemManager.Instance.SelectedBattleStageID,
            gameResult = (int)resultType, //0:lose, 1:win, 2:draw
            playTime = (int)(DateTime.Now - BattleManager.Instance.StartTime).TotalSeconds,  //플레이 시간(초)
            mobBossKillCnt = BattleManager.Instance.KilledBossCount,  //대장 죽인 갯수
            mobKillCnt = BattleManager.Instance.KilledEnemyCount,  // 졸병 죽인 갯수
            comboMaxCnt = BattleManager.Instance.MaxComboCount,  //최대 콤보 갯수 
            turnCnt = BattleManager.Instance.TurnCount,  //턴 횟수
            blockBrokenCnt = BattleManager.Instance.MatchedBlockCount,  //블록 파괴된 갯수
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponseOutpostPvPEnd(JsonConvert.DeserializeObject<OutpostPvpEndResponse>(response));
        }));
    }

    public void RequestDungeonList() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestDungeonList() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new DungeonListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseDungeonList(JsonConvert.DeserializeObject<DungeonListResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestDungeonStageList(int dungeonSubType, int dungeonID) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestDungeonStageList() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new DungeonStageListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            subType = dungeonSubType,
        },
        (string response) => {
            ResponseDungeonStageList(JsonConvert.DeserializeObject<DungeonStageListResponse>(response), dungeonID);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestDungeonPveStart(int dungeonStageID) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestDungeonPveStart() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new DungeonPveStartRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            dungeonStageId = dungeonStageID,
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponseDungeonPveStart(JsonConvert.DeserializeObject<DungeonPveStartResponse>(response));
        }));
    }

    public void RequestDungeonPveEnd(int dungeonStageID, int dungeonResult) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestDungeonPveEnd() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new DungeonPveEndRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            dungeonStageId = dungeonStageID,
            gameResult = dungeonResult,
            enemyKillCounts = GameDataManager.Instance.EnemyKillCounts,   //스테이지에 나온 적군의 정보(적군 이미지 파일명, 적군 속성, 적군 처치수)
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponseDungeonPveEnd(JsonConvert.DeserializeObject<DungeonPveEndResponse>(response));
        }));
    }

    public void RequestDungeonPvERestart(int dungeonStageID) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestDungeonPvERestart() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new DungeonPveReStartRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            stageId = dungeonStageID,
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponseDungeonPvERestart(JsonConvert.DeserializeObject<DungeonPveReStartResponse>(response));
        }));
    }

    public void RequestTournamentInfo(Action afterCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestTournamentInfo() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TournamentInfo() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseTournamentInfo(JsonConvert.DeserializeObject<TournamentInfoResponse>(response), afterCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestMyTournamentInfo(Action afterCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestMyTournamentInfo() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TournamentMyInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseMyTournamentInfo(JsonConvert.DeserializeObject<TournamentMyInfoResponse>(response), afterCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestTournamentAttend(MDeck defenceDeckCards, Action afterCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestTournamentAttend() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TournamentAttendRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            deckCards = defenceDeckCards
        },
        (string response) => {
            ResponseTournamentAttend(JsonConvert.DeserializeObject<TournamentAttendResponse>(response), afterCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestTournamentRankingList(Action completeCallback = null) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestTournamentRankingList() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TournamentRankerRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseTournamentRankingList(JsonConvert.DeserializeObject<TournamentRankerResponse>(response), completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestTournamentCardChange(DeckType deckType, List<MDeckCard> cards, Action completeCallback, bool updateTeamEditUI) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestTournamentCardChange() ..!!</color>\n");
#endif // SHOW_LOG

        StartCoroutine(SendRequest(new TournamentCardChangeRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            type = (int)deckType, //1 : 전투력 덱카드 변경, 2 : 방어력 덱카드 변경
            deckCards = cards,
        },
        (string response) => {
            ResponseTournamentCardChange(JsonConvert.DeserializeObject<TournamentCardChangeResponse>(response), updateTeamEditUI);
            if (null != completeCallback) {
                completeCallback();
            }
        }));
    }

    public void RequestTournamentPvpSearch(PvPSearchType tournamentSearchType) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestTournamentPvpSearch() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TournamentPvpSearchRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            searchType = (int)tournamentSearchType  // 0:최초 입장검색, 1:재검색
        },
        (string response) => {
            ResponseTournamentPvpSearch(JsonConvert.DeserializeObject<TournamentPvpSearchResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestTournamentPvPStart() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestTournamentPvpStart() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TournamentPvpStartRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponseTournamentPvPStart(JsonConvert.DeserializeObject<TournamentPvpStartResponse>(response));
        }));
    }

    public void RequestTournamentPvPEnd(ResultType resultType) {
#if SHOW_LOG
        Debug.LogFormat("\t<color=white>WebHttp : RequestPvPEnd() ..!!\t[ Result : {0} ({1}) \t/ killCount : {2} ]</color>\n",
            resultType, (int)resultType, BattleManager.Instance.KilledBossCount);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TournamentPvpEndRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            gameResult = (int)resultType, //0:lose, 1:win, 2:draw
            killCnt = BattleManager.Instance.KilledBossCount,  //대장 죽인 갯수
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponseTournamentPvPEnd(JsonConvert.DeserializeObject<TournamentPvpEndResponse>(response));
        }));
    }

    public void RequestTournamentRecordList(Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestTournamentRecordList() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TournamentRecordListRequest() {
            channelId = PlayerManager.Instance.GetChannelID()
        },
        (string response) => {
            ResponseTournamentRecordList(JsonConvert.DeserializeObject<TournamentRecordListResponse>(response), completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestTournamentPlayerInfo(string playerAccountName) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestTournamentPlayerInfo() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TournamentRecordPlayerInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            vsAccount = playerAccountName,
        },
        (string response) => {
            ResponseTournamentPlayerInfo(JsonConvert.DeserializeObject<TournamentRecordPlayerInfoResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestTournamentResultReward(Action closeCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestTournamentResultReward() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TournamentRewardRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseTournamentResultReward(JsonConvert.DeserializeObject<TournamentRewardResponse>(response), closeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestResetTournamentLoseCount(Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestResetTournamentLoseCount() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TournamentGemLoseResetRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseResetTournamentLoseCount(JsonConvert.DeserializeObject<TournamentGemLoseResetResponse>(response), completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestMissionList() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestMissionList..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new MissionListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseMissionList(JsonConvert.DeserializeObject<MissionListResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestMissionConfirm(int missionId) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestMissionConfirm..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new MissionConfirmRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            missionId = missionId,
        },
        (string response) => {
            ResponseMissionConfirm(JsonConvert.DeserializeObject<MissionConfirmResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestWantedQuestConfirm(int questId) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestWantedQuestConfirm..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new WantedQuestConfirmRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            questId = questId,
        },
        (string response) => {
            ResponseWantedQuestConfirm(JsonConvert.DeserializeObject<WantedQuestConfirmResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestWantedQuestReset(int questId, int resetType) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestWantedQuestReset..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new WantedQuestResetRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            questId = questId,
            resettype = resetType,
        },
        (string response) => {
            ResponseWantedQuestReset(JsonConvert.DeserializeObject<WantedQuestResetResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestSummonThemeList(Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestSummonThemeList() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new SummonListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseSummonThemeList(JsonConvert.DeserializeObject<SummonListResponse>(response), completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestSummonInfo() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestSummonInfo() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new SummonInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseSummonInfo(JsonConvert.DeserializeObject<SummonInfoResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestSummonPurchase(int priceID, int summonIndex) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestSummonPurchase() ..!!</color>\n");

        Debug.Log(string.Format("SummonPrice ID : {0}", priceID));
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.CircleType);

        StartCoroutine(SendRequest(new SummonPurchaseRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            priceId = priceID
        },
        (string response) => {
            ResponseSummonPurchase(JsonConvert.DeserializeObject<SummonPurchaseResponse>(response), summonIndex);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.CircleType);
        }));
    }

    public void RequestSummonBonusBoxConfirm(int summonIndex) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestSummonBonusConfirm() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.CircleType);

        StartCoroutine(SendRequest(new SummonBonusBoxConfirmRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            index = (int)summonIndex
        },
        (string response) => {
            ResponseSummonBonusConfirm(JsonConvert.DeserializeObject<SummonBonusBoxConfirmResponse>(response), summonIndex);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.CircleType);
        }));
    }

    public void RequestSummonFateList() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestSummonFateList() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.CircleType);

        StartCoroutine(SendRequest(new SummonFateListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseSummonFateList(JsonConvert.DeserializeObject<SummonFateListResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.CircleType);
        }));
    }

    public void RequestSummonFateConfirm(int heroID) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestSummonFateConfirm() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.CircleType);

        StartCoroutine(SendRequest(new SummonFateConfirmRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            selectCardid = heroID,
        },
        (string response) => {
            ResponseSummonFateConfirm(JsonConvert.DeserializeObject<SummonFateConfirmResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.CircleType);
        }));
    }

    public void RequestPurchase(int buycount, int id, System.Action<int> completeCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestPurchase..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new IngamePurchaseRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            priceId = id,
            purchaseQty = buycount,
        },
        (string response) => {
            ResponsePurchase(JsonConvert.DeserializeObject<IngamePurchaseResponse>(response), completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestShopInfo(System.Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestShopInfo..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ShopListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseShopInfo(JsonConvert.DeserializeObject<ShopListResponse>(response), completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestTreatyOver() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestTreatyOver..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TreatyBreakRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseTreatyOver(JsonConvert.DeserializeObject<TreatyBreakResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestSkypassInfo(System.Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestSkypassInfo..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new SeasonQuestListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseSkypassInfo(JsonConvert.DeserializeObject<SeasonQuestListResponse>(response), completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestSkypassComplete(int id, System.Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestSkypassInfo..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new SeasonQuestConfirmRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            questId = id,
        },
        (string response) => {
            ResponseSkypassComplete(JsonConvert.DeserializeObject<SeasonQuestConfirmResponse>(response), completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestSkypassReward(int lv, System.Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestSkypassReward..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new SeasonQuestRewardConfirmRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            level = lv,
        },
        (string response) => {
            ResponseSkypassReward(JsonConvert.DeserializeObject<SeasonQuestRewardConfirmResponse>(response), completeCallback, lv);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestSkypassFinalReward(System.Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestSkypassFinalReward..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new SeasonQuestFinalRewardRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseSkypassFinalReward(JsonConvert.DeserializeObject<SeasonQuestFinalRewardResponse>(response), completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestClanCreate(ClanCreateData clanCreateData) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanCreate() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.CircleType);

        StartCoroutine(SendRequest(new ClanCreateRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            clanname = clanCreateData.name,
            message = clanCreateData.message,
            pattern = clanCreateData.pattern,
            mark = clanCreateData.symbol,
            type = clanCreateData.publicType,
            trophy = clanCreateData.winScroe,
            language = clanCreateData.language,
        },
        (string response) => {
            ResponseClanCreate(JsonConvert.DeserializeObject<ClanCreateResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.CircleType);
        }));
    }

    public void RequestClanList() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanList() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.CircleType);

        StartCoroutine(SendRequest(new ClanListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseClanList(JsonConvert.DeserializeObject<ClanListResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.CircleType);
        }));
    }

    public void RequestClanListNameSearch(string clanName) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanListNameSearch() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.CircleType);

        StartCoroutine(SendRequest(new ClanListNameSearchRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            clanName = clanName
        },
        (string response) => {
            ResponseClanListNameSearch(JsonConvert.DeserializeObject<ClanListNameSearchResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.CircleType);
        }));
    }

    public void RequestClanInfo(int clanID, LobbySubType curLobbySubType) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanInfo() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            clanId = clanID
        },
        (string response) => {
            ResponseClanInfo(JsonConvert.DeserializeObject<ClanInfoResponse>(response), curLobbySubType);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestClanUserPlayerInfo(MClanUserInfo clanUserInfoData, int clanID) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanUserPlayerInfo() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.CircleType);

        StartCoroutine(SendRequest(new ClanUserPlayerInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            userseq = clanUserInfoData.m_userseq
        },
        (string response) => {
            ResponseClanUserPlayerInfo(JsonConvert.DeserializeObject<ClanUserPlayerInfoResponse>(response), clanUserInfoData, clanID);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.CircleType);
        }));
    }

    public void RequestClanFriendlyPvPStart() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanFriendlyPvPStart() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.CircleType);

        StartCoroutine(SendRequest(new ClanPvpStartRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseClanFriendlyPvPStart(JsonConvert.DeserializeObject<ClanPvpStartResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.CircleType);
        }));
    }

    public void RequestClanBossInfo(System.Action clickCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanBossInfo() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.CircleType);

        StartCoroutine(SendRequest(new ClanBossInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseClanBossInfo(JsonConvert.DeserializeObject<ClanBossInfoResponse>(response), clickCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.CircleType);
        }));
    }

    public void RequestClanBossStart(System.Action clickCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanBossStart() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.CircleType);

        StartCoroutine(SendRequest(new ClanBossRaidStartRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.CircleType);
            ResponseClanBossStart(JsonConvert.DeserializeObject<ClanBossRaidStartResponse>(response), clickCallback);
        }));
    }

    public void RequestClanBossGigUse(int deckNumber, int itemID, int clanID, System.Action<int> completeCallback) {
#if SHOW_LOG
        Debug.LogFormat("\t<color=white>WebHttp : RequestBattleItemUse..!! \t [ deckNum : {0} / itemID : {1} ]</color>\n", deckNumber, itemID);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanBossItemUseRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            deckNum = deckNumber,
            itemId = itemID,
            clanId = clanID,
        },
        (string response) => {
            ResponseClanBossGigUse(JsonConvert.DeserializeObject<ClanBossItemUseResponse>(response), completeCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestClanBossEnd(int damage, int clanID) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanBossEnd() ..!!</color>\n");
#endif // SHOW_LOG  
        if (GameDataManager.Instance.IsClanBossEndRequestSended) {
            return;
        }

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.CircleType);

        GameDataManager.Instance.IsClanBossEndRequestSended = true;

        StartCoroutine(SendRequest(new ClanBossRaidEndRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            damage = damage,
            clanId = clanID,
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.CircleType);
            ResponseClanBossEnd(JsonConvert.DeserializeObject<ClanBossRaidEndResponse>(response));
        }));
    }

    public void RequestClanBossRanking(System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanBossRanking() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanBossDamageHistoryRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseClanBossRanking(JsonConvert.DeserializeObject<ClanBossDamageHistoryResponse>(response), callback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestClanBossReward() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanBossReward() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanBossRewardRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseClanBossReward(JsonConvert.DeserializeObject<ClanBossRewardResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestClanChattingList(int clanID) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanChattingList() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanChattingListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            clanId = clanID
        },
        (string response) => {
            ResponseClanChattingList(JsonConvert.DeserializeObject<ClanChattingListResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestClanChatBlockUserList() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanChatBlockUserList() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanChatBlockUserListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseClanChatBlockUserList(JsonConvert.DeserializeObject<ClanChatBlockUserListResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestChatBlockUserInsert(int blockUserSeq) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestChatBlockUserInsert() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanChatBlockUserInsertRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            blockUserIndex = blockUserSeq
        },
        (string response) => {
            ResponseClanChatBlockUserInsert(JsonConvert.DeserializeObject<ClanChatBlockUserInsertResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestChatBlockUserCancel(int unblockUserSeq, System.Action refreshCallback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestChatBlockUserCancel() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanChatBlockUserCancelRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            blockUserIndex = unblockUserSeq
        },
        (string response) => {
            ResponseClanChatBlockUserCancel(JsonConvert.DeserializeObject<ClanChatBlockUserCancelResponse>(response), refreshCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestClanJoinUserList() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanJoinUserList() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanJoinUserListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseClanJoinUserList(JsonConvert.DeserializeObject<ClanJoinUserListResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestClanWarInfo(System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanWarInfo() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanWarInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseClanWarInfo(JsonConvert.DeserializeObject<ClanWarInfoResponse>(response), callback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestClanWarDefenseDeckChange(List<MDeckCard> deckCards, Action completeCallback, bool updateTeamEditUI) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanWarInfo() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanWarDeckCardChangeRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            deckCards = deckCards,
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponseClanWarDefenseDeckChanged(JsonConvert.DeserializeObject<ClanWarDeckCardChangeResponse>(response), updateTeamEditUI);
            if (null != completeCallback) {
                completeCallback();
            }
        }));
    }

    public void RequestClanWarUsedCardList(System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanWarUsedCardList() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanWarUsedDeckCardRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponseClanWarUsedCardList(JsonConvert.DeserializeObject<ClanWarUsedDeckCardResponse>(response), callback);
        }));
    }

    public void RequestClanWarBattleStart(System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanWarBattleStart() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanWarBattleStartRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            m_items = GameDataManager.Instance.GetClanWarAttackDeck(),
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponseClanWarBattleStart(JsonConvert.DeserializeObject<ClanWarBattleStartResponse>(response), callback);
        }));
    }

    public void RequestClanWarUserAttendChange(ClanWarAttendType attendType, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanWarBattleStart() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanWarUserAttendChangeRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            attendStatus = (int)attendType
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponseClanWarUserAttendChange(JsonConvert.DeserializeObject<ClanWarUserAttendChangeResponse>(response), callback);
        }));
    }

    public void RequestClanWarBattleFieldInfo(System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanWarBattleFieldInfo() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanWarBattleFieldInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponseClanWarBattleFieldInfo(JsonConvert.DeserializeObject<ClanWarBattleFieldInfoResponse>(response), callback);
        }));
    }

    public void RequestClanWarBattleEnd(ResultType resultType) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanWarBattleFieldInfo() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanWarBattleEndRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            getPoint = GameDataManager.Instance.GetClanWarHitDamage(),
            gameResult = (int)resultType,       //0:lose, 1:win, 2:draw
            vsUserseq = GameDataManager.Instance.ClanWarVsUserData.m_userseq,
            vsItems = GameDataManager.Instance.GetClanWarEndDeckInfo(),
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponseClanWarBattleEnd(JsonConvert.DeserializeObject<ClanWarBattleEndResponse>(response));
        }));
    }

    public void RequestClanWarCurrentRanking(System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanWarBattleFieldInfo() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanWarCurrentRankRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
            ResponseClanWarCurrentRanking(JsonConvert.DeserializeObject<ClanWarCurrentRankResponse>(response), callback);
        }));
    }

    public void RequestClanWarResultReward() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanWarResultReward() ..!!</color>\n");
#endif // SHOW_LOG

        StartCoroutine(SendRequest(new ClanWarResultRewardRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseClanWarResultReward(JsonConvert.DeserializeObject<ClanWarResultRewardResponse>(response));
        }));
    }

    public void RequestClanWarHistory(System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanWarHistory() ..!!</color>\n");
#endif // SHOW_LOG

        StartCoroutine(SendRequest(new ClanWarHistoryRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseClanWarHistory(JsonConvert.DeserializeObject<ClanWarHistoryResponse>(response), callback);
        }));
    }

    public void RequestClanWarBoxReward(System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestClanWarBoxReward() ..!!</color>\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanWarBoxRewardRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseClanWarBoxReward(JsonConvert.DeserializeObject<ClanWarBoxRewardResponse>(response), callback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestWemixTradingPost(string userAddress) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestWemixPoint() ..!!</color>\n");
#endif // SHOW_LOG

        StartCoroutine(SendRequest(new WemixTradingPostRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            userAddress = userAddress,
        },
        (string response) => {
            ResponseWemixTradingPost(JsonConvert.DeserializeObject<WemixTradingPostResponse>(response));
        }));
    }

    public void RequestWemixPointStaking(int honorPoint) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestWemixPointStaking() ..!!</color>\n");
#endif // SHOW_LOG

        StartCoroutine(SendRequest(new WemixPointStakingRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            honor = honorPoint,
        },
        (string response) => {
            ResponseWemixPointStaking(JsonConvert.DeserializeObject<WemixPointStakingResponse>(response));
        }));
    }

    public void RequestWemixonInfo() {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestWemixonInfo() ..!!</color>\n");
#endif // SHOW_LOG

        StartCoroutine(SendRequest(new WemixInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseWemixInfo(JsonConvert.DeserializeObject<WemixInfoResponse>(response));
        }));
    }

    public void RequestWemixDungeonStageReopen(int dungeonStageID) {
#if SHOW_LOG
        Debug.Log("\t<color=white>WebHttp : RequestWemixDungeonStageReopen() ..!!</color>\n");
#endif // SHOW_LOG

        StartCoroutine(SendRequest(new WemixDungeonStageReopenRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            dungeonStageId = dungeonStageID,
        },
        (string response) => {
            ResponseWemixDungeonStageReopen(JsonConvert.DeserializeObject<WemixDungeonStageReopenResponse>(response));
        }));
    }

    public void RequestWemixStructAllHarvest(int zoneID, System.Action<List<MItem>> procHarvestItemsCallback) {
#if SHOW_LOG
        Debug.LogFormat("[ {0} ]\t<color=white>WebHttp : RequestWemixStructAllHarvest..!!</color>\t\t[ zoneID : {1}({2}) ]\n", DateTime.UtcNow, zoneID, (BuildingType)zoneID);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new WemixStrucAllHarvestRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            zoneId = zoneID,
        },
        (string response) => {
            ResponseWemixStrucAllHarvest(JsonConvert.DeserializeObject<WemixStrucAllHarvestResponse>(response), procHarvestItemsCallback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestUserInfo(int userSeq, System.Action callback = null) {
#if SHOW_LOG
        Debug.LogFormat("<color=white>WebHttp : RequestUserInfo..!!</color>\t\t[ userSeq : {0} ]\n", userSeq);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new UserInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            userseq = userSeq,
        },
        (string response) => {
            ResponseUserInfo(JsonConvert.DeserializeObject<UserInfoResponse>(response), callback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestClanJoin(int joinStatus) {
#if SHOW_LOG
        Debug.LogFormat("<color=white>WebHttp : RequestClanJoin..!!</color>\t\t[ joinStatus : {0} ]\n", joinStatus);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ClanJoinRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            join_status = joinStatus,
        },
        (string response) => {
            ResponseClanJoin(JsonConvert.DeserializeObject<ClanJoinResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestGooglePurchaseKey(System.Action callback) {
#if SHOW_LOG
        Debug.LogFormat("<color=white>WebHttp : RequestGooglePurchaseKey..!!</color>]\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new GooglePlayReceiptVerificationKeyRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseGooglePurchaseKey(JsonConvert.DeserializeObject<GooglePlayReceiptVerificationKeyResponse>(response), callback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestCashPurchaseCheck(System.Action callback) {
        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        int purchaseType = 1;
        string orderID = string.Empty;
        string token = string.Empty;
        string packageName = string.Empty;
        string productID = string.Empty;

        PlayerPrefsManager.Instance.GetPurchaseData(ref purchaseType, ref packageName, ref productID, ref orderID, ref token);

        StartCoroutine(SendRequest(new GooglePlayReceiptVerificationRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            inAppType = (int)SystemManager.Instance.StoreType + 1,
            inapp_key = PlayerPrefsManager.Instance.GetGooglePurchaseKey(),
            purchaseType = purchaseType,
            package_name = packageName,
            product_id = productID,
            order_id = orderID,
            token_recvdata = token,
        },
        (string response) => {
            ResponseCashPurchaseCheck(JsonConvert.DeserializeObject<GooglePlayReceiptVerificationResponse>(response), callback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestBannerProductInfo(System.Action callback) {
#if SHOW_LOG
        Debug.LogFormat("<color=white>WebHttp : RequestBannerProductInfo..!!</color>]\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new BannerProductListRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            storeType = (int)SystemManager.Instance.StoreType + 1,
        },
        (string response) => {
            ResponseBannerProductInfo(JsonConvert.DeserializeObject<BannerProductListResponse>(response), callback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestRegisterLinkage(int type, string linkAccount, Action callback) {
#if SHOW_LOG
        Debug.LogFormat("<color=white>WebHttp : RequestRegisterLinkage..!! type : {0}\t linkAccount : {1}</color>]\n", type, linkAccount);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new RegisterLinkageRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            uuid = PlayerPrefsManager.Instance.GetAccountName(),
            account = linkAccount,
            type = type,
        },
        (string response) => {
            ResponseRegisterLinkage(JsonConvert.DeserializeObject<RegisterLinkageResponse>(response), callback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestRegisterUserInfo(int type, Action<RegisterUserInfoResponse> callback) {
#if SHOW_LOG
        Debug.LogFormat("<color=white>WebHttp : RequestRegisterUserInfo..!!</color>]\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new RegisterUserInfoRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            account = SignInManager.Instance.PlatformUserId,
            type = type,
        },
        (string response) => {
            ResponseRegisterUserInfo(JsonConvert.DeserializeObject<RegisterUserInfoResponse>(response), callback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestAgreementConfirmRequest() {
#if SHOW_LOG
        Debug.LogFormat("<color=white>WebHttp : RequestAgreementConfirmRequest..!!</color>]\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new AgreementConfirmRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
        },
        (string response) => {
            ResponseAgreementConfirm(JsonConvert.DeserializeObject<AgreementConfirmResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestUseItem(int itemID, int amount, int targetID = 0, System.Action callback = null) {
#if SHOW_LOG
        Debug.LogFormat("<color=white>WebHttp : RequestUseItem..!!</color> [ itemID : {0} / amount : {1} / targetID : {2} ]\n", itemID, amount, targetID);
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new ItemUseRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            itemRefId = itemID,
            useAmount = amount,
            tarCardId = targetID,
        },
        (string response) => {
            ResponseUseItem(JsonConvert.DeserializeObject<ItemUseResponse>(response), callback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestAdReward(string hash, int type, System.Action callback = null) {
#if SHOW_LOG
        Debug.LogFormat("<color=white>WebHttp : RequestAdReward..!!</color>]\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new AdvertisementConfirmRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            hash = hash,
            type = type,
        },
        (string response) => {
            ResponseAdReward(JsonConvert.DeserializeObject<AdvertisementConfirmResponse>(response), callback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestAvatarChange(int avatarId, int bgId, int pinId, Action callback = null) {
#if SHOW_LOG
        Debug.LogFormat("<color=white>WebHttp : RequestAvatarChange..!!</color>]\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new AvatarChangeRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            avatar = avatarId,
            background = bgId,
            pin = pinId,
        },
        (string response) => {
            ResponseAvatarChange(JsonConvert.DeserializeObject<AvatarChangeResponse>(response), callback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestNickNameChange(string changeName, Action callback = null) {
#if SHOW_LOG
        Debug.LogFormat("<color=white>WebHttp : RequestNickNameChange..!!</color>]\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new NickNameChangeRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            nickName = changeName,
        },
        (string response) => {
            ResponseNickNameChange(JsonConvert.DeserializeObject<NickNameChangeResponse>(response), callback);
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestSetTalentValues(int cardUID, List<int> talentValues) {
#if SHOW_LOG
        Debug.LogFormat("<color=white>WebHttp : RequestSetTalentValues..!!</color>]\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TalentInitAndLearnRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            type = 1,    // 1 : 배우기 , 2 : 초기화
            subtype = 0, // 초기화 일 경우 사용 (1 : 다이아 소비, 2 : 엠블럼 소비)
            cardId = cardUID,
            attributeChart = talentValues,
        },
        (string response) => {
            ResponseSetTalentValues(JsonConvert.DeserializeObject<TalentInitAndLearnResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    public void RequestResetTalentValues(int cardUID, bool useEmblem) {
#if SHOW_LOG
        Debug.LogFormat("<color=white>WebHttp : RequestResetTalents..!!</color>]\n");
#endif // SHOW_LOG

        LoadAnimManager.Instance.StartLoadingAnimation(LoadAnimType.NoneType);

        StartCoroutine(SendRequest(new TalentInitAndLearnRequest() {
            channelId = PlayerManager.Instance.GetChannelID(),
            type = 2,    // 1 : 배우기 , 2 : 초기화
            subtype = useEmblem ? 2 : 1, // 초기화 일 경우 사용 (1 : 다이아 소비, 2 : 엠블럼 소비)
            cardId = cardUID,
            attributeChart = new List<int>(),
        },
        (string response) => {
            ResponseResetTalentValues(JsonConvert.DeserializeObject<TalentInitAndLearnResponse>(response));
            LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
        }));
    }

    #endregion // public funcs







    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////







    #region private funcs

    private IEnumerator SendRequest(PacketRequest packetRequest, System.Action<string> responseCallback) {
#if SHOW_LOG
        _sendStartTime = DateTime.Now;
        Debug.LogFormat("<color=yellow>TIME CHECK : WebHttp : Send : Send Start Time : [ {0} ]</color>\n", Common.GetTimeToLog(_sendStartTime));
#endif // SHOW_LOG

        //messagepack test............
        //     var stream = new MemoryStream();
        //    MsgPack.Serialize(req, stream);
        //    stream.Position = 0;
        //    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        //    string parameter2 = reader.ReadToEnd();
        //-------------------------------------------

        string parameter = JsonConvert.SerializeObject(packetRequest);
        using (UnityWebRequest webRequest = UnityWebRequest.Put(_url, parameter)) {
            byte[] jsonToSend = new UTF8Encoding().GetBytes(parameter);
            webRequest.uploadHandler.Dispose();
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            //  webRequest.SetRequestHeader("Content-Type", "application/json");

            //   Debug.LogFormat("###################################################################JsonConvert.lenth:{0}:{1} , MsgPack.lenth:{2}:{3}\n", parameter.Length, parameter, parameter2.Length, parameter2);

            yield return webRequest.SendWebRequest();

            string requestPacketHeader = packetRequest.packetHeader;

#if SHOW_LOG
            DateTime now = DateTime.Now;
            Debug.LogFormat("<color=yellow>TIME CHECK : WebHttp : SendRequest : Receive End Time : [ {0} ]</color>\t\t<color=white>[ Packet : {1}\t\t/ LapseTime : {2} ]</color>\n",
                Common.GetTimeToLog(now), requestPacketHeader, Common.GetDetailTimeSpanLog(now - _sendStartTime));
#endif // SHOW_LOG

            if (webRequest.result == UnityWebRequest.Result.Success) {
                string responseStr = webRequest.downloadHandler.text;

                // string _aesKey = "1234";
                // string a = CryptUtil.AESDecryptString(responseStr, _aesKey);
                // Debug.LogErrorFormat("Web : Post : [ SERVER : {0}\t URL : {1}]\n", SystemManager.Instance.ReleaseType, responseStr);

                string responsePacketHeader = JsonConvert.DeserializeObject<PacketResponse>(responseStr).packetHeader;
                if (CheckValidPacketHeader(requestPacketHeader, responsePacketHeader)) {
                    if (!HasNetworkErrors(responseStr)) {
                        responseCallback(responseStr);
                    }
                }
                else {
                    Debug.LogErrorFormat("Web::Recv: packetHeader errror..!!\t[ SERVER : {0}\t URL : {1}\t ERROR : {2} ]\nCheck [ PacketType ] in [ Web ]\n", SystemManager.Instance.ReleaseType, _url, responsePacketHeader);
                }
            }
            else {
                Debug.LogErrorFormat("Web : Post : [ SERVER : {0}\t/ URL : {1}\t/ ERROR : {2}\t/ packet : {3} ]\n", SystemManager.Instance.ReleaseType, _url, webRequest.error, requestPacketHeader);

                if (ReleaseType.Dev == SystemManager.Instance.ReleaseType) {
                    PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ALERT_7030"),
                        string.Format("SERVER : {0}\nURL : {1}\nERROR : {2}\n", SystemManager.Instance.ReleaseType, _url, webRequest.error),
                        SystemManager.Instance.ExitGame,
                        0f,
                        true);
                }
                else {
                    PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ALERT_7030"),
                        string.Format("SERVER : {0}\nERROR : {1}\n", SystemManager.Instance.ReleaseType, webRequest.error),
                        SystemManager.Instance.ExitGame,
                        0f,
                        true);
                }
            }
        }
    }

    private bool HasNetworkErrors(string responseStr) {
        PacketResponse response = JsonConvert.DeserializeObject<PacketResponse>(responseStr);
        if ((int)PacketErrorResult.maintenanceInProgress == response.result) {
#if SHOW_LOG
            Debug.LogWarningFormat("WebHttps : HasMaintenanceError : Server has maintenancing..!! \t [ result : {0} ]\n", (PacketErrorResult)response.result);
#endif // SHOW_LOG

            PopupManager.Instance.ShowOKPopup(TSystemStrings.Instance.FindString("PATCH_1001"), // "점검중"
                TSystemStrings.Instance.FindString("PATCH_1039"), // 현재 서버를 점검 하고 있습니다.
                () => {
                    SystemManager.Instance.ExitGame();
                });

            return true;
        }
        else if ((int)PacketErrorResult.sessionIsNull == response.result) {
#if SHOW_LOG
            Debug.LogWarningFormat("WebHttps : HasMaintenanceError : out of session time..!! \t [ result : {0} ]\n", (PacketErrorResult)response.result);
#endif // SHOW_LOG

            TutorialManager.Instance.HideTutorial();

            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15016"), // "세선 종료"
                TStrings.Instance.FindString("ERROR_15017"), // 세션이 종료되어 재시작 합니다.
                () => {
                    SystemManager.Instance.Restart();
                });

            return true;
        }

#if UNITY_EDITOR
        if (0 != response.result) {
            Debug.LogFormat("<color=red>WebHttp : HasNetworkErrors : <b>[ PacketHeader : {0}  /  result : {1} ]</b></color>\n",
                response.packetHeader, response.result);
        }
#endif // UNITY_EDITOR

        return false;
    }

    private bool CheckValidPacketHeader(string requestPacketHeader, string responsePacketHeader) {
        if (0 == string.Compare(requestPacketHeader, responsePacketHeader)) {
            return true;
        }

        return false;
    }

    private void ResponseServerCheck(ServerCheckResponse res, Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseServerCheck..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            if (0 != res.checkStartTime && 0 != res.checkEndTime) {
                if (string.IsNullOrEmpty(res.message)) {
                    Debug.LogWarningFormat("WebHttp : ResponseServerCheck : 점검중 : 점검시간은 있고, 메세지는 없다..!! \t [ {0} ]\n", res.message);

                    PopupManager.Instance.ShowOKPopup(TSystemStrings.Instance.FindString("PATCH_1001"),
                        string.Format(TSystemStrings.Instance.FindString("PATCH_1002"), Common.ConvertJavaMillisecondToDateTimeUTC(res.checkStartTime), Common.ConvertJavaMillisecondToDateTimeUTC(res.checkEndTime)),
                        () => {
                            SystemManager.Instance.ExitGame();
                        });
                }
                else {
                    Debug.LogWarningFormat("WebHttp : ResponseServerCheck : 점검중 : 점검시간이 있고, 메세지도 있다..!! \t [ {0} ]\n", res.message);

                    PopupManager.Instance.ShowOKPopup(TSystemStrings.Instance.FindString("PATCH_1001"), res.message,
                        () => {
                            SystemManager.Instance.ExitGame();
                        });
                }
            }
            else {
                SystemManager.Instance.HttpStatus = HttpStatus.ServerChecked;

                if (null != completeCallback) {
                    completeCallback();
                }
            }
        }
        else {
            IntroSceneUIController.Instance.HideInfoStrings();

            if (res.result == 1) {
                PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ALERT_7030"),
                    TStrings.Instance.FindString("ALERT_7037"),
                    () => {
                        SystemManager.Instance.MoveToStoreSite();
                        SystemManager.Instance.ExitGame();
                    });
            }
            else {
                PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ALERT_7030"),
                    string.Format("{0} : {1}", res.ToString(), res.result),
                    SystemManager.Instance.ExitGame);
            }
        }
    }

    private void ResponseNewRegister(RegisterNewResponse res, Action<string> completeCallback) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseNewRegister..!! : account : {0}  uuid : {1}, type : {2}</color>\n", res.account, res.uuid, res.type);
#endif // SHOW_LOG

        if (0 == res.result) {
            PopupManager.Instance.ClosePopup(PopupManager.PopupType.NickName);

            string accountName = res.account;
            PlayerPrefsManager.Instance.SaveAccountName(accountName);

            SystemManager.Instance.HttpStatus = HttpStatus.Registered;

            if (null != completeCallback) {
                completeCallback(accountName);
            }

            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ALERT_7030"),
                TStrings.Instance.FindString("ALERT_7034"), () => { }, 2.5f);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseNewRegister : RegisterNewResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse register : ()", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseNewLogin(LoginNewResponse res, Action registerCallback, Action<string, int, int> completeCallback) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseNewLogin..!! account : {0}\t uuid : {1}</color>\n", res.account, res.uuid);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetNewLoginResult(res);
            SystemManager.Instance.HttpStatus = HttpStatus.LoggedIn;

            if (null != completeCallback) {
                completeCallback(res.gameserverIp, res.gameserverPort, res.userseq);
            }

            LoginPlatformType loginType = SystemManager.Instance.PlayerLoginType;
            if (loginType == LoginPlatformType.Init) {
                loginType = (LoginPlatformType)res.type;
                SystemManager.Instance.PlayerLoginType = loginType;
                PlayerPrefsManager.Instance.SaveLoginPlatformType(loginType);

                RequestAgreementConfirmRequest();
            }
#if SHOW_LOG
            Debug.LogFormat("\t\t<color=green>WebHttp : ResponseNewLogin : Save curlogintype : {0}</color>\n", SystemManager.Instance.PlayerLoginType);
#endif // SHOW_LOG

            AdsManager.Instance.Init();
        }
        else if (res.result == 2) { // DB에 존재하지 않는 유저 uid
            if (null != registerCallback) {
                registerCallback();
            }
        }
        else if (res.result == 4) { // 보안 이슈
            Debug.LogErrorFormat("Web : ResponseNewLogin : LoginNewResponse security error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse login : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseNewLogin : LoginNewResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse login : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseLoadGame(LoadgameResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseLoadGame..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetLoadGameResult(res);
            SystemManager.Instance.HttpStatus = HttpStatus.LoadedGame;
            GameDataManager.Instance.SetShopInfo(res.m_shopinfo);
            GameDataManager.Instance.SetSkypassInfo(res.m_seasonQuestInfo);
            GameDataManager.Instance.SetClanBossInfo(res.boss);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseLoadGame : LoadgameResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse load game : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseTutorialInfos(TutorialInfoResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseTutorialInfos..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetTutorialData(res.items);
            SystemManager.Instance.HttpStatus = HttpStatus.TutorialData;
        }
        else {
            Debug.LogErrorFormat("Web : ResponseTutorialInfos : TutorialInfosResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse tutorial infos : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseTutorialSave(TutorialConfirmResponse res, Action completeCallback) {
#if SHOW_LOG
        if (TutorialSaveType.Chapter == (TutorialSaveType)res.type) {
            Debug.LogFormat("\t\t<color=green>WebHttp : ResponseTutorialSave..!! \t [ type : {0} / id : {1}({2}) / status : {3} ]</color>\n",
                (TutorialSaveType)res.type, res.tutorialId, (TutorialChapterType)res.tutorialId, 1 == res.status);
        }
        else if (TutorialSaveType.LobbyBuilding == (TutorialSaveType)res.type) {
            Debug.LogFormat("\t\t<color=green>WebHttp : ResponseTutorialSave..!! \t [ type : {0} / id : {1}({2}) / status : {3} ]</color>\n",
                (TutorialSaveType)res.type, res.tutorialId, (BuildingType)res.tutorialId, 1 == res.status);
        }
        else if (TutorialSaveType.Skip == (TutorialSaveType)res.type) {
            Debug.LogFormat("\t\t<color=green>WebHttp : ResponseTutorialSave..!! \t [  type : {0} / status : {1} ]</color>\n",
                (TutorialSaveType)res.type, 1 == res.status);
        }
#endif // SHOW_LOG

        if (0 == res.result) {
            bool status = 1 == res.status;
            if (status) {
                if (TutorialSaveType.Chapter == (TutorialSaveType)res.type) {
                    PlayerManager.Instance.SetTutorialChapterConfirm((TutorialChapterType)res.tutorialId, status);
                }
                else if (TutorialSaveType.LobbyBuilding == (TutorialSaveType)res.type) {
                    PlayerManager.Instance.SetTutorialBuildingConfirm((BuildingType)res.tutorialId, status);
                }
                else if (TutorialSaveType.Skip == (TutorialSaveType)res.type) {
                    PlayerManager.Instance.SetAllTutorialChapterConfirm();
                }

                if (null != completeCallback) {
                    completeCallback();
                }
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponseTutorialChapterConfirm : ConfirmTutorialChapterResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse tutorial confirm : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseLobbyInfo(LobbyInfoResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseLobbyInfo..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            LobbySceneUIManager.Instance.ResponseLobbyInfo(res);
            SystemManager.Instance.HttpStatus = HttpStatus.ReceivedLobbyInfos;
        }
        else {
            Debug.LogErrorFormat("Web : ResponseLobbyInfo : LobbyInfoResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse lobby info : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseStageList(StageListResponse res, bool waitLobbyInfo) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseStageList..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            if (waitLobbyInfo) {
                StartCoroutine(SystemManager.Instance.WaitAndMoveStageList(res));
            }
            else {
                GameDataManager.Instance.ResponseStageList(res);
                LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.BattleSection, LobbyMainType.World);
                LoadAnimManager.Instance.StopLoadingAnimation(LoadAnimType.NoneType);
                TutorialManager.Instance.ShowNextTutorial();
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponseStageList : StageListResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse stage list : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponsePveStart(PveStartResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponsePveStart..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            SystemManager.Instance.ResponsePVEStart(res);
        }
        else {
            Debug.LogErrorFormat("Web : ResponsePveStart : PveStartResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse start pve : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponsePveEnd(PveEndResponse res) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponsePveEnd..!!\t\t[ {0} ]</color>\n", (ResultType)res.gameResult);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetPvEBattleRewards(res);
            PlayerManager.Instance.SetWantedQuests(res.m_wantedQuests);
            BattleManager.Instance.ResponseResult(res);
        }
        else {
            Debug.LogErrorFormat("Web : ResponsePveEnd : PveEndResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse end pve : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponsePvETicket(PveTicketResponse res, Action<PveTicketResponse> completeCallback) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponsePvETicket..!!\t\t[ {0} ]</color>\n", res.result);
#endif // SHOW_LOG

        if (0 == res.result) {
            int restTicketCount = PlayerManager.Instance.GetItemAmount(Common.AutoBattleTicketItemID) - res.use_key;
            PlayerManager.Instance.SetItemAmount(Common.AutoBattleTicketItemID, restTicketCount);
            PlayerManager.Instance.SetAssetAmount(AssetType.PvE_Key, res.key);
            PlayerManager.Instance.UpdatePvEKeyAmountAndKeyRegenTime(res.now, res.keyRegenTime);

            PlayerManager.Instance.SetBattleWinRewards(res.userExp, res.userLevelup, res.LevelupItems, res.items, null);
            PlayerManager.Instance.SetWantedQuests(res.m_wantedQuests);

            if (null != completeCallback) {
                completeCallback(res);
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponsePvETicket : PveTicketResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse pve ticket : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponsePvERestart(PveReStartResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponsePvERestart..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetAssetAmount(AssetType.Jewel, res.gem);
            BattleManager.Instance.SetBattleContinues();
        }
        else {
            Debug.LogErrorFormat("Web : ResponsePvERestart : PvERestart response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse restart pve : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseBattleItemUse(BattleItemUseResponse res, Action<int> completeCallback) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseBattleItemUse..!! \t [ deckNum : {0} / itemID : {1} ]</color>\n", res.deckNum, res.itemId);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SubtractItems(res.itemId, 1);

            if (null != completeCallback) {
                completeCallback(res.itemId);
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponseBattleItemUse : BattleItemUse response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse battle item use : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseMailList(MailListResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseMailList..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseMailList(res);

            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.MailList);

            TutorialManager.Instance.ShowNextTutorial();
        }
        else {
            Debug.LogErrorFormat("Web : ResponseMailList : MailListResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse mail list : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseMailConfirm(MailConfirmResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseMailConfirm..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseMailConfirm(res);

            LobbySceneUIManager.Instance.CurrentMenuUI.SetShow();

            TutorialManager.Instance.ShowNextTutorial();
        }
        else {
            Debug.LogErrorFormat("Web : ResponseMailConfirm : MailConfirmResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse confirm mail : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseMailDelete(MailDeleteResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseMailDeleted..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.DeleteMail(res.mailId);

            LobbySceneUIManager.Instance.CurrentMenuUI.HideSubUIsByESCBack();
            LobbySceneUIManager.Instance.CurrentMenuUI.SetShow();
        }
        else {
            Debug.LogErrorFormat("Web : ResponseMailDelete : MailDeleteResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse delete mail : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseCardChange(CardChangeResponse res, bool updateTeamEditUI) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseCardChange..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseCardChange(res);
            LobbySceneUIManager.Instance.RefreshUIDeckChanged(updateTeamEditUI);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseCardChange : CardChangeResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse change card : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseBattleItemChange(BattleItemChangeResponse res, bool updateTeamEditUI) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseBattleItemChange..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseBattleItemChange(res);
            LobbySceneUIManager.Instance.RefreshUIDeckChanged(updateTeamEditUI);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseBattleItemChange : BattleItemChangeResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse change battle item : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseDeckChange(DeckChangeResponse res, DeckType deckType, bool updateTeamEditUI) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseDeckChange..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseDeckChange(res, deckType);
            LobbySceneUIManager.Instance.RefreshUIDeckChanged(updateTeamEditUI);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseDeckChange : DeckChangeResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse change deck : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseChangeDefenceDeckFormation(DeckFormationResponse res, bool updateTeamEditUI) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseChangeDefenceDeckFormation..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseDeckFormation(res);
            LobbySceneUIManager.Instance.RefreshUIDeckChanged(updateTeamEditUI);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseChangeDefenceDeckFormation error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse change defence deck formation : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseCardUpgrade(CardUpgradeResponse res) {
#if SHOW_LOG
        MCard upgradedCard = res.tarCard;
        HeroData heroData = PlayerManager.Instance.GetHeroDataByUniqueID(upgradedCard.m_id);
        TCard tCard = TCards.Instance.Find(upgradedCard.m_id);
        Debug.LogFormat("\t<color=white>[ RECEIVED ] WebHttp : ResponseCardUpgrade..!!\t[ Request {0}\t/ target Card UID : {1}\t/ Grade : {2}\t/ Awaken : {3}\t/ level : {4}\t/ exp : {5}\t SkillLevel : {6} ]</color>\n",
            (CardUpgradeType)res.upgradeType, upgradedCard.m_id, heroData.CardData._grade, upgradedCard.m_gaksung, upgradedCard.m_level, upgradedCard.m_exp, upgradedCard.m_skillLevel);
#endif // SHOW_LOG

        if (0 == res.result) {
            int preMythrilAmount = PlayerManager.Instance.GetAssetAmount(AssetType.Mythril);
            int preSpiritAmount = PlayerManager.Instance.GetAssetAmount(AssetType.SpritStone);

            HeroData prevHeroData = PlayerManager.Instance.GetHeroDataByUniqueID(res.tarCard.m_id);
            PlayerManager.Instance.UpdateCharactersData(res.tarCard, res.resCardIds);

            // upgradeType 0:레벨업, 1:각성 레벨업, 2:스킬 레벨업
            if (0 == res.upgradeType) {
                PlayerManager.Instance.SetAssetAmount(AssetType.SpritStone, res.food);
                LobbySceneUIManager.Instance.HeroLevelUpCompleteCallback(prevHeroData);
            }
            else if (1 == res.upgradeType) {
                PlayerManager.Instance.SetAssetAmount(AssetType.SpritStone, res.food);
                PlayerManager.Instance.SubtractItems(res.resources);
                LobbySceneUIManager.Instance.HeroAwakeningCompleteCallback(prevHeroData);
            }
            else if (2 == res.upgradeType) {
                PlayerManager.Instance.SetAssetAmount(AssetType.Mythril, res.iron);
                PlayerManager.Instance.SubtractItems(res.resources);
                LobbySceneUIManager.Instance.HeroSkillLevelUpCompleteCallback(prevHeroData);
                LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
            }
#if SHOW_LOG
            int afterMythrilAmount = PlayerManager.Instance.GetAssetAmount(AssetType.Mythril);
            int afterSpiritAmount = PlayerManager.Instance.GetAssetAmount(AssetType.SpritStone);
            Debug.LogFormat("WebHttp : ResponseCardUpgrade : Change SpiritStone Amount [ prev : {0} / after : {1}\t/ changed : {2} ]\t Change Mythril Amount [ prev : {3} / after : {4} / changed : {5} ]\n",
                            preSpiritAmount, afterSpiritAmount, preSpiritAmount - afterSpiritAmount, preMythrilAmount, afterMythrilAmount, preMythrilAmount - afterMythrilAmount);

            List<MMaterial> materials = res.resources;
            for (int i = 0, count = materials.Count; i < count; ++i) {
                Debug.LogFormat("WebHttp : ResponseCardUpgrade : Consume Material :  [ itemID : {0}\t/ amount : {1} ]\n", materials[i].m_Id, materials[i].m_amount);
            }
#endif // SHOW_LOG
        }
        else {
            Debug.LogErrorFormat("Web : ResponseCardUpgrade : CardUpgradeResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse upgrade card : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseCardFavorite(CardLockResponse res, Action completeCallback) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseCardFavorite..!!</color>\t\t[ cardID : {0} / lockState : {1} ]\n", res.cardId, res.locked);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetCardFavorite(res.cardId, res.locked == 1);

            if (null != completeCallback) {
                completeCallback();
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponseCardFavorite : CardFavoriteResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse card favorite : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseDeckName(DeckNameResponse res, System.Action<string> completeCallback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseDeckName..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ChangeTeamName(res.deckNum, res.teamName);
            if (null != completeCallback) {
                completeCallback(res.teamName);
            }
            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
        }
        else {
            Debug.LogErrorFormat("Web : ResponseDeckName : DeckNameResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse deck name : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseBoxOpen(BoxOpenResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseBoxOpen..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            //LobbySceneUIManager.Instance.ResponseStageList(res);

            //public MBox box = new MBox();
            //public List<MItem> items = new List<MItem>();
            //public int spendGem = 0;
        }
        else {
            Debug.LogErrorFormat("Web : ResponseBoxOpen : BoxOpenResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse open box : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponsePvPLobbyInfo(PvpLobbyInfoResponse res, Action afterCallback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponsePvPLobbyInfo..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            LobbySceneUIManager.Instance.SetPvPLobbyInfo(res);

            if (null != afterCallback) {
                afterCallback();
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponsePvPLobbyInfo : ResponsePvPLobbyInfo error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse pvp lobby info : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseSeasonRank(SeasonRankResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseSeasonRank..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetRankResponse(res);
            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
        }
        else {
            Debug.LogErrorFormat("Web : ResponseSeasonRank : SeasonRankResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse season rank : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponsePvPSearch(PvpSearchResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponsePvPSearch..!!</color>\n");
#endif // SHOW_LOG

        //error code result   -1:통신오류, 0:성공, 1:세션없음, 2: , 3:대전상대 검색실패, 4: 재검색 골드 부족, 5:상대계정검색실패
        if (0 == res.result) {
            PlayerManager.Instance.SetAssetAmount(AssetType.SpritStone, res.food);
            SystemManager.Instance.SetPvPOpponent(res, false);

            if (LobbySceneUIManager.Instance.CurrentMainType == LobbyMainType.PvP) {
                if (LobbySceneUIManager.Instance.CurrentSubType != LobbySubType.ArenaBattleReady) {
                    LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.ArenaBattleReady);
                }
                else {
                    PvPBattleReadyUI pvpReadyUI = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.ArenaBattleReady) as PvPBattleReadyUI;
                    if (null != pvpReadyUI) {
                        pvpReadyUI.SetShow();
                    }
                    else {
                        Debug.LogError("Can't get PvPBattleReadyUI..!!\n");
                    }
                }
            }
        }
        else if (3 <= res.result && 5 >= res.result) {
            string contents = string.Empty;
            if (res.result == 3 || res.result == 5) {
                contents = TStrings.Instance.FindString("ERROR_15001");
            }
            else if (res.result == 4) {
                contents = TStrings.Instance.FindString("ERROR_15002");
            }

            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15000"), string.Format(contents, string.Format("({0})", res.result)), null);
        }
        else {
            Debug.LogErrorFormat("Web : ResponsePvPSearch : ResponsePvPSearch error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse search pvp : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponsePvPStart(PvpStartResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponsePvPStart..!!</color>\n");
#endif // SHOW_LOG

        //error code result   -1:통신오류, 0:성공, 1:세션없음, 2:재도전 횟수 부족, 3:재도전유저없음, 4: PVP열쇠 부족

        if (0 == res.result) {
            if (PvPMatchType.ReMatch == (PvPMatchType)res.gameType) { // 0:신규 도전, 1:패배시 재도전
#if SHOW_LOG
                Debug.LogWarning("WebHttp : ResponsePvPStart : 재도전 일 때 재도전 횟수 차감 및 관련 체크 필요..!!\n");        // TODO
#endif // SHOW_LOG
            }

            PlayerManager.Instance.SetAssetAmount(AssetType.PvP_Key, res.pvpKey);
            PlayerManager.Instance.UpdatePvPKeyAmountAndKeyRegenTime(res.now, res.pvpKeyRegenTime);

            SceneType targetSceneType = SceneType.Battle_PvP;

#if SHOW_LOG
            Debug.LogFormat("Called Load Battle Scene! Now load [ <color=cyan>{0} ]</color>\n", targetSceneType);
#endif // SHOW_LOG

            SystemManager.Instance.ChangeScene(targetSceneType, true);
        }
        else if (res.result == 2) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15003"), string.Format(TStrings.Instance.FindString("ERROR_15004"), string.Format("({0})", res.result)), null);
        }
        else if (res.result == 3) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15003"), string.Format(TStrings.Instance.FindString("ERROR_15013"), string.Format("({0})", res.result)), null);
        }
        else if (res.result == 4) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15003"), string.Format(TStrings.Instance.FindString("ERROR_15005"), string.Format("({0})", res.result)), null);
        }
        else {
            Debug.LogErrorFormat("Web : ResponsePvPStart : ResponsePvPStart error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse start pvp : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponsePvPEnd(PvpEndResponse res) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponsePvPEnd..!!\t\t[ {0} ]</color>\n", (ResultType)res.gameResult);
#endif // SHOW_LOG

        //error code result   -1:통신오류, 0:성공, 1:세션없음, 2:대전상대없음,  3:대전상대계정없음,  4:본계정없음,

        if (0 == res.result) {
            PlayerManager.Instance.SetPvPBattleRewards(res);
            PlayerManager.Instance.SetWantedQuests(res.m_wantedQuests);
            BattleManager.Instance.ResponseResult(res);
        }
        else if (res.result == 2) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15000"), string.Format(TStrings.Instance.FindString("ERROR_15010"), string.Format("({0})", res.result)), null);
        }
        else if (res.result == 3) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15000"), string.Format(TStrings.Instance.FindString("ERROR_15011"), string.Format("({0})", res.result)), null);
        }
        else if (res.result == 4) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15000"), string.Format(TStrings.Instance.FindString("ERROR_15012"), string.Format("({0})", res.result)), null);
        }
        else {
            Debug.LogErrorFormat("Web : ResponsePvpEnd : PvpEndResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse end pvp : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponsePVPRecordList(PvpRecordListResponse res, Action afterCallback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponsePVPRecord..!!</color>\n");
#endif // SHOW_LOG

        //error code result   -1:통신오류, 0:성공, 1:세션없음

        if (0 == res.result) {
            GameDataManager.Instance.ResponsePvPRecordList(res);

            if (null != afterCallback) {
                afterCallback();
            }
        }
        else {
            Debug.LogErrorFormat("WebHttp : ResponsePVPRecord : PVPRecord Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse pvp record : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponsePvpRevenge(PvpRevengeResponse res, string strRecordID) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponsePvpRevenge..!!</color>\n");
#endif // SHOW_LOG

        //error code result   -1:통신오류, 0:성공, 1:세션없음, 2:전적ID 없음, 3:복수상대계정없음,

        if (0 == res.result) {
            if (1 == res.vsConnect) {   //0:서버 비접속중, 1:서버접속중
                PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("PVP_6079"), TStrings.Instance.FindString("PVP_6080"), null);
                return;
            }

            GameDataManager.Instance.SetPvPRevengeRecordID(strRecordID);
            SystemManager.Instance.SetPvPOpponent(res, true);

            if (LobbySceneUIManager.Instance.CurrentMainType == LobbyMainType.PvP) {
                if (LobbySceneUIManager.Instance.CurrentSubType != LobbySubType.ArenaBattleReady) {
                    LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.ArenaBattleReady);
                }
                else {
                    PvPBattleReadyUI pvpReadyUI = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.ArenaBattleReady) as PvPBattleReadyUI;
                    if (null != pvpReadyUI) {
                        pvpReadyUI.SetShow();
                    }
                    else {
                        Debug.LogError("Can't get PvPBattleReadyUI..!!\n");
                    }
                }
            }
        }
        else if (res.result == 2) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15000"), string.Format(TStrings.Instance.FindString("ERROR_15008"), string.Format("({0})", res.result)), null);
        }
        else if (res.result == 3) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15000"), string.Format(TStrings.Instance.FindString("ERROR_15011"), string.Format("({0})", res.result)), null);
        }
        else {
            Debug.LogErrorFormat("WebHttp : ResponsePvpRevenge : PvpRevenge Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse PvpRevenge : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseRevengeStart(PvpRevengeStartResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseRevengeStart..!!</color>\n");
#endif // SHOW_LOG

        //error code result   -1:통신오류, 0:성공, 1:세션없음, 2:전적ID 없음, 3:복수가능횟수 0, 4:상대전적없음, 5:PVP키부족

        if (0 == res.result) {
            PlayerManager.Instance.SetAssetAmount(AssetType.PvP_Key, res.pvpKey);
            PlayerManager.Instance.UpdatePvPKeyAmountAndKeyRegenTime(res.now, res.pvpKeyRegenTime);

            SceneType targetSceneType = SceneType.Battle_PvP;
#if SHOW_LOG
            Debug.LogFormat("Called Load Battle Scene! Now load [ <color=cyan>{0} ]</color>\n", targetSceneType);
#endif // SHOW_LOG

            SystemManager.Instance.ChangeScene(targetSceneType, true);
        }
        else if (res.result == 2 || res.result == 4) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15000"), string.Format(TStrings.Instance.FindString("ERROR_15008"), string.Format("({0})", res.result)), null);
        }
        else if (res.result == 3) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15000"), string.Format(TStrings.Instance.FindString("ERROR_15014"), string.Format("({0})", res.result)), null);
        }
        else if (res.result == 5) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15000"), string.Format(TStrings.Instance.FindString("ERROR_15005"), string.Format("({0})", res.result)), null);
        }
        else {
            Debug.LogErrorFormat("WebHttp : ResponseRevengeStart : RevengeStart Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse RevengeStart : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponsePvpRecordInfo(PvpRecordInfoResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseRevengeStart..!!</color>\n");
#endif // SHOW_LOG

        //error code result   -1:통신오류, 0:성공, 1:세션없음,2:상대계정없음,

        if (0 == res.result) {
            //GameDataManager.Instance.SetOpponentPlayerInfo(res);
            GameDataManager.Instance.SetUserInfo(res);
            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.OpponentPlayerInfo);
        }
        else if (res.result == 2) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15000"), string.Format(TStrings.Instance.FindString("ERROR_15014"), string.Format("({0})", res.result)), null);
        }
        else {
            Debug.LogErrorFormat("WebHttp : ResponsePvpRecordInfo : Pvp Record Info error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15011")), null);
        }
    }

    private void ResponsePvPSeasonReward(SeasonRewardResponse res) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponsePvPSeasonReward..!!\t\t[ {0} ]</color>\n", (ResultType)res.result);
#endif // SHOW_LOG

        //error code result   -1:통신오류, 0:성공, 1:세션없음, 2 : 전 시즌에 PVP 참가하지 않은 유저, 3 : 이미 보상 받은 유저

        if (0 == res.result) {
            if (0 < res.RinkItems.Count) {
                PlayerManager.Instance.AddMItems(res.RinkItems);
            }
            if (0 < res.TierItems.Count) {
                PlayerManager.Instance.AddMItems(res.TierItems);
            }

            LobbySceneUIManager.Instance.ShowPvPSeasonResultReward(res);
        }
        else if (res.result == 2 || res.result == 3) {
            Debug.LogFormat("2 : 전 시즌에 PVP 참가하지 않은 유저, 3 : 이미 보상 받은 유저 : [ {0} ]\n", res.result);
        }
        else {
            Debug.LogErrorFormat("Web : ResponsePvPSeasonReward : PvPSeasonRewardResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse PvP season reward : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseStructuresZoneList(StrucZoneListResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseBuildingZoneList..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetStructuresZoneList(res.zones);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseStructuresZoneList : Structure Zone List Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse structures zone list : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseStartBuildStructure(StrucBuildResponse res) {
#if SHOW_LOG
        string buildStructData = string.Format("ZoneID : {0}({1}) \t/ StructID : {2} \t/ structIndex : {3} \t/ TableID : {4} \t/ ActionType : {5}({6}) \t/ actionEndTime : {7}",
            res.zoneId, (BuildingType)res.zoneId, res.struc.m_strucId, res.struc.m_strucIndex, res.struc.m_strucRefId, res.struc.m_actionType, (ConstructingType)res.struc.m_actionType, Common.ConvertJavaMillisecondToDateTimeUTC(res.struc.m_actionMaxTime).ToLocalTime());
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseStartBuildStructure..!!</color>\t\t[ result : {0} \t/ zone : {1}({2}) ] \t [ {3} ]\n", res.result, res.zoneId, (BuildingType)res.zoneId, buildStructData);
        TStrucInfo buildingTableData = TStrucInfos.Instance.Find(res.zoneId);
        Debug.LogFormat("Construct Building Table Data : [ tableIndex : {0} \t/ name : {1} \t/ costMythril : {2} \t/ costSpritStone : {3} \t/ constructing Sec : {4} ]\n",
            buildingTableData._index, buildingTableData.LangTypeName, buildingTableData._buildAndConversionCost_Mythril, buildingTableData._buildAndConversionCost_SpritStone, buildingTableData._buildAndConversionPeriodSec);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetStructure(res.zoneId, res.struc);

            PlayerManager.Instance.SetAssetAmount(AssetType.Mythril, res.iron);
            PlayerManager.Instance.SetAssetAmount(AssetType.SpritStone, res.food);

            LobbySceneUIManager.Instance.CurrentMenuUI.HideSubUIsByESCBack();
            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
        }
        else if (res.result == 2) {
            PopupManager.Instance.ShowOKPopup(
                TStrings.Instance.FindString("BATTLE_16013"),
                TStrings.Instance.FindString("BATTLE_16013"), null);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseStartBuildStructure : Start Build Structure Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse start build structure : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseConfirmStructureActionState(StrucConfirmResponse res, bool hidePrevPopups) {
#if SHOW_LOG
        string buildStructData = string.Format("structIndex : {0} \t/ TableID : {1} \t/ Level : {2} \t/ ActionType : {3}({4}) \t/ actionEndTime : {5} \t/ resourceMaxTime : {6} \t/ resourceSave : {7}",
            res.struc.m_strucIndex, res.struc.m_strucRefId, res.struc.m_strucLevel, res.struc.m_actionType, (ConstructingType)res.struc.m_actionType, Common.ConvertJavaMillisecondToDateTimeUTC(res.struc.m_actionMaxTime).ToLocalTime(), Common.ConvertJavaMillisecondToDateTimeUTC(res.struc.m_resouceMaxTime).ToLocalTime(), res.struc.m_resourceSave);
        Debug.LogFormat("[ {0} ]\t\t<color=green>WebHttp : ResponseConfirmStructureActionState..!!</color>\t\t[ result : {1} \t/ zoneID : {2}({3}) \t/ strucID : {4}({5}) \t/ usedDiamonds : {6} ] \t [ {7} ]\n", DateTime.UtcNow, res.result, res.zoneId, (BuildingType)res.zoneId, res.strucId, res.struc.m_strucId, res.resUse, buildStructData);
#endif // SHOW_LOG

        if (0 == res.result) {
            if (0 < res.resUse) {
                PlayerManager.Instance.SubtractAssetAmount(AssetType.Jewel, res.resUse);
            }

            PlayerManager.Instance.SetStructure(res.zoneId, res.struc);

            if (hidePrevPopups) {
                LobbySceneUIManager.Instance.CurrentMenuUI.HideSubUIsByESCBack();
            }

            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
        }
        else if (res.result == 2) { // 2:건물 없음
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16014"), TStrings.Instance.FindString("BATTLE_16014"), null);
        }
        else if (res.result == 3) { // 3:건설,업그레이드건설, 레벨업 중이 아님
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BUILD_17011"), TStrings.Instance.FindString("BUILD_17053"), null);
        }
        else if (res.result == 4) { // 4:시간부족
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16022"), TStrings.Instance.FindString("BATTLE_16022"), null);
        }
        else if (res.result == 5) { // 5:재화부족
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16023"), TStrings.Instance.FindString("BATTLE_16023"), null);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseConfirmStructureActionState : Confirm Structure Action State Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse confirm structure action state : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    /// <summary>
    /// 건물의 건설 / 업그레이드 (레벨업) 넘기기 서버 응답
    /// </summary>
    /// <param name="res"></param>
    /// <param name="hidePrevPopups"></param>
    private void ResponseStructSkipByGem(StrucSkipGemResponse res, bool hidePrevPopups) {
#if SHOW_LOG
        string buildStructData = string.Format("structIndex : {0} \t/ TableID : {1} \t/ Level : {2} \t/ ActionType : {3}({4}) \t/ actionEndTime : {5} \t/ resourceMaxTime : {6} \t/ resourceSave : {7}",
            res.struc.m_strucIndex, res.struc.m_strucRefId, res.struc.m_strucLevel, res.struc.m_actionType, (ConstructingType)res.struc.m_actionType, Common.ConvertJavaMillisecondToDateTimeUTC(res.struc.m_actionMaxTime).ToLocalTime(), Common.ConvertJavaMillisecondToDateTimeUTC(res.struc.m_resouceMaxTime).ToLocalTime(), res.struc.m_resourceSave);
        Debug.LogFormat("[ {0} ]\t\t<color=green>WebHttp : ResponseStructSkipByGem..!!</color>\t\t[ result : {1} \t/ zoneID : {2}({3}) \t/ strucID : {4}({5}) \t/ TotalDiamond : {6} ] \t [ {7} ]\n", DateTime.UtcNow, res.result, res.zoneId, (BuildingType)res.zoneId, res.strucId, res.struc.m_strucId, res.gem, buildStructData);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetAssetAmount(AssetType.Jewel, res.gem);

            if (LobbySubType.WatchTower == LobbySceneUIManager.Instance.CurrentSubType) {
                LobbySceneUIManager.Instance.CurrentMenuUI.StopAllCoroutines();
            }

            PlayerManager.Instance.SetStructure(res.zoneId, res.struc);

            if (hidePrevPopups) {
                LobbySceneUIManager.Instance.CurrentMenuUI.HideSubUIsByESCBack();
            }

            LobbySceneUIManager.Instance.UpdateAssetInfos();
            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
        }
        else if (res.result == 2) { // 2:건물 없음
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16014"), TStrings.Instance.FindString("BATTLE_16014"), null);
        }
        else if (res.result == 3) { // 3:건설,업그레이드건설, 레벨업 중이 아님
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16018"), TStrings.Instance.FindString("BATTLE_16018"), null);
        }
        else if (res.result == 4) { // 4:시간부족
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16022"), TStrings.Instance.FindString("BATTLE_16022"), null);
        }
        else if (res.result == 5) { // 5:재화부족
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16023"), TStrings.Instance.FindString("BATTLE_16023"), null);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseStructSkipByGem : Struct Skip By Gem Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse struct skip by gem : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private List<MItem> GetGroupedItems(List<MItem> items) {
        List<MItem> retItems = new List<MItem>();
        for (int i = 0, count = items.Count; i < count; ++i) {
            MItem item = items[i];
            bool addedItem = false;
            for (int j = 0, countJ = retItems.Count; j < countJ; ++j) {
                MItem groupedItem = retItems[j];
                if (item.m_type == groupedItem.m_type && item.m_id == groupedItem.m_id && item.m_refId == groupedItem.m_refId) {
                    groupedItem.m_amount += item.m_amount;
                    addedItem = true;
                    break;
                }
            }

            if (!addedItem) {
                retItems.Add(item);
            }
        }

        return retItems;
    }

    private void ResponseHarvest(StrucHarvestResponse res, System.Action<List<MItem>> procHarvestItemsCallback) {
#if SHOW_LOG
        string buildStructData = string.Format("structIndex : {0} \t/ TableID : {1} \t/ Level : {2} \t/ ActionType : {3}({4}) \t/ actionEndTime : {5} \t/ resourceMaxTime : {6} \t/ iron(mythril)EndTime : {7}({8}) \t/ food(spritstone)EndTime : {9}({10}) \t/ soldier(godstone)EndTime : {11}({12})",
            res.struc.m_strucIndex, res.struc.m_strucRefId, res.struc.m_strucLevel, res.struc.m_actionType, (ConstructingType)res.struc.m_actionType,
            Common.ConvertJavaMillisecondToDateTimeUTC(res.struc.m_actionMaxTime), Common.ConvertJavaMillisecondToDateTimeUTC(res.struc.m_resouceMaxTime),
            Common.ConvertJavaMillisecondToDateTimeUTC(res.struc.m_ironCreateCompletTime), Common.ConvertJavaMillisecondToDateTimeUTC(res.struc.m_ironCreateCompletTime),
            Common.ConvertJavaMillisecondToDateTimeUTC(res.struc.m_foodCreateCompletTime), Common.ConvertJavaMillisecondToDateTimeUTC(res.struc.m_foodCreateCompletTime),
            Common.ConvertJavaMillisecondToDateTimeUTC(res.struc.m_soldierCreateCompletTime), Common.ConvertJavaMillisecondToDateTimeUTC(res.struc.m_soldierCreateCompletTime));
        string harvestData = "Harvested Items : ";
        for (int i = 0, count = res.items.Count; i < count; ++i) {
            harvestData = String.Format("{0}itemType : {1}({2}) \t/ TableID : {3} \t/ amount : {4} \t/ ",
            harvestData, res.items[i].m_type, (AssetType)res.items[i].m_type, AssetType.CardType == (AssetType)res.items[i].m_type ? res.items[i].m_refId : res.items[i].m_id, res.items[i].m_amount);
        }
        Debug.LogFormat("[ {0} ]\t\t<color=green>WebHttp : ResponseHarvest..!!</color>\t\t[ result : {1} \t/ zoneID : {2}({3}) ] \t [ {4} ] \t [ {5} ]\n", DateTime.UtcNow, res.result, res.zoneId, (BuildingType)res.zoneId, buildStructData, harvestData);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetStructure(res.zoneId, res.struc);

            PlayerManager.Instance.AddMItems(res.items);

            //LobbySceneUIManager.Instance.UpdateAssetInfos();
            //LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();

            if (null != procHarvestItemsCallback) {
                procHarvestItemsCallback(GetGroupedItems(res.items));
            }
        }
        else if (res.result == 2) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16014"), TStrings.Instance.FindString("BATTLE_16014"), null);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseHarvest : Harvest Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse harvest : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseHarvestAll(StrucAllHarvestResponse res, System.Action<List<MItem>> procHarvestItemsCallback) {
#if SHOW_LOG
        string structsData = string.Empty;
        for (int i = 0, count = res.strucs.Count; i < count; ++i) {
            structsData = string.Format("{0}structIndex : {1} \t/ TableID : {2} \t/ Level : {3} \t/ ActionType : {4}({5}) \t/ actionEndTime : {6} \t/ resourceMaxTime : {7} \t/ resourceSave : {8} \t/ ",
                structsData, res.strucs[i].m_strucIndex, res.strucs[i].m_strucRefId, res.strucs[i].m_strucLevel, res.strucs[i].m_actionType, (ConstructingType)res.strucs[i].m_actionType, Common.ConvertJavaMillisecondToDateTimeUTC(res.strucs[i].m_actionMaxTime).ToLocalTime(), Common.ConvertJavaMillisecondToDateTimeUTC(res.strucs[i].m_resouceMaxTime).ToLocalTime(), res.strucs[i].m_resourceSave);
        }
        string harvestsData = string.Empty;
        for (int i = 0, count = res.items.Count; i < count; ++i) {
            harvestsData = String.Format("{0}itemType : {1}({2}) \t/ TableID : {3} \t/ amount : {4} \t/ ",
            harvestsData, res.items[i].m_type, (AssetType)res.items[i].m_type, AssetType.CardType == (AssetType)res.items[i].m_type ? res.items[i].m_refId : res.items[i].m_id, res.items[i].m_amount);
        }
        Debug.LogFormat("[ {0} ]\t\t<color=green>WebHttp : ResponseHarvestAll..!!</color>\t\t[ result : {1} \t/ zoneID : {2}({3}) ] \t [ {4} ] \t [ {5} ]\n", DateTime.UtcNow, res.result, res.zoneId, (BuildingType)res.zoneId, structsData, harvestsData);
#endif // SHOW_LOG

        if (0 == res.result) {
            // 건물 데이터 업데이트
            for (int i = 0, count = res.strucs.Count; i < count; ++i) {
                PlayerManager.Instance.SetStructure(res.zoneId, res.strucs[i]);
            }

            // 수확물 업데이트
            PlayerManager.Instance.AddMItems(res.items);

            // UI Refresh
            LobbySceneUIManager.Instance.UpdateAssetInfos();
            //LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();

            if (null != procHarvestItemsCallback) {
                procHarvestItemsCallback(GetGroupedItems(res.items));
            }
        }
        //else if (res.result == 2) {
        //    PopupManager.Instance.ShowOKPopup("지역 없음", "지역 없음", null);                // TODO string
        //}
        else {
            Debug.LogErrorFormat("Web : ResponseHarvestAll : Harvest All Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse harvest all : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseLevelUpStructure(StrucLevelupResponse res) {
#if SHOW_LOG
        string buildStructData = string.Format("structIndex : {0} \t/ TableID : {1} \t/ Level : {2} \t/ ActionType : {3}({4}) \t/ actionEndTime : {5} \t/ resourceMaxTime : {6} \t/ resourceSave : {7}",
            res.struc.m_strucIndex, res.struc.m_strucRefId, res.struc.m_strucLevel, res.struc.m_actionType, (ConstructingType)res.struc.m_actionType, Common.ConvertJavaMillisecondToDateTimeUTC(res.struc.m_actionMaxTime).ToLocalTime(), Common.ConvertJavaMillisecondToDateTimeUTC(res.struc.m_resouceMaxTime).ToLocalTime(), res.struc.m_resourceSave);
        Debug.LogFormat("[ {0} ]\t\t<color=green>WebHttp : ResponseLevelUpStructure..!!</color>\t\t[ result : {1} \t/ zoneID : {2}({3}) \t/ total food : {4} \t/ total Iron : {5} ] \t [ {6} ]\n",
            DateTime.UtcNow, res.result, res.zoneId, (BuildingType)res.zoneId, res.food, res.iron, buildStructData);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetStructure(res.zoneId, res.struc);
            BuildingType buildingType = (BuildingType)res.zoneId;

            if (0 < res.iron) {
                PlayerManager.Instance.SetAssetAmount(AssetType.Mythril, res.iron);
            }
            if (0 < res.food) {
                PlayerManager.Instance.SetAssetAmount(AssetType.SpritStone, res.food);
            }

            LobbySceneUIManager.Instance.UpdateAssetInfos();
            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
            LobbySceneUIManager.Instance.CurrentMenuUI.HideSubUIsByESCBack();
        }
        else if (res.result == 2) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16014"), TStrings.Instance.FindString("BATTLE_16014"), null);
        }
        else if (res.result == 3) { // 3:건설,업그레이드건설, 레벨업 중, 
            var iterZones = PlayerManager.Instance.StructuresZones.GetEnumerator();
            while (iterZones.MoveNext()) {
                List<StructureData> structruesData = iterZones.Current.Value;
                for (int i = 0, count = structruesData.Count; i < count; ++i) {
                    if (null == structruesData[i]) {
                        continue;
                    }

                    StructureData building = structruesData[i];
                    if (ConstructingType.Constructing == (ConstructingType)building._actionType) {
                        PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16015"), TStrings.Instance.FindString("BUILD_17024"), null);
                        return;
                    }
                    else if (ConstructingType.Upgrading == (ConstructingType)building._actionType) {
                        PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16015"), TStrings.Instance.FindString("BATTLE_16016"), null);
                        return;
                    }
                    else if (ConstructingType.LevelUping == (ConstructingType)building._actionType) {
                        PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16015"), TStrings.Instance.FindString("BATTLE_16017"), null);
                        return;
                    }
                }
            }

            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16015"), TStrings.Instance.FindString("BATTLE_16015"), null);
        }
        else if (res.result == 4) { // 4:더이상레벨업 못함
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16015"), TStrings.Instance.FindString("BUILD_17024"), null);
        }
        else if (res.result == 5 || res.result == 6) { // 5:음식재화부족 // 6:철재화부족
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16023"), TStrings.Instance.FindString("BUILD_17047"), null);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseLevelUpStructure : Level Up Structure Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse level up structure : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseStructResearchList(StrucResearchListResponse res, BuildingType buildingType) {
#if SHOW_LOG
        string researchsData = string.Empty;
        for (int i = 0, count = res.researchs.Count; i < count; ++i) {
            MResearch mResearch = res.researchs[i];
            researchsData = string.Format("{0}list Index : {1} : tableID : {2} \t/ actionType : {3}({4}) \t/ trainingReservedCount : {5} \t/ trainingEndCount : {6} \t/ actionEndTime : {7} \t\t /",
                researchsData, i, mResearch.m_refId, mResearch.m_actionType, (StructResearchTypes)mResearch.m_actionType, mResearch.m_trainingReserveCnt, mResearch.m_trainingEndCnt, Common.ConvertJavaMillisecondToDateTimeUTC(mResearch.m_actionMaxTime).ToLocalTime());
        }
        Debug.LogFormat("[ {0} ]\t\t<color=green>WebHttp : ResponseStructResearchList..!!</color>\t\t[ result : {1} \t/ structID : {2} ] \t Researches : [ {3} ]\n",
            DateTime.UtcNow, res.result, res.strucId, researchsData);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetStructureResearchs(buildingType, res.strucId, res.researchs);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseStructResearchList : Struct Research List Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse struct research list : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseStructResearchList(StrucResearchListResponse res, BuildingType buildingType, bool moveToMenu) {
#if SHOW_LOG
        string researchsData = string.Empty;
        for (int i = 0, count = res.researchs.Count; i < count; ++i) {
            MResearch mResearch = res.researchs[i];
            researchsData = string.Format("{0}list Index : {1} : tableID : {2} \t/ actionType : {3}({4}) \t/ trainingReservedCount : {5} \t/ trainingEndCount : {6} \t/ actionEndTime : {7} \t\t /",
                researchsData, i, mResearch.m_refId, mResearch.m_actionType, (StructResearchTypes)mResearch.m_actionType, mResearch.m_trainingReserveCnt, mResearch.m_trainingEndCnt, Common.ConvertJavaMillisecondToDateTimeUTC(mResearch.m_actionMaxTime).ToLocalTime());
        }
        Debug.LogFormat("[ {0} ]\t\t<color=green>WebHttp : ResponseStructResearchList..!!</color>\t\t[ result : {1} \t/ structID : {2} ] \t Researches : [ {3} ]\n",
            DateTime.UtcNow, res.result, res.strucId, researchsData);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetStructureResearchs(buildingType, res.strucId, res.researchs);

            if (moveToMenu) {
                LobbySceneUIManager.Instance.SelectedBuildingType = buildingType;
                LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.FactoryList);
            }
            else {
                LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
                LobbySceneUIManager.Instance.CheckCurrentSubMenuBack();
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponseStructResearchList : Struct Research List Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse struct research list : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseStartResearchOrTraining(StrucResearchOkResponse res, BuildingType buildingType, StructResearchTypes structResearchType) {
#if SHOW_LOG
        string materialsData = "Materials Data To Remove : [";
        if (0 < res.resourceIds.Count) {
            for (int i = 0, count = res.resourceIds.Count; i < count; ++i) {
                materialsData = string.Format("{0}Items ( ItemID : {1} \t/ amount : {2} ) \t/ ", materialsData, res.resourceIds[i].m_Id, res.resourceIds[i].m_amount);
            }
        }
        if (0 < res.weaponIds.Count) {
            for (int i = 0, count = res.weaponIds.Count; i < count; ++i) {
                materialsData = string.Format("{0}Weapons ( WeaponID : {1} \t/ amount : {2} ) \t/ ", materialsData, res.weaponIds[i].m_Id, res.weaponIds[i].m_amount);
            }
        }
        if (0 < res.cardIds.Count) {
            for (int i = 0, count = res.cardIds.Count; i < count; ++i) {
                materialsData = string.Format("{0}Cards ( CardID : {1} \t/ amount : {2} ) \t/ ", materialsData, res.cardIds[i].m_Id, res.cardIds[i].m_amount);
            }
        }
        materialsData = string.Format("{0} ]\n", materialsData);

        string researchData = string.Format("MResearch Data : [ researchTableID : {0} \t/ actionType : {1}({2}) \t/ trainingReservedCnt : {3} \t/ trainingEndCount : {4} \t/ actionEndTime : {5} ]\n",
            res.research.m_refId, res.research.m_actionType, (StructResearchTypes)res.research.m_actionType, res.research.m_trainingReserveCnt, res.research.m_trainingEndCnt, Common.ConvertJavaMillisecondToDateTimeUTC(res.research.m_actionMaxTime).ToLocalTime());

        Debug.LogFormat("[ {0} ]\t\t<color=green>WebHttp : ResponseStructResearchStart..!!</color>\t\t[ result : {1} \t/ structID : {2} \t/ researchTableID : {3} \t/ actionType : {4}({5}) \t/ restTotalFood : {6} \t/ restTotalIron : {7} \t/ restTotalSoldier : {8} ] \t {9} \t {10}\n",
            DateTime.UtcNow, res.result, res.strucId, res.reserRefId, res.actionType, (StructResearchTypes)res.actionType, res.food, res.iron, res.soldier, materialsData, researchData);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetStructureResearch(buildingType, res.strucId, res.research, structResearchType);

            PlayerManager.Instance.SubtractItems(res.resourceIds);
            PlayerManager.Instance.RemoveWeapons(res.weaponIds);
            PlayerManager.Instance.RemoveCards(res.cardIds);

            PlayerManager.Instance.SetAssetAmount(AssetType.SpritStone, res.food);
            PlayerManager.Instance.SetAssetAmount(AssetType.Mythril, res.iron);
            PlayerManager.Instance.SetAssetAmount(AssetType.GodStone, res.soldier);

            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
            LobbySceneUIManager.Instance.CheckCurrentSubMenuBack();
        }
        else {
            Debug.LogErrorFormat("Web : ResponseStructResearchStart : Struct Research Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse start research or training : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseHavestTraining(StrucResearchHavestResponse res, BuildingType buildingType, int structID, int researchID, int retrainingRefID, System.Action<int, int, List<MItem>> procHarvestItemsCallback) {
#if SHOW_LOG
        string debLog = string.Format("[ BuildingType : {0} \t/ structID : {1} \t/ researchID : {2} \t/ ]\t\t", buildingType, structID, researchID);
        //res.strucId
        for (int i = 0, count = res.items.Count; i < count; ++i) {
            debLog = string.Format("{0}[ ID : {1} \t/ type : {2} \t/ UID : {3} \t/ tableID : {4} \t/ amount : {5} ]\t/",
                debLog, res.items[i].m_id, (AssetType)res.items[i].m_type, res.items[i].m_id, res.items[i].m_refId, res.items[i].m_amount);
        }

        Debug.LogFormat("[ {0} ]\t\t<color=green>WebHttp : ResponseHavestTraining..!!</color>\t\t[ result : {1} ]\t\t{2}\n",
            DateTime.UtcNow, res.result, debLog);
#endif // SHOW_LOG

        if (0 == res.result) {
            List<MItem> groupedMItems = GetGroupedItems(res.items);
            PlayerManager.Instance.AddMItems(groupedMItems);
            PlayerManager.Instance.SetStructureResearch(buildingType, res.strucId, res.research, StructResearchTypes.Training);

            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();

            if (null != procHarvestItemsCallback) {
                procHarvestItemsCallback(researchID, retrainingRefID, groupedMItems);
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponseHavestTraining : havest training response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse harvest training : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseHarvestTrainingAll(StrucResearchAllHavestResponse res, int zoneID, System.Action<List<MItem>> afterCallback) {
#if SHOW_LOG
        string debLog = string.Empty;
        //res.strucId
        for (int i = 0, count = res.items.Count; i < count; ++i) {
            debLog = string.Format("{0}[ ID : {1} \t/ type : {2} \t/ UID : {3} \t/ tableID : {4} \t/ amount : {5} ]\t/",
                debLog, res.items[i].m_id, (AssetType)res.items[i].m_type, res.items[i].m_id, res.items[i].m_refId, res.items[i].m_amount);
        }

        Debug.LogFormat("[ {0} ]\t\t<color=green>WebHttp : ResponseHarvestTrainingAll..!!</color>\t\t[ result : {1} ]\t\t{2}\n",
            DateTime.UtcNow, res.result, debLog);
#endif // SHOW_LOG

        if (0 == res.result) {
            List<MItem> groupedMItems = GetGroupedItems(res.items);
            PlayerManager.Instance.AddMItems(groupedMItems);
            PlayerManager.Instance.ResetTrainings(res.strucresearchs, zoneID);

            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();

            if (null != afterCallback) {
                afterCallback(groupedMItems);
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponseHarvestTrainingAll : harvest training all response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse harvest training all : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    /// <summary>
    /// 연구 또는 훈련의 넘기기 서버 응답
    /// </summary>
    /// <param name="res"></param>
    /// <param name="structResearchType"></param>
    /// <param name="buildingType"></param>
    /// <param name="structID"></param>
    /// <param name="researchID"></param>
    /// <param name="retrainingTargetID"></param>
    /// <param name="procHarvestItemsCallback"></param>
    private void ResponseSkipResearchOrTrainingByGem(StrucResearchSkipGemResponse res, StructResearchTypes structResearchType, BuildingType buildingType, int structID, int researchID, int retrainingTargetID, System.Action<int, int, List<MItem>> procHarvestItemsCallback) {
#if SHOW_LOG
        string debugString = string.Format("\t\tResearch Info : [ TableID : {0} \t/ actionType : {1} \t/ reservedCount : {2} \t/ endCount : {3} \t/ actionEndTime : {4} ]",
            res.research.m_refId, (StructResearchTypes)res.research.m_actionType, res.research.m_trainingReserveCnt, res.research.m_trainingEndCnt, Common.ConvertJavaMillisecondToDateTimeUTC(res.research.m_actionMaxTime));
        Debug.LogFormat("[ {0} ]\t\t<color=green>WebHttp : ResponseSkipStructResearchByGem..!!</color>\t\t[ result : {1} \t/ reset Jewel : {2} ]{3}\n", DateTime.UtcNow, res.result, res.gem, debugString);
#endif // SHOW_LOG

        if (0 == res.result) {
            if (StructResearchTypes.Research == structResearchType) {
                PlayerManager.Instance.SetAssetAmount(AssetType.Jewel, res.gem);

                PlayerManager.Instance.SkipResearch(structID, res.research);

                LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
            }
            else if (StructResearchTypes.Training == structResearchType) {
                PlayerManager.Instance.SetAssetAmount(AssetType.Jewel, res.gem);

                List<MItem> groupedMItems = GetGroupedItems(res.items);
                PlayerManager.Instance.AddMItems(groupedMItems);

                PlayerManager.Instance.ResetTraining(buildingType, structID, researchID);

                LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();

                if (null != procHarvestItemsCallback) {
                    procHarvestItemsCallback(researchID, retrainingTargetID, groupedMItems);
                }
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponseSkipStructResearchByGem : Skip struct research by gem response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse skip research or training by gems : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseChangeStructTraining(StrucResearchAddResponse res, BuildingType buildingType) {
#if SHOW_LOG
        string debugString = string.Format("\t\tResearch Info : [ TableID : {0} \t/ actionType : {1} \t/ reservedCount : {2} \t/ endCount : {3} \t/ actionEndTime : {4} ]",
            res.research.m_refId, (StructResearchTypes)res.research.m_actionType, res.research.m_trainingReserveCnt, res.research.m_trainingEndCnt, Common.ConvertJavaMillisecondToDateTimeUTC(res.research.m_actionMaxTime));
        Debug.LogFormat("[ {0} ]\t\t<color=green>WebHttp : ResponseChangeStructTraining..!!</color>\t\t[ result : {1} ]\t{2}\n", DateTime.UtcNow, res.result, debugString);
#endif // SHOW_LOG

        if (0 == res.result) {
            if (0 < res.add) { // 재료의 증감이 아니고 훈련 갯수의 증감을 나타낸다. // 예약 건 지정  -:차감, +:증가
                PlayerManager.Instance.SubtractItems(res.resourceIds);
            }
            else {
                PlayerManager.Instance.AddItems(res.resourceIds);
            }

            PlayerManager.Instance.SetAssetAmount(AssetType.SpritStone, res.food);
            PlayerManager.Instance.SetAssetAmount(AssetType.Mythril, res.iron);
            PlayerManager.Instance.SetAssetAmount(AssetType.GodStone, res.soldier);

            // 무기랑 카드의 경우에는 훈련의 변경이 없다..!!
            //res.weaponIds       //무기 ID
            //res.cardIds         //카드 ID
            PlayerManager.Instance.SetStructureResearch(buildingType, res.strucId, res.research, StructResearchTypes.Training);   // 요청한 상태 정보

            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
        }
        else {
            Debug.LogErrorFormat("Web : ResponseChangeStructTraining : change struct training response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse chang struct training : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseWeaponChange(WeaponChangeResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseSwapWeaponChange..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
        }
        else if (res.result == 2) {
        }
    }

    private void ResponseWeaponUpgrade(WeaponUpgradeResponse res) {
#if SHOW_LOG
        MWeapon upgradedWeapon = res.tarWeapon;
        WeaponData weaponData = PlayerManager.Instance.GetWeaponDataByUniqueID(upgradedWeapon.m_id);
        TWeapon tWeapon = TWeapons.Instance.Find(upgradedWeapon.m_id);
        Debug.LogFormat("\t<color=white>[ RECEIVED ] WebHttp : ResponseWeaponUpgrade..!!\t[ Request {0}\t/ target Weapon UID : {1}\t/ Grade : {2}\t/ Awaken : {3}\t/ level : {4}\t/ exp : {5} ]</color>\n",
            0 == res.upgradeType ? "레벨업" : "각성", upgradedWeapon.m_id, weaponData.WeaponCardData._grade, upgradedWeapon.m_gaksung, upgradedWeapon.m_level, upgradedWeapon.m_exp);
#endif // SHOW_LOG

        if (0 == res.result) {
            int prevSpriteStoneAmount = PlayerManager.Instance.GetAssetAmount(AssetType.SpritStone);

            PlayerManager.Instance.SetAssetAmount(AssetType.SpritStone, res.gold);

#if SHOW_LOG
            int afterSpriteStoneAmount = PlayerManager.Instance.GetAssetAmount(AssetType.SpritStone);
            Debug.LogFormat("WebHttp : ResponseWeaponUpgrade : Change SpriteStone Amount [ prev : {0}\t/ after : {1}\t/ changed : {2} ]\n", prevSpriteStoneAmount, afterSpriteStoneAmount, prevSpriteStoneAmount - afterSpriteStoneAmount);
#endif // SHOW_LOG

            WeaponData prevWeaponData = PlayerManager.Instance.GetWeaponDataByUniqueID(res.tarWeapon.m_id);
            PlayerManager.Instance.UpdateWeaponsData(res.tarWeapon, res.resIds);

            // 0:레벨업, 1:각성 레벨업
            if (0 == res.upgradeType) {
                LobbySceneUIManager.Instance.WeaponLevelUpCompleteCallback(prevWeaponData);
            }
            else if (1 == res.upgradeType) {
                PlayerManager.Instance.SubtractItems(res.resources);
                LobbySceneUIManager.Instance.WeaponAwakeningCompleteCallback(prevWeaponData);
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponseWeaponUpgrade : WeaponUpgradeResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse upgrade weapon : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseUserDetailInfo(UserDetailInfoResponse res) {
        int maxClearStageId = res.maxClearStageId;
        int pvpWinCount = res.pvpWinCount;
        int maxPvpWinPoint = res.maxPvpWinPoint;
        int maxPvpTier = res.maxPvpTier;
        int maxPvpWinningStreak = res.maxPvpWinningStreak;
        int maxCombo = res.maxCombo;

#if SHOW_LOG
        string debLog = string.Format("[ maxClearStageId : {0} \t/ pvpWinCount : {1} \t/ maxPvpWinPoint : {2} \t/ maxPvpTier : {3} \t/ maxPvpWinningStreak : {4} \t/ maxCombo : {4} \t/]",
            maxClearStageId, pvpWinCount, maxPvpWinPoint, maxPvpTier, maxPvpWinningStreak, maxCombo);

        Debug.LogFormat("[ {0} ]\t\t<color=green>WebHttp : ResponseUserDetailInfo..!!</color>\t\t[ result : {1} ]\t\t{2}\n",
            DateTime.UtcNow, res.result, debLog);
#endif // SHOW_LOG
        ///-1:통신오류, 0:성공, 1:세션없음, 2 : 유저 정보 없음,
        if (0 == res.result) {
            GameDataManager.Instance.SetUserDetailResponse(res);
            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.PlayerProfile);
        }
        else if (res.result == 2) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15003"), string.Format(TStrings.Instance.FindString("ERROR_15012"), string.Format("({0})", res.result)), null);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseUserDetailInfo : User detail response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse user detail info : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseOutpostPvPSearch(OutpostPvpSearchResponse res, LobbySubType targetLobbySubType) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseOutpostPvPSearch..!!</color>\n");
#endif // SHOW_LOG

        //error code result   -1:통신오류, 0:성공, 1:세션없음, 2: , 3:대전상대 검색실패, 4: 재검색 골드 부족, 5:상대계정검색실패 (6, 7, 8 : 상대 역시 유저가 없다)
        if (0 == res.result) {
            if (BattleType.OutpostPvP != SystemManager.Instance.BattleType) {
                SystemManager.Instance.SetBattleType(BattleType.OutpostPvP);
            }

            PlayerManager.Instance.SetAssetAmount(AssetType.SpritStone, res.food);
            SystemManager.Instance.SetPvPOpponent(res, false);

            if (LobbySceneUIManager.Instance.CurrentMainType == LobbyMainType.World) {
                if (LobbySceneUIManager.Instance.CurrentSubType != targetLobbySubType) {
                    LobbySceneUIManager.Instance.SetLobbyMenu(targetLobbySubType);
                }
                else {
                    if (LobbySubType.ArenaOutpostBattleLobby == targetLobbySubType) {
                        PvPOutpostBattleLobbyUI outpostPvpLobbyUI = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.ArenaOutpostBattleLobby) as PvPOutpostBattleLobbyUI;
                        if (null != outpostPvpLobbyUI) {
                            outpostPvpLobbyUI.SetShow();
                        }
                        else {
                            Debug.LogError("Can't get PvPOutpostBattleLobbyUI..!!\n");
                        }
                    }
                    else if (LobbySubType.ArenaBattleReady == targetLobbySubType) {
                        PvPBattleReadyUI pvpReadyUI = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.ArenaBattleReady) as PvPBattleReadyUI;
                        if (null != pvpReadyUI) {
                            pvpReadyUI.SetShow();
                        }
                        else {
                            Debug.LogError("Can't get PvPBattleReadyUI..!!\n");
                        }
                    }
                }
            }
        }
        else if (3 <= res.result && 8 >= res.result) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15000"), string.Format(TStrings.Instance.FindString("PVE_5033"), string.Format("({0})", res.result)), null);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseOutpostPvPSearch : OutpostPvPSearch Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse search outpost pvp : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }

#if SHOW_LOG
        if (res.result != 0) {
            Debug.LogFormat("Web : ResponseOutpostPvPSearch : [ result : {0} ]\n", res.result);
        }
#endif // SHOW_LOG
    }

    public void ResponseOutpostPvPStart(OutpostPvpStartResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseOutpostPvPStart..!!</color>\n");
#endif // SHOW_LOG

        //error code result   -1:통신오류, 0:성공, 1:세션없음, 2:재도전 횟수 부족, 3:재도전유저없음, 4: PVP열쇠 부족

        if (0 == res.result) {
            if (PvPMatchType.ReMatch == (PvPMatchType)res.gameType) { // 0:신규 도전, 1:패배시 재도전
#if SHOW_LOG
                Debug.LogWarning("WebHttp : ResponsePvPStart : 재도전 일 때 재도전 횟수 차감 및 관련 체크 필요..!!\n");        // TODO
#endif // SHOW_LOG
            }

            PlayerManager.Instance.SetCurrentPlayStageID(SystemManager.Instance.SelectedBattleStageID);
            PlayerManager.Instance.SetAssetAmount(AssetType.PvP_Key, res.pvpKey);
            PlayerManager.Instance.UpdatePvPKeyAmountAndKeyRegenTime(res.now, res.pvpKeyRegenTime);

            SceneType targetSceneType = SceneType.Battle_PvP;

#if SHOW_LOG
            Debug.LogFormat("Called Load Battle Scene! Now load [ <color=cyan>{0} ]</color>\n", targetSceneType);
#endif // SHOW_LOG

            SystemManager.Instance.ChangeScene(targetSceneType, true);
        }
        else if (res.result == 2) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15003"), string.Format(TStrings.Instance.FindString("ERROR_15004"), string.Format("({0})", res.result)), null);
        }
        else if (res.result == 3) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15003"), string.Format(TStrings.Instance.FindString("ERROR_15013"), string.Format("({0})", res.result)), null);
        }
        else if (res.result == 4) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15003"), string.Format(TStrings.Instance.FindString("ERROR_15005"), string.Format("({0})", res.result)), null);
        }
        else {
            Debug.LogErrorFormat("Web : ResponsePvPStart : ResponsePvPStart error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse start outpost pvp : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseOutpostPvPEnd(OutpostPvpEndResponse res) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseOutpostPvPEnd..!!\t\t[ {0} ]</color>\n", (ResultType)res.gameResult);
#endif // SHOW_LOG

        //error code result   -1:통신오류, 0:성공, 1:세션없음, 2:대전상대없음,  3:대전상대계정없음,  4:본계정없음,

        if (0 == res.result) {
            PlayerManager.Instance.SetOutpostPvPBattleRewards(res);
            PlayerManager.Instance.SetWantedQuests(res.m_wantedQuests);
            BattleManager.Instance.ResponseResult(res);
        }
        else if (res.result == 2) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15000"), string.Format(TStrings.Instance.FindString("ERROR_15010"), string.Format("({0})", res.result)), null);
        }
        else if (res.result == 3) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15000"), string.Format(TStrings.Instance.FindString("ERROR_15011"), string.Format("({0})", res.result)), null);
        }
        else if (res.result == 4) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15000"), string.Format(TStrings.Instance.FindString("ERROR_15012"), string.Format("({0})", res.result)), null);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseOutpostPvPEnd : OutpostPvpEndResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse end outpost pvp : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseDungeonList(DungeonListResponse res) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseDungeonList..!!\t\t[ {0} ]</color>\n", res.result);
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetDungeonList(res.items);
            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.DungeonLobby);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseDungeonList : DungeonListResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse dungeon list : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseDungeonStageList(DungeonStageListResponse res, int dungeonID) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseDungeonStageList..!!\t\t[ {0} ]</color>\n", res.result);
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetDungeonStageList(res.items, dungeonID);
            if (0 < dungeonID && DungeonStateType.NotEntered == GameDataManager.Instance.GetDungeonState(dungeonID)) {
                GameDataManager.Instance.SetDungeonState(dungeonID, DungeonStateType.Entered);
            }
            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.DungeonStageList);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseDungeonStageList : DungeonStageListResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse dungeon stage list : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseDungeonPveStart(DungeonPveStartResponse res) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseDungeonPveStart..!!\t\t[ {0} ]</color>\n", res.result);
#endif // SHOW_LOG

        if (0 == res.result) {
            SystemManager.Instance.ResponseDungeonPVEStart(res);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseDungeonPveStart : DungeonPveStart Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse dungeon pve start : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseDungeonPveEnd(DungeonPveEndResponse res) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseDungeonPveEnd..!!\t\t[ {0} ]</color>\n", res.result);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetDungeonPvEBattleRewards(res);
            PlayerManager.Instance.SetWantedQuests(res.m_wantedQuests);
            BattleManager.Instance.ResponseResult(res);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseDungeonPveEnd : DungeonPveEnd Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse dungeon pve end : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseDungeonPvERestart(DungeonPveReStartResponse res) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseDungeonPvERestart..!!\t\t[ {0} ]</color>\n", res.result);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetAssetAmount(AssetType.Jewel, res.gem);
            BattleManager.Instance.SetBattleContinues();
        }
        else {
            Debug.LogErrorFormat("Web : ResponseDungeonPvERestart : DungeonPvERestart response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse dungeon pve restart : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseTournamentInfo(TournamentInfoResponse res, System.Action afterCallback) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseTournamentInfo..!!\t\t[ {0} ]</color>\n", res.result);
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetTournamentData(res.tournamentRegular, res.tounamentAttendStatus);

            if (null != afterCallback) {
                afterCallback();
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponseTournamentInfo : TournamentInfo response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse tournament info : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseMyTournamentInfo(TournamentMyInfoResponse res, System.Action afterCallback) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseMyTournamentInfo..!!\t\t[ {0} ]</color>\n", res.result);
#endif // SHOW_LOG

        //error result : 1. 세션 없음, 2. 미참가자일 경우, 
        if (0 == res.result) {
            GameDataManager.Instance.SetTournamentMyData(res);

            if (null != afterCallback) {
                afterCallback();
            }
        }
        else if (res.result == 2) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("Tournament_23046"),
                TStrings.Instance.FindString("Tournament_23076"),
                () => {
                    LobbySceneUIManager.Instance.BackLobbyMenuTo(LobbySubType.ZoneMap, LobbyMainType.BaseCamp);
                });
        }
        else {
            Debug.LogErrorFormat("Web : ResponseMyTournamentInfo : MyTournamentInfo response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse my tournament info : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseTournamentAttend(TournamentAttendResponse res, Action afterCallback) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseTournamentAttend..!!\t\t[ {0} ]</color>\n", res.result);
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetTournamentAttendance(res.deckCards);

            if (null != afterCallback) {
                afterCallback();
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponseTournamentAttend : TournamentAttend response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse Tournament attend : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseTournamentRankingList(TournamentRankerResponse res, Action completeCallback) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseTournamentRankingList..!!\t\t[ {0} ]</color>\n", res.result);
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetTournamentRankingList(res);
            if (null != completeCallback) {
                completeCallback();
            }
            else {
                LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponseTournamentAttend : TournamentAttend response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse Tournament attend : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseTournamentCardChange(TournamentCardChangeResponse res, bool updateTeamEditUI) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseTournamentCardItemChange..!!\t\t[ {0} ]</color>\n", res.result);
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetTournamentDeck(res);
            LobbySceneUIManager.Instance.RefreshUIDeckChanged(updateTeamEditUI);
        }
        else if (res.result == 2 || res.result == 3 || res.result == 4) { //error result : 1. 세션 없음, 2. 카드변경 일 지남, 3. 인벤에 카드가 존재하지 않음,  4. 카드 덱 변경 실패
            string contents = string.Empty;
            if (res.result == 2) {
                contents = TStrings.Instance.FindString("Tournament_23072");
            }
            else if (res.result == 3) {
                contents = TStrings.Instance.FindString("Tournament_23073");
            }
            else if (res.result == 4) {
                contents = TStrings.Instance.FindString("Tournament_23082");
            }

            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("Tournament_23046"), contents, () => {
                LobbySceneUIManager.Instance.BackLobbyMenuTo(LobbySubType.ZoneMap, LobbyMainType.BaseCamp);
            });
        }
        else {
            Debug.LogErrorFormat("Web : ResponseTournamentCardItemChange : TournamentCardItemChange response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse tournament card item change : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseTournamentPvpSearch(TournamentPvpSearchResponse res) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseTournamentPvpSearch..!!\t\t[ {0} ]</color>\n", res.result);
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetTournamentOpponentData(res);
            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.TournamentReady);
        }
        //error result : 1. 세션 없음, 2. 토너먼트 대전일이 아닐경우, 3. 참가 신청을 하지 않은 유저, 4. 매칭된 유저가 참가 신청을 하지 않은 유저, 5. 에너지가 부족할 경우
        else if (2 <= res.result && 5 >= res.result) {
            string contents = string.Empty;
            if (res.result == 2) {
                contents = TStrings.Instance.FindString("Tournament_23075");
            }
            else if (res.result == 3) {
                contents = TStrings.Instance.FindString("Tournament_23076");
            }
            else if (res.result == 4) {
                contents = TStrings.Instance.FindString("Tournament_23074");
            }
            else if (res.result == 5) {
                contents = TStrings.Instance.FindString("Tournament_23062");
            }

            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("Tournament_23046"), contents, () => {
                LobbySceneUIManager.Instance.BackLobbyMenuTo(LobbySubType.ZoneMap, LobbyMainType.BaseCamp);
            });
        }
        else {
            Debug.LogErrorFormat("Web : ResponseTournamentPvpSearch : TournamentPvpSearch response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse Tournament Pvp Search : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseTournamentPvPStart(TournamentPvpStartResponse res) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseTournamentPvpStart..!!\t\t[ {0} ]</color>\n", res.result);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetAssetAmount(AssetType.TournamentEnergy, res.attackEnergy);
            SceneType targetSceneType = SceneType.Battle_PvP;

#if SHOW_LOG
            Debug.LogFormat("Called Load Battle Scene! Now load [ <color=cyan>{0} ]</color>\n", targetSceneType);
#endif // SHOW_LOG

            SystemManager.Instance.ChangeScene(targetSceneType, true);
        }
        //error result : 1. 세션 없음, 2. 진행 기간이 아님, 3. 참가 유저가 아님, 4. vs 참가자가 아님, 5. 에너지가 부족함
        else if (2 <= res.result && 5 >= res.result) {
            string contents = string.Empty;
            if (res.result == 2) {
                contents = TStrings.Instance.FindString("Tournament_23083");
            }
            else if (res.result == 3) {
                contents = TStrings.Instance.FindString("Tournament_23076");
            }
            else if (res.result == 4) {
                contents = TStrings.Instance.FindString("Tournament_23077");
            }
            else if (res.result == 5) {
                contents = TStrings.Instance.FindString("Tournament_23061");
            }

            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("Tournament_23046"), contents, () => {
                LobbySceneUIManager.Instance.BackLobbyMenuTo(LobbySubType.ZoneMap, LobbyMainType.BaseCamp);
            });
        }
        else {
            Debug.LogErrorFormat("Web : ResponseTournamentPvpStart : TournamentPvpStart response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse Tournament Pvp Start : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseTournamentPvPEnd(TournamentPvpEndResponse res) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseTournamentPvPEnd..!!\t\t[ {0} ]</color>\n", (ResultType)res.gameResult);
#endif // SHOW_LOG

        //error result : 1. 세션 없음, 2. 참가 신청을 하지 않은 유저, 3. 매칭된 유저가 참가 신청을 하지 않은 유저

        if (0 == res.result) {
#if SHOW_LOG
            Debug.LogFormat("[ heroPoint : {0} \t/ bonusPoint : {1} \t/ getPoint : {2} \t/ result : {3} ({4}) ]\n", res.heroPoint, res.bonusPoint, res.getPoint, res.gameResult, (ResultType)res.gameResult);
#endif // SHOW_LOG

            BattleManager.Instance.ResponseResult(res);
        }
        else if (res.result == 2) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("Tournament_23046"),
                TStrings.Instance.FindString("Tournament_23076"),
                () => {
                    SceneType targetSceneType = SceneType.Main;

#if SHOW_LOG
                    Debug.LogFormat("Called Load Battle Scene! Now load [ <color=cyan>{0} ]</color>\n", targetSceneType);
#endif // SHOW_LOG

                    SystemManager.Instance.ChangeScene(targetSceneType, true);
                });
        }
        else if (res.result == 3) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("Tournament_23046"),
                TStrings.Instance.FindString("Tournament_23074"),
                () => {
                    SceneType targetSceneType = SceneType.Main;

#if SHOW_LOG
                    Debug.LogFormat("Called Load Battle Scene! Now load [ <color=cyan>{0} ]</color>\n", targetSceneType);
#endif // SHOW_LOG

                    SystemManager.Instance.ChangeScene(targetSceneType, true);
                });
        }
        else {
            Debug.LogErrorFormat("Web : ResponseTournamentPvPEnd : Tournament PvP End error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse end pvp : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseTournamentRecordList(TournamentRecordListResponse res, Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseTournamentRecordList..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetTournamentRecordList(res);
            if (null != completeCallback) {
                completeCallback();
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponseTournamentRecordList : TournamentRecordList Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse Tournament Record list : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseTournamentPlayerInfo(TournamentRecordPlayerInfoResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseTournamentPlayerInfo..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetOpponentPlayerInfo(res);
            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.OpponentPlayerInfo);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseTournamentRecordList : TournamentRecordList Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse Tournament Record list : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseTournamentResultReward(TournamentRewardResponse res, Action closeCallback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseTournamentResultReward..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetTournamentResultRewards(res);

            if (0 < res.Items.Count) {
                LobbySceneUIManager.Instance.ShowTournamentResultRewards(res, closeCallback);
            }
        }
        else if (2 <= res.result || 4 >= res.result) {
#if SHOW_LOG
            Debug.LogFormat("WebHttp : ResponseTournamentResultReward : [ No Reward : {0} ]\n", 2 == res.result ? "참가하지 않음" : 3 == res.result ? "이미 보상 받음" : "보상 받을 기간이 아님");
#endif // SHOW_LOG

            if (null != closeCallback) {
                closeCallback();
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponseTournamentResultReward : TournamentResultReward Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse tournament result reward : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseResetTournamentLoseCount(TournamentGemLoseResetResponse res, Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseResetTournamentLoseCount..!!</color>\n");
#endif // SHOW_LOG

        //error result : 1. 세션 없음, 2 : 토너먼트에 참가 유저가 아님, 3 : 재화 부족

        if (0 == res.result) {
            PlayerManager.Instance.SetAssetAmount(AssetType.Jewel, res.gem);

            GameDataManager.Instance.TournamentMyData.ResetLoseCount();

            if (null != completeCallback) {
                completeCallback();
            }
        }
        else if (2 == res.result) { // 토너먼트에 참가 유저가 아님
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("Tournament_23001"),
                TStrings.Instance.FindString("Tournament_23076"),
                () => {
                    SceneType targetSceneType = SceneType.Main;

#if SHOW_LOG
                    Debug.LogFormat("Called Load Battle Scene! Now load [ <color=cyan>{0} ]</color>\n", targetSceneType);
#endif // SHOW_LOG

                    SystemManager.Instance.ChangeScene(targetSceneType, true);
                });
        }
        else if (3 == res.result) { // 재화 부족
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("BATTLE_16023"),
                TStrings.Instance.FindString("BATTLE_16023"), null);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseResetTournamentLoseCount : ResetTournamentLoseCount Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse reset tournament lose count : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseMissionList(MissionListResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseMissionList..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseMissionList(res);

            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.MissionList);
        }
        else {
            Debug.LogErrorFormat("Web : ResponseMissionList : MissionListResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse mission list : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseMissionConfirm(MissionConfirmResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseMissionConfirm..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseMissionConfirm(res);
            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
            LobbySceneUIManager.Instance.RefreshUI();
        }
        else {
            Debug.LogErrorFormat("Web : ResponseMissionConfirm : MissionConfirmResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse mission confirm : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseWantedQuestConfirm(WantedQuestConfirmResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseWantedQuestConfirm..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseWantedQuestConfirm(res);
            LobbySceneUIManager.Instance.RefreshUI();
        }
        else {
            Debug.LogErrorFormat("Web : ResponseWantedQuestConfirm : WantedQuestConfirmResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse wantedquest confirm : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseWantedQuestReset(WantedQuestResetResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseWantedQuestReset..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseWantedQuestReset(res);
            PlayerManager.Instance.SetAssetAmount(AssetType.Jewel, res.gem);
            LobbySceneUIManager.Instance.RefreshUI();
        }
        else {
            Debug.LogErrorFormat("Web : ResponseWantedQuestReset : WantedQuestResetResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse wantedquest confirm : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseSummonThemeList(SummonListResponse res, Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseSummonThemaList..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetSummonThemeList(res);

            if (null != completeCallback) {
                completeCallback();
            }
        }
        else {
            Debug.LogErrorFormat("Web :ResponseSummonThemaList : SummonListResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse summon theme list : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseSummonInfo(SummonInfoResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseSummonInfo..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetSummonInfo(res);
            LobbySceneUIManager.Instance.ResponseSummonInfo(res);
        }
        else {
            Debug.LogErrorFormat("Web :ResponseSummonInfo : SummonInfoResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse summon info : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseSummonPurchase(SummonPurchaseResponse res, int summonIndex) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseSummonPurchase..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetSummonPurchaseResponse(res, summonIndex);
            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
            LobbySceneUIManager.Instance.ShowSummonProduction();
        }
        else {
            Debug.LogErrorFormat("Web :ResponseSummonPurchase : SummonPurchaseResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse summon purchase : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseSummonBonusConfirm(SummonBonusBoxConfirmResponse res, int summonIndex) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseSummonBonusConfirm..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetSummonBonusConfirm(res, summonIndex);
            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
        }
        else {
            Debug.LogErrorFormat("Web :ResponseSummonPurchase : SummonBonusConfirmResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse summon bonus confirm : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseSummonFateList(SummonFateListResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseSummonFateList..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetSummonFateList(res);
            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.SummonFate);
        }
        else {
            Debug.LogErrorFormat("Web :ResponseSummonFateList : SummonFateListResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse summon fate list : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseSummonFateConfirm(SummonFateConfirmResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseSummonFateConfirm..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetSummonFateConfirm(res);

            LobbySceneUIManager.Instance.CurrentMenuUI.HideSubUIsByESCBack();
            LobbySceneUIManager.Instance.ShowSummonFateProduction();

        }
        else {
            Debug.LogErrorFormat("Web :ResponseSummonFateConfirm : SummonFateConfirmResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse summon fate confirm : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponsePurchase(IngamePurchaseResponse res, System.Action<int> completeCallback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponsePurchase..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetAssetAmount(AssetType.Jewel, res.gem);
            PlayerManager.Instance.SetAssetAmount(AssetType.SpritStone, res.food);
            PlayerManager.Instance.SetAssetAmount(AssetType.Mythril, res.iron);
            PlayerManager.Instance.SetAssetAmount(AssetType.PvP_Key, res.pvpkey);
            PlayerManager.Instance.SetAssetAmount(AssetType.PvE_Key, res.key);
            //PlayerManager.Instance.SetAssetAmount(AssetType.FriendlyMatchEnergy, res.frkey);

            LobbySceneUIManager.Instance.UpdateAssetInfos();

            PlayerManager.Instance.AddItems(res.items);

            GameDataManager.Instance.SetShopInfo(res);
            PlayerManager.Instance.SetAssetLimit(res.infos);

            if (null != completeCallback) {
                completeCallback(res.priceId);
            }

#if SHOW_LOG
            Debug.LogFormat("\t<color=white> WebHttp : ResPonsePurchase : AssetAmount \t [/Gem : {0} \t /SpritStone : {1} \t /Mythril : {2} \t /PvP_Key : {3} \t /PvE_Key : {4}] </color>\n",
            res.gem, res.food, res.iron, res.pvpkey, res.key);
            Debug.LogFormat("\t<color=white> WebHttp : ResPonsePurchase : Infos \t [/CardCnt : {0} \t /WeaponCnt : {1} \t /TeamCnt : {2} \t /VipTime : {3} \t /PassTime : {4} \t /ProtectTime : {5}]</color>\n",
            res.infos.m_cardCnt, res.infos.m_weaponCnt, res.infos.m_teamCnt, res.infos.m_vipTime, res.infos.m_passTime, res.infos.m_protectedTime);
#endif // SHOW_LOG
        }
        else {
            Debug.LogErrorFormat("Web : ResponsePurchase : PurchaseResponse error..!!\t[ result : {0} ]\n", res.result);
        }
    }

    private void ResponseShopInfo(ShopListResponse res, System.Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseShopInfo..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetShopInfo(res);
            PlayerManager.Instance.SetAssetLimit(res.m_shopinfo);

            if (null != completeCallback) {
                completeCallback();
            }

#if SHOW_LOG
            Debug.LogFormat("\t<color=white> WebHttp : ResPonsePurchase : Infos \t [/CardCnt : {0} \t /WeaponCnt : {1} \t /TeamCnt : {2} \t /VipTime : {3} \t /PassTime : {4} \t /ProtectTime : {5}] </color>\n",
            res.m_shopinfo.m_cardCnt, res.m_shopinfo.m_weaponCnt, res.m_shopinfo.m_teamCnt, res.m_shopinfo.m_vipTime, res.m_shopinfo.m_passTime, res.m_shopinfo.m_protectedTime);
#endif // SHOW_LOG
        }
        else {
            Debug.LogErrorFormat("Web : ResponseShopInfo : ResponseShopInfo error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse wantedquest confirm : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseTreatyOver(TreatyBreakResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseTreatyOver..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetShopInfo(res.shopinfo);
            if (null != LobbySceneUIManager.Instance) {
                LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
            }
#if SHOW_LOG
            Debug.LogFormat("\t<color=white> WebHttp : ResponseTreatyOver : Infos \t [/CardCnt : {0} \t /WeaponCnt : {1} \t /TeamCnt : {2} \t /VipTime : {3} \t /PassTime : {4} \t /ProtectTime : {5}  \t /ProtectCooltime : {6}] </color>\n",
            res.shopinfo.m_cardCnt, res.shopinfo.m_weaponCnt, res.shopinfo.m_teamCnt, res.shopinfo.m_vipTime, res.shopinfo.m_passTime, res.shopinfo.m_protectedTime, res.shopinfo.m_protectRestartTime);
#endif // SHOW_LOG
        }
        else {
            Debug.LogErrorFormat("Web : ResponseTreatyOver : ResponseTreatyOver error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse wantedquest confirm : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseSkypassInfo(SeasonQuestListResponse res, System.Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseSkypassInfo..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetSkypassInfo(res);

            if (null != completeCallback) {
                completeCallback();
            }

#if SHOW_LOG
            for (int i = 0, count = res.items.Count; i < count; ++i) {
                if (res.items[i].m_type == (int)SkyPassQuestType.Event) {
                    Debug.LogWarningFormat("\t<color=yellow> WebHttp : ResponseSkypassInfo : Event \t [/ID : {0} \t /QuestType : {1} \t /TargetValue : {2} \t /Confirm : {3} ] </color>\n",
                                        res.items[i].m_refid, res.items[i].m_type, res.items[i].m_targetvalue, res.items[i].m_confirm);
                }
                else {
                    Debug.LogWarningFormat("\t<color=white> WebHttp : ResponseSkypassInfo : Items \t [/ID : {0} \t /QuestType : {1} \t /TargetValue : {2} \t /Confirm : {3} ] </color>\n",
                                        res.items[i].m_refid, res.items[i].m_type, res.items[i].m_targetvalue, res.items[i].m_confirm);
                }
            }
#endif // SHOW_LOG

        }
        else {
            Debug.LogErrorFormat("Web : ResponseSkypassInfo : ResponseSkypassInfo error..!!\t[ result : {0} ]\n", res.result);
        }
    }

    private void ResponseSkypassComplete(SeasonQuestConfirmResponse res, System.Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseSkypassComplete..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetSkypassInfo(res.items, res.m_seasonQuestInfo);

            if (null != completeCallback) {
                completeCallback();
            }

#if SHOW_LOG
            for (int i = 0, count = res.items.Count; i < count; ++i) {
                if (res.items[i].m_type == (int)SkyPassQuestType.Event) {
                    Debug.LogWarningFormat("\t<color=yellow> WebHttp : ResponseSkypassComplete : Event \t [/ID : {0} \t /QuestType : {1} \t /TargetValue : {2} \t /Confirm : {3} ] </color>\n",
                                        res.items[i].m_refid, res.items[i].m_type, res.items[i].m_targetvalue, res.items[i].m_confirm);
                }
                else {
                    Debug.LogWarningFormat("\t<color=white> WebHttp : ResponseSkypassComplete : Items \t [/ID : {0} \t /QuestType : {1} \t /TargetValue : {2} \t /Confirm : {3} ] </color>\n",
                                        res.items[i].m_refid, res.items[i].m_type, res.items[i].m_targetvalue, res.items[i].m_confirm);
                }
            }
#endif // SHOW_LOG

        }
        else {
            Debug.LogErrorFormat("Web : ResponseSkypassComplete : ResponseSkypassComplete error..!!\t[ result : {0} ]\n", res.result);
        }
    }

    private void ResponseSkypassReward(SeasonQuestRewardConfirmResponse res, System.Action completeCallback, int level) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseSkypassReward..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.AddItems(res.normallist);
            PlayerManager.Instance.AddItems(res.chargelist);
            PlayerManager.Instance.SetAssetAmount(AssetType.Jewel, res.gem);

            GameDataManager.Instance.SetSkyPassReward(res, level);

            if (null != completeCallback) {
                completeCallback();
            }

#if SHOW_LOG
            for (int i = 0, count = res.normallist.Count; i < count; ++i) {
                Debug.LogWarningFormat("<color=yellow>FreeItem{0} Type : {1} \t Amount : {2} \t ID : {3}</color>",
                    i, res.normallist[i].m_type, res.normallist[i].m_amount, res.normallist[i].m_refId);
            }
            for (int i = 0, count = res.chargelist.Count; i < count; ++i) {
                Debug.LogWarningFormat("<color=yellow>ChargeItem{0} Type : {1} \t Amount : {2} \t ID : {3}</color>",
                    i, res.chargelist[i].m_type, res.chargelist[i].m_amount, res.chargelist[i].m_refId);
            }
#endif // SHOW_LOG

        }
        else {
            Debug.LogErrorFormat("Web : ResponseSkypassReward : ResponseSkypassReward error..!!\t[ result : {0} ]\n", res.result);
        }
    }

    private void ResponseSkypassFinalReward(SeasonQuestFinalRewardResponse res, System.Action completeCallback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseSkypassFinalReward..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.AddItems(res.items);
            PlayerManager.Instance.SetAssetAmount(AssetType.Jewel, res.gem);

            GameDataManager.Instance.SetSkyPassReward(res);

            if (null != completeCallback) {
                completeCallback();
            }

#if SHOW_LOG
            for (int i = 0, count = res.items.Count; i < count; ++i) {
                Debug.LogWarningFormat("<color=yellow>Item{0} Type : {1} \t Amount : {2} \t ID : {3}</color>",
                    i, res.items[i].m_type, res.items[i].m_amount, res.items[i].m_refId);
            }
#endif // SHOW_LOG

        }
        else {
            Debug.LogErrorFormat("Web : ResponseSkypassFinalReward : ResponseSkypassFinalReward error..!!\t[ result : {0} ]\n", res.result);
        }
    }

    private void ResponseClanCreate(ClanCreateResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanCreate..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseClanCreateData(res);
            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
            PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), TStrings.Instance.FindString("Clan_29011"), PopupNormalIconType.Clan, null);
            TCPSocketManager.Instance.RequestClanCreate();
            GameDataManager.Instance.SetClanBossInfo(res.boss);
        }
        else if (res.result == 4) { //클랜 이름 중복
            PopupManager.Instance.ShowOKIconPopup(TStrings.Instance.FindString("Clan_29000"), TStrings.Instance.FindString("Clan_29112"), PopupNormalIconType.Clan, null);
        }
        else {
            Debug.LogErrorFormat("Web :ResponseClanCreate : ClanCreateResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse clan create : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanList(ClanListResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanList..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetClanInfoList(res.clanInfo);

            ClanUIController clanUICtrlr = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.Clan) as ClanUIController;
            clanUICtrlr.SetShowClanJoinUI();
        }
        else {
            Debug.LogErrorFormat("Web :ResponseClanList : ClanListResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse clan list : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanListNameSearch(ClanListNameSearchResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanListNameSearch..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetClanInfoList(res.clanInfo);

            ClanUIController clanUICtrlr = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.Clan) as ClanUIController;
            clanUICtrlr.SetShowClanJoinUI();
        }
        else {
            Debug.LogErrorFormat("Web :ResponseClanListNameSearch : ClanListNameSearchResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse clan list name search : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanInfo(ClanInfoResponse res, LobbySubType lobbySubType) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanInfo..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            if (lobbySubType == LobbySubType.None) {
                PlayerManager.Instance.ResponseClanInfoData(res);
                GameDataManager.Instance.SetPopupClanInfo(res);
                TCPSocketManager.Instance.RequestClanLoginMemberList();
            }
            else if (lobbySubType == LobbySubType.Clan) {
                GameDataManager.Instance.SetPopupClanInfo(res);
                LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.ClanInfo);
            }
            else if (lobbySubType == LobbySubType.ClanInfo) {
                PlayerManager.Instance.ResponseClanInfoData(res);
                TCPSocketManager.Instance.RequestClanLoginMemberList();
                LobbySceneUIManager.Instance.SetPreviousLobbyMenu();
            }
            else if (lobbySubType == LobbySubType.OpponentPlayerInfo) {
                GameDataManager.Instance.SetPopupClanInfo(res);
                LobbySceneUIManager.Instance.SetPreviousLobbyMenu();

                if (LobbySceneUIManager.Instance.PreviousSubType != LobbySubType.ClanInfo) {
                    LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.ClanInfo, LobbySceneUIManager.Instance.CurrentMainType);
                }
            }
            else {
                GameDataManager.Instance.SetPopupClanInfo(res);
                if (LobbySceneUIManager.Instance.PreviousSubType != LobbySubType.ClanInfo) {
                    LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.ClanInfo, LobbySceneUIManager.Instance.CurrentMainType);
                }
            }
        }
        else {
            Debug.LogErrorFormat("Web :ResponseClanInfo : ClanMemberInfoResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse clan info : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanUserPlayerInfo(ClanUserPlayerInfoResponse res, MClanUserInfo clanUserInfoData, int clanID) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanUserPlayerInfo..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetOpponentPlayerInfo(res, clanUserInfoData, clanID);
            if (LobbySceneUIManager.Instance.PreviousSubType == LobbySubType.OpponentPlayerInfo) {
                LobbySceneUIManager.Instance.SetPreviousLobbyMenu();
            }
            else {
                LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.OpponentPlayerInfo);
            }
        }
        else {
            Debug.LogErrorFormat("Web :ResponseClanInfo : ClanMemberInfoResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse clan user player info : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanFriendlyPvPStart(ClanPvpStartResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanFriendlyPvPStart..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetAssetAmount(AssetType.FriendlyMatchEnergy, res.clanPvpKey);
            SystemManager.Instance.SetPvPOpponent(res, false);
            SceneType targetSceneType = SceneType.Battle_PvP;

#if SHOW_LOG
            Debug.LogFormat("Called Load Battle Scene! Now load [ <color=cyan>{0} ]</color>\n", targetSceneType);
#endif // SHOW_LOG

            SystemManager.Instance.ChangeScene(targetSceneType, true);
        }
        else {
            Debug.LogErrorFormat("Web :ResponseClanFriendlyPvPStart : ClanPvpStartResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse clan friendly PvP Start : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanBossInfo(ClanBossInfoResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanBossInfo..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetClanBossInfo(res.boss);
            GameDataManager.Instance.SetClanBossHistory(res.historys);

            if (null != callback) {
                callback();
            }
        }
        else {
            Debug.LogErrorFormat("Web :ResponseClanBossInfo : ClanBossInfoResponse error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanBossInfo : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanBossStart(ClanBossRaidStartResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanBossStart..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetAssetAmount(AssetType.TitanEnergy, res.clanBossKey);
            PlayerManager.Instance.UpdateClanBossKeyAmountAndKeyRegenTime(res.now, res.clanBossKeyRegenTime);

            if (null != callback) {
                callback();
            }
        }
        else {
            if (res.result == 1) {
#if SHOW_LOG
                Debug.LogError("Web :ResponseClanBossStart : ClanBossRaidStartResponse error..!!\\t[ NO Session ]");
#endif //SHOW_LOG
                PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n Error Result : ({0})", res.result)), SystemManager.Instance.ExitGame);
            }
            else if (res.result == 2) {
#if SHOW_LOG
                Debug.LogWarning("Web :ResponseClanBossStart : ClanBossRaidStartResponse error..!!\\t[ NO KEY ]");
#endif //SHOW_LOG
                PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n Error Result : ({0})", res.result)), SystemManager.Instance.ExitGame);
            }
            else if (res.result == 3) {
#if SHOW_LOG
                Debug.LogWarning("Web :ResponseClanBossStart : ClanBossRaidStartResponse error..!!\\t[ Boss Already Died ]");
#endif //SHOW_LOG
                PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("Clan_29086"), TStrings.Instance.FindString("Clan_29087"), null);
            }
        }
    }

    private void ResponseClanBossGigUse(ClanBossItemUseResponse res, Action<int> completeCallback) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseClanBossGigUse..!! \t [ deckNum : {0} / itemID : {1} ]</color>\n", res.deckNum, res.itemId);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SubtractItems(res.itemId, 1);

            if (null != completeCallback) {
                completeCallback(res.itemId);
            }
        }
        else {
            Debug.LogErrorFormat("Web : ResponseClanBossGigUse : BattleItemUse response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse battle item use : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanBossEnd(ClanBossRaidEndResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanBossEnd..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetClanBossInfo(res.boss);
            BattleManager.Instance.ResponseResult(res);
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanBossEnd : ClanBossRaidEndResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanBossEnd : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanBossRanking(ClanBossDamageHistoryResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanBossRanking..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetClanBossRanking(res.historys);

            if (null != callback) {
                callback();
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanBossEnd : ResponseClanBossRanking error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanBossRanking : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanBossReward(ClanBossRewardResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanBossReward..!!</color>\n");
#endif // SHOW_LOG
        if (0 == res.result) {
            GameDataManager.Instance.SetClanBossReward(res);
            PlayerManager.Instance.AddItems(res.Items);
            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.ClanBossRaidResultReward);
        }
        else {
#if SHOW_LOG
            Debug.LogWarningFormat("Web :ResponseClanBossEnd : ResponseClanBossReward error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
        }
    }

    private void ResponseClanChattingList(ClanChattingListResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanChattingList..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseClanChatInfo(res);

            if (SystemManager.Instance.CurrSceneType == SceneType.Main && LobbySceneUIManager.Instance != null) {
                if (LobbySceneUIManager.Instance.CurrentSubType == LobbySubType.Clan) {
                    ClanUIController clanUIController = LobbySceneUIManager.Instance.GetResourcedLobbyMenu(LobbySubType.Clan) as ClanUIController;
                    clanUIController.InitializeMiniChatList();
                }
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanChattingList : ClanChattingListResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanChattingList : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanChatBlockUserList(ClanChatBlockUserListResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanChatBlockUserList..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseClanChatBlockUserList(res);
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanChatBlockUserList : ClanChatBlockUserListResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanChatBlockUserList : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanChatBlockUserInsert(ClanChatBlockUserInsertResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanChatBlockUserInsert..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseClanChatBlockUserInsert(res);
            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanChatBlockUserInsert : ClanChatBlockUserInsertResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanChatBlockUserInsert : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanChatBlockUserCancel(ClanChatBlockUserCancelResponse res, System.Action refreshCallback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanChatBlockUserCancel..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseClanChatBlockUserCancel(res);
            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();

            if (null != refreshCallback) {
                refreshCallback();
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanChatBlockUserCancel : ClanChatBlockUserCancelResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanChatBlockUserCancel : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanJoinUserList(ClanJoinUserListResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanJoinUserList..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseClanJoinRequestList(res);
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanJoinUserList : ResponseClanJoinUserList error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanJoinUserList : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanWarInfo(ClanWarInfoResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanWarInfo..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetClanWarInfo(res);
            if (null != callback) {
                callback();
            }

            PlayerManager.Instance.SetAssetAmount(AssetType.ClanWarEnergy, res.warEnergy);
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanWarInfo : ClanWarInfoResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanWarInfo : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanWarDefenseDeckChanged(ClanWarDeckCardChangeResponse res, bool updateTeamEditUI) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanWarDefenseDeckChanged..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetClanWarDefenseDeck(res.deckCards);
            LobbySceneUIManager.Instance.RefreshUIDeckChanged(updateTeamEditUI);
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanWarDefenseDeckChanged : ClanWarDeckCardChangeResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanWarDefenseDeckChanged : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanWarUsedCardList(ClanWarUsedDeckCardResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanWarUsedCardList..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetClanWarUsedItemList(res.m_items);
            if (null != callback) {
                callback();
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanWarUsedCardList : ClanWarUsedDeckCardResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanWarUsedCardList : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanWarBattleStart(ClanWarBattleStartResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanWarBattleStart..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetAssetAmount(AssetType.ClanWarEnergy, res.warEnergy);
            if (null != callback) {
                callback();
            }
        }
        else if (res.result == 2) {
            PopupManager.Instance.ShowOKPopup("no key", "clan war key < 0", null);
        }
        else {
            // res.result : 1 NO SESSION  // res.result : 2 NO ENERGY // res.result : 3 DUPLICATION CARD
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanWarBattleStart : ClanWarBattleStartResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanWarBattleStart : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanWarUserAttendChange(ClanWarUserAttendChangeResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanWarUserAttendChange..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetClanAttendType(res);
            if (null != callback) {
                callback();
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanWarUserAttendChange : ClanWarUserAttendChangeResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanWarUserAttendChange : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanWarBattleFieldInfo(ClanWarBattleFieldInfoResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanWarBattleFieldInfo..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetClanWarMapInfo(res);
            for (int i = 0, count = res.clanWarBattleUsers.m_items.Count; i < count; ++i) {
                if (res.clanWarBattleUsers.m_items[i].m_userseq == PlayerManager.Instance.UserSeq) {
                    PlayerManager.Instance.SetAssetAmount(AssetType.ClanWarEnergy, res.clanWarBattleUsers.m_items[i].m_war_energy);
                }
            }
            if (null != callback) {
                callback();
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanWarBattleFieldInfo : ClanWarBattleFieldInfoResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanWarBattleFieldInfo : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanWarBattleEnd(ClanWarBattleEndResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanWarBattleEnd..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.ClanWarPoint = res.getPoint;
            BattleManager.Instance.ResponseResult(res);
            TCPSocketManager.Instance.RequestClanWarBattleEnd(res.getPoint);
            TCPSocketManager.Instance.RequestClanWarProgress(GameDataManager.Instance.ClanWarInfo.vsClanInfo.m_clanId, GameDataManager.Instance.ClanWarVsUserData.m_userseq, (int)ClanWarProfile.ProgressType.Attack);
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanWarBattleEnd : ClanWarBattleEndResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanWarBattleEnd : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanWarCurrentRanking(ClanWarCurrentRankResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanWarHistory..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetClanWarRanking(res.clanWarBattleRank);
            if (null != callback) {
                callback();
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanWarHistory : ClanWarCurrentRankResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanWarHistory : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanWarResultReward(ClanWarResultRewardResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanWarResultReward..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetClanWarRewardResponse(res);
            if (res.ClanWarResult.m_id != 0) {
                LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.ClanWarResultReward);
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanWarResultReward : ClanWarResultRewardResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanWarResultReward : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanWarHistory(ClanWarHistoryResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanWarHistory..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetClanWarhistory(res.clanwarhistory);

            if (null != callback) {
                callback();
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanWarHistory : ClanWarHistoryResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanWarHistory : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanWarBoxReward(ClanWarBoxRewardResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanWarHistory..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.AddItems(res.Items);
            GameDataManager.Instance.SetClanWarBoxInfo(res);

            if (null != callback) {
                callback();
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanWarHistory : ClanWarHistoryResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanWarHistory : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseWemixTradingPost(WemixTradingPostResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseWemixPoint..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseWemixTradingPost(res);
            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.TradingPost);
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseWemixPoint : WemixPointResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseWemixPoint : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseWemixPointStaking(WemixPointStakingResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseWemixPointStaking..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseWemixPointStaking(res);
            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
        }
        else if (res.result == (int)WemixErrorType.LackHonor) {
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("Exchange_31006"), TStrings.Instance.FindString("Exchange_31020"), null);
        }
        else if (res.result == (int)WemixErrorType.TimeToCalculate) {
            string title = TStrings.Instance.FindString("Exchange_31006");
            string content = TStrings.Instance.FindString("Exchange_31010");
            PopupManager.Instance.ShowOKPopup(title, content, null);
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseWemixPointStaking : WemixPointStakingResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseWemixPointStaking : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseWemixInfo(WemixInfoResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseWemixInfo..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseWemixInfo(res);
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseWemixInfo : WemixInfoResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseWemixInfo : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseWemixDungeonStageReopen(WemixDungeonStageReopenResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseWemixDungeonStageReopen..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.ResponseWemixDungeonStageReopen(res);
            GameDataManager.Instance.SetDungeonStageListItem(res.stage);
            LobbySceneUIManager.Instance.CurrentMenuUI.SetShow();
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseWemixDungeonStageReopen : WemixDungeonStageReopenResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseWemixDungeonStageReopen : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseWemixStrucAllHarvest(WemixStrucAllHarvestResponse res, System.Action<List<MItem>> procHarvestItemsCallback) {
#if SHOW_LOG
        string structsData = string.Empty;
        for (int i = 0, count = res.strucs.Count; i < count; ++i) {
            structsData = string.Format("{0}structIndex : {1} \t/ TableID : {2} \t/ Level : {3} \t/ ActionType : {4}({5}) \t/ actionEndTime : {6} \t/ resourceMaxTime : {7} \t/ resourceSave : {8} \t/ ",
                structsData, res.strucs[i].m_strucIndex, res.strucs[i].m_strucRefId, res.strucs[i].m_strucLevel, res.strucs[i].m_actionType, (ConstructingType)res.strucs[i].m_actionType, Common.ConvertJavaMillisecondToDateTimeUTC(res.strucs[i].m_actionMaxTime).ToLocalTime(), Common.ConvertJavaMillisecondToDateTimeUTC(res.strucs[i].m_resouceMaxTime).ToLocalTime(), res.strucs[i].m_resourceSave);
        }
        string harvestsData = string.Empty;
        for (int i = 0, count = res.items.Count; i < count; ++i) {
            harvestsData = String.Format("{0}itemType : {1}({2}) \t/ TableID : {3} \t/ amount : {4} \t/ ",
            harvestsData, res.items[i].m_type, (AssetType)res.items[i].m_type, AssetType.CardType == (AssetType)res.items[i].m_type ? res.items[i].m_refId : res.items[i].m_id, res.items[i].m_amount);
        }
        Debug.LogFormat("[ {0} ]\t\t<color=green>WebHttp : ResponseWemixStrucAllHarvest..!!</color>\t\t[ result : {1} \t/ zoneID : {2}({3}) ] \t [ {4} ] \t [ {5} ]\n", DateTime.UtcNow, res.result, res.zoneId, (BuildingType)res.zoneId, structsData, harvestsData);
#endif // SHOW_LOG

        if (0 == res.result) {
            // 건물 데이터 업데이트
            for (int i = 0, count = res.strucs.Count; i < count; ++i) {
                PlayerManager.Instance.SetStructure(res.zoneId, res.strucs[i]);
            }

            // 수확물 업데이트
            PlayerManager.Instance.AddMItems(res.items);

            //공훈치 업데이트
            PlayerManager.Instance.ResponseWemixStrucAllHarvest(res);

            // UI Refresh
            LobbySceneUIManager.Instance.UpdateAssetInfos();
            //LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();

            if (null != procHarvestItemsCallback) {
                procHarvestItemsCallback(GetGroupedItems(res.items));
            }
        }
        //else if (res.result == 2) {
        //    PopupManager.Instance.ShowOKPopup("지역 없음", "지역 없음", null);                // TODO string
        //}
        else {
            Debug.LogErrorFormat("Web : ResponseWemixStrucAllHarvest : Harvest All Response error..!!\t[ result : {0} ]\n", res.result);
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\nresponse harvest all : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseUserInfo(UserInfoResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseUserInfo..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetUserInfo(res.userinfo);
            LobbySceneUIManager.Instance.SetLobbyMenu(LobbySubType.OpponentPlayerInfo);
            PlayerManager.Instance.UpdateClanFriendlyPvpKeyRegenTime(res.clanPvpKeyRegenTime);

            if (callback != null) {
                callback();
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseUserInfo : UserInfoResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseUserInfo : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseClanJoin(ClanJoinResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseClanJoin..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            //nothing
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseClanJoin : ClanJoinResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseClanJoin : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseGooglePurchaseKey(GooglePlayReceiptVerificationKeyResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseGooglePurchaseKey..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerPrefsManager.Instance.SaveGooglePurchaseKey(res.inapp_key);

            if (null != callback) {
                callback();
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseGooglePurchaseKey : GooglePlayReceiptVerificationKeyResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseGooglePurchaseKey : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseCashPurchaseCheck(GooglePlayReceiptVerificationResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseGooglePlayPurchase..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            if (SystemManager.Instance.StoreType == StoreType.OneStore) {
                IAPManager.Instance.OnestorePurchaseComplete();
            }

            PlayerManager.Instance.AddItems(res.Items);

            if (null != callback) {
                callback();
            }

            PlayerPrefsManager.Instance.DeleteGooglePurchaseKey();
            PlayerPrefsManager.Instance.DeletePurchaseData();

            IAPManager.Instance.StopCheckPurchase();

            PopupManager.Instance.HideNormalPopup();
        }
        else if (4 == res.result) {
            if (SystemManager.Instance.StoreType == StoreType.OneStore) {
                IAPManager.Instance.OnestorePurchaseComplete();
            }
        }
        else {
            IAPManager.Instance.StartCheckPurchase();
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseGooglePlayPurchase : GooglePlayReceiptVerificationResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
        }
    }

    private void ResponseBannerProductInfo(BannerProductListResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseBannerProductList..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            GameDataManager.Instance.SetBannerProductInfo(res);

            if (null != callback) {
                callback();
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseBannerProductList : BannerProductListResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseBannerProductList : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseRegisterUserInfo(RegisterUserInfoResponse res, Action<RegisterUserInfoResponse> callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseRegisterUserInfo..!!</color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            if (null != callback) {
                callback(res);
            }
        }
        else if (res.result == 2) { // 유저 정보 없음
            LoginPlatformType type = LoginPlatformType.Guest;
#if UNITY_ANDROID
            type = LoginPlatformType.Google;
#elif UNITY_IOS
        type = LoginPlatformType.Apple;
#endif
            string linkAccount = SignInManager.Instance.PlatformUserId;
            RequestRegisterLinkage((int)type, linkAccount, LobbySceneUIManager.Instance.RefreshUI);
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseRegisterUserInfo : RegisterUserInfoResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseRegisterLinkage : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseRegisterLinkage(RegisterLinkageResponse res, Action callback) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseRegisterLinkage..!! type : {0}\t account : {1}</color>\n", res.type, res.account);
#endif // SHOW_LOG

        if (0 == res.result) {
            LoginPlatformType loginType = (LoginPlatformType)res.type;
            SystemManager.Instance.PlayerLoginType = loginType;
            PlayerPrefsManager.Instance.SaveLoginPlatformType(loginType);
            PlayerPrefsManager.Instance.SaveAccountName(res.account);

            if (null != callback) {
                callback();
                LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseRegisterLinkage : RegisterLinkageResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseRegisterLinkage : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseAgreementConfirm(AgreementConfirmResponse res) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseAgreementConfirm..!!</color>\n");
#endif // SHOW_LOG

        if (res.result == 0) {
            if (res.confirm == 1) {
#if SHOW_LOG
                Debug.LogFormat("Web : ResponseAgreementConfirm : RegisterUserInfoResponse result Success..!!\t[ confirm : {0} ]\n", res.confirm);
#endif // SHOW_LOG
            }
            else if (res.confirm == 2) {
#if SHOW_LOG
                Debug.LogErrorFormat("Web : ResponseAgreementConfirm : RegisterUserInfoResponse result Success. but confirm error..!!\t[ confirm : {0} ]\n", res.confirm);
#endif // SHOW_LOG
                PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseAgreementConfirm confirm : ({0})", res.confirm)), SystemManager.Instance.ExitGame);
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web : ResponseAgreementConfirm : AgreementConfirmResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseAgreementConfirm : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseUseItem(ItemUseResponse res, System.Action callback) {
#if SHOW_LOG
        Debug.Log("\t\t<color=green>WebHttp : ResponseUseItem..!!</color>\n");
#endif // SHOW_LOG

        if (res.result == 0) {
            if (0 != res.food) {
                PlayerManager.Instance.SetAssetAmount(AssetType.SpritStone, res.food);
            }
            if (0 != res.iron) {
                PlayerManager.Instance.SetAssetAmount(AssetType.Mythril, res.iron);
            }
            if (0 != res.key) {
                PlayerManager.Instance.SetAssetAmount(AssetType.PvE_Key, res.key);
            }
            if (0 != res.pvpkey) {
                PlayerManager.Instance.SetAssetAmount(AssetType.PvP_Key, res.pvpkey);
            }
            if (0 != res.clanBosskey) {
                PlayerManager.Instance.SetAssetAmount(AssetType.TitanEnergy, res.clanBosskey);
            }
            PlayerManager.Instance.SubtractItems(res.itemRefId, res.useAmount);

            if (0 != res.tarCard.m_id) {
                HeroData prevHeroData = PlayerManager.Instance.GetHeroDataByUniqueID(res.tarCard.m_id);
                PlayerManager.Instance.UpdateCharactersData(res.tarCard, null);
                LobbySceneUIManager.Instance.HeroLevelUpCompleteCallback(prevHeroData);
            }

            PlayerManager.Instance.AddItems(res.items);

            if (null != LobbySceneUIManager.Instance) {
                LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
            }

            if (null != callback) {
                callback();
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web : ResponseUseItem : ItemUseResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseAgreementConfirm : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseAdReward(AdvertisementConfirmResponse res, System.Action callback = null) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseAdReward..!! type : {0}\t</color>\n", res.type);
#endif // SHOW_LOG

        if (0 == res.result) {
            if (0 != res.Items.Count) {
                PlayerManager.Instance.AddMItems(res.Items);
                PopupManager.Instance.ShowRewardPopup(TStrings.Instance.FindString("appAD_20001"), TStrings.Instance.FindString("appAD_20003"), res.Items);
            }

            if (res.type == (int)AdsType.Summon) {
                PopupManager.Instance.HideRewardPopup();
            }

            if (null != callback) {
                callback();
            }

            AdsManager.Instance.ResetServerCheckCount();
        }
        else if (2 == res.result) {
            AdsManager.Instance.StartRequest();
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseAdReward : AdvertisementConfirmResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseAdReward : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseAvatarChange(AvatarChangeResponse res, System.Action callback = null) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseAvatarChange..!! avatarId : {0}\t bgId : {1}\t, pinId : {2}\t</color>\n", res.avatar, res.backgroud, res.pin);
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetProfileData(res);

            if (null != callback) {
                callback();
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseAvatarChange : AvatarChangeResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseAvatarChange : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    public void ResponseNickNameChange(NickNameChangeResponse res, Action callback) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseNickNameChange..!! </color>\n");
#endif // SHOW_LOG

        if (0 == res.result) {
            PlayerManager.Instance.SetNickname(res);
            PlayerManager.Instance.SetAssetAmount(AssetType.Jewel, res.gem);
            if (callback != null) {
                callback();
            }
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web :ResponseNickNameChange : NickNameChangeResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseNickNameChange : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ShowErrorPopupAboutTalent(int errorResult) {
        string talentErrorKey = string.Empty;
        if (2 == errorResult) {
            // 2.카드가 존재하지 않는다,
            talentErrorKey = "ERROR_15026";
        }
        else if (3 == errorResult) {
            // 3 : 특성 레벨업 순서가 맞지 않음(잘 못된 데이타일 경우),
            talentErrorKey = "ERROR_15027";
        }
        else if (4 <= errorResult && 7 >= errorResult) {
            // 4 : 재화 부족(음식), 5 : 재화 부족(철), 6 : 재화 부족(엠블럼), 7 : 재화 부족(다이아),
            talentErrorKey = "ERROR_15028";
        }
        else if (8 == errorResult) {
            // 8 : 특성 초기화 되어 있음
            talentErrorKey = "ERROR_15029";
        }

        PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"),
            TStrings.Instance.FindString(talentErrorKey),
            null, 0f, true);
    }

    private void ResponseSetTalentValues(TalentInitAndLearnResponse res) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseSetTalentPoint..!! </color>\n");
#endif // SHOW_LOG

        //error result : 1. 세션 없음, 2. 카드가 존재하지 않는다, 3 : 특성 레벨업 순서가 맞지 않음(잘 못된 데이타일 경우), 4 : 재화 부족(음식), 5 : 재화 부족(철), 6 : 재화 부족(엠블럼), 7 : 재화 부족(다이아),  8 : 특성 초기화 되어 있음
        if (0 == res.result) {
            PlayerManager.Instance.SetAssetAmount(AssetType.Jewel, res.gem);       // 차감 후 총 보유 보석
            PlayerManager.Instance.SetAssetAmount(AssetType.Mythril, res.iron);    // 차감 후 총량
            PlayerManager.Instance.SetAssetAmount(AssetType.SpritStone, res.food); // 차감 후 총량

            PlayerManager.Instance.AddItems(res.gainItems);     // 획득 아이템 갯수
            PlayerManager.Instance.SubtractItems(res.useitems); //사용 아이템 갯수

            PlayerManager.Instance.SetTalents(res.cardId, res.attributeLevel, res.attributeChart);

            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
        }
        else if (2 <= res.result && 8 >= res.result) {
            ShowErrorPopupAboutTalent(res.result);
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web : ResponseSetTalentPoint : SetTalentPointResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseSetTalentPoint : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    private void ResponseResetTalentValues(TalentInitAndLearnResponse res) {
#if SHOW_LOG
        Debug.LogFormat("\t\t<color=green>WebHttp : ResponseResetTalents..!! </color>\n");
#endif // SHOW_LOG

        //error result : 1. 세션 없음, 2. 카드가 존재하지 않는다, 3 : 특성 레벨업 순서가 맞지 않음(잘 못된 데이타일 경우), 4 : 재화 부족(음식), 5 : 재화 부족(철), 6 : 재화 부족(엠블럼), 7 : 재화 부족(다이아),  8 : 특성 초기화 되어 있음
        if (0 == res.result) {
            PlayerManager.Instance.SetAssetAmount(AssetType.Jewel, res.gem);       // 차감 후 총 보유 보석
            PlayerManager.Instance.SetAssetAmount(AssetType.Mythril, res.iron);    // 차감 후 총량
            PlayerManager.Instance.SetAssetAmount(AssetType.SpritStone, res.food); // 차감 후 총량

            PlayerManager.Instance.AddItems(res.gainItems);     // 획득 아이템 갯수
            PlayerManager.Instance.SubtractItems(res.useitems); //사용 아이템 갯수

            PlayerManager.Instance.ResetTalents(res.cardId);

            LobbySceneUIManager.Instance.CurrentMenuUI.RefreshUI();
        }
        else if (2 <= res.result && 8 >= res.result) {
            ShowErrorPopupAboutTalent(res.result);
        }
        else {
#if SHOW_LOG
            Debug.LogErrorFormat("Web : ResponseResetTalents : ResetTalentsResponse error..!!\t[ result : {0} ]\n", res.result);
#endif // SHOW_LOG
            PopupManager.Instance.ShowOKPopup(TStrings.Instance.FindString("ERROR_15006"), string.Format(TStrings.Instance.FindString("ERROR_15007"), string.Format("\n ResponseResetTalents : ({0})", res.result)), SystemManager.Instance.ExitGame);
        }
    }

    #endregion //private funcs









    #region test codes

    #endregion // test codes
}
