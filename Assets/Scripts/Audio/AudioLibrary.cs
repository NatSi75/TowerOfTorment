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
    EnemyAttackBlunt,
    EnemyAttackSharp,
    EnemyBlock,
    EnemyHeal,
}

[System.Serializable]
public class SoundClip
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}

[System.Serializable]
public class StatusEffectSound
{
    public StatusEffectType statusEffect;
    [Tooltip("Played when this debuff is put on a combatant")]
    public SoundClip applied = new();
    [Tooltip("Played when this debuff triggers / loses a stack at turn start")]
    public SoundClip tick = new();
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
    [Tooltip("Enemy attack with a blunt hit: fists, bodies, shields, staffs, spell blasts")]
    public SoundClip enemyAttackBlunt = new();
    [Tooltip("Enemy attack with something sharp: blades, spears, scythes, claws, fangs, spikes")]
    public SoundClip enemyAttackSharp = new();
    [Tooltip("Enemy with a Block intent gains armor")]
    public SoundClip enemyBlock = new();
    [Tooltip("Enemy with a Heal intent heals")]
    public SoundClip enemyHeal = new();

    [Header("Debuffs")]
    [Tooltip("A debuff is put on a combatant (unless the debuff has its own sound below)")]
    public SoundClip debuffApplied = new();
    [Tooltip("A debuff triggers / loses a stack at turn start (unless the debuff has its own sound below)")]
    public SoundClip debuffTick = new();
    [Tooltip("Optional own sounds per debuff; an empty clip falls back to the two above")]
    public List<StatusEffectSound> statusEffectSounds = new();
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
            Sfx.EnemyAttackBlunt => enemyAttackBlunt,
            Sfx.EnemyAttackSharp => enemyAttackSharp,
            Sfx.EnemyBlock => enemyBlock,
            Sfx.EnemyHeal => enemyHeal,
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

    public SoundClip GetDebuffApplied(StatusEffectType type)
    {
        SoundClip own = statusEffectSounds.Find(s => s.statusEffect == type)?.applied;
        if (own != null && own.clip != null) return own;
        if (type == StatusEffectType.BURN && burn.clip != null) return burn;
        return debuffApplied;
    }

    public SoundClip GetDebuffTick(StatusEffectType type)
    {
        SoundClip own = statusEffectSounds.Find(s => s.statusEffect == type)?.tick;
        return own != null && own.clip != null ? own : debuffTick;
    }

    public SoundClip GetSceneMusic(string sceneName)
    {
        SceneMusic entry = sceneMusic.Find(m => m.sceneNames.Contains(sceneName));
        return entry?.music;
    }
}
