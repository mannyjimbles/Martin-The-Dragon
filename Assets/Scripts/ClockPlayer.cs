
using UnityEngine;

// Hopper Studios [AC, GW, JC, JG, JP]

// Clock Player script, handles jumping, ghosting and returning to play
// Nobody gets eliminated anymore, everybody gets another chance! :)

public class ClockPlayer : MonoBehaviour
{
    [Header("Player")]
    public int playerIndex; // 0, 1, 2 or 3
    public GameManager gameManager;

    [Header("Appearance")]
    public Transform characterVisual;

    [Header("Jump Settings")]
    public float jumpDuration = 0.7f;
    public float jumpScale = 1.4f;

    [Header("Ghost Settings")]
    public float ghostDuration = 5f; // Five seconds in ghost jail
    public float ghostOpacity = 0.3f; // Make the character transparent

    private bool isAlive;
    private bool isJumping;
    private bool isGhost;

    private float jumpTimer;
    private float ghostTimer;

    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private Color originalColour;

    public bool IsAlive => isAlive;
    public bool IsGhost => isGhost;
    public bool IsSafe => isAlive && isJumping && !isGhost;

    void Awake()
    {
        originalScale = characterVisual.localScale;

        spriteRenderer =
            characterVisual.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColour = spriteRenderer.color;
    }

    void OnEnable()
    {
        PlayerInput.OnPressed += HandleButtonPress;
    }

    void OnDisable()
    {
        PlayerInput.OnPressed -= HandleButtonPress;
    }

    void Update()
    {
        if (isGhost)
        {
            ghostTimer -= Time.deltaTime;

            if (ghostTimer <= 0f)
                ReturnFromGhost();

            return; // Ghosts can't jump, spooky
        }

        if (!isJumping)
            return;

        jumpTimer += Time.deltaTime;

        // Same jumping animation as before
        float progress =
            Mathf.Clamp01(jumpTimer / jumpDuration);

        float height = Mathf.Sin(progress * Mathf.PI);

        characterVisual.localScale =
            originalScale *
            (1f + height * (jumpScale - 1f));

        if (jumpTimer >= jumpDuration)
        {
            isJumping = false;
            characterVisual.localScale = originalScale;
        }
    }

    void HandleButtonPress(int pressedPlayer)
    {
        if (!gameManager.IsPlaying)
            return;

        if (pressedPlayer != playerIndex)
            return; // Wrong podium, pal

        if (!isAlive || isJumping || isGhost)
            return; // No jumping in ghost jail!

        Jump();
    }

    void Jump() // Boing!
    {
        isJumping = true;
        jumpTimer = 0f;
    }

    public void DemoJump() // Automatically jump in the demonstration
    {
        if (!isAlive || isJumping || isGhost)
            return;

        Jump();
    }

    public void BecomeGhost() // Bonk! You're a ghost now
    {
        if (!isAlive || isGhost)
            return;

        isGhost = true;
        isJumping = false;
        ghostTimer = ghostDuration;

        characterVisual.localScale = originalScale;

        // Make our poor contestant semi-transparent
        if (spriteRenderer != null)
        {
            Color ghostColour = originalColour;
            ghostColour.a = ghostOpacity;
            spriteRenderer.color = ghostColour;
        }

        gameManager.UpdateGameScores();
    }

    void ReturnFromGhost() // Welcome back to the living!
    {
        isGhost = false;
        ghostTimer = 0f;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColour;

        gameManager.UpdateGameScores();
    }

    public void PreparePlayer(bool participating)
    {
        isAlive = participating;
        isJumping = false;
        isGhost = false;

        jumpTimer = 0f;
        ghostTimer = 0f;

        characterVisual.localScale = originalScale;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColour;

        characterVisual.gameObject.SetActive(participating);
    }
}


