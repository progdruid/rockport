using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace MapGameplay
{
public class EffectAnimation : MonoBehaviour
{
    [SerializeField] private float fps;
    [SerializeField] private bool loop;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private float _framePeriod;
    
    private void Awake()
    {
        Assert.IsNotNull(spriteRenderer);
        Assert.IsTrue(frames.Length > 0);
        
        _framePeriod = 1f / fps;
        
        PlayAnimation().Start(this);
    }

    private IEnumerator PlayAnimation()
    {
        var frameCount = frames.Length;
        var currentFrame = 0;

        while (true)
        {
            spriteRenderer.sprite = frames[currentFrame];
            currentFrame++;

            if (currentFrame >= frameCount)
            {
                if (loop)
                    currentFrame = 0;
                else
                {
                    Destroy(gameObject);
                    yield break;
                }
            }

            yield return new WaitForSeconds(_framePeriod);
        }
    }
}
}