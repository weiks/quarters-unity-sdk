using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImaginationOverflow.UniversalDeepLinking;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking
{
    public class UniversalDeeplinkingRuntimeScript : MonoBehaviour
    {

        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                StartCoroutine(CallDeepLinkManagerAfterDelay());
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                StartCoroutine(CallDeepLinkManagerAfterDelay());
            }
        }

        private bool _onJob = false;
        public IEnumerator CallDeepLinkManagerAfterDelay()
        {
            if (_onJob)
                yield break;

            _onJob = true;

            yield return new WaitForSeconds(.2f);
            try
            {
                DeepLinkManager.Instance.GameCameFromPause();
            }
            catch (Exception e)
            {
                Debug.LogError("RuntimeScript " + e, gameObject);
            }
            finally
            {
                _onJob = false;
            }
        }
    }
}
