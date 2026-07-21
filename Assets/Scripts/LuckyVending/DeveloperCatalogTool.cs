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
        var rectTransform = catalogText.rectTransform;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Mathf.Max(420f, catalogText.preferredHeight + 24f));
    }

    private void SetPanelVisible(bool visible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }
    }
}
