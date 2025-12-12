using System.Collections;
using UnityEngine;

public class TestAnim : MonoBehaviour
{
    public Animator animator;
    public unity4dv.Plugin4DS piece4DS;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {

            StartCoroutine(CouroutineAnimation());
        }
    }

    public IEnumerator CouroutineAnimation()
    {
        animator.gameObject.SetActive(true);
        animator.Play("New Animation", -1, 0f);

        int nbFrame = piece4DS.SequenceNbOfFrames;
        float frameRate = piece4DS.Framerate;
        piece4DS.Play(true);
        yield return new WaitForSeconds(nbFrame / frameRate);
        piece4DS.Play(false);
        animator.gameObject.SetActive(false);
    }

}
