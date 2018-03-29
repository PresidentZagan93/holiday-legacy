using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDDead : MonoBehaviour {

    public Text time;
    public Text kills;
    public Text gold;
    public Text pickups;
    public Text score;
    public Text hiddenRooms;

    public Text levelMultiplier;
    public Text finalScore;
    public Text highScore;

    public float timePerStat = 1f;
    public float timer;
    public int statOn;

    float timeAmount;
    float killsAmount;
    float goldAmount;
    float pickupsAmount;
    float scoreAmount;
    float hiddenRoomsAmount;

    public AudioClip fillSound;
    public float fillRate;

    AudioSource sound;

    bool newHighscore;
    float nextFill;

    private void Awake()
    {
        sound = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        timeAmount = 0f;
        killsAmount = 0f;
        goldAmount = 0f;
        pickupsAmount = 0f;
        scoreAmount = 0f;
        hiddenRoomsAmount = 0f;

        statOn = 0;
        timer = 0f;
        newHighscore = false;
        sound.pitch = 0.5f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if(timer >= timePerStat)
        {
            timer = 0f;
            statOn++;
        }

        if(Time.time > nextFill && statOn < 9)
        {
            nextFill = Time.time + fillRate;
            sound.pitch += 0.065f;
            sound.PlayOneShot(fillSound);
        }

        float t = Time.deltaTime * 5f;

        if (statOn >= 0) timeAmount = Mathf.Lerp(timeAmount, Settings.Temporary.time, t);
        if (statOn >= 1) killsAmount = Mathf.Lerp(killsAmount, Settings.Temporary.kills, t);
        if (statOn >= 2) goldAmount = Mathf.Lerp(goldAmount, Settings.Temporary.gold, t);
        if (statOn >= 3) pickupsAmount = Mathf.Lerp(pickupsAmount, Settings.Temporary.pickups, t);
        if (statOn >= 5) scoreAmount = Mathf.Lerp(scoreAmount, Settings.Temporary.score, t);
        if (statOn >= 6) hiddenRoomsAmount = Mathf.Lerp(hiddenRoomsAmount, Settings.Temporary.secretsFound, t);

        float superScore = killsAmount * 100 + goldAmount * 100 + pickupsAmount * 100 + scoreAmount + hiddenRoomsAmount * 100;

        superScore *= (GeneratorManager.Stage + 1);

        time.text = "-"+timeAmount + " * 10";
        kills.text = killsAmount.RoundToInt() + " * 100";
        gold.text = goldAmount.RoundToInt() + " * 100";
        pickups.text = pickupsAmount.RoundToInt() + " * 100";
        score.text = scoreAmount.RoundToInt() + "";
        hiddenRooms.text = hiddenRoomsAmount.RoundToInt() + " * 100";
        levelMultiplier.text = " * "+(GeneratorManager.Stage + 1);

        finalScore.text = (superScore - timeAmount * 10).RoundToInt().ToString();

        if (finalScore.text.ToInt() > Settings.Game.highscore)
        {
            if(!newHighscore)
            {
                newHighscore = true;
            }

            Settings.Game.highscore = finalScore.text.ToInt();
            highScore.text = finalScore.text;
            if (newHighscore)
            {
                highScore.text = "NEW! " + highScore.text;
            }
        }

        if (statOn == 7)
        {
            statOn = 8;
        }
    }
}
