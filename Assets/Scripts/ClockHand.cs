
using UnityEngine;

// Hopper Studios [AC, GW, JC, JG, JP]

// Clock Hand script, rotates the hand and knocks out grounded players
// Gets faster the longer everyone survives, currently rather difficult... but can be modified in editor

public class ClockHand : MonoBehaviour
{
    [Header("Players")]
    public ClockPlayer[] players = new ClockPlayer[4];

    [Header("Clock Settings")]
    public float startingSpeed = 90f; // Degrees per second
    public float acceleration = 8f; // Extra speed each second
    public float maximumSpeed = 540f; // Speed limit, thankfully
    public float startingAngle = 20f; // Begin just past 12 o'clock

    private float currentSpeed;
    private float currentAngle;
    private bool spinning;


    void Awake() // Prepare the hand before gameplay begins
    {
        StopRound(); // Reset rotation and speed
    }

    void OnDisable()
    {
        Debug.Log("ClockHand has been disabled!");
    }


    void Update()
    {
        if (!spinning)
            return;


        float previousAngle = currentAngle;

        currentSpeed = Mathf.Min(
            currentSpeed + acceleration * Time.deltaTime,
            maximumSpeed);

        float movement = currentSpeed * Time.deltaTime;
        currentAngle = Mathf.Repeat(currentAngle + movement, 360f);

        // Negative Z rotation makes the hand move clockwise
        transform.rotation = Quaternion.Euler(0, 0, -currentAngle);

        // Check everyone against the angle the hand just covered
        foreach (ClockPlayer player in players)
        {
            if (!spinning)
                break;

            if (!player.IsAlive)
                continue;

            Vector2 direction =
                player.transform.position - transform.position;

            // Clock angles start at 12 and increase clockwise
            float playerAngle = Mathf.Repeat(
                Mathf.Atan2(direction.x, direction.y)
                * Mathf.Rad2Deg, 360f);

            float distanceToPlayer =
                Mathf.Repeat(playerAngle - previousAngle, 360f);

            // Did the hand pass this player during this frame?
            bool passedPlayer =
                movement >= 360f || distanceToPlayer <= movement;

            if (passedPlayer && !player.IsSafe)
                player.Eliminate(); // Bonk!
        }
    }

    public void BeginRound() // Here we go!
    {

        currentSpeed = startingSpeed;
        currentAngle = startingAngle;
        spinning = true;

        transform.rotation = Quaternion.Euler(0, 0, -currentAngle);
    }

    public void StopRound()
    {
        spinning = false;
        currentSpeed = startingSpeed;
        currentAngle = startingAngle;

        transform.rotation = Quaternion.Euler(0, 0, -currentAngle);
    }
}

