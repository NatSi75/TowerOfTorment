using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] private TMP_Text type;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text mana;
    [SerializeField] private SpriteRenderer imageSR;
    [SerializeField] private Image image;
    [SerializeField] private GameObject wrapper;
    [SerializeField] private LayerMask dropLayer;
    private CardRarity rarity;
    [SerializeField] private GameObject backgroundParentCard;
    public RarityVisual[] rarityVisualsKnight;
    public RarityVisual[] rarityVisualsWizard;
    [SerializeField] private Color upgradedTitleColor = new(0.45f, 0.95f, 0.35f);
    private Color? defaultTitleColor;

    [Header("Card Art Window")]
    [Tooltip("Opening of the card frame for the world art (Image Sr), in its parent's local space. Size 0 = keep as is.")]
    [SerializeField] private Rect worldArtWindow;
    [Tooltip("Opening of the card frame for the UI art (Image), in its parent's local space. Size 0 = keep as is.")]
    [SerializeField] private Rect uiArtWindow;
    private static readonly Dictionary<(Sprite, float), Sprite> croppedArt = new();
    public Card Card { get; private set; }
    private Vector3 dragStartPosition;
    private Quaternion dragStartRotation;
    private Vector3 dragOffset;

    public void Setup (Card card)
    {
        Card = card;
        type.text = card.Type.ToString();
        title.text = card.Title;
        // Kurale has no bold weight, so upgraded cards are marked with a colour and synthetic bold.
        defaultTitleColor ??= title.color;
        title.color = card.IsUpgradeVersion ? upgradedTitleColor : defaultTitleColor.Value;
        title.fontStyle = card.IsUpgradeVersion ? title.fontStyle | FontStyles.Bold : title.fontStyle & ~FontStyles.Bold;
        description.text = card.description;
        mana.text = card.Mana.ToString();
        SetArt(card);
        rarity = card.RarityVisual;
        ApplyRarityVisuals(rarity);
    }

    // Card art comes in different sizes (256 / 512 px, square). Crop it to the frame opening's aspect
    // and scale it to exactly fill the opening, so it is never too big or too small.
    private void SetArt(Card card)
    {
        if (worldArtWindow.size.x > 0f && worldArtWindow.size.y > 0f && card.Image != null)
        {
            Sprite art = CropToAspect(card.Image, worldArtWindow.width / worldArtWindow.height);
            imageSR.sprite = art;
            imageSR.drawMode = SpriteDrawMode.Simple;
            Transform artTransform = imageSR.transform;
            artTransform.localPosition = new Vector3(worldArtWindow.center.x, worldArtWindow.center.y, artTransform.localPosition.z);
            artTransform.localRotation = Quaternion.identity;
            Vector2 artSize = art.rect.size / art.pixelsPerUnit;
            artTransform.localScale = new Vector3(worldArtWindow.width / artSize.x, worldArtWindow.height / artSize.y, 1f);
        }
        else
        {
            imageSR.sprite = card.Image;
        }

        if (uiArtWindow.size.x > 0f && uiArtWindow.size.y > 0f && card.ImageUI != null)
        {
            image.sprite = CropToAspect(card.ImageUI, uiArtWindow.width / uiArtWindow.height);
            image.preserveAspect = false;
            RectTransform artRect = image.rectTransform;
            artRect.anchorMin = artRect.anchorMax = artRect.pivot = new Vector2(0.5f, 0.5f);
            artRect.anchoredPosition = uiArtWindow.center;
            artRect.sizeDelta = uiArtWindow.size;
            artRect.localScale = Vector3.one;
        }
        else
        {
            image.sprite = card.ImageUI;
        }
    }

    private static Sprite CropToAspect(Sprite source, float aspect)
    {
        if (croppedArt.TryGetValue((source, aspect), out Sprite cached) && cached != null) return cached;

        Rect rect = source.rect;
        if (rect.width / rect.height > aspect)
        {
            float width = rect.height * aspect;
            rect.x += (rect.width - width) / 2f;
            rect.width = width;
        }
        else
        {
            float height = rect.width / aspect;
            rect.y += (rect.height - height) / 2f;
            rect.height = height;
        }
        Sprite cropped = Sprite.Create(source.texture, rect, new Vector2(0.5f, 0.5f), source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
        cropped.name = source.name + " (card art)";
        croppedArt[(source, aspect)] = cropped;
        return cropped;
    }

    private void ApplyRarityVisuals(CardRarity rarity)
    {
        if (GameDataManager.Instance.indexHero == 0) // Knight
        {
            foreach (var visual in rarityVisualsKnight)
            {
                if (visual.rarity == rarity)
                {
                    if (visual.backgroundSprite != null && backgroundParentCard != null)
                    {
                        Instantiate(visual.backgroundSprite, backgroundParentCard.transform);
                    }
                    break;
                }
            }
        } else // Wizard
        {
            foreach (var visual in rarityVisualsWizard)
            {
                if (visual.rarity == rarity)
                {
                    if (visual.backgroundSprite != null && backgroundParentCard != null)
                    {
                        Instantiate(visual.backgroundSprite, backgroundParentCard.transform);
                    }
                    break;
                }
            }
        }   
    }

    void OnMouseEnter()
    {
        if (!Interactions.Instance.PlayerCanHover()) return;
        wrapper.SetActive(false);
        Vector3 pos = new(transform.position.x, 0, 0);
        CardViewHoverSystem.Instance.Show(Card, pos);
    }

    void OnMouseExit()
    {
        if (!Interactions.Instance.PlayerCanHover()) return;
        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);
        ResetCardVisual();
    }

    public void OnCardClick()
    {
        var restSiteManager = Object.FindAnyObjectByType<RestSiteManager>();
        var pillarManager = Object.FindAnyObjectByType<PillarManager>();
        if (restSiteManager != null)
        {

            if (restSiteManager.currentState == RestSiteManager.RestSiteState.SelectingCardForUpgrade)
            {
                restSiteManager.ShowUpgradePreview(Card);
            }
            else if (restSiteManager.currentState == RestSiteManager.RestSiteState.SelectingCardForRemove)
            {
                restSiteManager.ShowRemovePreview(Card);
            }
        }
        if (pillarManager != null)
        {

            if (pillarManager.currentState == PillarManager.PillarState.SelectingCardForUpgrade)
            {
                pillarManager.ShowUpgradePreview(Card);
            }
            else if (pillarManager.currentState == PillarManager.PillarState.SelectingCardForRemove)
            {
                pillarManager.ShowRemovePreview(Card);
            }
        }
    }

    void OnMouseDown()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;
        ResetCardVisual();
        if (Card.ManualTargetEffect != null && Card.ManualTargetEffect.Count > 0)
        {
            ManualTargetSystem.Instance.StartTargeting(MouseUtil.GetMousePositionInWorldSpace());
        }
        else
        {
            Interactions.Instance.PlayerIsDragging = true;
            if (MobileLayout.IsSmallScreen)
            {
                // touch: keep the enlarged preview while the finger rests on the card, hide it once dragging starts
                touchPreview = true;
                touchStartPosition = MouseUtil.GetMousePositionInWorldSpace();
            }
            else
            {
                wrapper.SetActive(true);
                CardViewHoverSystem.Instance.Hide();
            }
            dragStartPosition = transform.position;
            dragStartRotation = transform.rotation;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            dragOffset = transform.position - MouseUtil.GetMousePositionInWorldSpace();
        }
    }

    void OnMouseDrag()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;
        if (Card.ManualTargetEffect != null && Card.ManualTargetEffect.Count > 0) return;
        Vector3 mousePosition = MouseUtil.GetMousePositionInWorldSpace();
        if (touchPreview && Vector3.Distance(mousePosition, touchStartPosition) > TouchDragThreshold) EndTouchPreview();
        transform.position = mousePosition + dragOffset;
    }

    private const float TouchDragThreshold = 1f;
    private bool touchPreview;
    private Vector3 touchStartPosition;

    private void EndTouchPreview()
    {
        touchPreview = false;
        wrapper.SetActive(true);
        CardViewHoverSystem.Instance.Hide();
    }

    void OnMouseUp()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;
        ResetCardVisual();
        if (Card.ManualTargetEffect != null && Card.ManualTargetEffect.Count > 0)
        {
            EnemyView target = ManualTargetSystem.Instance.EndTargeting(MouseUtil.GetMousePositionInWorldSpace());
            if (target != null && ManaSystem.Instance.HasEnoughMana(Card.Mana))
            {
                PlayCardGA playCardGA = new(Card, target);
                ActionSystem.Instance.Perform(playCardGA);
            }
        }
        else
        {
            if (touchPreview) EndTouchPreview();
            Vector3 checkPosition = transform.position;
            checkPosition.z = 0f;
            Collider2D hitCollider = Physics2D.OverlapPoint(checkPosition, dropLayer);
            if (hitCollider != null
                && hitCollider.TryGetComponent(out DropAreaUtil area) 
                && ManaSystem.Instance.HasEnoughMana(Card.Mana))
            {
                PlayCardGA playCardGA = new(Card);
                ActionSystem.Instance.Perform(playCardGA);
            } 
            else
            {
                transform.position = dragStartPosition;
                transform.rotation = dragStartRotation;
            }

            Interactions.Instance.PlayerIsDragging = false;
        }
    }

    public void ResetCardVisual()
    {
           Card.description = Card.Description;
    }
}
