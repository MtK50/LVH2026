using Meta.Voice.Net.WebSockets;
using Oculus.VoiceSDK.UX;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private unity4dv.Plugin4DS plugin4DS;


    private void Awake()
    {
        StartCoroutine(PauseResumeAnimation());
    }


    private IEnumerator PauseResumeAnimation()
    {

        for(int i = 0; i < 3; i++)
        {
            plugin4DS.Play(false);
            yield return new WaitForSeconds(5f);
            plugin4DS.Play(true);
            yield return new WaitForSeconds(5f);
        }
    }
}
