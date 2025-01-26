using UnityEngine;

public class HandController : MonoBehaviour
{
    private Animator handAnimator;
    public SpriteRenderer handSpriteRenderer;

    public Sprite defaultHandSprite;  // Rock sprite (default hand)
    public Sprite paperHandSprite;
    public Sprite scissorsHandSprite;

    void Start()
    {
        handAnimator = GetComponent<Animator>();
        handSpriteRenderer.sprite = defaultHandSprite;
    }

    public void StartShaking(string choice)
    {
        // Reset hand to default first
        ResetHandToDefault();

        // Trigger the shaking animation
        handAnimator.SetTrigger("Shake");

        // Store the choice so the correct animation plays after shake
        Invoke(nameof(ChangeHandState), 1.0f);  // Delay to allow shake to finish

        PlayerPrefs.SetString("HandChoice", choice);
    }

    private void ChangeHandState()
    {
        string selectedChoice = PlayerPrefs.GetString("HandChoice");

        if (selectedChoice == "Paper")
        {
            handAnimator.SetTrigger("ChoosePaper");
        }
        else if (selectedChoice == "Scissors")
        {
            handAnimator.SetTrigger("ChooseScissors");
        }
        else
        {
            handSpriteRenderer.sprite = defaultHandSprite;  // Default rock hand
        }
    }

    private void ResetHandToDefault()
    {
        handSpriteRenderer.sprite = defaultHandSprite;  // Reset sprite to default rock hand
        handAnimator.Rebind();  // Reset animator to default state
    }

    public void SelectRock()
    {
        StartShaking("Rock");
    }

    public void SelectPaper()
    {
        StartShaking("Paper");
    }

    public void SelectScissors()
    {
        StartShaking("Scissors");
    }
}
