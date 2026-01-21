using UnityEngine;

public class AnimationController : MonoBehaviour  
{

    public Animator animator;
    bool PushTog = false;
    public GameObject hammer;
    public bool Ragdolled = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            PushTog=!PushTog;
            animator.SetBool("Pushup", PushTog);
        }



    }
      
}    


