using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetIndicator : MonoBehaviour
{
    public Camera mainCamera;
    public Canvas worldSpaceCanvas;
    public RectTransform distanceTextPrefab;
    public RectTransform arrowPrefab;
    public RectTransform meteoriteDistanceTextPrefab;
    public RectTransform warningSignPrefab;
    public float edgePadding = 50f;
    public Vector2 distanceTextOffset = new Vector2(0, 50);

    private List<TargetData> planetTargets = new List<TargetData>();
    private List<TargetData> meteoriteTargets = new List<TargetData>();

    private class TargetData
    {
        public Transform targetTransform;
        public RectTransform distanceTextUI;
        public RectTransform arrowUI;
        public Text distanceTextComponent;
        public Image warningSign;

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
        InitializeTargets("Ground", distanceTextPrefab, arrowPrefab, planetTargets);
        InitializeTargets("Meteorite", meteoriteDistanceTextPrefab, warningSignPrefab, meteoriteTargets);
    }

    void Update()
    {
        UpdateTargets(planetTargets, isMeteorite: false);
        UpdateTargets(meteoriteTargets, isMeteorite: true);
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

            canvasPosition += distanceTextOffset;

            targetData.distanceTextUI.anchoredPosition = canvasPosition;
            targetData.distanceTextComponent.text = $"{distanceToTarget:F1}m";
        }
        else
        {
            if (isMeteorite)
            {
                targetData.distanceTextUI?.gameObject.SetActive(false);
                targetData.warningSign?.gameObject.SetActive(true);

                RectTransform canvasRect = worldSpaceCanvas.GetComponent<RectTransform>();
                Vector2 arrowPosition = CalculateOffScreenPosition(screenPoint, canvasRect);
                targetData.warningSign.rectTransform.anchoredPosition = arrowPosition;
            }
            else
            {
                targetData.distanceTextUI?.gameObject.SetActive(false);
                targetData.arrowUI?.gameObject.SetActive(true);

                RectTransform canvasRect = worldSpaceCanvas.GetComponent<RectTransform>();
                Vector2 arrowPosition = CalculateOffScreenPosition(screenPoint, canvasRect);
                targetData.arrowUI.anchoredPosition = arrowPosition;
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
