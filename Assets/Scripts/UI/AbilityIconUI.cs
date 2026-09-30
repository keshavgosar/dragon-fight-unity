using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Skill icon with a radial cooldown overlay and countdown text.</summary>
public class AbilityIconUI : MonoBehaviour
{
    [SerializeField] DragonCombat owner;
    [SerializeField] int abilityIndex;
    [SerializeField] Image icon;
    [SerializeField] Image cooldownOverlay; // Image Type = Filled, Radial 360, Origin Top
    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text keyText;
    [SerializeField] string keyLabel = "1";

    void Start()
    {
        var a = owner.Abilities[abilityIndex];
        if (icon && a.icon) icon.sprite = a.icon;
        if (keyText) keyText.text = keyLabel;
    }

    void Update()
    {
        var a = owner.Abilities[abilityIndex];
        cooldownOverlay.fillAmount = a.Normalized;
        if (timerText) timerText.text = a.Remaining > 0.05f ? a.Remaining.ToString("0.0") : "";
    }
}
