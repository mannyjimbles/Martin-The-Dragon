using UnityEngine;
// Hopper Studios [AC, GW, JC, JG, JP]

// Script for defining attending players, sets up players/confirms teams/confirms attendance/sets score to 0
[System.Serializable]
public class PlayerData // Setting up details pertinent to each player and sets score to 0
{
    public string playerName; // Either Martin or either Dragon
    public string teamName; // Team Martin vs Team Dragon
    public bool hasJoined; // Confirm attendance of players at podium
    public int score; // Keeps track of each player's score in the minigames

    public PlayerData(string name, string team) // Setting up individual player assets
    {
        playerName = name; // Martin/Dragon
        teamName = team; // Team Martin/Team Dragon
        hasJoined = false; // Attending or not
        score = 0; // Sets score to 0 at beginning of each playthrough
    }
}
