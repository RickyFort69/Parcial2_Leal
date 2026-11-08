using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] TMP_Text statText;
    [SerializeField] Image iconImage;
    [SerializeField] Button button;

    public void Setup(UpgradeData data, string statLine, Action<UpgradeData> onChosen)
    {
        titleText.text = data.title;
        descriptionText.text = data.description;
        if (statText != null) statText.text = statLine;

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.gameObject.SetActive(data.icon != null);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onChosen(data));
    }
}