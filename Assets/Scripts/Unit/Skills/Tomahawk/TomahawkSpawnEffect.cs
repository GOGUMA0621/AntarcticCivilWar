using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TomahawkSpawnEffect : MonoBehaviour
{
    public Animator effectAnim;
    // Start is called before the first frame update
    public void PlayEffect()
    {
        effectAnim.Play("Effect");
    }
}
