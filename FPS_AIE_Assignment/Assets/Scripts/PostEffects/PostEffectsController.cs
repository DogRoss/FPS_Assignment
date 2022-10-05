using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class PostEffectsController : MonoBehaviour
{
    public Shader postShader;
    Material postEffectMaterial;

    public Color screenTint;

    // source - comes from render image
    // destination - buffer that gets its data written to
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(postEffectMaterial == null)
        {
            postEffectMaterial = new Material(postShader);
        }
        //create a temporary render texture to edit
        RenderTexture renderTexture = RenderTexture.GetTemporary(
                   source.width, source.height, 0, source.format);

        postEffectMaterial.SetColor("_ScreenTint", screenTint);

        //draws from source to destination buffer
        Graphics.Blit(source, renderTexture, postEffectMaterial, 0);
        Graphics.Blit(renderTexture, destination);

        //free up memory allocated towards temporary render texture
        RenderTexture.ReleaseTemporary(renderTexture);
    }
}
