using UnityEngine;

public class ScopeScript : MonoBehaviour
{
    public Transform mainCamera;   
    public Transform scopeCamera;  
    public Transform realLens;     // 必须确保这个变量已经拖入了物体！

    void LateUpdate()
    {
        // 1. 确定位置 (保持不变)
        Vector3 targetPos = (realLens != null) ? realLens.position : transform.position;
        scopeCamera.position = targetPos + (transform.forward * 0.01f); // 稍微往前推一点防穿模

        // 2. 计算视线方向 (保持不变)
        Vector3 direction = targetPos - mainCamera.position;

        // 3. 核心修改：旋转修正 🛠️
        // 把 mainCamera.up 改成 realLens.up
        // 意思：相机的"头顶"要和镜片的"头顶"保持一致
        if (realLens != null)
        {
            scopeCamera.rotation = Quaternion.LookRotation(direction, realLens.up);
        }
        else
        {
            // 如果没拖 realLens，就用当前脚本物体的 up
            scopeCamera.rotation = Quaternion.LookRotation(direction, transform.up);
        }
    }
}