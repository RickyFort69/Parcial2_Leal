using UnityEngine;

[CreateAssetMenu(menuName = "FF/UI Theme", fileName = "FFUITheme")]
public class FFUITheme : ScriptableObject
{
    [Header("Ventanas")]
    public Color windowColor = new Color(0.05f, 0.08f, 0.45f, 0.92f);
    public Color borderColor = new Color(0.85f, 0.85f, 0.95f, 1f);

    [Header("Texto")]
    public Color textColor = Color.white;
    public Color selectedColor = new Color(1f, 0.92f, 0.3f, 1f);
    public Color disabledColor = new Color(0.55f, 0.55f, 0.65f, 1f);
    public int fontSize = 34;

    [Header("HP")]
    public Color hpLowColor = new Color(1f, 0.85f, 0.2f, 1f);        // menos del 50%
    public Color hpCriticalColor = new Color(1f, 0.3f, 0.3f, 1f);    // menos del 25%

    [Header("Barras")]
    public Color atbColor = new Color(0.3f, 0.8f, 1f, 1f);
    public Color atbReadyColor = new Color(1f, 0.95f, 0.4f, 1f);
    public Color limitColor = new Color(1f, 0.55f, 0.15f, 1f);
    public Color barBackColor = new Color(0f, 0f, 0f, 0.6f);

    [Header("Números flotantes")]
    public Color damageColor = Color.white;
    public Color healColor = new Color(0.4f, 1f, 0.5f, 1f);
}