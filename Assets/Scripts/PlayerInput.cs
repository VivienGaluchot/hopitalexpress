using System.Collections.Generic;
using UnityEngine;


public class PlayerInput {

    public static readonly List<PlayerInput> All = new List<PlayerInput>() {
        {new PlayerInput(Kind.Keyboard,
                         KeyCode.Return,
                         KeyCode.Backspace,
                         "Joy0X",
                         "Joy0Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick1Button0,
                         KeyCode.Joystick1Button1,
                         "Joy1X",
                         "Joy1Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick2Button0,
                         KeyCode.Joystick2Button1,
                         "Joy2X",
                         "Joy2Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick3Button0,
                         KeyCode.Joystick3Button1,
                         "Joy3X",
                         "Joy3Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick4Button0,
                         KeyCode.Joystick4Button1,
                         "Joy4X",
                         "Joy4Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick5Button0,
                         KeyCode.Joystick5Button1,
                         "Joy5X",
                         "Joy5Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick6Button0,
                         KeyCode.Joystick6Button1,
                         "Joy6X",
                         "Joy6Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick7Button0,
                         KeyCode.Joystick7Button1,
                         "Joy7X",
                         "Joy7Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick8Button0,
                         KeyCode.Joystick8Button1,
                         "Joy8X",
                         "Joy8Y")},
    };


    public enum Kind {
        Keyboard,
        Jostick
    }


    public readonly Kind kind;

    private readonly KeyCode action0;

    private readonly KeyCode action1;

    private readonly string xAxis;

    private readonly string yAxis;


    PlayerInput(Kind kind, KeyCode action0, KeyCode action1, string xAxis, string yAxis) {
        this.kind = kind;
        this.action0 = action0;
        this.action1 = action1;
        this.xAxis = xAxis;
        this.yAxis = yAxis;
    }

    public bool GetAction0() {
        return Input.GetKeyDown(action0);
    }

    public bool GetAction1() {
        return Input.GetKeyDown(action1);
    }

    public float GetX() {
        return Input.GetAxis(xAxis);
    }

    public float GetY() {
        return Input.GetAxis(yAxis);
    }

}
