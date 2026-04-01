using UnityEngine;

public class ColorReference : MonoBehaviour
{
    public static readonly Color[] PlayerColors = new Color[]
    {
        new Color(1f, 0.3f, 0.3f),   // P1 red
        new Color(0.3f, 0.5f, 1f),   // P0 blue
        new Color(0.3f, 1f, 0.4f),   // P2 green
        new Color(1f, 0.85f, 0.2f),  // P3 yellow
    };

    public static readonly Color[] PlayerColorsHilt = new Color[]
    {
        HexColor("914B44"),  // P1 red
        HexColor("44497B"),  // P2 blue
        HexColor("497B44"),  // P3 green
        HexColor("DCC73F"),  // P4 yellow
    };

    private static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }
}
