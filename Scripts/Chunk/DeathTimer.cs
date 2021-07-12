using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace caveFog
{
    public class DeathTimer : MonoBehaviour
    {
        public Action TimerEnd;
        private Coroutine _timerCoroutine;
        public void StartDeathTimer(float _timeToDie)
        {
            if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            _timerCoroutine = StartCoroutine(CO_DeathTimer(_timeToDie));
        }

        public void StopDeathTimer()
        {
            if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            else Debug.LogWarning("Stoping timer which hasn't started");
        }

        private IEnumerator CO_DeathTimer(float in_time)
        {
            yield return new WaitForSeconds(in_time);

            if (TimerEnd != null) TimerEnd.Invoke();
            else Debug.LogError("Timer Expired without listeners");
        }
    }

}
