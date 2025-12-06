using UnityEngine;
using UnityEditor;

/// <summary>
/// 编辑器脚本：为场景添加地牢风格的横梁房顶，保持光照
/// </summary>
public class AddDungeonCeiling
{
    [MenuItem("Tools/Add Dungeon Ceiling (Beams)")]
    static void AddDungeonCeilingToScene()
    {
        // 查找场景中是否有现有的横梁房顶
        GameObject existingCeiling = GameObject.Find("DungeonCeiling");
        if (existingCeiling != null)
        {
            if (EditorUtility.DisplayDialog("横梁房顶已存在", "场景中已存在名为'DungeonCeiling'的对象，是否要删除并重新创建？", "是", "否"))
            {
                Object.DestroyImmediate(existingCeiling);
            }
            else
            {
                return;
            }
        }

        // 使用外层墙（Wall_North, Wall_South, Wall_East, Wall_West）- 这是高的外墙
        GameObject wallNorth = GameObject.Find("Wall_North");
        GameObject wallSouth = GameObject.Find("Wall_South");
        GameObject wallEast = GameObject.Find("Wall_East");
        GameObject wallWest = GameObject.Find("Wall_West");

        if (wallNorth == null || wallSouth == null || wallEast == null || wallWest == null)
        {
            EditorUtility.DisplayDialog("错误", "找不到所有外层墙壁对象！\n\n需要：Wall_North, Wall_South, Wall_East, Wall_West", "确定");
            return;
        }

        Debug.Log("✅ 使用外层墙（Wall_North等）来计算天花板位置");

        // 获取墙壁的位置和尺寸
        Vector3 wallNorthPos = wallNorth.transform.position;
        Vector3 wallNorthScale = wallNorth.transform.lossyScale;
        Vector3 wallEastPos = wallEast.transform.position;
        Vector3 wallEastScale = wallEast.transform.lossyScale;
        Vector3 wallWestPos = wallWest.transform.position;
        Vector3 wallWestScale = wallWest.transform.lossyScale;
        Vector3 wallSouthPos = wallSouth.transform.position;
        Vector3 wallSouthScale = wallSouth.transform.lossyScale;

        // 计算房间尺寸和天花板高度
        float wallHeight = wallNorthScale.y;
        float ceilingY = wallNorthPos.y + wallHeight / 2f;
        
        // 精确计算房间边界（使用墙壁的内边缘，确保横梁紧贴墙壁）
        // 西墙内边缘 = 西墙位置 + 西墙厚度/2
        // 东墙内边缘 = 东墙位置 - 东墙厚度/2
        float minX = wallWestPos.x + wallWestScale.x / 2f;
        float maxX = wallEastPos.x - wallEastScale.x / 2f;
        float minZ = wallSouthPos.z + wallSouthScale.z / 2f;
        float maxZ = wallNorthPos.z - wallNorthScale.z / 2f;
        
        float centerX = (wallEastPos.x + wallWestPos.x) / 2f;
        float centerZ = (wallNorthPos.z + wallSouthPos.z) / 2f;
        
        // 获取材质
        Material beamMaterial = null;
        if (wallNorth.GetComponent<Renderer>() != null)
        {
            beamMaterial = wallNorth.GetComponent<Renderer>().sharedMaterial;
        }
        
        float actualWidth = maxX - minX;
        float actualLength = maxZ - minZ;
        
        // 验证计算结果
        if (actualWidth < 1f || actualLength < 1f)
        {
            EditorUtility.DisplayDialog("错误", $"计算出的房间尺寸无效：宽度={actualWidth}, 长度={actualLength}\n\n请检查墙壁对象的位置和大小。", "确定");
            return;
        }

        // 创建横梁房顶的父对象
        GameObject ceilingParent = new GameObject("DungeonCeiling");
        
        // 如果还没找到材质，尝试从其他墙获取
        if (beamMaterial == null)
        {
            if (wallEast.GetComponent<Renderer>() != null)
            {
                beamMaterial = wallEast.GetComponent<Renderer>().sharedMaterial;
            }
            else if (wallWest.GetComponent<Renderer>() != null)
            {
                beamMaterial = wallWest.GetComponent<Renderer>().sharedMaterial;
            }
        }
        
        // 如果还是没找到材质，创建深色石质/木质材质（地牢风格）
        if (beamMaterial == null)
        {
            beamMaterial = new Material(Shader.Find("Standard"));
            beamMaterial.color = new Color(0.25f, 0.22f, 0.2f, 1f); // 深灰棕色，更符合地牢风格
            beamMaterial.SetFloat("_Metallic", 0.05f); // 低金属度
            beamMaterial.SetFloat("_Glossiness", 0.1f); // 低光泽度（粗糙）
        }
        
        // 横梁参数 - 地牢风格（增强版）
        float beamThickness = 1.2f;   // 横梁厚度（更粗，更有地牢感）
        float beamHeight = 1.0f;      // 横梁高度（更高，更显眼，更有压迫感）
        float spacing = 8f;            // 横梁间距（更密集，营造压抑的地牢氛围）
        float wallMargin = 0f;         // 与墙壁的距离（完全紧贴，无缝隙）
        float irregularity = 0.15f;    // 不规则性（让横梁有点随机偏移，更有地牢感）
        
        // 计算横梁数量
        int numLongitudinalBeams = Mathf.Max(4, Mathf.RoundToInt(actualWidth / spacing)); // 纵向横梁数量（至少4根）
        int numTransverseBeams = Mathf.Max(4, Mathf.RoundToInt(actualLength / spacing));  // 横向横梁数量（至少4根）

        // 创建纵向横梁（沿着Z方向，从南到北）- 紧贴墙壁边缘
        // 横梁应该延伸到墙壁内边缘，消除缝隙
        float startZ = minZ; // 直接使用边界，不留空隙
        float endZ = maxZ;
        float beamLength = endZ - startZ;
        
        for (int i = 0; i < numLongitudinalBeams; i++)
        {
            // 确保横梁均匀分布，第一根和最后一根靠近墙壁边缘
            float xPos;
            if (numLongitudinalBeams == 1)
            {
                xPos = centerX;
            }
            else
            {
                // 从minX开始，到maxX结束，均匀分布
                xPos = minX + (actualWidth) * i / (numLongitudinalBeams - 1f);
            }
            
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = $"LongitudinalBeam_{i + 1}";
            beam.transform.parent = ceilingParent.transform;
            
            // 添加轻微的不规则性（地牢风格 - 横梁不是完全笔直）
            float xOffset = (Random.value - 0.5f) * irregularity * beamThickness;
            float thicknessVariation = 1f + (Random.value - 0.5f) * 0.2f; // 厚度有轻微变化
            
            beam.transform.position = new Vector3(xPos + xOffset, ceilingY - beamHeight / 2f, centerZ);
            beam.transform.localScale = new Vector3(beamThickness * thicknessVariation, beamHeight, beamLength);
            
            // 设置材质和光照
            Renderer renderer = beam.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = beamMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; // 投射阴影
                renderer.receiveShadows = true;
                renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
                renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
            }
            
            // 移除碰撞体（横梁不需要碰撞）
            Object.DestroyImmediate(beam.GetComponent<Collider>());
            
            // 设置为静态对象
            StaticEditorFlags flags = StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic;
            GameObjectUtility.SetStaticEditorFlags(beam, flags);
        }

