using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OmnicatLabs.Timers;

public class AbilityButtonControl : MonoBehaviour
{
    public Image grayedOut;
    public TextMeshProUGUI text;
    [HideInInspector]
    private Image image;
    private Timer timer;
    private void Start() {
        image = GetComponent<Image>();
    }

    public void StartCoolDown(Timer newTimer) {
        grayedOut.enabled = true;
        text.enabled = true;
        timer = newTimer;
    }

    public void Update() {
        if (grayedOut.enabled) {
            text.SetText(timer.timeRemaining.ToString());
        }
    }

    public void EndCooldown() {
        grayedOut.enabled = false;
        text.enabled = false;
    }
}
