using System;
using System.Collections;
using UnityEngine;

namespace FS_ThirdPerson
{
    public class AsyncUtil
    {
        public static IEnumerator RunAfterFrames(int numOfFrames, Action action)
        {
            for (int i = 0; i < numOfFrames; i++)
                yield return null;

            action.Invoke();
        }

        public static IEnumerator RunAfterDelay(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);

            action.Invoke();
        }
    }
}