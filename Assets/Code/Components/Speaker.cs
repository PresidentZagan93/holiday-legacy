using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Speaker : MonoBehaviour {

    public enum SpeakerShape
    {
        Box,
        Circle
    }

    public Vector2 offset;
    public SpeakerShape shape = SpeakerShape.Box;
    public Vector2 sizeBox = new Vector2(32, 32);
    public float sizeCircle = 32f;
    float nextCheck;
    float nextSay;
    string speech;
    Text speechText;
    bool speaking;
    int characterOn;
    float nextCharacter;
    public Font font;
    public AudioClip talkSound;
    ObjectSoundEmitter sound;

    private void Awake()
    {
        sound = GetComponent<ObjectSoundEmitter>();
        if(!sound) sound = gameObject.AddComponent<ObjectSoundEmitter>();
        sound.CreateSource("Speech", AudioManager.AudioType.Other);

        speechText = new GameObject("Speech", typeof(RectTransform)).AddComponent<Text>();
        speechText.text = "";
        speechText.font = font;
        speechText.verticalOverflow = VerticalWrapMode.Overflow;
        speechText.horizontalOverflow = HorizontalWrapMode.Wrap;
        speechText.resizeTextForBestFit = false;
        speechText.fontSize = 16;
        speechText.rectTransform.sizeDelta = new Vector2(400, 100);
        speechText.alignment = TextAnchor.MiddleCenter;

        var outline = speechText.gameObject.AddComponent<UnityEngine.UI.Extensions.NicerOutline>();
        outline.effectColor = Color.black;

        speechText.transform.SetParent(UIManager.OverlayCanvas.transform.Find("Overlay"));
    }

    private void Update()
    {
        if(speaking)
        {
            speechText.transform.position = transform.position;
            speechText.text = speech.Substring(0, characterOn);

            if(Time.time > nextCharacter)
            {
                nextCharacter = Time.time + 0.05f;
                characterOn++;
                if(characterOn > speech.Length)
                {
                    characterOn = speech.Length;
                }
                else
                {
                    AudioSource source = sound.GetSource("Speech");
                    source.pitch = Random.Range(0.95f, 1.05f);
                    source.PlayOneShot(talkSound);
                }
            }

            if (Time.time > nextSay)
            {
                speechText.text = "";
                speaking = false;
            }

            return;
        }

        if (Time.time > nextCheck)
        {
            nextCheck = Time.time + 0.25f;

            Rect rect = new Rect(offset.x + transform.position.x - (sizeBox.x * 0.5f), offset.y + transform.position.y - (sizeBox.y * 0.5f), sizeBox.x, sizeBox.y);
            bool inside = rect.Contains(Character.Player.transform.position);
            if(inside)
            {

            }
        }
    }

    public void Say(string speech, float duration = 3f)
    {
        nextSay = Time.time + duration + speech.Length * 0.05f;
        this.speech = speech;
        speaking = true;
        characterOn = 0;
        speechText.text = "";
    }

    private void OnDrawGizmosSelected()
    {
        if(shape == SpeakerShape.Box)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)offset, sizeBox);
        }
        else if(shape == SpeakerShape.Circle)
        {
            Gizmos.DrawWireSphere(transform.position + (Vector3)offset, sizeCircle);
        }
    }
}
