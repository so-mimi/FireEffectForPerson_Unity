using System;
using UnityEngine;

namespace FireAI.Effects
{
    /// <summary>
    /// 炎のエフェクトがカメラに対して常にいい感じの角度を保つようにするスクリプト
    /// もしかしていらない
    /// </summary>
    [Obsolete("炎のエフェクトが回転処理いらない場合消す")]
    public class LookAtCamera : MonoBehaviour
    {
        private void Update()
        {
            transform.LookAt(Camera.main.transform);
            transform.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
        }
    }
}
