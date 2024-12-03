using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetIndicator : MonoBehaviour
{
    public Camera mainCamera; // 主摄像机
    public Canvas worldSpaceCanvas; // 世界空间的 Canvas
    public RectTransform distanceTextPrefab; // 距离文本预制件
    public RectTransform arrowPrefab; // 箭头预制件
    public RectTransform meteoriteDistanceTextPrefab; // 陨石距离文本预制件
    public RectTransform warningSignPrefab; // 警告标志预制件
    public float edgePadding = 50f; // 屏幕边缘的填充
    public Vector2 planetOffset = new Vector2(0, 50); // 行星文本的偏移
    public Vector2 meteoriteOffset = new Vector2(0, 30); // 陨石文本的偏移

    private List<TargetData> planetTargets = new List<TargetData>(); // 行星目标列表
    private List<TargetData> meteoriteTargets = new List<TargetData>(); // 陨石目标列表

    // 存储目标相关数据的类
    private class TargetData
    {
        public Transform targetTransform; // 目标位置
        public RectTransform distanceTextUI; // 距离文本 UI
        public RectTransform arrowUI; // 箭头 UI
        public Text distanceTextComponent; // 文本组件
        public Image warningSign; // 警告标志

        public TargetData(Transform target, RectTransform distanceText, RectTransform arrow, Text distanceTextComp, Image warningSignImage)
        {
            targetTransform = target;
            distanceTextUI = distanceText;
            arrowUI = arrow;
            distanceTextComponent = distanceTextComp;
            warningSign = warningSignImage;
        }
    }

    void Start()
    {
        // 初始化行星和陨石目标
        InitializeTargets("Ground", distanceTextPrefab, arrowPrefab, planetTargets);
        InitializeTargets("Meteorite", meteoriteDistanceTextPrefab, warningSignPrefab, meteoriteTargets);
    }

    void Update()
    {
        // 更新目标的 UI
        UpdateTargets(planetTargets, isMeteorite: false);
        UpdateTargets(meteoriteTargets, isMeteorite: true);

        // 检查是否有新的陨石生成
        CheckForNewMeteorites();
    }

    private void InitializeTargets(string tag, RectTransform distancePrefab, RectTransform arrowPrefab, List<TargetData> targetList)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in targets)
        {
            if (obj != null)
            {
                AddTarget(obj.transform, distancePrefab, arrowPrefab, targetList, tag == "Meteorite");
            }
        }
    }

    private void AddTarget(Transform target, RectTransform distancePrefab, RectTransform arrowPrefab, List<TargetData> targetList, bool isMeteorite)
    {
        RectTransform distanceText = Instantiate(distancePrefab, worldSpaceCanvas.transform);
        distanceText.localScale = Vector3.one;

        RectTransform arrow = null;
        Image warningSign = null;

        if (isMeteorite)
        {
            warningSign = Instantiate(arrowPrefab, worldSpaceCanvas.transform).GetComponent<Image>();
            warningSign.rectTransform.localScale = Vector3.one;
        }
        else
        {
            arrow = Instantiate(arrowPrefab, worldSpaceCanvas.transform);
            arrow.localScale = Vector3.one;
        }

        Text distanceTextComp = distanceText.GetComponent<Text>();
        targetList.Add(new TargetData(target, distanceText, arrow, distanceTextComp, warningSign));
    }

    private void UpdateTargets(List<TargetData> targetList, bool isMeteorite)
    {
        for (int i = targetList.Count - 1; i >= 0; i--) // 倒序遍历以安全移除元素
        {
            var targetData = targetList[i];

            if (targetData.targetTransform == null) // 如果目标已被销毁
            {
                RemoveTarget(targetData, targetList);
                continue;
            }

            UpdateTargetUI(targetData, isMeteorite);
        }
    }

    private void RemoveTarget(TargetData targetData, List<TargetData> targetList)
    {
        if (targetData.distanceTextUI != null)
            Destroy(targetData.distanceTextUI.gameObject);

        if (targetData.arrowUI != null)
            Destroy(targetData.arrowUI.gameObject);

        if (targetData.warningSign != null)
            Destroy(targetData.warningSign.gameObject);

        targetList.Remove(targetData);
    }

    private void UpdateTargetUI(TargetData targetData, bool isMeteorite)
    {
        if (mainCamera == null || worldSpaceCanvas == null)
        {
            return;
        }

        Vector3 screenPoint = mainCamera.WorldToScreenPoint(targetData.targetTransform.position);
        bool isTargetVisible = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < Screen.width && screenPoint.y > 0 && screenPoint.y < Screen.height;

        float distanceToTarget = Vector3.Distance(mainCamera.transform.position, targetData.targetTransform.position);

        if (isTargetVisible)
        {
            // 显示距离文本，隐藏箭头或警告标志
            if (isMeteorite)
            {
                targetData.warningSign?.gameObject.SetActive(false);
                targetData.distanceTextUI?.gameObject.SetActive(true);
            }
            else
            {
                targetData.arrowUI?.gameObject.SetActive(false);
                targetData.distanceTextUI?.gameObject.SetActive(true);
            }

            RectTransform canvasRect = worldSpaceCanvas.GetComponent<RectTransform>();
            Vector2 viewportPoint = mainCamera.WorldToViewportPoint(targetData.targetTransform.position);
            Vector2 canvasPosition = new Vector2(
                (viewportPoint.x - 0.5f) * canvasRect.sizeDelta.x,
                (viewportPoint.y - 0.5f) * canvasRect.sizeDelta.y
            );

            // 应用偏移量
            canvasPosition += isMeteorite ? meteoriteOffset : planetOffset;

            targetData.distanceTextUI.anchoredPosition = canvasPosition;
            targetData.distanceTextComponent.text = $"{distanceToTarget:F1}m";
        }
        else
        {
            // 显示箭头或警告标志
            if (isMeteorite)
            {
                targetData.distanceTextUI?.gameObject.SetActive(false);
                targetData.warningSign?.gameObject.SetActive(true);

                RectTransform canvasRect = worldSpaceCanvas.GetComponent<RectTransform>();
                Vector2 arrowPosition = CalculateOffScreenPosition(screenPoint, canvasRect);
                targetData.warningSign.rectTransform.anchoredPosition = arrowPosition;

                // 计算箭头旋转方向
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Vector2 direction = (new Vector2(screenPoint.x, screenPoint.y) - screenCenter).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
                targetData.warningSign.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                targetData.distanceTextUI?.gameObject.SetActive(false);
                targetData.arrowUI?.gameObject.SetActive(true);

                RectTransform canvasRect = worldSpaceCanvas.GetComponent<RectTransform>();
                Vector2 arrowPosition = CalculateOffScreenPosition(screenPoint, canvasRect);
                targetData.arrowUI.anchoredPosition = arrowPosition;

                // 计算箭头旋转方向
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Vector2 direction = (new Vector2(screenPoint.x, screenPoint.y) - screenCenter).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
                targetData.arrowUI.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    private Vector2 CalculateOffScreenPosition(Vector3 screenPoint, RectTransform canvasRect)
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Vector3 direction = (screenPoint - screenCenter).normalized;

        float maxX = canvasRect.sizeDelta.x / 2f - edgePadding;
        float maxY = canvasRect.sizeDelta.y / 2f - edgePadding;

        return new Vector2(
            Mathf.Clamp(direction.x * maxX, -maxX, maxX),
            Mathf.Clamp(direction.y * maxY, -maxY, maxY)
        );
    }

    private void CheckForNewMeteorites()
    {
        GameObject[] newMeteorites = GameObject.FindGameObjectsWithTag("Meteorite");
        foreach (GameObject meteorite in newMeteorites)
        {
            if (!meteoriteTargets.Exists(t => t.targetTransform == meteorite.transform))
            {
                AddTarget(meteorite.transform, meteoriteDistanceTextPrefab, warningSignPrefab, meteoriteTargets, true);
            }
        }
    }
}