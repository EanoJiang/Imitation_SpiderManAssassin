using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class MoveTrail : MonoBehaviour
{
    [System.Serializable]
    public struct MaterialData
    {
        public MaterialData(TrailRenderer trailRenderer, Vector2 uvScale, float move)
        {
            m_trailRenderer = trailRenderer;
            m_uvTiling = uvScale;
            m_move = move;
        }

        public TrailRenderer m_trailRenderer;
        [HideInInspector] public Vector2 m_uvTiling;
        [HideInInspector] public float m_move;
    }

    public Transform m_moveObject;
    public string m_shaderPropertyName = "_MoveToMaterialUV";
    [HideInInspector] public int m_shaderPropertyID;

    public MaterialData[] m_materialData = new MaterialData[1] { new MaterialData(null, Vector2.one, 0f) };

    [Header("UV滚动效果的时长控制，值越小滚动越快")]
    public float m_effectDuration = 1.0f;

    [Header("模型的段数")]
    public int m_numPositions = 10;

    [Header("结束拖尾时是否立即清空")]
    public bool m_clearImmediately = false;


    private Vector3 m_beforePosW = Vector3.zero;

    void Start() => Initialize();

    void LateUpdate()
    {
        #region 前置校验
        if (m_moveObject == null || m_materialData == null || m_materialData.Length == 0)
            return;

        Vector3 nowPosW = m_moveObject.position;
        if (nowPosW == m_beforePosW)
            return;
        #endregion

        #region 1. 世界位移
        Vector3 deltaWorld = nowPosW - m_beforePosW;
        #endregion

        #region 2. 投影到 Trail 切线方向
        Vector3 tangent = Vector3.zero;
        var trail = m_materialData[0].m_trailRenderer;   // 取第一条 Trail
        if (trail != null && trail.positionCount >= 2)
        {
            int last = trail.positionCount - 1;
            tangent = (trail.GetPosition(last) - trail.GetPosition(last - 1)).normalized;
        }
        else
        {
            tangent = deltaWorld.normalized;
        }
        float distance = Vector3.Dot(deltaWorld, tangent);
        #endregion

        #region 3. 处理缩放
        Vector3 scale = m_moveObject.lossyScale;
        float uniformScale = (scale.x + scale.y + scale.z) / 3f;
        distance /= Mathf.Max(uniformScale, 0.0001f);
        #endregion

        #region 4. 计算 UV 增量因子
        float timeFactor = m_effectDuration > 0 ? Time.deltaTime / m_effectDuration : 0f;
        float segmentFactor = m_numPositions > 0 ? 1f / m_numPositions : 0f;
        #endregion

        #region 5. 更新所有材质
        for (int i = 0; i < m_materialData.Length; i++)
        {
            var data = m_materialData[i];
            if (data.m_trailRenderer == null)
                continue;

            float uvDist = WorldToUV(distance, data.m_trailRenderer);
            data.m_move += uvDist * data.m_uvTiling.x * timeFactor * segmentFactor;
            if (data.m_move > 1f)
                data.m_move %= 1f;

            Material mat = data.m_trailRenderer.sharedMaterial;
            if (mat != null)
                mat.SetFloat(m_shaderPropertyID, data.m_move);

            m_materialData[i] = data;
        }
        #endregion

        m_beforePosW = nowPosW;
    }


    public void Initialize()
    {
        if (m_materialData == null || m_materialData.Length == 0)
            return;

        m_shaderPropertyID = Shader.PropertyToID(m_shaderPropertyName);

        for (int i = 0; i < m_materialData.Length; i++)
        {
            var data = m_materialData[i];
            if (data.m_trailRenderer != null)
            {
                Material mat = data.m_trailRenderer.sharedMaterial;
                if (mat != null)
                    data.m_uvTiling = mat.mainTextureScale;
            }
            data.m_move = 0f;
            m_materialData[i] = data;
        }
    }

    /// <summary>
    /// 结束拖尾
    /// </summary>
    public void FinishTrail()
    {
        StartCoroutine(ClearTrailRoutine());
    }


    private float WorldToUV(float worldDist, TrailRenderer trail)
    {
        if (trail == null)
            return worldDist;

        Bounds b = trail.bounds;
        float refLength = b.size.magnitude;
        refLength = Mathf.Max(refLength, 0.001f);
        return worldDist / refLength;
    }

    private IEnumerator ClearTrailRoutine()
    {
        #region 1. 停止发射
        foreach (var data in m_materialData)
        {
            if (data.m_trailRenderer == null)
                continue;
            data.m_trailRenderer.emitting = false;
        }
        #endregion

        #region 2. 等待剩余段走完
        if (!m_clearImmediately)
        {
            float maxTime = 0f;
            foreach (var data in m_materialData)
            {
                if (data.m_trailRenderer == null)
                    continue;
                maxTime = Mathf.Max(maxTime, data.m_trailRenderer.time);
            }
            yield return new WaitForSeconds(maxTime);
        }
        #endregion

        #region 3. 清空段然后恢复发射
        foreach (var data in m_materialData)
        {
            if (data.m_trailRenderer == null)
                continue;

            if (m_clearImmediately)
                data.m_trailRenderer.time = 0f;

            data.m_trailRenderer.Clear();
            data.m_trailRenderer.emitting = true;
        }
        #endregion
    }

}