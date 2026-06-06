using System;
using UnityEngine;

namespace Necrocis
{
    public class PlayerClass : MonoBehaviour
    {
        [Header("현재 직업")]
        [SerializeField] private JobType currentJob = JobType.None;

        [Header("전직 가능 여부")]
        [SerializeField] private bool canAdvance = false;

        public JobType CurrentJob => currentJob;
        public bool CanAdvance => canAdvance;
        public bool HasAdvanced => currentJob != JobType.None;

        public event Action<JobType> OnClassChanged;
        public event Action OnAdvanceUnlocked;

        public void UnlockAdvance()
        {
            if (HasAdvanced || canAdvance)
                return;

            canAdvance = true;
            Debug.Log("전직 가능 상태가 되었습니다.");
            OnAdvanceUnlocked?.Invoke();
        }

        public bool AdvanceTo(JobType newJob)
        {
            if (!canAdvance)
            {
                Debug.LogWarning("아직 전직할 수 없습니다.");
                return false;
            }

            if (HasAdvanced)
            {
                Debug.LogWarning("이미 전직했습니다.");
                return false;
            }

            if (newJob == JobType.None)
            {
                Debug.LogWarning("기본 직업으로는 전직할 수 없습니다.");
                return false;
            }

            currentJob = newJob;
            canAdvance = false;

            Debug.Log($"전직 완료: {currentJob}");
            OnClassChanged?.Invoke(currentJob);

            return true;
        }
    }
}