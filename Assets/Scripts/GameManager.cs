
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Hopper Studios [AC, GW, JC, JG, JP]

// Game Manager script, handles registration, Clock Jump and the winning player
// Only one minigame now, no need to worry about rounds or scores!

public class GameManager : MonoBehaviour // :)
{
    // Different stages of Clock Jump
    public enum GameState
    {
        Registration, // Players joining the game
        Countdown, // Waiting for the clock to begin
        Playing, // Jump for your lives!
        Results // Somebody won, hopefully
    }

    [Header("Players")]
    public PlayerData[] players = new PlayerData[4]; // 2 Martins, 2 Dragons
    public ClockPlayer[] clockPlayers = new ClockPlayer[4]; // Characters on the clock
    public ClockHand clockHand; // Our spinning obstacle

    [Header("Screens")]
    public GameObject registrationScreen;
    public GameObject gameScreen;
    public GameObject resultsScreen;

    [Header("Text")]
    public TMP_Text[] playerLabels = new TMP_Text[4];
    public TMP_Text statusLabel;
    public TMP_Text gameLabel;
    public TMP_Text resultsLabel;

    [Header("Settings")]
    public float inactivityLimit = 15f; // Reset if nobody interacts
    public float countdownDuration = 5f; // Give people time to join
    public float resultsDuration = 10f; // Show the winner before resetting
    public float maxGameDuration = 45f; // Nobody can play forever, sorry

    private GameState currentState;
    private float stateStartTime;
    private float lastInputTime;
    private int startingPlayers;

    public bool IsPlaying => currentState == GameState.Playing;

    void Awake() // Introducing our contestants!
    {
        players[0] = new PlayerData("Martin #1", "Martin"); // Yep
        players[1] = new PlayerData("Martin #2", "Martin"); // Cool
        players[2] = new PlayerData("Dragon #1", "Dragon"); // Imagine
        players[3] = new PlayerData("Dragon #2", "Dragon"); // Dragons
    }

    void OnEnable()
    {
        PlayerInput.OnPressed += HandleButtonPress;
    }

    void OnDisable()
    {
        PlayerInput.OnPressed -= HandleButtonPress;
    }

    void Start() // :)
    {
        ResetGame();
    }

    void Update()
    {
        // Manual reset for testing, press Esc
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ResetGame();
            return;
        }

        float stateTime = Time.time - stateStartTime;

        if (currentState == GameState.Countdown)
        {
            float remaining = countdownDuration - stateTime;

            statusLabel.text =
                "STARTING IN " + Mathf.CeilToInt(remaining);

            if (remaining <= 0)
                StartGame();
        }

        if (currentState == GameState.Playing)
        {
            float remaining = maxGameDuration - stateTime;

            gameLabel.text =
                "SURVIVE!\n" + Mathf.CeilToInt(remaining);

            // Safety limit so the game can't run forever
            if (remaining <= 0)
                FinishGame();

            return;
        }

        if (currentState == GameState.Results)
        {
            if (stateTime >= resultsDuration)
                ResetGame(); // Next group please!

            return;
        }

        // Don't reset while waiting for the first player
        if (currentState == GameState.Registration)
            return;

        // Reset if everyone walks away during the countdown
        if (Time.time - lastInputTime >= inactivityLimit)
            ResetGame();
    }

    void HandleButtonPress(int playerIndex)
    {
        if (currentState == GameState.Registration ||
            currentState == GameState.Countdown)
        {
            lastInputTime = Time.time;
            JoinPlayer(playerIndex);
        }

        // ClockPlayer handles jumping during the game
    }

    void JoinPlayer(int playerIndex)
    {
        if (!players[playerIndex].hasJoined)
            players[playerIndex].hasJoined = true;

        UpdateRegistrationUI();

        if (currentState == GameState.Registration)
            ChangeState(GameState.Countdown);
    }

    void StartGame() // Let the jumping begin!
    {
        startingPlayers = 0;

        // Activate the characters whose players joined
        for (int i = 0; i < clockPlayers.Length; i++)
        {
            bool joined = players[i].hasJoined;
            clockPlayers[i].PreparePlayer(joined);

            if (joined)
                startingPlayers++;
        }

        ChangeState(GameState.Playing);
        clockHand.BeginRound(); // Spin that clock!
    }

    public void PlayerEliminated() // Called whenever somebody gets hit
    {
        if (!IsPlaying)
            return;

        int survivors = CountSurvivors();

        // End once there is only one player left
        // Solo players keep going until they fall or time runs out
        if (startingPlayers > 1 && survivors <= 1)
            FinishGame();

        if (startingPlayers == 1 && survivors == 0)
            FinishGame();
    }

    int CountSurvivors()
    {
        int survivors = 0;

        foreach (ClockPlayer player in clockPlayers)
        {
            if (player.IsAlive)
                survivors++;
        }

        return survivors;
    }

    void FinishGame() // Determine the winner
    {
        if (!IsPlaying)
            return;

        clockHand.StopRound();

        int survivors = CountSurvivors();
        ClockPlayer winner = null;

        foreach (ClockPlayer player in clockPlayers)
        {
            if (player.IsAlive)
                winner = player;
        }

        if (survivors == 1 && startingPlayers > 1)
        {
            PlayerData winningPlayer = players[winner.playerIndex];

            resultsLabel.text =
                winningPlayer.teamName.ToUpper() +
                " TEAM WINS!\n\n" +
                winningPlayer.playerName;
        }
        else if (survivors == 1 && startingPlayers == 1)
        {
            resultsLabel.text =
                "YOU SURVIVED!\n\nWELL DONE!";
        }
        else if (survivors == 0)
        {
            resultsLabel.text = "EVERYONE WAS ELIMINATED!";
        }
        else
        {
            resultsLabel.text = "TIME'S UP!\nIT'S A DRAW!";
        }

        ChangeState(GameState.Results);
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
            newState == GameState.Playing);

        resultsScreen.SetActive(
            newState == GameState.Results);
    }

    public void ResetGame() // Wipe the slate clean
    {
        clockHand.StopRound();

        foreach (PlayerData player in players)
        {
            player.hasJoined = false;
            player.score = 0; // Leftover from old scoring system
        }

        foreach (ClockPlayer player in clockPlayers)
            player.PreparePlayer(false);

        startingPlayers = 0;

        ChangeState(GameState.Registration);
        UpdateRegistrationUI();
    }
}

