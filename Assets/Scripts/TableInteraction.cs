using UnityEngine;

public class TableInteraction : MonoBehaviour
{
    public MagicTableGlow glow;
    public TableRuneAnimation runes;
    public ParticleSystem successEffect;

    private Cauldron cauldron;

    private void Start()
    {
        cauldron = FindObjectOfType<Cauldron>();
    }

    private void Update()
    {
        if (cauldron != null && cauldron.IsSuccess())
        {
            glow.activeColor = new Color(0.5f, 0.2f, 1f);
            runes.ActivateRunes();
            if (successEffect) successEffect.Play();
            enabled = false;
        }
    }
}