﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmDialog
{
    private AudioClip clickSound;
    private AudioElement audioElement;
    private bool confirming = false, clickYes = false, clickNo = false;
    private Rect confirmRect;
    private float buttonWidth = 50, buttonHeight = 20, padding = 10;
    private Vector2 messageDimensions;

    public void StartConfirmation(AudioClip clickSound, AudioElement audioElement)
    {
        this.clickSound = clickSound;
        this.audioElement = audioElement;
        confirming = true;
        clickYes = false;
        clickNo = false;
    }

    public void EndConfirmation()
    {
        confirming = false;
        clickYes = false;
        clickNo = false;
    }
    public bool IsConfirming()
    {
        return confirming;
    }
    public bool MadeChoice()
    {
        return clickYes || clickNo;
    }
    public bool ClickedYes()
    {
        return clickYes;
    }
    public bool ClickedNo()
    {
        return clickNo;
    }
    public void Show(string message)
    {
        ShowDialog(message);
    }
    public void Show(string message, GUISkin skin)
    {
        GUI.skin = skin;
        ShowDialog(message);
    }
    private void ShowDialog(string message)
    {
        messageDimensions = GUI.skin.GetStyle("window").CalcSize(new GUIContent(message));
        float width = messageDimensions.x + 2 * padding;
        float height = messageDimensions.y + buttonHeight + 2 * padding;
        float leftPos = Screen.width / 2 - width / 2;
        float topPos = Screen.height / 2 - height / 2;
        confirmRect = new Rect(leftPos, topPos, width, height);
        confirmRect = GUI.Window(0, confirmRect, Dialog, message);
    }
    private void Dialog(int windowID)
    {
        float buttonLeft = messageDimensions.x / 2 - buttonWidth - padding / 2;
        float buttonTop = messageDimensions.y + padding;
        if (GUI.Button(new Rect(buttonLeft, buttonTop, buttonWidth, buttonHeight), "Yes"))
        {
            confirming = false;
            clickYes = true;
            PlayClick();
        }
        buttonLeft += buttonWidth + padding;
        if (GUI.Button(new Rect(buttonLeft, buttonTop, buttonWidth, buttonHeight), "No"))
        {
            confirming = false;
            clickNo = true;
            PlayClick();
        }
    }
    private void PlayClick()
    {
        if (audioElement != null) audioElement.Play(clickSound);
    }
}