        // 创建横向横梁（沿着X方向，从西到东）- 紧贴墙壁边缘
        // 横梁应该延伸到墙壁内边缘，消除缝隙
        float startX = minX; // 直接使用边界，不留空隙
        float endX = maxX;
        float beamWidth = endX - startX;
        
        for (int i = 0; i < numTransverseBeams; i++)
        {
            // 确保横梁均匀分布，第一根和最后一根靠近墙壁边缘
            float zPos;
            if (numTransverseBeams == 1)
            {
                zPos = centerZ;
            }
            else
            {
                // 从minZ开始，到maxZ结束，均匀分布
                zPos = minZ + (actualLength) * i / (numTransverseBeams - 1f);
            }
            
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = $"TransverseBeam_{i + 1}";
            beam.transform.parent = ceilingParent.transform;
            
            // 添加轻微的不规则性（地牢风格 - 横梁不是完全笔直）
            float zOffset = (Random.value - 0.5f) * irregularity * beamThickness;
            float thicknessVariation = 1f + (Random.value - 0.5f) * 0.2f; // 厚度有轻微变化
            
            beam.transform.position = new Vector3(centerX, ceilingY - beamHeight / 2f, zPos + zOffset);
            beam.transform.localScale = new Vector3(beamWidth, beamHeight, beamThickness * thicknessVariation);
            
            // 设置材质和光照
            Renderer renderer = beam.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = beamMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
                renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
                renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
            }
            
