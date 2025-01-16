using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class HealthContainer : Control
{
    List<TextureRect> hearts;

    [Export]
    Texture2D fullHeart;

    [Export]
    Texture2D halfHeart;

    [Export]
    Texture2D emptyHeart;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        hearts = GetNode<Control>("HBoxContainer").GetChildren().Select((x) => (TextureRect)x).ToList();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    const int heartSegments = 2;

    public void OnHealthChanged(int health)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            // 2, 4, 6
            var heartAmount = (i + 1) * heartSegments;

            if (health >= heartAmount)
            {
                hearts[i].Texture = fullHeart;
            }
            else if (health >= heartAmount-1)
            {
                hearts[i].Texture = halfHeart;
            }
            else
            {
                hearts[i].Texture = emptyHeart;
            }
        }
    }
}
