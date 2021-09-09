using ErgoShop.Managers;
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
    public static ThemeManager Instance;

    [SerializeField] private Color DefaultColor = Color.white;
    [SerializeField] private Color ThemeSombre = Color.gray;

    public THEMEAMENAGE3D Theme = THEMEAMENAGE3D.Default;

    [SerializeField] private GameObject backGround = null;
    [SerializeField] private GameObject backGround3D = null;
    [SerializeField] private GameObject backGroundPreviewFurniture = null;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this;
    }

    public void ChangeTheme(THEMEAMENAGE3D _NewTheme)
    {
        Theme = (THEMEAMENAGE3D)_NewTheme;
        //Debug.Log(Theme.ToString());
        switch (Theme)
        {
            case THEMEAMENAGE3D.Default:
                if (backGround)
                {
                    backGround.GetComponent<SpriteRenderer>().color = DefaultColor;

                    backGround3D.GetComponent<Renderer>().material.color = DefaultColor;

                    backGroundPreviewFurniture.GetComponent<Camera>().backgroundColor = DefaultColor;
                }
                break;
            case THEMEAMENAGE3D.Sombre:
                if (backGround)
                {
                    backGround.GetComponent<SpriteRenderer>().color = ThemeSombre;

                    backGround3D.GetComponent<Renderer>().material.color = ThemeSombre;

                    backGroundPreviewFurniture.GetComponent<Camera>().backgroundColor = ThemeSombre;
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
        ErgoShop.Managers.SettingsManager.Instance.SoftwareParameters.Theme = Theme;
        ChangeTheme(Theme);
        SettingsManager.Instance.SaveParameters();
    }
}
