using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OmnicatLabs.Timers;

public class AbilityButtonControl : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Image darken;
    private Timer timer;
    private bool disabled;

    public void StartCoolDown(Timer newTimer) {
        disabled = true;
        text.enabled = true;
        darken.enabled = true;
        timer = newTimer;
    }

    public void Update() {
        if (disabled) {
            text.SetText((Mathf.Round(timer.timeRemaining * 10f) / 10f).ToString());
            darken.fillAmount = timer.timeRemaining / timer.amountOfTime;
        }
    }

    public void EndCooldown() {
        disabled = false;
        text.enabled = false;
        darken.enabled = false;
    }
}
