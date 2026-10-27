
using UnityEngine;

// Hopper Studios [AC, GW, JC, JG, JP]

// Clock Hand script, demonstration and gameplay
// Successfully clear the hand for points, or spend 5 seconds as a ghost!

public class ClockHand : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public ClockPlayer[] players = new ClockPlayer[4];

    [Header("Clock Settings")]
    public float startingSpeed = 90f;
    public float acceleration = 8f;
    public float maximumSpeed = 540f;
    public float startingAngle = 20f;

    [Header("Demonstration")]
    public float demoSpeed = 100f;
    public float demoStartingAngle = 315f;
    public float jumpLeadAngle = 30f;

    private float currentSpeed;
    private float currentAngle;
    private float demoDistance;

    private bool spinning;
    private bool demonstrating;
    private bool[] demonstratedJump = new bool[4];

    void Awake() // Don't use Start here, remember our GitHub incident!
    {
        StopRound();
    }

    void Update()
    {
        if (demonstrating)
        {
            UpdateDemo();
            return;
        }

        if (spinning)
            UpdateClock();
    }

    void UpdateDemo() // Teaching the festival visitors
    {
        float previousAngle = currentAngle;
        float movement = demoSpeed * Time.deltaTime;

        demoDistance += movement;

        currentAngle =
            Mathf.Repeat(currentAngle + movement, 360f);

        RotateHand();

        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].IsAlive || demonstratedJump[i])
                continue;

            float playerAngle = GetPlayerAngle(players[i]);

            float jumpAngle = Mathf.Repeat(
                playerAngle - jumpLeadAngle, 360f);

            float distance = Mathf.Repeat(
                jumpAngle - previousAngle, 360f);

            if (movement >= 360f || distance <= movement)
            {
                players[i].DemoJump(); // Look, that's how you do it!
                demonstratedJump[i] = true;
            }
        }

        if (demoDistance >= 360f)
        {
            demonstrating = false;
            gameManager.DemoFinished();
        }
    }

    void UpdateClock() // Real gameplay, points are on the line
    {
        float previousAngle = currentAngle;

        currentSpeed = Mathf.Min(
            currentSpeed + acceleration * Time.deltaTime,
            maximumSpeed);

        float movement = currentSpeed * Time.deltaTime;

        currentAngle =
            Mathf.Repeat(currentAngle + movement, 360f);

        RotateHand();

        for (int i = 0; i < players.Length; i++)
        {
            ClockPlayer player = players[i];

            if (!player.IsAlive || player.IsGhost)
                continue; // Leave the ghosts alone!

            float playerAngle = GetPlayerAngle(player);

            float distance = Mathf.Repeat(
                playerAngle - previousAngle, 360f);

            // Did the clock hand sweep past this player?
            bool passedPlayer =
                movement >= 360f || distance <= movement;

            if (!passedPlayer)
                continue;

            if (player.IsSafe)
            {
                // Successfully cleared the obstacle, +1 point!
                gameManager.AwardJumpPoint(player.playerIndex);
            }
            else
            {
                player.BecomeGhost(); // Ouch, 5-second timeout
            }
        }
    }

    float GetPlayerAngle(ClockPlayer player)
    {
        Vector2 direction =
            player.transform.position - transform.position;

        // 12 = 0, 3 = 90, 6 = 180, 9 = 270
        return Mathf.Repeat(
            Mathf.Atan2(direction.x, direction.y) *
            Mathf.Rad2Deg, 360f);
    }

    void RotateHand()
    {
        transform.rotation =
            Quaternion.Euler(0, 0, -currentAngle);
    }

    public void BeginDemo() // Practice time
    {
        spinning = false;
        demonstrating = true;

        demoDistance = 0f;
        currentAngle = demoStartingAngle;

        for (int i = 0; i < demonstratedJump.Length; i++)
            demonstratedJump[i] = false;

        RotateHand();
    }

    public void BeginRound() // START!
    {
        demonstrating = false;
        spinning = true;

        currentSpeed = startingSpeed;
        currentAngle = startingAngle;

        RotateHand();
    }

    public void StopRound()
    {
        spinning = false;
        demonstrating = false;

        currentSpeed = startingSpeed;
        currentAngle = startingAngle;
        demoDistance = 0f;

        RotateHand();
    }
}



