using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanWarMoveBox : MonoBehaviour
{
    #region type def

    private enum BoxType
    {
        Mine,
        Enemy,
    }

    #endregion // type def





    #region serialized field

    [SerializeField] private List<GameObject> _backGrounds;
    [SerializeField] private List<Text> _nickNames;
    [SerializeField] private ProfileItem _allyProfileItem;
    [SerializeField] private ProfileItem _enemyProfileItem;

    #endregion //serialized field





    #region private field

    private bool _isFirstTargetArrived = false;
    private bool _isSecondTargetArrived = false;
    private bool _isThirdTargetArrived = false;

    #endregion // private field





    #region public funcs

    public void SetBox(bool isEnemy, string name, int avatarId, int bgId, int pinId) {
        if (isEnemy) {
            _backGrounds[(int)BoxType.Enemy].SetActive(true);
            _backGrounds[(int)BoxType.Mine].SetActive(false);
            _nickNames[(int)BoxType.Enemy].text = name;
            _enemyProfileItem.SetShow(avatarId, bgId, pinId);
        }
        else {
            _backGrounds[(int)BoxType.Enemy].SetActive(false);
            _backGrounds[(int)BoxType.Mine].SetActive(true);
            _nickNames[(int)BoxType.Mine].text = name;
            _allyProfileItem.SetShow(avatarId, bgId, pinId);
        }
    }

    public IEnumerator UpdateMovement(Transform firstTarget, Transform secondTarget, Transform thirdTarget) {

        float speed = 3f;
        float time = 100f * Time.deltaTime;

        while (!_isFirstTargetArrived) {
            // 현재 위치에서 목표 위치로 향하는 벡터를 계산합니다.
            Vector3 direction = (firstTarget.position - transform.position).normalized;

            // 목표로 향하는 방향으로 이동합니다.
            transform.Translate(direction * speed * time);

            float distanceToTarget = Vector3.Distance(transform.position, firstTarget.position);

            if (distanceToTarget <= 5f) {
                _isFirstTargetArrived = true;
            }

            yield return null;
        }

        while (!_isSecondTargetArrived) {
            // 현재 위치에서 목표 위치로 향하는 벡터를 계산합니다.
            Vector3 direction = (secondTarget.position - transform.position).normalized;

            // 목표로 향하는 방향으로 이동합니다.
            transform.Translate(direction * speed * time);

            float distanceToTarget = Vector3.Distance(transform.position, secondTarget.position);

            if (distanceToTarget <= 5f) {
                _isSecondTargetArrived = true;
            }

            yield return null;
        }

        while (!_isThirdTargetArrived) {
            // 현재 위치에서 목표 위치로 향하는 벡터를 계산합니다.
            Vector3 direction = (thirdTarget.position - transform.position).normalized;

            // 목표로 향하는 방향으로 이동합니다.
            transform.Translate(direction * speed * time);

            float distanceToTarget = Vector3.Distance(transform.position, thirdTarget.position);

            if (distanceToTarget <= 5f) {
                _isThirdTargetArrived = true;
                gameObject.SetActive(false);
                GameDataManager.Instance.DoCallback();
            }

            yield return null;
        }

        yield break;
    }

    public void Move(Transform startPosition, Transform firstTarget, Transform secondTarget, Transform thirdTarget) {
        if (null == startPosition || null == firstTarget || null == secondTarget || null == thirdTarget) {
            return;
        }

        gameObject.SetActive(true);
        transform.position = startPosition.position;

        _isFirstTargetArrived = false;
        _isSecondTargetArrived = false;
        _isThirdTargetArrived = false;

        StartCoroutine(UpdateMovement(firstTarget, secondTarget, thirdTarget));
    }


    #endregion //public funcs

    //test code

    //private Transform _target;

    //public void Update() {
    //    transform.position = Vector3.MoveTowards(transform.position, _target.position, 5f);
    //}

    //public void MoveLerp(Transform startPos, Transform endPos) {
    //    this.transform.position = startPos.position;
    //    _target = endPos;

    //    this.gameObject.SetActive(true);
    //}
}
