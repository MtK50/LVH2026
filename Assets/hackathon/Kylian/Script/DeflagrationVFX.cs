using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class DeflagrationVFX : MonoBehaviour
{

    public VisualEffect deflagration;
    public unity4dv.Plugin4DS piece4DS;

    public IEnumerator ShowVFXEffectDeflagration()
    {
        piece4DS.Play(true);
        deflagration.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        deflagration.gameObject.SetActive(false);
        piece4DS.Play(false);
        piece4DS.GoToFrame(piece4DS.FirstActiveFrame);


    }


}
