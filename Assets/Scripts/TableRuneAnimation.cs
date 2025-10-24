using UnityEngine;

public class TableRuneAnimation : MonoBehaviour
{
    public Material runeMaterial;
    public float scrollSpeed = 0.5f;
    public string textureName = "_MainTex";

    private void Update()
    {
        if (runeMaterial)
        {
            Vector2 offset = runeMaterial.GetTextureOffset(textureName);
            offset.y += Time.deltaTime * scrollSpeed;
            runeMaterial.SetTextureOffset(textureName, offset);
        }
    }

    public void ActivateRunes()
    {
        scrollSpeed = 2f;
        Invoke("DeactivateRunes", 3f);
    }

    private void DeactivateRunes()
    {
        scrollSpeed = 0.5f;
    }
}