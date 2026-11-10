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

    // Si algo no se arrastró en el Inspector, lo busca entre los hijos
    void ResolveReferences()
    {
        foreach (var t in GetComponentsInChildren<TMP_Text>(true))
        {
            string n = t.name.ToLower();
            if (descriptionText == null && n.Contains("desc")) descriptionText = t;
            else if (statText == null && n.Contains("stat")) statText = t;
            else if (titleText == null && t != descriptionText && t != statText) titleText = t;
        }

        if (iconImage == null)
        {
            foreach (var img in GetComponentsInChildren<Image>(true))
            {
                if (img.gameObject != gameObject) { iconImage = img; break; }
            }
        }

        if (button == null) button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
            button.targetGraphic = GetComponent<Image>();
        }
    }

    public void Setup(UpgradeData data, string statLine, Action<UpgradeData> onChosen)
    {
        ResolveReferences();

        if (titleText == null || descriptionText == null)
        {
            Debug.LogError("UpgradeCard: no encuentro los textos de título o descripción en el prefab", this);
            return;
        }

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