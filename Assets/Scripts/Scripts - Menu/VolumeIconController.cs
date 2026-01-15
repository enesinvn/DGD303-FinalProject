using UnityEngine;
using UnityEngine.UI;

public class VolumeIconController : MonoBehaviour
{
    [Header("Volume Icon Sprites")]
    [SerializeField] private Sprite mutedIcon;      // 0%
    [SerializeField] private Sprite lowIcon;        // 1-33%
    [SerializeField] private Sprite mediumIcon;     // 34-66%
    [SerializeField] private Sprite highIcon;       // 67-100%

    [Header("Icon Image")]
    [SerializeField] private Image volumeIconImage;

    // Update icon based on volume value
    public void UpdateVolumeIcon(float volume)
    {
        if (volumeIconImage == null) return;

        // Volume between 0-1 (slider value)
        if (volume <= 0f)
        {
            volumeIconImage.sprite = mutedIcon;
        }
        else if (volume <= 0.33f)
        {
            volumeIconImage.sprite = lowIcon;
        }
        else if (volume <= 0.66f)
        {
            volumeIconImage.sprite = mediumIcon;
        }
        else
        {
            volumeIconImage.sprite = highIcon;
        }
    }
}