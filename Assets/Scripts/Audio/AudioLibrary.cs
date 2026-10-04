using System.Collections.Generic;
using UnityEngine;

public enum Sfx
{
    ButtonClick,
    PlayCard,
    KnightAttack,
    WizardAttack,
    Burn,
    RestHeal,
    ShopBuy,
    ChestOpen,
    RewardPickup,
    PillarSacrifice,
    TormentFull,
    Victory,
    Defeat,
}

[System.Serializable]
public class SoundClip
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}

[System.Serializable]
public class SceneMusic
{
    public string trackName;
    public SoundClip music = new();
    [Tooltip("Scenes that play this track, names exactly as in Build Settings")]
    public List<string> sceneNames = new();
}

// Every music track and sound effect of the game. The asset must live at Resources/AudioLibrary.
// Scenes that are not listed keep playing the current music; scenes sharing a track keep it playing.
[CreateAssetMenu(menuName = "Data/Audio Library")]
public class AudioLibrary : ScriptableObject
{
    [Header("Background Music (BGM)")]
    public List<SceneMusic> sceneMusic = new();
    [Tooltip("Played in Battle against the boss. Leave empty to use the Battle music.")]
    public SoundClip bossBattleMusic = new();

    [Header("Sound Effects (SFX)")]
    public SoundClip buttonClick = new();
    public SoundClip playCard = new();
    public SoundClip knightAttack = new();
    public SoundClip wizardAttack = new();
    [Tooltip("Enemy applies Burn (Demon / Evil Wizard Fire)")]
    public SoundClip burn = new();
    public SoundClip restHeal = new();
    public SoundClip shopBuy = new();
    public SoundClip chestOpen = new();
    [Tooltip("Picking a relic or a reward card")]
    public SoundClip rewardPickup = new();
    public SoundClip pillarSacrifice = new();
    public SoundClip tormentFull = new();
    public SoundClip victory = new();
    public SoundClip defeat = new();

    [Header("Default Volume (first launch)")]
    [Range(0f, 1f)] public float defaultBgmVolume = 0.6f;
    [Range(0f, 1f)] public float defaultSfxVolume = 0.8f;

    public SoundClip Get(Sfx sfx)
    {
        return sfx switch
        {
            Sfx.ButtonClick => buttonClick,
            Sfx.PlayCard => playCard,
            Sfx.KnightAttack => knightAttack,
            Sfx.WizardAttack => wizardAttack,
            Sfx.Burn => burn,
            Sfx.RestHeal => restHeal,
            Sfx.ShopBuy => shopBuy,
            Sfx.ChestOpen => chestOpen,
            Sfx.RewardPickup => rewardPickup,
            Sfx.PillarSacrifice => pillarSacrifice,
            Sfx.TormentFull => tormentFull,
            Sfx.Victory => victory,
            Sfx.Defeat => defeat,
            _ => null,
        };
    }

    public SoundClip GetSceneMusic(string sceneName)
    {
        SceneMusic entry = sceneMusic.Find(m => m.sceneNames.Contains(sceneName));
        return entry?.music;
    }
}
