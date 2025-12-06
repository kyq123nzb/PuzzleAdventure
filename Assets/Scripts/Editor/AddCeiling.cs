using UnityEngine;
using UnityEditor;

/// <summary>
/// 编辑器脚本：为场景添加天花板，同时保持光照
/// </summary>
public class AddCeiling
{
    [MenuItem("Tools/Add Ceiling to Scene")]
    static void AddCeilingToScene()
    {
        // 查找场景中是否有现有的天花板
        GameObject existingCeiling = GameObject.Find("Ceiling");
        if (existingCeiling != null)
        {
            if (EditorUtility.DisplayDialog("天花板已存在", "场景中已存在名为'Ceiling'的对象，是否要删除并重新创建？", "是", "否"))
            {
                Object.DestroyImmediate(existingCeiling);
            }
            else
            {
                return;
            }
        }

        // 查找墙来确定房间尺寸
        GameObject wallNorth = GameObject.Find("Wall_North");
        GameObject wallSouth = GameObject.Find("Wall_South");
        GameObject wallEast = GameObject.Find("Wall_East");
        GameObject wallWest = GameObject.Find("Wall_West");

        if (wallNorth == null || wallSouth == null || wallEast == null || wallWest == null)
        {
            EditorUtility.DisplayDialog("错误", "找不到所有墙壁对象（需要Wall_North, Wall_South, Wall_East, Wall_West）", "确定");
            return;
        }

        // 获取墙壁的位置和尺寸
        Vector3 wallNorthPos = wallNorth.transform.position;
        Vector3 wallNorthScale = wallNorth.transform.lossyScale;
        Vector3 wallEastPos = wallEast.transform.position;
        Vector3 wallEastScale = wallEast.transform.lossyScale;

        // 计算房间尺寸（根据墙的位置和缩放）
        // 假设墙高为50，天花板应该在墙顶
        float wallHeight = wallNorthScale.y;
        float ceilingY = wallNorthPos.y + wallHeight / 2f;

        // 计算天花板的大小（根据墙的长度）
        float roomWidth = Mathf.Max(wallNorthScale.x, 150f); // 默认150如果计算失败
        float roomLength = Mathf.Max(wallEastScale.z, 100f); // 默认100如果计算失败

        // 计算房间中心位置
        float centerX = (wallEastPos.x + wallWest.transform.position.x) / 2f;
        float centerZ = (wallNorthPos.z + wallSouth.transform.position.z) / 2f;

        // 创建天花板GameObject
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.name = "Ceiling";
        
        // 设置位置（Plane默认是10x10单位，中心在原点）
        ceiling.transform.position = new Vector3(centerX, ceilingY, centerZ);
        ceiling.transform.rotation = Quaternion.Euler(0, 0, 180); // 翻转，让下面可见
        
        // 设置大小以匹配房间
        ceiling.transform.localScale = new Vector3(roomWidth / 10f, 1f, roomLength / 10f);

        // 移除默认的Collider（如果需要，可以添加一个BoxCollider）
        if (ceiling.GetComponent<MeshCollider>() != null)
        {
            Object.DestroyImmediate(ceiling.GetComponent<MeshCollider>());
        }

        // 添加BoxCollider（可选，如果需要碰撞检测）
        BoxCollider boxCollider = ceiling.AddComponent<BoxCollider>();
        boxCollider.center = new Vector3(0, -0.05f, 0); // 稍微往下偏移，避免浮点误差
        boxCollider.size = new Vector3(1, 0.1f, 1);

        // 设置材质
        Renderer renderer = ceiling.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 尝试使用墙壁的材质
            Material wallMaterial = null;
            if (wallNorth.GetComponent<Renderer>() != null)
            {
                wallMaterial = wallNorth.GetComponent<Renderer>().sharedMaterial;
            }

            if (wallMaterial != null)
            {
                renderer.sharedMaterial = wallMaterial;
            }
            else
            {
                // 创建默认材质
                Material defaultMaterial = new Material(Shader.Find("Standard"));
                defaultMaterial.color = new Color(0.8f, 0.8f, 0.8f, 1f); // 浅灰色
                defaultMaterial.SetFloat("_Metallic", 0.1f);
                defaultMaterial.SetFloat("_Glossiness", 0.3f);
                renderer.sharedMaterial = defaultMaterial;
            }

            // 确保接收光照
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // 不投射阴影，避免遮挡
            renderer.receiveShadows = true; // 接收阴影

            // 启用Light Probes以保持间接光照
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
        }

        // 设置为静态对象（用于光照烘焙）
        StaticEditorFlags flags = StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic;
        GameObjectUtility.SetStaticEditorFlags(ceiling, flags);

        Debug.Log($"天花板已创建：位置({centerX}, {ceilingY}, {centerZ}), 尺寸({roomWidth}, {roomLength})");
        EditorUtility.DisplayDialog("成功", "天花板已添加到场景中！\n\n提示：\n- 天花板会接收光照和反射\n- 如果需要调整位置或大小，请选中Ceiling对象进行修改\n- 可以调整材质属性来改变外观", "确定");

        // 选中新创建的天花板
        Selection.activeGameObject = ceiling;
    }
}

