using Godot;
using System;

public partial class MainMenu : Node2D
{
    [Export]
    SubViewport inputViewport;
    
    [Export]
    Control viewportGraphic;

    public override void _Input(InputEvent @event)
    {

        if (@event is InputEventMouse mouseEvent)
        {
            // GET MOUSE POSITION RELATIVE TO THE GRAPHIC
            Vector2 currentFactor = new (inputViewport.Size.X / viewportGraphic.Size.X, inputViewport.Size.Y / viewportGraphic.Size.Y);
            var relativePos = mouseEvent.Position - viewportGraphic.Position;

            /*
            GD.Print("MOUSE POS " + mouseEvent.Position);
            GD.Print("GRAPHIC POS " + viewportGraphic.Position);
            GD.Print("RELATIVE POS" + relativePos);
            GD.Print("RELATIVE POS SCALED" + relativePos * currentFactor);
            */

            mouseEvent.Position = relativePos * currentFactor;
            inputViewport.PushInput(@event);
        }
    }

}
