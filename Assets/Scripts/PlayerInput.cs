using System.Collections.Generic;
using UnityEngine;


public class PlayerInput {

    public static readonly List<PlayerInput> All = new List<PlayerInput>() {
        {new PlayerInput(Kind.Keyboard,
                         KeyCode.Space,
                         KeyCode.Backspace,
                         KeyCode.Return,
                         "Joy0X",
                         "Joy0Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick1Button0,
                         KeyCode.Joystick1Button1,
                         KeyCode.Joystick1Button0,
                         "Joy1X",
                         "Joy1Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick2Button0,
                         KeyCode.Joystick2Button1,
                         KeyCode.Joystick2Button0,
                         "Joy2X",
                         "Joy2Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick3Button0,
                         KeyCode.Joystick3Button1,
                         KeyCode.Joystick3Button0,
                         "Joy3X",
                         "Joy3Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick4Button0,
                         KeyCode.Joystick4Button1,
                         KeyCode.Joystick4Button0,
                         "Joy4X",
                         "Joy4Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick5Button0,
                         KeyCode.Joystick5Button1,
                         KeyCode.Joystick5Button0,
                         "Joy5X",
                         "Joy5Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick6Button0,
                         KeyCode.Joystick6Button1,
                         KeyCode.Joystick6Button0,
                         "Joy6X",
                         "Joy6Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick7Button0,
                         KeyCode.Joystick7Button1,
                         KeyCode.Joystick7Button0,
                         "Joy7X",
                         "Joy7Y")},
        {new PlayerInput(Kind.Jostick,
                         KeyCode.Joystick8Button0,
                         KeyCode.Joystick8Button1,
                         KeyCode.Joystick8Button0,
                         "Joy8X",
                         "Joy8Y")},
    };


    public enum Kind {
        Keyboard,
        Jostick
    }


    public readonly Kind kind;

    // space / A
    private readonly KeyCode action0;

    // backspace / B
    private readonly KeyCode action1;

    // enter / A
    private readonly KeyCode action2;

    private readonly string xAxis;

    private readonly string yAxis;


    PlayerInput(Kind kind, KeyCode action0, KeyCode action1, KeyCode action2, string xAxis, string yAxis) {
        this.kind = kind;
        this.action0 = action0;
        this.action1 = action1;
        this.action2 = action2;
        this.xAxis = xAxis;
        this.yAxis = yAxis;
    }

    public bool GetAction0() {
        return Input.GetKeyDown(action0);
    }

    public bool GetAction0Up() {
        return Input.GetKeyUp(action0);
    }

    public bool GetAction1() {
        return Input.GetKeyDown(action1);
    }

    public bool GetAction1Up() {
        return Input.GetKeyUp(action1);
    }

    public bool GetAction2() {
        return Input.GetKeyDown(action2);
    }

    public bool GetAction2Up() {
        return Input.GetKeyUp(action2);
    }

    public float GetX() {
        return Input.GetAxis(xAxis);
    }

    public float GetY() {
        return Input.GetAxis(yAxis);
    }

}
