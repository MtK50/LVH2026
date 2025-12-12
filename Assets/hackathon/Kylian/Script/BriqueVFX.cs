using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class BriqueVFX : MonoBehaviour
{

    public Animator animator;
    public unity4dv.Plugin4DS piece4DS;

    public IEnumerator ShowVFXBrique()
    {
        animator.gameObject.SetActive(true);
        
        //animator.Play("New Animation", -1, 0f);

        int nbFrame = piece4DS.SequenceNbOfFrames;
        float frameRate = piece4DS.Framerate;
        Debug.Log("nbFrame / frameRate" + nbFrame / frameRate);
        piece4DS.Play(true);
        yield return new WaitForSeconds(nbFrame / frameRate);
        piece4DS.Play(false);
        piece4DS.GoToFrame(piece4DS.FirstActiveFrame);
        animator.gameObject.SetActive(false);
    }

}
