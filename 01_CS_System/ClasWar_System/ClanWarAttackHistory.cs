using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanWarAttackHistory : MonoBehaviour
{
    #region type def

    private enum TextType
    {
        AttackNick,
        Point,
        DefenseNick,
    }

    #endregion // type def





    #region serialized fields

    [SerializeField] private List<Text> _texts;

    #endregion //serialized fields





    #region public funcs

    public void SetShow(string attackNick, string deffenseNick, int point, bool isAttack) {
        this.gameObject.SetActive(true);

        if (isAttack) {
            _texts[(int)TextType.AttackNick].text = string.Format("<color=#3bc5eb>{0}</color>" /*Color : Blue*/ , attackNick);
            _texts[(int)TextType.Point].text = string.Format("<color=#3bc5eb>{0}</color>" /*Color : Blue*/ , point);
            _texts[(int)TextType.DefenseNick].text = string.Format("<color=#e81829>{0}</color>" /*Color : Red*/ , deffenseNick);
        }
        else {
            _texts[(int)TextType.AttackNick].text = string.Format("<color=#e81829>{0}</color>" /*Color : Red*/ , attackNick);
            _texts[(int)TextType.Point].text = string.Format("<color=#e81829>{0}</color>" /*Color : Red*/ , point);
            _texts[(int)TextType.DefenseNick].text = string.Format("<color=#3bc5eb>{0}</color>" /*Color : Blue*/ , deffenseNick);
        }
    }

    public void SetHide() {
        this.gameObject.SetActive(false);
    }

    #endregion // public funcs
}
