using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum THEMEAMENAGE3D
{
    Default,
    Sombre,
    Finish,
}

public class ThemeManager : MonoBehaviour
{
    [SerializeField] private Color DefaultColor = Color.white;
    [SerializeField] private Color ThemeSombre = Color.gray;

    public THEMEAMENAGE3D Theme = THEMEAMENAGE3D.Default;

    [SerializeField] private GameObject backGround = null;

    public void ChangeTheme(THEMEAMENAGE3D _NewTheme)
    {
        Theme = (THEMEAMENAGE3D)_NewTheme;
        Debug.Log(Theme.ToString());
        switch (Theme)
        {
            case THEMEAMENAGE3D.Default:
                if (backGround)
                {
                    backGround.GetComponent<SpriteRenderer>().color = DefaultColor;
                }
                break;
            case THEMEAMENAGE3D.Sombre:
                if (backGround)
                {
                    backGround.GetComponent<SpriteRenderer>().color = ThemeSombre;
                }
                break;
            default:
                Debug.LogError("Theme not Find");
                break;
        }
    }

    public void ToggleTheme()
    {
        Theme++;
        if (Theme == THEMEAMENAGE3D.Finish)
        {
            Theme = THEMEAMENAGE3D.Default;
        }
        ChangeTheme(Theme);
    }
}
