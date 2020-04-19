using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LetterManager : MonoBehaviour
{
    public Dictionary<int, Letter> letterMap;
    public Text letterText;

    private Letter[] possibleRealLetters = new List<Letter>() {
            new Letter(true, "I was scared of losing you so I acted aloof and wasn't emotionally available. \nI was self-sabotaging and I'm so sorry."),
            new Letter(true, "I could have been more empathetic when you were acting out. \nI was just so stuck on how unfair the situation was for me, and wasn't thinking about us."),
            new Letter(true, "Let's take steps to understand where each of us are coming from."),
            new Letter(true, "I felt hurt when you said that I didn't properly clean the dishes. \nNext time will you say it nicer?"),
        }.ToArray();
    private Letter[] possibleFakeLetters = new List<Letter>() {
            new Letter(false, "You ALWAYS look at your phone when I have important stuff to talk about. \nWhy are you always so distracted?"),
            new Letter(false, "You are full of insecurities, and you don't deserve me. \nMy ex handled situations like this way better."),
            new Letter(false, "I understand you completely and you shouldn't worry about the questions in your head. \nWe're like, the same person!"),
            new Letter(false, "You are the worst. \nI hope you are ashamed of yourself for making me feel this way."),
            new Letter(false, "I wish I had never met you."),
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

    public void SetFakeLetter(Cell cell, string from, string to)
    {
        int index = Random.Range(0, possibleFakeLetters.Length);
        while (used_real.Contains(index))
        {
            index = Random.Range(0, possibleFakeLetters.Length);
        }
        cell.SetLetterObject(possibleFakeLetters[index], from, to);
        used_fake.Add(index);
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
