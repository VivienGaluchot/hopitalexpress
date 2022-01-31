using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PrefabItem : MonoBehaviour {

    public string path;
    public List<Next> Nexts;
    public float myTime;

    [Serializable]
    public struct Next {
        public Next(float _proba, PrefabItem _item) { proba = _proba; item = _item; }

        public float proba;
        public PrefabItem item;
    }

    private List<LineRenderer> startingLines, endingLines;
    public bool isNexted { get; private set; }

    private void Start() {
        Nexts = new List<Next>();
        startingLines = new List<LineRenderer>();
        endingLines = new List<LineRenderer>();
    }

    private void Update() {
        foreach (LineRenderer lr in startingLines)
            lr.SetPosition(0, transform.position);

        foreach (LineRenderer lr in endingLines)
            lr.SetPosition(1, transform.position);
    }

    public void SetPath(string _path) {
        path = _path;
    }

    public void StoreDisplayedValue() {
        string value = transform.Find("ValueDisplayer(Clone)/InputField/Text").gameObject.GetComponent<Text>().text;
        myTime = value != "" ? float.Parse(value) : 0f;
    }

    // We try to add the item as next
    public bool TryAddNext(LineRenderer lr, PrefabItem item, float proba = 1f) {
        foreach(Next n in Nexts) {
            if (n.item == item)
                return false;
        }

        // We say to thee item we want it to be our next
        if (!item.TrySetNexted(lr, this))
            return false;

        // You're not already my next, you accepted to be my next, so be it
        Nexts.Add(new Next(proba, item));
        startingLines.Add(lr);

        return true;
    }

    // I can be your next only if you're not onlready MY next
    public bool TrySetNexted(LineRenderer lr, PrefabItem item) {
        foreach (Next n in Nexts) {
            if (n.item == item)
                return false;
        }

        isNexted = true;
        endingLines.Add(lr);

        return true;
    }
}
