using Godot;
using Godot.Collections;
using System;

public partial class SceneSwitcher : Node
{

    public static SceneSwitcher Instance { get; set; }

    Dictionary newSceneParameters;

    public override void _Ready()
    {
        Instance = this;
    }

    public void ChangeScene(string nextSceneFilename, Dictionary parameters = null)
    {
        newSceneParameters = parameters;
        GetTree().ChangeSceneToFile(nextSceneFilename);
    }

    public object GetParameter(string paramName)
    {
        if (newSceneParameters != null && newSceneParameters.ContainsKey(paramName))
        {
            return newSceneParameters[paramName];
        }

        return null;
    }
}
