using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LetterManager : MonoBehaviour
{
    public Dictionary<int, Letter> letterMap;
    public Text letterText;

    private Letter[] possibleRealLetters = new List<Letter>() {
            new Letter(true, "I was scared of losing you so I acted aloof and wasn't emotionally available. \n\nI was self-sabotaging and I'm so sorry."),
            new Letter(true, "I could have been more empathetic when you were acting out. \n\nI was just so stuck on how unfair the situation was for me, and wasn't thinking about us."),
            new Letter(true, "Let's take steps to understand where each of us are coming from. \n\n I love you so much, and it would break my heart if this ended like this."),
            new Letter(true, "I felt hurt when you said that I didn't properly clean the dishes. \nNext time will you say it nicer?"),
        }.ToArray();
    private Letter[] possibleFakeLetters = new List<Letter>() {
            new Letter(false, "You ALWAYS look at your phone when I have important stuff to talk about. \n\n Did your parents ever teach you that's rude?? \n\nWhy are you always so distracted?"),
            new Letter(false, "You are full of insecurities, and you don't deserve me. \n\nMy ex handled situations like this way better."),
            new Letter(false, "I understand you completely and you shouldn't worry about the doubts on your mind. \n\nWe're like, the same person! \n\n I love you forever and ever and I know you do too."),
            new Letter(false, "You are the worst. \n\nI hope you are ashamed of yourself for making me feel this way."),
            new Letter(false, "I wish I had never met you. \n\n I hope you and all your loved ones live a sad life for the rest of your life."),
            new Letter(false, "Why are you always like this? You're driving me insane!!!!"),
            new Letter(false, "I never did anything wrong. It's always you. You don't deserve me if you can't handle me at my worst!!"),
        }.ToArray();

    private HashSet<int> used_real = new HashSet<int>();
    private HashSet<int> used_fake = new HashSet<int>();

    // Start is called before the first frame update
    void Start()
    {
    }

    public void ShowLetter(Letter letter, bool toggle)
    {
        if (!toggle)
        {
            // going from hide to show
            GetComponent<SpriteRenderer>().enabled = true;
            letterText.text = "To " + letter.GetTo() + ", \n\n\n" + letter.ReadContent() + "\n\n\n From " + letter.GetFrom();
        }
        else
        {
            // going from show to hide
            GetComponent<SpriteRenderer>().enabled = false;
            letterText.text = "";
        }

    }

    public void SetRealLetter(Cell cell, string from, string to)
    {
        int index = Random.Range(0, possibleRealLetters.Length); 
        while (used_real.Contains(index))
        {
            index = Random.Range(0, possibleRealLetters.Length);
        }
        cell.SetLetterObject(possibleRealLetters[index], from, to);
        used_real.Add(index);
    }

    public bool SetFakeLetter(Cell cell, string from, string to)
    {
        int max_tries = 5;
        int index = Random.Range(0, possibleFakeLetters.Length);
        while (used_fake.Contains(index))
        {
            if (max_tries == 0)
            {
                return false;
            }
            index = Random.Range(0, possibleFakeLetters.Length);
            max_tries -= 1;
        }
        cell.SetLetterObject(possibleFakeLetters[index], from, to);
        used_fake.Add(index);
        return true;
    }

    public void SetLetterWithExistingLetter(Cell cell, Letter letter)
    {
        cell.SetLetterObject(letter);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
