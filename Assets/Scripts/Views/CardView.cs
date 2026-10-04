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
        imageSR.sprite = card.Image;
        image.sprite = card.ImageUI;
        rarity = card.RarityVisual;
        ApplyRarityVisuals(rarity);
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
            wrapper.SetActive(true);
            CardViewHoverSystem.Instance.Hide();
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
        transform.position = MouseUtil.GetMousePositionInWorldSpace() + dragOffset;
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
