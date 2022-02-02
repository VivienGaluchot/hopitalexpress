using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PrefabItem : MonoBehaviour {

    public string path;
    public List<Next> Nexts;

    public class Next {
        public Next(PrefabItem _item, InputField _proba = null) { proba = _proba; item = _item; }

        public InputField proba;
        public PrefabItem item;
    }

    private List<LineRenderer> startingLines, endingLines;
    public bool isNexted { get; private set; }

    private void Start() {
        if (Nexts == null)
            Nexts = new List<Next>();
        if (startingLines == null)
            startingLines = new List<LineRenderer>();
        if (endingLines == null)
            endingLines = new List<LineRenderer>();
    }

    private void Update() {
        for (int i = 0; i < startingLines.Count; i++) {
            if (startingLines[i])
                startingLines[i].SetPosition(0, transform.position);
            else
                startingLines.RemoveAt(i);
        }
        for (int i = 0; i < endingLines.Count; i++) {
            if (endingLines[i])
                endingLines[i].SetPosition(1, transform.position);
            else
                endingLines.RemoveAt(i);
        }
    }

    public void Delete() {
        foreach (LineRenderer lr in startingLines)
            Destroy(lr.gameObject);
        foreach (LineRenderer lr in endingLines)
            Destroy(lr.gameObject);

        Destroy(gameObject);
    }

    public void DeleteIfNexted(PrefabItem pi) {
        Nexts.RemoveAll(n => n.item == pi);
    }

    public void UnNext() {
        isNexted = false;
    }

    public void AddToStartingLine(LineRenderer newLR) {
        if (startingLines == null)
            startingLines = new List<LineRenderer>();
        startingLines.Add(newLR);
    }

    public void AddToEndingLine(LineRenderer newLR) {
        if (endingLines == null)
            endingLines = new List<LineRenderer>();
        endingLines.Add(newLR);
        isNexted = true;
    }

    public float TimeDisplayedValue() {
        string value = transform.Find("ValueDisplayer(Clone)/InputField").GetComponent<InputField>().text;
        return value != "" ? float.Parse(value) : 0f;
    }

    // We try to add the item as next
    public bool TryAddNext(LineRenderer lr, PrefabItem item) {
        foreach(Next n in Nexts) {
            if (n.item == item)
                return false;
        }

        // We say to thee item we want it to be our next
        if (!item.TrySetNexted(lr, this))
            return false;

        // You're not already my next, you accepted to be my next, so be it
        Nexts.Add(new Next(item, lr.gameObject.GetComponent<LineController>().ProbaDisplayedValue()));
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
