using System.Collections.Generic;
using UnityEngine;


public class PlayerInput {

    public static readonly PlayerInput Keyboard = new PlayerInput(KeyCode.Return, KeyCode.Backspace, "Joy0X", "Joy0Y");
    public static readonly PlayerInput Joystick1 = new PlayerInput(KeyCode.Joystick1Button0, KeyCode.Joystick1Button1, "Joy1X", "Joy1Y");
    public static readonly PlayerInput Joystick2 = new PlayerInput(KeyCode.Joystick2Button0, KeyCode.Joystick2Button1, "Joy2X", "Joy2Y");
    public static readonly PlayerInput Joystick3 = new PlayerInput(KeyCode.Joystick3Button0, KeyCode.Joystick3Button1, "Joy3X", "Joy3Y");
    public static readonly PlayerInput Joystick4 = new PlayerInput(KeyCode.Joystick4Button0, KeyCode.Joystick4Button1, "Joy4X", "Joy4Y");
    public static readonly PlayerInput Joystick5 = new PlayerInput(KeyCode.Joystick5Button0, KeyCode.Joystick5Button1, "Joy5X", "Joy5Y");
    public static readonly PlayerInput Joystick6 = new PlayerInput(KeyCode.Joystick6Button0, KeyCode.Joystick6Button1, "Joy6X", "Joy6Y");
    public static readonly PlayerInput Joystick7 = new PlayerInput(KeyCode.Joystick7Button0, KeyCode.Joystick7Button1, "Joy7X", "Joy7Y");
    public static readonly PlayerInput Joystick8 = new PlayerInput(KeyCode.Joystick8Button0, KeyCode.Joystick8Button1, "Joy8X", "Joy8Y");

    public static readonly List<PlayerInput>  All = new List<PlayerInput>() {
        Keyboard,
        Joystick1,
        Joystick2,
        Joystick3,
        Joystick4,
        Joystick5,
        Joystick6,
        Joystick7,
        Joystick8,
    };

    
    private KeyCode action0;
    
    private KeyCode action1;

    private string xAxis;

    private string yAxis;

    
    PlayerInput(KeyCode action0, KeyCode action1, string xAxis, string yAxis) {
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
