using UniRx;
using UnityEngine;

public class FireVisual : MonoBehaviour
{
    [SerializeField] private ReceiveUDP ReceiveUDP;
    [SerializeField] private GameObject fireObject;
    
    
    void Start()
    {
        fireObject.SetActive(false);
        Bind();
    }

    private void Bind()
    {
        ReceiveUDP.onReceiveSimilerPKFire
            .ObserveOnMainThread() 
            .Subscribe(value =>
            {
                Debug.Log(value);
                
                if (value < 0.8f)
                {
                    //pkFireをしている間は画像を表示
                    DisplayImage();
                }
                else
                {
                    HideImage();
                }
            });
        
        
    }

    private void DisplayImage()
    {
        fireObject.SetActive(true);
        
    }
    
    private void HideImage()
    {
        fireObject.SetActive(false);
    }
}
