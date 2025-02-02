using Godot;
using System;

public partial class MainMenu : Control
{
    [Export]
    Control mainMenuContainer;

    [Export]
    Control levelSelectContainer;

    [Export]
    MenuList mainMenuList;

    [Export]
    MenuList levelSelectList;

    Control currentPage;
    Control nextPage;

    bool isFadingIn = false;
    bool isFadingOut = false;

    float fadeInTime = 0.2f;
    float fadeOutTime = 0.2f;
    float fadeTimer = 0;

    public override void _Ready()
    {
        mainMenuList.OnMenuEntrySelected += OnMainMenuClicked;
        levelSelectList.OnMenuEntrySelected += OnLevelSelectClicked;

        currentPage = mainMenuContainer;
    }

    public override void _Process(double delta)
    {
        if (nextPage != null)
        {
            fadeTimer += (float)delta;

            if (isFadingOut)
            {
                GD.Print("FADING CURR: " + currentPage.Name);
                ((ShaderMaterial)currentPage.Material).SetShaderParameter("dissolve_value", fadeTimer / fadeOutTime);

                if (fadeTimer >= fadeOutTime)
                {
                    fadeTimer = 0;
                    isFadingOut = false;
                    isFadingIn = true;
                    currentPage.Hide();
                    ((ShaderMaterial)nextPage.Material).SetShaderParameter("dissolve_value", 1);
                    nextPage.Show();
                }
            }
            else if (isFadingIn)
            {
                GD.Print("FADING IN NEXT: " + nextPage.Name);
                ((ShaderMaterial)nextPage.Material).SetShaderParameter("dissolve_value", 1 - (fadeTimer / fadeInTime));

                if (fadeTimer >= fadeInTime)
                {
                    fadeTimer = 0;
                    isFadingIn = false;
                    currentPage = nextPage;
                    nextPage = null;
                }
            }
        }
    }

    private void OnMainMenuClicked(string identifier)
    {
        if (nextPage != null) return;

        switch (identifier)
        {
            case "LEVEL_SELECT":
                nextPage = levelSelectContainer;
                isFadingOut = true;
                break;
        }
    }

    private void OnLevelSelectClicked(string identifier)
    {
        if (nextPage != null) return;

        switch (identifier)
        {
            case "TEST_LEVEL":
                var parameters = new Godot.Collections.Dictionary()
                {
                    ["level_path"] = "res://assets/scenes/levels/TestLevel.tscn",
                };
                SceneSwitcher.Instance.ChangeScene("res://assets/scenes/levels/levelcontainer/LevelContainer.tscn", parameters);
                break;
            case "BACK":
                nextPage = mainMenuContainer;
                isFadingOut = true;
                break;
        }
    }
}
