using TMPro;
using UnityEngine;

public class WallPickupTextManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI text;

    public static WallPickupTextManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    public void SetText(string newText)
    {
        Debug.Log("Setting wall text to "+newText);
        text.text = newText;
    }
}
