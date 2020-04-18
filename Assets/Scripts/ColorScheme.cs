using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorScheme
{
    public Color32[] color_scheme;

    public ColorScheme()
    {
        int dice_roll = Random.Range(1, 6);
        switch (dice_roll) {
            default:
                color_scheme = new Color32[] {
                    new Color32(0xCE, 0xED, 0xEB, 0xFF),
                    new Color32(0xCC, 0xE0, 0xDF, 0xFF),
                    new Color32(0xB6, 0xDE, 0xDB, 0xFF),
                    /**
                    new Color32(0xCE, 0xED, 0xEB, 0xFF),
                    new Color32(0x76, 0xD3, 0xC9, 0xFF),
                    new Color32(0x1F, 0x8A, 0xD8, 0xFF),
                    new Color32(0xFB, 0xA2, 0x85, 0xFF),
                    new Color32(0xFE, 0xE2, 0xE1, 0xFF),
                    new Color32(0xFE, 0xF3, 0xF2, 0xFF)
                     **/
                };
                break;
        }
    
    }
    public Color GetBg1()
    {
        return color_scheme[0];
    }

    public Color GetBg2()
    {
        return color_scheme[1];
    }

    public Color GetRandomColor()
    {
        return color_scheme[Random.Range(0, 3)];
    }

    public void SetStarting()
    {

    }
}