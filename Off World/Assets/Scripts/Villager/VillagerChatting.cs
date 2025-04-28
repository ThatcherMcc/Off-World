using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;

public class VillagerChatting : MonoBehaviour, InteractableI
{
    [Header("Dialogue")]
    public GameObject chatBoxPanel; //  Assign the ChatBox Panel
    public TextAsset dialogueFile;
    private List<string> dialogue = new List<string>();
    private int currentDialogueIndex = 0;

    [Header("Dialogue Settings")]
    public bool showGreetingWhenPlayerApproaches = true;

    private TextMeshProUGUI dialogueText; //  TextMeshPro Text
    private bool isPlayerInRange = false;
    private InteractController controller;

    private void Start()
    {
        LoadDialogueFromJson();
        //chatBoxPanel = GameObject.FindGameObjectWithTag("ChatBoxPanel"); //  Find by tag is inefficient
        dialogueText = chatBoxPanel.GetComponentInChildren<TextMeshProUGUI>();
        chatBoxPanel.SetActive(false); //  Ensure it's hidden at start
    }

    private void Update()
    {
        if (!isPlayerInRange)
        {
            HideDialogue();
            if (controller != null) //  Hide when out of range
            {
                controller.chatting = false;
            }

        }
    }

    public void PlayerInRange(bool inRange)
    {
        isPlayerInRange = inRange;
        if (!inRange)
        {
            HideDialogue();
        }
    }

    public void Interact(InteractController controller)
    {
        this.controller = controller;
        AdvanceDialogue(); // Now call AdvanceDialogue()
    }

    private void ShowDialogue()
    {
        if (dialogue.Count > 0)
        {
            chatBoxPanel.SetActive(true);
            UpdateDialogueText();
        }
    }

    private void HideDialogue()
    {
        chatBoxPanel.SetActive(false);
        currentDialogueIndex = 0;
    }

    private void UpdateDialogueText()
    {
        dialogueText.text = dialogue[currentDialogueIndex];
    }

    public void AdvanceDialogue()
    {
        if (isPlayerInRange)
        {
            if (!chatBoxPanel.activeSelf) // If chatbox is hidden, show it
            {
                controller.chatting = true;
                ShowDialogue();
            }
            else // If chatbox is already showing, advance text
            {
                currentDialogueIndex++;
                if (currentDialogueIndex < dialogue.Count)
                {
                    controller.chatting = true;
                    UpdateDialogueText();
                }
                else
                {
                    controller.chatting = false;
                    HideDialogue();
                }
            }
        }
    }

    private void LoadDialogueFromJson()
    {
        if (dialogueFile != null)
        {
            string json = dialogueFile.text;
            DialogueWrapper wrapper = JsonUtility.FromJson<DialogueWrapper>(json);
            dialogue = wrapper.dialogue;
        }
        else
        {
            Debug.LogError("Dialogue file not assigned!");
        }
    }

    [System.Serializable] // Important!
    private class DialogueWrapper
    {
        public List<string> dialogue;
    }
}