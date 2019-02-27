using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
     public float lifetime = 4.0f;
 
     void Start ()
     {
         Destroy (gameObject, lifetime);
     }
}
