using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blur3DItem : MonoBehaviour {
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private float maxBlurAmount = 0.03f;

    private MaterialPropertyBlock propertyBlock;
    private static readonly int BlurAmountID = Shader.PropertyToID("_BlurAmount");

    private void Awake() {
        if (objectRenderer == null) {
            objectRenderer = GetComponentInChildren<Renderer>();
        }
        propertyBlock = new MaterialPropertyBlock();
    }

    /// <summary>
    /// Sets blur state without generating Material instance garbage.
    /// </summary>
    public void SetBlur(bool isBlurred) {
        if (objectRenderer == null) return;

        objectRenderer.GetPropertyBlock(propertyBlock);

        // Pass blur amount parameter directly to the Shader Graph property
        float blurVal = isBlurred ? maxBlurAmount : 0f;
        propertyBlock.SetFloat(BlurAmountID, blurVal);

        objectRenderer.SetPropertyBlock(propertyBlock);
    }
}
