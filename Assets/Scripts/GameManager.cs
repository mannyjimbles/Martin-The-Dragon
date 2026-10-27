
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;

// Hopper Studios [AC, GW, JC, JG, JP]

// Game Manager script, handles Clock Jump, points and the Hooley scoreboard
// Nobody gets eliminated permanently. We're keeping this friendly! :)

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Registration, // Players joining
        Countdown, // Waiting for the demonstration
        Demo, // Watch and learn!
        StartPrompt, // START!
        Playing, // 45 seconds of clock-related danger
        Results, // Which team scored the most?
        FestivalScores // Overall Hooley standings
    }

    [Header("Players")]
    public PlayerData[] players = new PlayerData[4];
    public ClockPlayer[] clockPlayers = new ClockPlayer[4];
    public ClockHand clockHand;

    [Header("Screens")]
    public GameObject registrationScreen;
    public GameObject gameScreen;
    public GameObject resultsScreen;
    public GameObject festivalScoresScreen;

    [Header("Registration Text")]
    public TMP_Text[] playerLabels = new TMP_Text[4];
    public TMP_Text statusLabel;

    [Header("Gameplay Text")]
    public TMP_Text gameLabel;
    public TMP_Text[] playerScoreLabels = new TMP_Text[4];
    public TMP_Text teamScoresLabel;

    [Header("Results Text")]
    public TMP_Text resultsLabel;
    public TMP_Text festivalScoresLabel;

    [Header("Settings")]
    public float inactivityLimit = 15f;
    public float countdownDuration = 5f;
    public float startPromptDuration = 1f;
    public float maxGameDuration = 45f;
    public float resultsDuration = 5f;
    public float festivalScoresDuration = 7f;
    public float menuInputDelay = 0.5f;

    private GameState currentState;
    private float stateStartTime;
    private float lastInputTime;

    private bool martinParticipating;
    private bool dragonParticipating;
    private bool staffViewingScores;

    private int martinFestivalWins;
    private int dragonFestivalWins;

    public bool IsPlaying => currentState == GameState.Playing;

    void Awake() // Meet the contestants
    {
        players[0] = new PlayerData("Martin #1", "Martin"); // Yep
        players[1] = new PlayerData("Martin #2", "Martin"); // Cool
        players[2] = new PlayerData("Dragon #1", "Dragon"); // Ballin'
        players[3] = new PlayerData("Dragon #2", "Dragon"); // Fab
    }

    void OnEnable()
    {
        PlayerInput.OnPressed += HandleButtonPress;
    }

    void OnDisable()
    {
        PlayerInput.OnPressed -= HandleButtonPress;
    }

    void Start()
    {
        LoadFestivalScores();
        ResetGame();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;

        // Staff controls, don't let the visitors near these!
        if (keyboard != null)
        {
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                ResetGame();
                return;
            }

            if (keyboard.hKey.wasPressedThisFrame)
            {
                ToggleFestivalScores();
                return;
            }
        }

        float elapsed = Time.time - stateStartTime;

        switch (currentState)
        {
            case GameState.Countdown:
                statusLabel.text =
                    "STARTING IN " +
                    Mathf.Max(0, Mathf.CeilToInt(
                        countdownDuration - elapsed));

                if (elapsed >= countdownDuration)
                    StartDemo();
                break;

            case GameState.StartPrompt:
                if (elapsed >= startPromptDuration)
                    StartGame();
                break;

            case GameState.Playing:
                float remaining = Mathf.Max(
                    0f, maxGameDuration - elapsed);

                gameLabel.text =
                    "SURVIVE!\n" + Mathf.CeilToInt(remaining);

                // Matches ALWAYS finish after 45 seconds
                if (remaining <= 0f)
                    FinishGame();
                break;

            case GameState.Results:
                if (elapsed >= resultsDuration)
                    ShowFestivalScores();
                break;

            case GameState.FestivalScores:
                if (!staffViewingScores &&
                    elapsed >= festivalScoresDuration)
                    ResetGame();
                break;
        }

        if (currentState == GameState.Countdown &&
            Time.time - lastInputTime >= inactivityLimit)
        {
            ResetGame();
        }
    }

    void HandleButtonPress(int playerIndex)
    {
        // S, D, F or G all work for menu navigation
        switch (currentState)
        {
            case GameState.Registration:
            case GameState.Countdown:
                lastInputTime = Time.time;
                JoinPlayer(playerIndex);
                break;

            case GameState.Results:
                if (Time.time - stateStartTime >= menuInputDelay)
                    ShowFestivalScores();
                break;

            case GameState.FestivalScores:
                // Staff's manually opened scoreboard stays under staff control
                if (!staffViewingScores &&
                    Time.time - stateStartTime >= menuInputDelay)
                    ResetGame();
                break;
        }

        // ClockPlayer listens for gameplay button presses separately
    }

    void JoinPlayer(int playerIndex)
    {
        players[playerIndex].hasJoined = true;

        UpdateRegistrationUI();

        if (currentState == GameState.Registration)
            ChangeState(GameState.Countdown);
    }

    void StartDemo() // One practice lap for everyone
    {
        martinParticipating = false;
        dragonParticipating = false;

        for (int i = 0; i < clockPlayers.Length; i++)
        {
            bool joined = players[i].hasJoined;

            players[i].score = 0; // Everyone begins on zero
            clockPlayers[i].PreparePlayer(joined);

            if (joined && players[i].teamName == "Martin")
                martinParticipating = true;

            if (joined && players[i].teamName == "Dragon")
                dragonParticipating = true;
        }

        ChangeState(GameState.Demo);

        gameLabel.text = "WATCH HOW TO JUMP!";
        UpdateGameScores();

        clockHand.BeginDemo();
    }

    public void DemoFinished() // 360 degrees completed
    {
        if (currentState != GameState.Demo)
            return;

        // Everyone gets a fresh start after the tutorial
        for (int i = 0; i < clockPlayers.Length; i++)
            clockPlayers[i].PreparePlayer(players[i].hasJoined);

        ChangeState(GameState.StartPrompt);
        gameLabel.text = "START!";
        UpdateGameScores();
    }

    void StartGame() // It's happening!
    {
        ChangeState(GameState.Playing);
        gameLabel.text = "SURVIVE!\n45";

        clockHand.BeginRound();
    }

    public void AwardJumpPoint(int playerIndex) // +1 for clearing the hand
    {
        if (!IsPlaying)
            return;

        if (playerIndex < 0 || playerIndex >= players.Length)
            return;

        if (!players[playerIndex].hasJoined ||
            clockPlayers[playerIndex].IsGhost)
            return;

        players[playerIndex].score++; // Lovely, another point!

        UpdateGameScores();
    }



    public void UpdateGameScores() // Display individual player points
    {
        for (int i = 0; i < players.Length; i++)
        {
            // Hide the score if nobody's using that podium
            if (!players[i].hasJoined)
            {
                playerScoreLabels[i].text = "";
                continue;
            }

            // Just a number, nice and tidy :)
            playerScoreLabels[i].text =
                players[i].score.ToString();
        }
    }



    void GetTeamScores(out int martinPoints,
                       out int dragonPoints)
    {
        martinPoints = 0;
        dragonPoints = 0;

        foreach (PlayerData player in players)
        {
            if (!player.hasJoined)
                continue;

            if (player.teamName == "Martin")
                martinPoints += player.score;
            else if (player.teamName == "Dragon")
                dragonPoints += player.score;
        }
    }

    void FinishGame() // The 45-second timer is up!
    {
        if (!IsPlaying)
            return;

        clockHand.StopRound();

        GetTeamScores(out int martinPoints,
                      out int dragonPoints);

        string result;

        // No festival points for games with only one team
        if (!martinParticipating || !dragonParticipating)
        {
            result = "CHALLENGE COMPLETE!\nWELL DONE!";
        }
        else if (martinPoints > dragonPoints)
        {
            result = "TEAM MARTIN WINS!";

            martinFestivalWins++; // +1 for the whole day
            SaveFestivalScores();
        }
        else if (dragonPoints > martinPoints)
        {
            result = "TEAM DRAGON WINS!";

            dragonFestivalWins++; // The dragons strike back!
            SaveFestivalScores();
        }
        else
        {
            result = "IT'S A DRAW!";
        }

        resultsLabel.text =
            result + "\n\n" +
            "MARTIN: " + martinPoints + "\n" +
            "DRAGON: " + dragonPoints + "\n\n" +
            "PRESS TO CONTINUE";

        ChangeState(GameState.Results);
    }

    void ShowFestivalScores() // Display the Hooley totals
    {
        staffViewingScores = false;
        UpdateFestivalScoresUI();
        ChangeState(GameState.FestivalScores);
    }

    void UpdateFestivalScoresUI()
    {
        festivalScoresLabel.text =
            "THE DUNDEE HOOLEY\n\n" +
            "TEAM MARTIN\n" +
            martinFestivalWins + " WINS\n\n" +
            "TEAM DRAGON\n" +
            dragonFestivalWins + " WINS\n\n" +
            (staffViewingScores
                ? "OFFICIAL STANDINGS"
                : "PRESS TO PLAY AGAIN");
    }

    void ToggleFestivalScores() // H for Hooley!
    {
        if (currentState == GameState.Registration)
        {
            staffViewingScores = true;
            ChangeState(GameState.FestivalScores);
            UpdateFestivalScoresUI();
        }
        else if (currentState == GameState.FestivalScores &&
                 staffViewingScores)
        {
            ResetGame();
        }
    }

    void LoadFestivalScores() // Remember today's results!
    {
        string today = DateTime.Now.ToString("yyyyMMdd");

        string storedDate = PlayerPrefs.GetString(
            "HooleyScoreDate", "");

        if (storedDate != today)
        {
            martinFestivalWins = 0;
            dragonFestivalWins = 0;

            PlayerPrefs.SetString("HooleyScoreDate", today);
            SaveFestivalScores();
        }
        else
        {
            martinFestivalWins =
                PlayerPrefs.GetInt("MartinFestivalWins", 0);

            dragonFestivalWins =
                PlayerPrefs.GetInt("DragonFestivalWins", 0);
        }
    }

    void SaveFestivalScores()
    {
        PlayerPrefs.SetInt(
            "MartinFestivalWins", martinFestivalWins);

        PlayerPrefs.SetInt(
            "DragonFestivalWins", dragonFestivalWins);

        PlayerPrefs.Save(); // Keep them safe!
    }

    [ContextMenu("Reset Festival Scores")]
    public void ResetFestivalScores() // Fresh competition
    {
        martinFestivalWins = 0;
        dragonFestivalWins = 0;

        PlayerPrefs.SetString(
            "HooleyScoreDate",
            DateTime.Now.ToString("yyyyMMdd"));

        SaveFestivalScores();
        UpdateFestivalScoresUI();

        Debug.Log("Festival scores reset!");
    }

    void UpdateRegistrationUI()
    {
        int joinedCount = 0;

        for (int i = 0; i < players.Length; i++)
        {
            bool joined = players[i].hasJoined;

            playerLabels[i].text =
                players[i].playerName + "\n" +
                (joined ? "READY!" : "PRESS TO JOIN");

            if (joined)
                joinedCount++;
        }

        statusLabel.text = joinedCount == 0
            ? "PRESS YOUR BUTTON TO JOIN!"
            : joinedCount + " PLAYER(S) READY!";
    }

    void ChangeState(GameState newState)
    {
        currentState = newState;
        stateStartTime = Time.time;
        lastInputTime = Time.time;

        registrationScreen.SetActive(
            newState == GameState.Registration ||
            newState == GameState.Countdown);

        gameScreen.SetActive(
            newState == GameState.Demo ||
            newState == GameState.StartPrompt ||
            newState == GameState.Playing);

        resultsScreen.SetActive(
            newState == GameState.Results);

        festivalScoresScreen.SetActive(
            newState == GameState.FestivalScores);
    }

    public void ResetGame() // Back to square one, but keep daily victories
    {
        clockHand.StopRound();

        foreach (PlayerData player in players)
        {
            player.hasJoined = false;
            player.score = 0;
        }

        foreach (ClockPlayer player in clockPlayers)
            player.PreparePlayer(false);

        martinParticipating = false;
        dragonParticipating = false;
        staffViewingScores = false;

        ChangeState(GameState.Registration);
        UpdateRegistrationUI();
    }
}



