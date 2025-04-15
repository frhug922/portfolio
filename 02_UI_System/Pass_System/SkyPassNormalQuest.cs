using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkyPassNormalQuest : LobbyUIBase
{

    #region type def

    private enum TextType
    {
        Title,
        ProgressBar,
        Desc,
        GainXP,
        CompleteButton,
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
    private TSeasonQuest _quest;
    private MSeasonQuest _questData;
    private int _index;

    #endregion





    #region public funcs

    public void SetShow(int id, int index) {
        _quest = TSeasonQuests.Instance.Find(id);

        if ((int)SkyPassQuestType.Daily == _quest._missionType) {
            _questData = GameDataManager.Instance.SkypassDailyQuest[_index];
        }
        else if ((int)SkyPassQuestType.Challenge == _quest._missionType) {
            _questData = GameDataManager.Instance.SkypassSeasonQuest[_index];
        }

        _index = index;

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
        SetText((int)TextType.Title, _quest.LangTypeName);

        if (_questData.m_confirm == (int)QuestConfirmType.AfterConfirm) {
            SetText((int)TextType.ProgressBar, TStrings.Instance.FindString("NoviceQuest_27007"));
        }
        SetText((int)TextType.Desc, _quest.LangTypeDesc);
        SetText((int)TextType.GainXP, string.Format("+{0}", _quest._exp));
        SetText((int)TextType.CompleteButton, TStrings.Instance.FindString("TEAMEDIT_10012"));
    }

    public void OnClick_Complete() {
        if (null == _clickCallback) {
            return;
        }

        _clickCallback(_passIcon, _quest._id);
    }

    #endregion





    #region private funcs

    private void UpdateContentsUI() {
        SetMissionIcon(ref _questImage, _quest._iconImage);

        SetSlider();

        _activeObjects[(int)ActiveType.ButtonComplete].SetActive((int)QuestConfirmType.BeforeConfirm == _questData.m_confirm);
        _activeObjects[(int)ActiveType.Desc].SetActive((int)QuestConfirmType.BeforeConfirm != _questData.m_confirm);
        _activeObjects[(int)ActiveType.ImageComplete].SetActive((int)QuestConfirmType.AfterConfirm == _questData.m_confirm);
    }

    private void SetSlider() {
        if (_questData.m_confirm == (int)QuestConfirmType.AfterConfirm) {
            return;
        }

        long currAmount = _questData.m_targetvalue;
        long maxAmount = _quest._targetValue;

        if (currAmount > maxAmount) {
            currAmount = maxAmount;
        }

        _textSlider.SetShow(true, currAmount, maxAmount, true);
    }

    #endregion
}


