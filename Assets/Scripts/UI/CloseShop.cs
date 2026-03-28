using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseShop : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }
    public Animator animator;
    public void OnHideComplete()
    {
        gameObject.SetActive(false);
    }
        public void OpenShop()
    {
        gameObject.SetActive(true);
        animator.SetBool("isOpen", true);
    }

    public void Close()
    {
        animator.SetBool("isOpen", false);
    }
}
