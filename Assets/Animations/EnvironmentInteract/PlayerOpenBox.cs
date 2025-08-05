using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;

public class PlayerOpenBox : MonoBehaviour
{
    bool isPlaying = false;

    IEnumerator OnInteract()
    {
        if (list.Count > 0 && isPlaying == false)
        {
            isPlaying = true;

            var thirdPersonMove = GetComponent<ThirdPersonMove>();
            thirdPersonMove.enabled = false;
            var thirdPersonJump = GetComponent<ThirdPersonJump>();
            thirdPersonJump.enabled = false;
            var thirdPersonRoll = GetComponent<ThirdPersonRoll>();
            thirdPersonRoll.enabled = false;

            var director = list[0];
            list.RemoveAt(0);
            var pos = director.transform.Find("PlayerStandPosition");
            transform.position = pos.position;
            //Debug.Log(pos.position);
            transform.rotation = pos.rotation;
            var animator = GetComponent<Animator>();
            foreach (var output in director.playableAsset.outputs)
            {
                if (output.streamName == "PlayerTrack")
                {
                    director.SetGenericBinding(output.sourceObject, animator);
                    break;
                }
            }
            director.Play();
            while(director.state == PlayState.Playing)
            {
                yield return null;
            }
            thirdPersonMove.enabled = true;
            thirdPersonJump.enabled = true;
            thirdPersonRoll.enabled = true;
            isPlaying = false;
        }
    }
    
    List<PlayableDirector> list = new List<PlayableDirector>();
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Box")
        {
            var director = other.gameObject.GetComponent<PlayableDirector>();
            if (director != null && !list.Contains(director))
            {
                list.Add(director);
            }
            //Debug.Log(transform.position);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Box")
        {
            var director = other.gameObject.GetComponent<PlayableDirector>();
            if (director != null && list.Contains(director))
            {
                list.Remove(director);
            }
        }
    }
    
}
