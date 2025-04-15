using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkyPassEventQuest : LobbyUIBase
{

    #region type def

    private enum TextType
    {
        Time,
        Title,
        Progrss,
        Slider,
        Desc,
        XP,
        Complete,
    }

    private enum ActiveType
    {
        ButtonComplete,
        ImageComplete,
        Desc,
    }

    private enum QuestConfirmType
    {
        BeforeClear = 0,
        BeforeConfirm = 1,
        AfterConfirm = 2,
    }

    #endregion





    #region serialize fields

    [SerializeField] private GameObject _passIcon;
    [SerializeField] private Image _questImage;
    [SerializeField] private TextSlider _textSlider;
    [SerializeField] private List<GameObject> _activeObjects;

    #endregion





    #region private fields

    private System.Action<GameObject, int> _clickCallback;
    private TSeasonQuest _eventQuest;
    private MSeasonQuest _eventQuestData;
    private TSeasonQuestEvent _eventQuestTimeData;

    #endregion





    #region public funcs

    public override void SetShow() {
        _eventQuestData = GameDataManager.Instance.SkypassEventQuest;
        _eventQuest = TSeasonQuests.Instance.Find(_eventQuestData.m_refid);
        _eventQuestTimeData = TSeasonQuestEvents.Instance.Find(GameDataManager.Instance.SkypassInfo.eventid);

        base.SetShow();

        RefreshUI();
    }

    public void Initialize(System.Action<GameObject, int> clickCallback) {
        _clickCallback = clickCallback;
    }


    public override void RefreshUI() {
        UpdateTextsUI();
        UpdateContentsUI();
    }

    public override void UpdateTextsUI() {
        System.TimeSpan timespan = Common.ConvertJavaMillisecondToDateTimeUTC(_eventQuestData.m_removetime) - System.DateTime.UtcNow;

        if (0 < timespan.Days) {
            SetText((int)TextType.Time, string.Format("{0} {1}", TStrings.Instance.FindString("HunterPath_25003"), string.Format(TStrings.Instance.FindString("SHOP_9034"), timespan.Days, timespan.Hours)));
        }
        else {
            SetText((int)TextType.Time, string.Format("{0} {1}", TStrings.Instance.FindString("HunterPath_25003"), string.Format(TStrings.Instance.FindString("SHOP_9035"), timespan.Hours, timespan.Minutes)));
        }

        SetText((int)TextType.Title, _eventQuest.LangTypeName);
        SetText((int)TextType.Progrss, string.Format(TStrings.Instance.FindString("HunterPath_25044"), _eventQuestTimeData._questCount, _eventQuest._condition));/////////////////////////////////////condition에서 몇번째 임무인지로 바꾸기

        if (_eventQuestData.m_confirm == (int)QuestConfirmType.AfterConfirm) {
            SetText((int)TextType.Slider, TStrings.Instance.FindString("NoviceQuest_27010"));
        }
        SetText((int)TextType.Desc, _eventQuest.LangTypeDesc);
        SetText((int)TextType.XP, string.Format("+{0}", _eventQuest._exp));
        SetText((int)TextType.Complete, TStrings.Instance.FindString("TEAMEDIT_10012"));
    }

    public void OnClick_Complete() {
        if (null == _clickCallback) {
            return;
        }

        _clickCallback(_passIcon, _eventQuest._id);
    }

    #endregion





    #region private funcs

    private void UpdateContentsUI() {
        SetMissionIcon(ref _questImage, _eventQuest._iconImage);

        SetSlider();

        _activeObjects[(int)ActiveType.ButtonComplete].SetActive(_eventQuestData.m_optionvalue >= _eventQuest._targetValue && (int)QuestConfirmType.AfterConfirm != _eventQuestData.m_confirm);
        _activeObjects[(int)ActiveType.Desc].SetActive(_eventQuestData.m_optionvalue < _eventQuest._targetValue || (int)QuestConfirmType.AfterConfirm == _eventQuestData.m_confirm);
        _activeObjects[(int)ActiveType.ImageComplete].SetActive((int)QuestConfirmType.AfterConfirm == _eventQuestData.m_confirm);
    }

    private void SetSlider() {
        if (_eventQuestData.m_confirm == (int)QuestConfirmType.AfterConfirm) {
            return;
        }

        long currAmount = _eventQuestData.m_targetvalue;
        long maxAmount = _eventQuest._targetValue;

        if (_eventQuestData.m_targetvalue > _eventQuest._targetValue) {
            _eventQuestData.m_targetvalue = _eventQuest._targetValue;
        }

        _textSlider.SetShow(true, currAmount, maxAmount, true);
    }

    #endregion
}


