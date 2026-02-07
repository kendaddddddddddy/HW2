using UnityEngine;

public class ScopeScript : MonoBehaviour
{
    [Header("必须拖拽赋值")]
    public Transform mainCamera;   // 你的 VR 头显 (Main Camera)
    public Transform scopeCamera;  // 用于透视的相机 (Scope Camera)
    public Transform realLens;     // <--- 这里拖入放大镜原本的【Lens/Glass】子物体

    void LateUpdate()
    {
        // 1. 安全检查：万一你忘了拖 Lens，就还是用默认中心，防止报错
        Vector3 targetPos = (realLens != null) ? realLens.position : transform.position;

        // 2. 位置同步：让透视相机去到镜片的中心
        // 技巧：为了防止相机卡在镜片模型内部看到黑色，我们可以沿着镜片朝向稍微往前推 1 厘米
        // 如果你的镜片很薄或者你已经做了 Layer 剔除，直接用 targetPos 也可以
        scopeCamera.position = targetPos + (realLens.forward * 0.01f); 

        // 3. 计算视线向量：从【眼睛】指向【镜片中心】
        Vector3 direction = targetPos - mainCamera.position;

        // 4. 旋转同步：让相机看向这个方向
        // 依然使用 LookRotation 配合 mainCamera.up 来修正头部倾斜
        scopeCamera.rotation = Quaternion.LookRotation(direction, mainCamera.up);
    }
}