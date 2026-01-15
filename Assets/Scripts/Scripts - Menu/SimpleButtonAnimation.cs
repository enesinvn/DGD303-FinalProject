using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Switch to Hover animation
        animator.Play("Hover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Return to Normal animation
        animator.Play("Normal");
    }
}