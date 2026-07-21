using UnityEngine;
using UnityEngine.UI;

public sealed class DeveloperCatalogTool : MonoBehaviour
{
    public VendingCatalog catalog;
    public Button openButton;
    public GameObject panelRoot;
    public Button closeButton;
    public Text catalogText;

    private void Awake()
    {
        Bind();
        SetPanelVisible(false);
    }

    private void OnEnable()
    {
        Bind();
    }

    private void Bind()
    {
        if (openButton != null)
        {
            openButton.onClick.RemoveListener(Open);
            openButton.onClick.AddListener(Open);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
            closeButton.onClick.AddListener(Close);
        }
    }

    public void Open()
    {
        RefreshCatalogText();
        SetPanelVisible(true);
    }

    public void Close()
    {
        SetPanelVisible(false);
    }

    private void RefreshCatalogText()
    {
        if (catalogText == null)
        {
            return;
        }

        if (catalog == null)
        {
            catalogText.text = "未绑定商品库。";
            return;
        }

        catalog.EnsureDefaults();
        catalogText.text = DeveloperCatalogFormatter.BuildGroupedCatalogText(catalog.Items);
        UpdateScrollContentHeight();
    }

    private void UpdateScrollContentHeight()
    {
        var textRect = catalogText.rectTransform;
        float contentHeight = Mathf.Max(420f, catalogText.preferredHeight + 24f);
        textRect.sizeDelta = new Vector2(textRect.sizeDelta.x, contentHeight);

        var contentRect = textRect.parent as RectTransform;
        if (contentRect != null)
        {
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentHeight);
        }
    }

    private void SetPanelVisible(bool visible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }
    }
}
