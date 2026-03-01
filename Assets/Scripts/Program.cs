using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

[Serializable]
public class WordList
{
    public string key;
    public List<string> values;
}

[Serializable]
public class WordDictionaryWrapper
{
    public List<WordList> entries = new List<WordList>();
}

public class Program : MonoBehaviour
{
    string dataPath1;
    string dataPath2;

    void Awake()
    {
        dataPath1 = Path.Combine(UnityEngine.Application.dataPath, "sampleprocessed.json");
        UnityEngine.Debug.Log("JSON saved to: " + dataPath1);
        dataPath2 = Path.Combine(UnityEngine.Application.dataPath, "20kprocessed.json");
    }

    private void InitializePaths()
    {
        // now these are valid
        dataPath1 = Path.Combine(UnityEngine.Application.dataPath, "sampleprocessed.json");
        dataPath2 = Path.Combine(UnityEngine.Application.dataPath, "20kprocessed.json");
    }


    public void Trainer()
    {
        InitializePaths();
        var dict = new Dictionary<string, List<(string Word, int Count)>>();

        string text = File.ReadAllText(Path.Combine(UnityEngine.Application.streamingAssetsPath, "sample.txt"));
        string[] words = text.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        string english2k = File.ReadAllText(Path.Combine(UnityEngine.Application.streamingAssetsPath, "20k.txt"));
        string[] english2kwords = english2k.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < words.Length - 1; i++)
        {
            string currentWord = words[i].ToLower();
            string nextWord = words[i + 1].ToLower();

            if (!dict.ContainsKey(currentWord))
                dict[currentWord] = new List<(string Word, int Count)>();

            int index = dict[currentWord].FindIndex(item => item.Word == nextWord);

            if (index == -1)
                dict[currentWord].Add((nextWord, 1));
            else
                dict[currentWord][index] =
                    (nextWord, dict[currentWord][index].Count + 1);
        }

        List<string> commonwords = new List<string>
        { "the", "be", "to", "of", "and", "a", "in", "that" };

        WordDictionaryWrapper wrapper = new WordDictionaryWrapper();

        foreach (var pair in dict)
        {
            var sorted = pair.Value
                .OrderByDescending(x => x.Count)
                .ToList();

            List<string> finalWords = new List<string>();

            foreach (var item in sorted)
            {
                if (!commonwords.Contains(item.Word) && item.Count > 1)
                    finalWords.Add(item.Word);
            }

            wrapper.entries.Add(new WordList
            {
                key = pair.Key,
                values = finalWords
            });
        }

        string json1 = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(dataPath1, json1);

        string json2 = JsonUtility.ToJson(new StringArrayWrapper { words = english2kwords }, true);
        File.WriteAllText(dataPath2, json2);

        //Debug.Log("Training Complete");
    }

    public string[] Predictor(string previousWord, string currentLetters)
    {
        if (!File.Exists(dataPath1) || !File.Exists(dataPath2))
        {
           //Debug.LogError("JSON files not found. Run Trainer() first.");
            return new string[8];
        }

        string json1 = File.ReadAllText(dataPath1);
        string json2 = File.ReadAllText(dataPath2);

        WordDictionaryWrapper wrapper =
            JsonUtility.FromJson<WordDictionaryWrapper>(json1);

        StringArrayWrapper englishWrapper =
            JsonUtility.FromJson<StringArrayWrapper>(json2);

        string input1 = previousWord.ToLower();
        string input2 = currentLetters.ToLower();

        List<string> predictwords = new List<string>();

        if (string.IsNullOrEmpty(input1) || input1.Contains('.'))
        {
            predictwords.AddRange(englishWrapper.words);
        }
        else
        {
            var entry = wrapper.entries
                .FirstOrDefault(e => e.key == input1);

            if (entry != null)
                predictwords.AddRange(entry.values);

            predictwords.AddRange(englishWrapper.words);
        }

        if (!string.IsNullOrEmpty(input2))
        {
            predictwords = predictwords
                .Where(w => w.StartsWith(input2))
                .ToList();
        }

        string[] top8 = predictwords
            .Distinct()
            .Take(8)
            .ToArray();

        string[] padded = new string[8];

        for (int i = 0; i < 8; i++)
        {
            padded[i] = i < top8.Length ? top8[i] : "null";
        }

        return padded;
    }
}

[Serializable]
public class StringArrayWrapper
{
    public string[] words;
}