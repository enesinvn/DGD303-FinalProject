using UnityEngine;
using UnityEngine.UI;

public class DebugButtonSprites : MonoBehaviour
{
    void Start()
    {
        // Find all buttons in scene
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        
        Debug.Log($"=== BUTTON SPRITE DEBUG - Found {buttons.Length} buttons ===");
        
        foreach (Button button in buttons)
        {
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                Debug.Log($"Button: {button.gameObject.name}");
                Debug.Log($"  - Has Image: YES");
                Debug.Log($"  - Sprite: {(image.sprite != null ? image.sprite.name : "NULL/NONE")}");
                Debug.Log($"  - Color: {image.color}");
                Debug.Log($"  - Material: {(image.material != null ? image.material.name : "NULL")}");
                Debug.Log("---");
            }
            else
            {
                Debug.LogWarning($"Button {button.gameObject.name} has NO Image component!");
            }
        }
        
        Debug.Log("=== END BUTTON SPRITE DEBUG ===");
    }
}
