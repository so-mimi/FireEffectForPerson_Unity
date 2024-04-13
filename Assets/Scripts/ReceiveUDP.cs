using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

public class ReceiveUDP : MonoBehaviour
{
    int LOCAL_PORT = 5005;
    static UdpClient udp;
    Thread thread;
    
    private static Subject<float> ReceiveSimilerPKFire = new Subject<float>();
    public IObservable<float> onReceiveSimilerPKFire => ReceiveSimilerPKFire;
    
    // float[]を扱うSubjectを定義
    private static Subject<float[]> ReceivePositionArray = new Subject<float[]>();
    
    public IObservable<float[]> onReceivePositionArray => ReceivePositionArray;

    [SerializeField] private Transform rightWristTransform;
    [SerializeField] private Transform leftWristTransform;
    [SerializeField] private Transform rightIndexTransform;
    [SerializeField] private Transform leftIndexTransform;
    
    [SerializeField] private Transform fireTransform;
    
    [Header("大きくなると左右の感度がよくなる"),SerializeField] private float offsetWidth = 1.0f;
    [Header("小さくなると上下の感度がよくなる"),SerializeField] private float offsetHeight = 1.0f;
    
    [Header("大きくすると右側に行く"), SerializeField] private float offsetRight = 0.0f;
    
    


    void Start ()
    {
        udp = new UdpClient(LOCAL_PORT);
        //udp.Client.ReceiveTimeout = 1000;
        thread = new Thread(new ThreadStart(ThreadMethod));
        thread.IsBackground = true; // Unityエディタの停止ボタンを押した時にスレッドが終了するようにする
        thread.Start(); 
        
        // SubjectをSubscribeし、メインスレッドでpositionArrayを更新
        ReceivePositionArray
            .ObserveOnMainThread() // UniRxでメインスレッドに切り替え
            .Subscribe(array =>
            {
                // positionArrayを更新
                rightWristTransform.position = new Vector3(((array[0] - 0.5f) * 20.2f + 1f) * -1 * offsetWidth + offsetRight, (array[1] - 0.5f) * -11.4f * offsetHeight, 0);
                leftWristTransform.position = new Vector3(((array[2] - 0.5f) * 20.2f + 1f) * -1 * offsetWidth + offsetRight, (array[3] - 0.5f) * -11.4f * offsetHeight, 0);
                rightIndexTransform.position = new Vector3(((array[4] - 0.5f) * 20.2f + 1f) * -1 * offsetWidth + offsetRight, (array[5] - 0.5f) * -11.4f * offsetHeight, 0);
                leftIndexTransform.position = new Vector3(((array[6] - 0.5f) * 20.2f + 1f) * -1 * offsetWidth + offsetRight, (array[7] - 0.5f) * -11.4f * offsetHeight, 0);
                
                //fireTransformのPositionはrightWristTransformとleftWristTransformの中間
                fireTransform.position = (rightWristTransform.position + leftWristTransform.position) / 2;
                //炎の向きとなるため、左右の人差し指の中点を取得
                Vector3 FireTarget = (rightIndexTransform.position + leftIndexTransform.position) / 2;
                fireTransform.LookAt(FireTarget);
                //炎の向きをFireTargetに向ける

                // 必要に応じて他の処理をここに追加
                Debug.Log("Updated positionArray on MainThread");
            });
    }
    
    void OnApplicationQuit()
    {
        thread.Abort();
        udp.Close();
    }

    private static void ThreadMethod()
    {
        while (true)
        {
            IPEndPoint remoteEP = null;
            byte[] data = udp.Receive(ref remoteEP);
            string text = System.Text.Encoding.UTF8.GetString(data);
            
            //textの中身から"["と""]"を取り除く
            text = text.Replace("[", "").Replace("]", "");
            
            //textの中身の,,,で分割して5つのfloat型を取得
            string[] textArray = text.Split(new string[] {",,,",}, StringSplitOptions.None);
            float[] floatArray = new float[textArray.Length];
            for (int i = 0; i < textArray.Length; i++)
            {
                floatArray[i] = float.Parse(textArray[i]);
            }
            //Debug.Log("floatArray: " + floatArray[0] + ", " + floatArray[1] + ", " + floatArray[2] + ", " + floatArray[3] + ", " + floatArray[4]);
            if (floatArray.Length >= 9) // 配列の長さチェック
            {
                ReceiveSimilerPKFire.OnNext(floatArray[0]);
                float[] positionData = new float[8];
                Array.Copy(floatArray, 1, positionData, 0, 8); // 1番目から8つの要素をコピー
                ReceivePositionArray.OnNext(positionData);
            }
        }
    }
}
