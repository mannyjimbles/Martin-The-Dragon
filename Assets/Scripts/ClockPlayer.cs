
using UnityEngine;

// Hopper Studios [AC, GW, JC, JG, JP]

// Clock Player script, handles jumping and elimination
// All four characters use this same script, lovely

public class ClockPlayer : MonoBehaviour
{
    [Header("Player")]
    public int playerIndex; // 0, 1, 2 or 3
    public GameManager gameManager;

    [Header("Appearance")]
    public Transform characterVisual; // Sprite that jumps up and down

    [Header("Jump Settings")]
    public float jumpDuration = 0.7f; // How long a jump lasts
    public float jumpScale = 1.4f; // How big the sprite gets mid-jump

    private bool isAlive;
    private bool isJumping;
    private float jumpTimer;
    private Vector3 originalScale;

    public bool IsAlive => isAlive;
    public bool IsSafe => isAlive && isJumping;

    void Awake()
    {
        originalScale = characterVisual.localScale;
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
        if (!isJumping)
            return;

        jumpTimer += Time.deltaTime;

        // Smoothly grow and shrink during the jump
        float progress = Mathf.Clamp01(jumpTimer / jumpDuration);
        float height = Mathf.Sin(progress * Mathf.PI);

        characterVisual.localScale =
            originalScale * (1f + height * (jumpScale - 1f));

        if (jumpTimer >= jumpDuration)
        {
            isJumping = false; // Back on the ground
            characterVisual.localScale = originalScale;
        }
    }

    void HandleButtonPress(int pressedPlayer)
    {
        if (!gameManager.IsPlaying)
            return;

        if (pressedPlayer != playerIndex)
            return; // That's somebody else's button!

        if (!isAlive || isJumping)
            return; // Can't jump if dead or already jumping

        Jump();
    }

    void Jump() // Boing!
    {
        isJumping = true;
        jumpTimer = 0f;
    }

    public void PreparePlayer(bool participating)
    {
        isAlive = participating;
        isJumping = false;
        jumpTimer = 0f;

        characterVisual.localScale = originalScale;
        characterVisual.gameObject.SetActive(participating);
    }

    public void Eliminate() // Ded
    {
        if (!isAlive)
            return;

        isAlive = false;
        isJumping = false;

        characterVisual.gameObject.SetActive(false);

        gameManager.PlayerEliminated();
    }
}