            // 移除碰撞体
            Object.DestroyImmediate(beam.GetComponent<Collider>());
            
            // 设置为静态对象
            StaticEditorFlags flags = StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic;
            GameObjectUtility.SetStaticEditorFlags(beam, flags);
        }

        // 在横梁交汇处添加支撑柱（增强地牢感）
        for (int i = 0; i < numLongitudinalBeams; i++)
        {
            for (int j = 0; j < numTransverseBeams; j++)
            {
                float xPos;
                float zPos;
                
                if (numLongitudinalBeams == 1)
                {
                    xPos = centerX;
                }
                else
                {
                    xPos = minX + (actualWidth) * i / (numLongitudinalBeams - 1f);
                }
                
                if (numTransverseBeams == 1)
                {
                    zPos = centerZ;
                }
                else
                {
                    zPos = minZ + (actualLength) * j / (numTransverseBeams - 1f);
                }
                
                // 创建支撑柱（连接横梁，使用方形更符合地牢风格）
                // 每隔几个交叉点才创建支撑柱，而不是全部，增加地牢的不规则感
                if ((i + j) % 2 == 0 || Random.value > 0.7f) // 只在一部分交叉点创建支撑柱
                {
                    GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    pillar.name = $"CeilingPillar_{i}_{j}";
                    pillar.transform.parent = ceilingParent.transform;
                    
                    // 支撑柱稍微大一点，更显眼
                    float pillarSize = beamThickness * 1.1f;
                    float pillarHeight = beamHeight * 1.2f;
                    
                    pillar.transform.position = new Vector3(xPos, ceilingY - pillarHeight / 2f, zPos);
                    pillar.transform.localScale = new Vector3(pillarSize, pillarHeight, pillarSize);
                    pillar.transform.rotation = Quaternion.identity;
                    
                    // 设置材质和光照
                    Renderer renderer = pillar.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.sharedMaterial = beamMaterial;
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                        renderer.receiveShadows = true;
                        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
                        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
                    }
                    
                    // 移除碰撞体
                    Object.DestroyImmediate(pillar.GetComponent<Collider>());
                    
                    // 设置为静态对象
                    StaticEditorFlags flags = StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic;
                    GameObjectUtility.SetStaticEditorFlags(pillar, flags);
                }
            }
        }

        // 添加边缘框架横梁（连接到墙壁，形成完整的地牢天花板框架）
        // 沿着墙壁边缘创建粗壮的边缘横梁，确保无缝隙
        
        float edgeBeamThickness = beamThickness * 1.5f; // 边缘横梁更粗，更有结构感
        float edgeOverlap = beamThickness * 0.3f; // 边缘横梁稍微延伸，确保覆盖
        
        // 东边缘横梁（紧贴东墙内边缘，延伸到两端）
        GameObject edgeBeamEast = GameObject.CreatePrimitive(PrimitiveType.Cube);
        edgeBeamEast.name = "EdgeBeam_East";
        edgeBeamEast.transform.parent = ceilingParent.transform;
        edgeBeamEast.transform.position = new Vector3(maxX - edgeBeamThickness / 2f, ceilingY - beamHeight / 2f, centerZ);
        edgeBeamEast.transform.localScale = new Vector3(edgeBeamThickness, beamHeight, actualLength + edgeOverlap * 2f);
        SetBeamProperties(edgeBeamEast, beamMaterial);
        
        // 西边缘横梁（紧贴西墙内边缘，延伸到两端）
        GameObject edgeBeamWest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        edgeBeamWest.name = "EdgeBeam_West";
        edgeBeamWest.transform.parent = ceilingParent.transform;
        edgeBeamWest.transform.position = new Vector3(minX + edgeBeamThickness / 2f, ceilingY - beamHeight / 2f, centerZ);
        edgeBeamWest.transform.localScale = new Vector3(edgeBeamThickness, beamHeight, actualLength + edgeOverlap * 2f);
        SetBeamProperties(edgeBeamWest, beamMaterial);
        
        // 北边缘横梁（紧贴北墙内边缘，延伸到两端，覆盖东西边缘横梁）
        GameObject edgeBeamNorth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        edgeBeamNorth.name = "EdgeBeam_North";
        edgeBeamNorth.transform.parent = ceilingParent.transform;
        edgeBeamNorth.transform.position = new Vector3(centerX, ceilingY - beamHeight / 2f, maxZ - edgeBeamThickness / 2f);
        edgeBeamNorth.transform.localScale = new Vector3(actualWidth + edgeOverlap * 2f, beamHeight, edgeBeamThickness);
        SetBeamProperties(edgeBeamNorth, beamMaterial);
        
        // 南边缘横梁（紧贴南墙内边缘，延伸到两端，覆盖东西边缘横梁）
        GameObject edgeBeamSouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        edgeBeamSouth.name = "EdgeBeam_South";
        edgeBeamSouth.transform.parent = ceilingParent.transform;
        edgeBeamSouth.transform.position = new Vector3(centerX, ceilingY - beamHeight / 2f, minZ + edgeBeamThickness / 2f);
        edgeBeamSouth.transform.localScale = new Vector3(actualWidth + edgeOverlap * 2f, beamHeight, edgeBeamThickness);
        SetBeamProperties(edgeBeamSouth, beamMaterial);

        // 移除重复的支撑柱创建代码（已在上面修改为条件创建）
        // 删除旧的完整支撑柱循环，因为已经在上面用条件创建了
        
        Debug.Log($"✅ 地牢横梁房顶已创建：位置({centerX}, {ceilingY}, {centerZ}), 房间尺寸({actualWidth}, {actualLength})");
        EditorUtility.DisplayDialog("成功", 
            $"地牢横梁房顶已添加到场景中！\n\n" +
            $"横梁配置：\n" +
            $"- 纵向横梁：{numLongitudinalBeams} 根（带有不规则性）\n" +
            $"- 横向横梁：{numTransverseBeams} 根（带有不规则性）\n" +
            $"- 边缘框架：4根粗壮边缘横梁\n" +
            $"- 支撑柱：部分交叉点（不规则分布）\n\n" +
            $"地牢风格特性：\n" +
            $"- 横梁更粗更密集，营造压抑氛围\n" +
            $"- 添加了轻微不规则性，更真实\n" +
            $"- 完全紧贴墙壁，无缝隙\n\n" +
            $"光照设置：\n" +
            $"- 横梁会投射和接收阴影\n" +
            $"- 使用Light Probes保持间接光照\n" +
            $"- 光照可以穿透横梁之间的空隙", 
            "确定");

        // 选中新创建的房顶
        Selection.activeGameObject = ceilingParent;
    }
    
    // 辅助方法：设置横梁的材质和光照属性
    static void SetBeamProperties(GameObject beam, Material material)
    {
        Renderer renderer = beam.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
        }
        
        // 移除碰撞体
        Collider collider = beam.GetComponent<Collider>();
        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }
        
        // 设置为静态对象
        StaticEditorFlags flags = StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic;
        GameObjectUtility.SetStaticEditorFlags(beam, flags);
    }
}

