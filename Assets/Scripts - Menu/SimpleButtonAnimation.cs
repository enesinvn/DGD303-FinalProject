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
        // Hover animasyonuna geç
        animator.Play("Hover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Normal animasyona dön
        animator.Play("Normal");
    }
}