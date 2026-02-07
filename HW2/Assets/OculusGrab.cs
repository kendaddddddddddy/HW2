using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class OculusGrab : MonoBehaviour
{
    [Header("XR 输入设置（推荐）")]
    // 选择这只手：RightHand / LeftHand
    public XRNode handNode = XRNode.RightHand;

    // Grip 是 0~1 的模拟值（握把按压）
    public float grabThreshold = 0.5f;

    [Header("备用：旧 Input Manager（可选）")]
    // 如果你项目不是 XR 输入，这里才会用到；否则忽略
    public bool useLegacyInputManager = false;
    public string gripInputName = "XRI_Right_Grip";

    [Header("加分项设置 (Extra Credit)")]
    public bool useDoubleRotation = false;

    // --- 内部变量 ---
    private GameObject collidingObject;
    private Transform grabbedObject;

    private Vector3 lastHandPos;
    private Quaternion lastHandRot;

    // 记录每个物体被几只手抓着：防止一只手松开就把重力恢复了
    private static readonly Dictionary<Transform, int> HoldCounts = new Dictionary<Transform, int>();

    private InputDevice device;
    private bool deviceValid = false;

    void OnEnable()
    {
        TryInitDevice();
    }

    void Update()
    {
        float gripValue = GetGripValue01();

        // 抓 / 松逻辑
        if (gripValue > grabThreshold && grabbedObject == null && collidingObject != null)
        {
            Grab();
        }
        else if (gripValue < grabThreshold && grabbedObject != null)
        {
            Release();
        }

        // 抓着时应用增量（支持两只手同时叠加）
        if (grabbedObject != null)
        {
            ApplyTransformMath();
        }
    }

    private void TryInitDevice()
    {
        device = InputDevices.GetDeviceAtXRNode(handNode);
        deviceValid = device.isValid;
    }

    private float GetGripValue01()
    {
        if (useLegacyInputManager)
        {
            // 旧输入系统：需要你在 Input Manager 配好 axis 名字，否则会报错
            // 建议别用这个分支
            try
            {
                return Input.GetAxis(gripInputName);
            }
            catch
            {
                return 0f;
            }
        }

        // XR 输入：Grip 0~1
        if (!deviceValid || !device.isValid)
        {
            TryInitDevice();
        }

        if (deviceValid && device.TryGetFeatureValue(CommonUsages.grip, out float grip))
        {
            return Mathf.Clamp01(grip);
        }

        // 有些设备只有 gripButton（按下/松开），那就映射成 0 或 1
        if (deviceValid && device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripBtn))
        {
            return gripBtn ? 1f : 0f;
        }

        return 0f;
    }

    void Grab()
    {
        grabbedObject = collidingObject.transform;

        // 记录这一帧手的位置和旋转，作为起点
        lastHandPos = transform.position;
        lastHandRot = transform.rotation;

        // 计数 + 关重力（只有第一次被抓时关）
        if (!HoldCounts.ContainsKey(grabbedObject)) HoldCounts[grabbedObject] = 0;
        HoldCounts[grabbedObject]++;

        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
        if (rb != null && HoldCounts[grabbedObject] == 1)
        {
            rb.useGravity = false;
            // 不要 isKinematic=true（你原注释是对的）
        }
    }

    void Release()
    {
        if (grabbedObject == null) return;

        // 计数 - 恢复重力（只有最后一只手松开才恢复）
        if (HoldCounts.ContainsKey(grabbedObject))
        {
            HoldCounts[grabbedObject]--;
            if (HoldCounts[grabbedObject] <= 0)
            {
                HoldCounts.Remove(grabbedObject);

                Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
                if (rb != null) 
{
    // 1. 确保重力是关闭的
    rb.useGravity = false; 
    
    // 2. 关键一步：把速度清零！
    // 如果不写这两句，物体虽然不掉下去，但会保留你松手时的速度，一直飘走
    rb.linearVelocity = Vector3.zero;        // 停止移动
    rb.angularVelocity = Vector3.zero; // 停止旋转
}
            }
        }

        grabbedObject = null;
    }

    // --- 作业 Part 2 的核心数学逻辑 ---
    void ApplyTransformMath()
    {
        // A. 手的位移增量
        Vector3 deltaPos = transform.position - lastHandPos;

        // B. 手的旋转增量（四元数）
        Quaternion deltaRot = transform.rotation * Quaternion.Inverse(lastHandRot);

        // 加分：双倍旋转
        if (useDoubleRotation)
        {
            deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
            angle *= 2.0f;
            deltaRot = Quaternion.AngleAxis(angle, axis);
        }

        // C. 先算“绕控制器旋转导致的位置变化”
        Vector3 direction = grabbedObject.position - transform.position;
        Vector3 rotatedDirection = deltaRot * direction;
        Vector3 pivotOffset = rotatedDirection - direction;

        // D. 应用：位移（基础位移 + 绕轴偏移），然后旋转
        // 这样两只手同时运行时：位移会相加、旋转会按顺序叠乘（符合要求）
        grabbedObject.position += deltaPos + pivotOffset;
        grabbedObject.rotation = deltaRot * grabbedObject.rotation;

        // 更新记录
        lastHandPos = transform.position;
        lastHandRot = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            collidingObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == collidingObject)
        {
            collidingObject = null;
        }
    }
}
