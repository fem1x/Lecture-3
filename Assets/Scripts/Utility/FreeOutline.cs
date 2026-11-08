using UnityEngine;  
using UnityEngine.EventSystems;  
  
public class FreeOutline : MonoBehaviour
{  
    [SerializeField] private RenderingLayerMask outlineLayer;
  
    private Renderer[] renderers;  
    private uint originalLayer;  
  
    private void Awake()  
    {        
        renderers = TryGetComponent<Renderer>(out var meshRenderer)  
            ? new[] {meshRenderer}  
            : GetComponentsInChildren<Renderer>();  
        originalLayer = renderers[0].renderingLayerMask;  
    }  
    
    public void SetOutline(bool enable)  
    {        
        foreach (var rend in renderers)  
        {            
            rend.renderingLayerMask = enable 
                ? originalLayer | outlineLayer 
                : originalLayer; 
        }    
    }
}