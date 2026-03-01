using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;

public class StickComboDisplay : MonoBehaviour
{
    public TextMeshProUGUI letterText;
    public TextMeshProUGUI predictiveBox;
    public AIWheelDisplay aiWheel;
    public float deadzone = 0.3f;
    private Dictionary<int, string> lookup;

    private string output = "";

    private bool capsOn = false;
    private bool typeReady = false;

    int[] numbers = new int[8] {3, 2, 1, 8, 7, 6, 5, 4 };
    private string letter = "_";

    private Program markov = new Program();
    

    void Awake()
    {
       // markov.Trainer();
        lookup = new Dictionary<int, string>
        {
            { 81, " and " },
            { 82, " for " },
            { 83, " in " },
            { 84, " that " },
            { 85, " the " },
            { 86, " be " },
            { 87, " to " },
            { 88, " of" },

            { 71, "d" },
            { 72, "e" },
            { 73, "f" },
            { 74, "g" },
            { 75, " " },
            { 76, "a" },
            { 77, "b" },
            { 78, "c" },

            { 61, "l" },
            { 62, "m" },
            { 63, "n" },
            { 64, "o" },
            { 65, "h" },
            { 66, "i" },
            { 67, "j" },
            { 68, "k" },

            { 51, "t" },
            { 52, "u" },
            { 53, "v" },
            { 54, "w" },
            { 55, "p" },
            { 56, "q" },
            { 57, "r" },
            { 58, "s" },

            { 41, "'ve" },
            { 42, "n't" },
            { 43, "'re" },
            { 44, "'s " },
            { 45, "x " },
            { 46, "y " },
            { 47, "z " },
            { 48, "'ll " },

            { 31, "Backspace" },
            { 32, "Caps" },
            { 33, "" },
            { 34, "" },
            { 35, "Ctrl+Backspace" },
            { 36, "" },
            { 37, "" },
            { 38, "" },

            { 21, "?" },
            { 22, "!" },
            { 23, "'" },
            { 24, "@" },
            { 25, "." },
            { 26, "," },
            { 27, ";" },
            { 28, ":" },

            { 11, "test5" },
            { 12, "test6" },
            { 13, "test7" },
            { 14, "test8" },
            { 15, "test1" },
            { 16, "test2" },
            { 17, "test3" },
            { 18, "test4" },

        };
    }


    void Update()
    {
        if (Gamepad.current == null)
        {
            letterText.text = "_";
            return;
        }
        getPredictions();

        int leftDir = GetDirection(Gamepad.current.leftStick.ReadValue());
        int rightDir = GetDirection(Gamepad.current.rightStick.ReadValue());
        if (rightDir != -1)
        {
            rightDir = numbers[rightDir - 1];
        }


        string[] words = updateWheel(leftDir);

        //Stick release checks
        if (letter != "_") 
        {
            typeReady = true;
        }
        if (typeReady == true && (leftDir == -1 || rightDir == -1))
        {
            if (letter == "Backspace")
                {
                    Backspace();
                }
            else if (letter == "Ctrl+Backspace")
                {
                    CtrlBackspace();
                }
            else if (letter == "Caps")
                {
                    Caps();
                }
            else
                {
                    output += letter;
                }
        }

            typeReady = false;

        //Go fetch new letter 
        letter = GetLetterFromCombo(leftDir, rightDir);

        //set the output fields
        
        letterText.text = output+letter;
        aiWheel.SetWords(words);
        
    }

    string[] updateWheel(int left)
    {
        string[] results = lookup
            .Where(kv => kv.Key.ToString().StartsWith(left.ToString()))
            .Select(kv => kv.Value)
            .ToArray();

        UnityEngine.Debug.Log("results: " + results[1] + results[2] + results[7]);
        return results;
    }

    //FIND COMBINATION OF STICKS
    int GetDirection(Vector2 stick)
    {
        if (stick.magnitude < deadzone)
            return -1;

        float angle = Mathf.Atan2(stick.y, stick.x) * Mathf.Rad2Deg;
        if (angle < 0)
            angle += 360f;



        // angle = (angle + 180f) % 360f;  Miles what is this?? anything on a boundary is gunna be separated

        int direction = Mathf.FloorToInt((angle + 22.5f) / 45f) % 8;
        
        direction = (direction + 4) % 8; // So instead I rotate indices, not angles
        
        return direction + 1;
    }

    //FIND WHAT TO RETURN
    string GetLetterFromCombo(int left, int right)
    {
        if (left == -1 || right == -1)
        {
            return "";
        }
        

        int index = (left * 10) + right;
        string found;

        //CTRL, CAPS, Backspace, etc
        if (left == 3)
        {
            if (left == 3 && lookup.TryGetValue(index, out string special))
            {
                return special; // return "Backspace", "Caps", etc. as preview
            }
        }

        //Letter section (default)
        if (lookup.TryGetValue(index, out found))
        {
            if (Regex.IsMatch(lookup[index], "[a-z]") && capsOn){
                return lookup[index].ToUpper();
            }else{
                return lookup[index];
            }
        }
        return "";
    }


    public void Backspace()
    {
        if (!string.IsNullOrEmpty(output))
        {
            output = output.Remove(output.Length - 1, 1);
        }
    }

    void CtrlBackspace()
    {
        if (string.IsNullOrEmpty(output))
            return;

        int end = output.Length - 1;
        while (end >= 0 && output[end] == ' ')
            end--;

        if (end < 0)
        {
            output = "";
            return;
        }

        int start = end;
        while (start >= 0 && output[start] != ' ')
            start--;

        output = output.Remove(start + 1, end - start);
    }

    private void getPredictions()
    {
        string lastWord = getLatestWord();
        string previousWord = getPreviousWord();
        UnityEngine.Debug.Log(lastWord + " " + previousWord);

        string[] nextWords = markov.Predictor(previousWord ?? "", lastWord ?? "");

        // Build predicted output
        string predictedLineOutput = "";
        int[] indices = { 7, 8, 1, 2, 3, 4, 5, 6 };
        for (int i = 0; i < 8; i++)
        {
            int j = indices[i];
            UnityEngine.Debug.Log("index: " + i);
            lookup[10 + j] = nextWords[i];
            predictedLineOutput += nextWords[i] + " ";
            UnityEngine.Debug.Log("lookup index " + (11+i) + lookup[11+i]);
        }
        predictiveBox.text = predictedLineOutput.Trim();
        
    }

    void Caps()
    {
        if (capsOn) { capsOn = false; }
        else { capsOn = true; }
    }

    private string getLatestWord()
    {
        if (string.IsNullOrWhiteSpace(output))
            return null;

        int lastSpace = output.LastIndexOf(' ');
        return lastSpace >= 0 ? output.Substring(lastSpace + 1) : output;
    }

    private string getPreviousWord()
    {
        string previousWord = null;
        if (!string.IsNullOrWhiteSpace(output))
        {
           
            int lastSpace = output.LastIndexOf(' ');
            if (lastSpace > 0)
            {
                int prevSpace = output.LastIndexOf(' ', lastSpace - 1);
                previousWord = prevSpace >= 0 ? output.Substring(prevSpace + 1, lastSpace - prevSpace - 1)
                                              : output.Substring(0, lastSpace);
            }
        }
        return previousWord;
    }
}