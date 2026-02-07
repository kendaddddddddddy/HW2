using UnityEngine;
using UnityEngine.XR;

public class TwoViewToggleStartAsRoom : MonoBehaviour
{
    public Transform outsideView;   // 室外视角点（空物体）
    public Transform xrCamera;      // XR Rig 里的 Main Camera
    public XRNode controllerNode = XRNode.RightHand;

    private Transform roomAnchor;   // 运行时自动创建：记录“室内视角”
    private bool isOutside = false;
    private bool prevPressed = false;

    void Start()
    {
        // 记录你一开始在房间里的“头显位置/朝向”，作为室内视角
        roomAnchor = new GameObject("RoomAnchor_Runtime").transform;
        roomAnchor.position = xrCamera.position;
        roomAnchor.rotation = Quaternion.Euler(0f, xrCamera.eulerAngles.y, 0f);
    }

    void Update()
    {
        var device = InputDevices.GetDeviceAtXRNode(controllerNode);
        if (!device.isValid) return;

        if (device.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed))
        {
            if (pressed && !prevPressed)
            {
                isOutside = !isOutside;
                TeleportHeadTo(isOutside ? outsideView : roomAnchor);
            }
            prevPressed = pressed;
        }
    }

    // 把“头显相机”对齐到目标点（目标点建议放在你想要的视线位置/高度）
    void TeleportHeadTo(Transform target)
    {
        if (target == null || xrCamera == null) return;

        float yawDelta = target.eulerAngles.y - xrCamera.eulerAngles.y;
        transform.Rotate(0f, yawDelta, 0f, Space.World);

        Vector3 headOffset = xrCamera.position - transform.position;
        transform.position = target.position - headOffset;
    }
}
