using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSchemes
{
    public List<ColorScheme> color_schemes;
    public ColorSchemes()
    {
        color_schemes = new List<ColorScheme>()
        {
            new ColorScheme(new Color32[] {
                    new Color32(0xCE, 0xED, 0xEB, 0xFF),
                    new Color32(0xCC, 0xE0, 0xDF, 0xFF),
                    new Color32(0xB6, 0xDE, 0xDB, 0xFF),
                }),
            new ColorScheme(new Color32[] {
                    new Color32(0xF2, 0xF2, 0xF2, 0xFF),
                    new Color32(0xE8, 0xE8, 0xE8, 0xFF),
                    new Color32(0xBC, 0xC4, 0xC3, 0xFF),
                }),
            new ColorScheme(new Color32[] {
                    new Color32(0xFC, 0xF5, 0xED, 0xFF),
                    new Color32(0xFF, 0xEE, 0xDB, 0xFF),
                    new Color32(0xF0, 0xE9, 0xE1, 0xFF),
                }),
            new ColorScheme(new Color32[] {
                    new Color32(0xEB, 0xD1, 0xDD, 0xFF),
                    new Color32(0xFC, 0xE3, 0xEF, 0xFF),
                    new Color32(0xF7, 0xE4, 0xED, 0xFF),
                }),
          };
    }

    public ColorScheme getRandomColorScheme()
    {
        return color_schemes[Random.Range(0, color_schemes.Count)];
    }
}
public class ColorScheme
{
    public Color32[] color_scheme;
    public ColorScheme(Color32[] colors)
    {
        this.color_scheme = colors;
    }
    public Color GetBg1()
    {
        return color_scheme[0];
    }

    public Color GetBg2()
    {
        return color_scheme[1];
    }

    public Color GetBg3()
    {
        return color_scheme[2];
    }

    public Color GetRandomColor()
    {
        return color_scheme[Random.Range(0, 3)];
    }

    public void SetStarting()
    {

    }
}