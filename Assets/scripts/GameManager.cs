using System;
using UnityEngine;
using System.Collections;
using Managers;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using TMPro;


public class GameManager : MonoBehaviour
{
    [Header("Rounds")]
    public int currentRound = 1;
    public MMF_Player round_effect;
    public TextMeshProUGUI roundtxt;

    #region Event lists
    [Header("Scheduled Effects")]
    public List<ScheduledLowRiskBonus> scheduledLowRiskBonuses
        = new List<ScheduledLowRiskBonus>();
    
    public List<ScheduledAllShotsBonus> scheduledAllShotsBonuses
        = new List<ScheduledAllShotsBonus>();
    
    public List<ScheduledStartingPointsBonus> scheduledStartingPointsBonuses
        = new List<ScheduledStartingPointsBonus>();
    
    public List<ScheduledEnemyDebuff> scheduledEnemyDebuffs
        = new List<ScheduledEnemyDebuff>();
    
    public List<ScheduledRewardBonus> scheduledRewardBonuses
        = new List<ScheduledRewardBonus>();
    
    public List<ScheduledLossCondition> scheduledLossConditions
        = new List<ScheduledLossCondition>();
    
    [Header("Player Selection")]
    public HorseSO selectedHorseSO;

    [Header("Scene Horses (TOP → BOTTOM order)")]
    public HorseView[] horseViews;

    [Header("UI")]
    public TurnUI turnUI;
    
    [Header("Permanent Buffs")]
    public int permanentAllShotsBonus = 0;
    
    [Header("Dice Effects")]
    public int playerExtraRolls = 0;
    
    [Header("Temporary Turn Effects")]
    public int leaderPenaltyNextTurn = 0;
    
    [Header("Item States")]
    public int guaranteedBonusNextRoll = 0;

    [Header("Item States")]
    public int enemiesBlockedThisTurn = 0;
    
    #endregion
    
    public SoundLibrary uiSfx;
    public GameFlowManager flowManager;
    private HorseRuntime[] horses;
    private int playerIndex = -1;
    private bool resolvingTurn = false;
    public bool HasRaceInitialized { get; private set; }
    private int lossRoundForPermanentAllShots = -1;
    private int startingAdvantageThisRace = 0;
    private bool lowDisabledThisRace = false;
    
    [Header("Item: Swap (Mid hinders enemies)")]
    public bool swapMidActive = false;
    public int swapMidAmount = 0;

    [Header("Shot Modifiers")]
    public int midShotModifier = 0;
    public int highShotModifier = 0;
    public int lowShotModifier = 0;

    [Header("UI Text")] 
    public indicatorsUpdate updateIndicators;
    [SerializeField] private int cameraPenaltyNextTurn = 0;
    [SerializeField] private int cameraPenaltyAmount = 2;
    
    [Header("Boss Behavior")]
    [Range(0f, 1f)]
    public float bossPunishChance = 0.35f; // 35% por defecto
    
    bool cameraPending = false;
    
    // cosas para el boss
    public bool isBossRace = false;
    public int bossRound = 20;
    public void InitRaceData()
    {
        StartRace();
        HasRaceInitialized = true;
    }
    public void StartRaceFlow()
    {
        if (!HasRaceInitialized)
        {
            InitRaceData();
        }

        turnUI.Show();
    }
    public HorseRuntime GetPlayerHorse()
    {
        if (!HasRaceInitialized || playerIndex < 0 || horses == null)
            return null;

        return horses[playerIndex];

    }
    public void ConsumeRound()
    {
        currentRound++;
        updateIndicators.UpdateText();
        Debug.Log($"Round {currentRound}");
        round_effect.PlayFeedbacks();
    }
    public void UpdateTextRound()
    {
        roundtxt.text = currentRound.ToString() + "/20";
    }
    
    [Header("Turn Modifiers")]
    public bool BlockLeaderNextTurn = false;
    
    [Header("Next Race Modifiers")]
    public int startingBonusNextRace = 0;
    
    void StartRace()
    {
        horses = new HorseRuntime[horseViews.Length];

        for (int i = 0; i < horseViews.Length; i++)
        {
            horses[i] = new HorseRuntime(horseViews[i].horseData);
            horseViews[i].Initialize(horses[i]);

            if (horseViews[i].horseData == selectedHorseSO)
                playerIndex = i;
        }

        if (playerIndex == -1)
        {
            Debug.LogError(" Selected horse not found in HorseViews");
            return;
        }

        Debug.Log($"layer horse: {selectedHorseSO.horseName}");
        StartNewRound();
    }
    
    int ApplyPlayerShotModifiers(
        ShotType shot,
        int rollGain,
        out bool swapConsumed
    )
    {
        swapConsumed = false;

        if (rollGain <= 0)
            return rollGain;

        int result = rollGain;

        // 1. Modificadores permanentes
        switch (shot)
        {
            case ShotType.High:
                result += highShotModifier;
                break;
            case ShotType.Medium:
                result += midShotModifier;
                break;
            case ShotType.Low:
                result += lowShotModifier;
                break;
        }

        // 2. Swap Mid (PRIORIDAD)
        if (swapMidActive && shot == ShotType.Medium)
        {
            swapConsumed = true;
            return 0; // el jugador NO avanza
        }

        // 3. Zanahoria (bonus garantizado)
        if (guaranteedBonusNextRoll > 0)
        {
            result += guaranteedBonusNextRoll;
            guaranteedBonusNextRoll = 0;
        }

        return result;
    }
    
    public void PlayerChooseShot(ShotType shot)
    {
        if (resolvingTurn) return;

        turnUI.Hide();
        StartCoroutine(ResolveTurn(shot));
    }
    
    IEnumerator ResolveTurn(ShotType playerShot)
{
    resolvingTurn = true;
    bool usedExtraRoll = false;

    for (int i = 0; i < horses.Length; i++)
    {
        bool isPlayer = (i == playerIndex);
        bool isBoss = isBossRace && horses[i].baseData is BossHorseSO;

        ShotType shotToUse =
            isPlayer ? playerShot :
            isBoss ? ChooseBossShot() :
            AIShotChooser.ChooseShot();

        // ================= CAMERA =================
        if (cameraPending && i == GetLeaderIndex())
        {
            int penalty = leaderPenaltyNextTurn;

            horses[i].currentPoints = Mathf.Max(
                horses[i].baseData.startingPoints,
                horses[i].currentPoints - penalty
            );

            horseViews[i].GetComponent<HorseResultSpawner>()
                ?.ShowCustomText("-" + penalty, Color.red);

            AudioManager.Instance?.Play(
                uiSfx, ConstantManager.Sfx.Race.Negative
            );

            cameraPending = false;
            leaderPenaltyNextTurn = 0;

            yield return new WaitForSeconds(0.6f);
            horseViews[i].UpdatePositionSmooth();
        }

        // ================= PEPPER SPRAY =================
        if (!isPlayer && !isBoss && enemiesBlockedThisTurn > 0)
        {
            enemiesBlockedThisTurn--;

            horseViews[i].GetComponent<HorseResultSpawner>()
                ?.ShowCustomText("NOPE", Color.red);

            AudioManager.Instance?.Play(
                uiSfx, ConstantManager.Sfx.Race.Negative
            );

            yield return new WaitForSeconds(1f);
            continue;
        }

        // ================= RAT TRAP =================
        if (!isBoss && BlockLeaderNextTurn && i == GetLeaderIndex())
        {
            horseViews[i].GetComponent<HorseResultSpawner>()
                ?.ShowCustomText("TRAP", Color.red);

            AudioManager.Instance?.Play(
                uiSfx, ConstantManager.Sfx.Race.Negative
            );

            yield return new WaitForSeconds(1f);
            continue;
        }

        // ================= LOW BLOQUEADO =================
        if (isPlayer && shotToUse == ShotType.Low && lowDisabledThisRace)
        {
            horseViews[i].GetComponent<HorseResultSpawner>()
                ?.ShowCustomText("NOPE", Color.red);

            AudioManager.Instance?.Play(
                uiSfx, ConstantManager.Sfx.Race.Negative
            );

            yield return new WaitForSeconds(1f);
            continue;
        }

        // ================= TIRADA BASE =================
        int rollGain = isBoss
            ? ResolveBossRoll(shotToUse)
            : ShotResolver.ResolvePureLuck(shotToUse);

        bool swapConsumed = false;
        int baseGain = rollGain;

        // ================= PLAYER MODIFIERS =================
        if (isPlayer)
        {
            baseGain = ApplyPlayerShotModifiers(
                shotToUse,
                rollGain,
                out swapConsumed
            );
        }

        // ================= CARROT =================
        int carrotGain = 0;
        if (isPlayer && guaranteedBonusNextRoll > 0)
        {
            carrotGain = guaranteedBonusNextRoll;
            guaranteedBonusNextRoll = 0;
        }

        // ================= SWAP MID =================
        if (swapConsumed)
        {
            int hinder = midShotModifier;

            horseViews[playerIndex].GetComponent<HorseResultSpawner>()
                ?.ShowCustomText("-" + hinder, Color.white);

            AudioManager.Instance?.Play(
                uiSfx, ConstantManager.Sfx.Race.PopScore
            );

            yield return new WaitForSeconds(0.8f);

            for (int e = 0; e < horses.Length; e++)
            {
                if (e == playerIndex) continue;

                int maxBack =
                    horses[e].currentPoints -
                    horses[e].baseData.startingPoints;

                int applied = Mathf.Clamp(hinder, 0, maxBack);
                if (applied <= 0) continue;

                horses[e].currentPoints -= applied;

                horseViews[e].GetComponent<HorseResultSpawner>()
                    ?.ShowCustomText("-" + applied, Color.red);

                AudioManager.Instance?.Play(
                    uiSfx, ConstantManager.Sfx.Race.Negative
                );

                yield return new WaitForSeconds(0.25f);
                horseViews[e].UpdatePositionSmooth();
            }

            yield return new WaitForSeconds(0.25f);
        }

        // ================= EXTRA ROLL (PLAYER) =================
        if (isPlayer && playerExtraRolls > 0)
        {
            usedExtraRoll = true;

            var sp = horseViews[i].GetComponent<HorseResultSpawner>();
            sp?.ShowResult(baseGain);
            yield return new WaitForSeconds(0.8f);

            int second = ShotResolver.ResolvePureLuck(shotToUse);
            baseGain += second;
            playerExtraRolls--;

            sp?.ShowResult(second);
            yield return new WaitForSeconds(0.8f);
        }

        // ================= BOSS DOUBLE ROLL =================
        if (isBoss && UnityEngine.Random.value < 0.3f)
        {
            int extra = ResolveBossRoll(shotToUse);
            baseGain += extra;

            horseViews[i].GetComponent<HorseResultSpawner>()
                ?.ShowResult(extra);

            yield return new WaitForSeconds(0.6f);
        }

        // ================= BONUS =================
        int bonusGain = 0;
        if (isPlayer)
        {
            bonusGain += permanentAllShotsBonus;
            bonusGain += GetAllShotsBonusForCurrentRound();
            if (shotToUse == ShotType.Low)
                bonusGain += GetLowRiskBonusForCurrentRound();
        }

        int totalGain = baseGain + carrotGain + bonusGain;
        horses[i].currentPoints += totalGain;

        // ================= UI =================
        var spawner = horseViews[i].GetComponent<HorseResultSpawner>();
        AudioManager.Instance?.Play(
            uiSfx, ConstantManager.Sfx.Race.PopScore
        );

        if (!usedExtraRoll && !swapConsumed)
        {
            spawner?.ShowResult(baseGain);
            yield return new WaitForSeconds(0.8f);
        }

        if (isPlayer && carrotGain > 0)
        {
            spawner?.ShowBonusResult(carrotGain);
            yield return new WaitForSeconds(0.4f);
        }

        if (isPlayer && bonusGain > 0)
        {
            spawner?.ShowBonusResult(bonusGain);
            
            AudioManager.Instance?.Play(
                uiSfx,
                ConstantManager.Sfx.Race.BonusPoints
            );
            
            yield return new WaitForSeconds(0.4f);
        }

        horseViews[i].UpdatePositionSmooth();
        yield return new WaitForSeconds(0.25f);

        // ================= ENEMY DEBUFF PROGRAMADO =================
        if (!isPlayer && !isBoss && baseGain > 0)
        {
            int enemyPenalty = GetEnemyDebuffForCurrentRound();

            if (enemyPenalty > 0)
            {
                int maxBack =
                    horses[i].currentPoints - horses[i].baseData.startingPoints;

                int applied = Mathf.Clamp(enemyPenalty, 0, maxBack);

                if (applied > 0)
                {
                    horses[i].currentPoints -= applied;

                    horseViews[i].GetComponent<HorseResultSpawner>()
                        ?.ShowCustomText("-" + applied, Color.red);

                    AudioManager.Instance?.Play(
                        uiSfx,
                        ConstantManager.Sfx.Race.Negative
                    );

                    yield return new WaitForSeconds(0.35f);
                    horseViews[i].UpdatePositionSmooth();
                }
            }
        }
        
        // ================= BOSS PUNISH PLAYER =================
        if (
            isBoss &&
            horses[playerIndex].currentPoints >
            horses[playerIndex].baseData.startingPoints &&
            UnityEngine.Random.value < bossPunishChance
        )
        {
            int punish = 2;

            horses[playerIndex].currentPoints = Mathf.Max(
                horses[playerIndex].baseData.startingPoints,
                horses[playerIndex].currentPoints - punish
            );

            horseViews[playerIndex].GetComponent<HorseResultSpawner>()
                ?.ShowCustomText("-" + punish, Color.red);

            AudioManager.Instance?.Play(
                uiSfx, ConstantManager.Sfx.Race.Negative
            );

            yield return new WaitForSeconds(0.4f);
            horseViews[playerIndex].UpdatePositionSmooth();
        }

        // ================= CHECK WIN =================
        if (horses[i].currentPoints >= horses[i].baseData.pointsToWin)
        {
            bool playerWon = (i == playerIndex);

            if (playerWon)
            {
                int reward = GetMoneyRewardForRound(currentRound);
                EconomyManager.Instance.AddMoney(reward);
            }
            else
            {
                CheckLossConditions();
            }

            ConsumeRound();
            CleanupRoundEffects();
            ResetRace();
            yield break;
        }
    }

    resolvingTurn = false;
    EndTurn();
}


    
    void EndTurn()
    {
        resolvingTurn = false;

        // limpiar flags de turno
        BlockLeaderNextTurn = false;
        enemiesBlockedThisTurn = 0;
        
        StartNewRound();
    }
    
    /*
    IEnumerator ResolveTurn(ShotType playerShot)
    {
        int carrotBonus = 0;
        
        resolvingTurn = true;
        bool usedExtraRoll = false;

        for (int i = 0; i < horses.Length; i++)
        {
            ShotType shotToUse =
                (i == playerIndex)
                    ? playerShot
                    : AIShotChooser.ChooseShot();

            bool isPlayer = (i == playerIndex);
            
            // BLOQUEAR ENEMIGOS POR PEPPER SPRAY
            if (!isPlayer && enemiesBlockedThisTurn > 0)
            {
                enemiesBlockedThisTurn--;

                HorseResultSpawner spawnerBlocked =
                    horseViews[i].GetComponent<HorseResultSpawner>();

                if (spawnerBlocked != null)
                {
                    spawnerBlocked.ShowCustomText("NOPE", Color.red);
                }

                AudioManager.Instance?.Play(
                    uiSfx,
                    ConstantManager.Sfx.Race.Negative
                );

                yield return new WaitForSeconds(1f);
                continue;
            }

            
            if (BlockLeaderNextTurn)
            {
                int leaderIndex = GetLeaderIndex();

                if (i == leaderIndex)
                {
                    HorseResultSpawner spawneraBlock =
                        horseViews[i].GetComponent<HorseResultSpawner>();

                    if (spawneraBlock != null)
                    {
                        spawneraBlock.ShowCustomText("trap", Color.red);
                    }

                    AudioManager.Instance.Play(
                        uiSfx,
                        ConstantManager.Sfx.Race.Negative
                    );

                    yield return new WaitForSeconds(1f);
                    continue; 
                }
            }
            
            // BLOQUEAR LOW SHOT SI HAY VENTAJA INICIAL
            if (isPlayer &&
                shotToUse == ShotType.Low &&
                lowDisabledThisRace)
            {
                HorseResultSpawner spawner_nope =
                    horseViews[i].GetComponent<HorseResultSpawner>();

                if (spawner_nope != null)
                {
                    spawner_nope.ShowCustomText("NOPE", Color.red);
                    AudioManager.Instance.Play(uiSfx, ConstantManager.Sfx.Race.Negative);
                }

                yield return new WaitForSeconds(1f);
                yield return new WaitForSeconds(0.3f);
                continue;
            }

            // BASE (resultado puro)
            int rollGain = ShotResolver.ResolvePureLuck(shotToUse);
            int baseGain = rollGain;
            
            // APLICAR BONUS PERMANENTE POR TIPO DE TIRO (SOLO JUGADOR)
            if (isPlayer && rollGain > 0)
            {
                switch (shotToUse)
                {
                    case ShotType.High:
                        baseGain += highShotModifier;
                        break;

                    case ShotType.Medium:
                        baseGain += midShotModifier;
                        break;

                    case ShotType.Low:
                        baseGain += lowShotModifier;
                        break;
                }
            }
            
            if (isPlayer && swapMidActive && shotToUse == ShotType.Medium && rollGain > 0)
            {
                int hinder = rollGain + midShotModifier; 

                // 1) tú no avanzas
                baseGain = 0;

                // 2) muestra "-X" en BLANCO (no FAIL)
                HorseResultSpawner spPlayer = horseViews[i].GetComponent<HorseResultSpawner>();
                if (spPlayer != null)
                    spPlayer.ShowCustomText("-" + hinder, Color.white);

                AudioManager.Instance?.Play(uiSfx, ConstantManager.Sfx.Race.PopScore);
                yield return new WaitForSeconds(0.8f);

                // 3) aplica retroceso a TODOS los enemigos inmediatamente
                for (int e = 0; e < horses.Length; e++)
                {
                    if (e == playerIndex) continue;

                    int maxBack = horses[e].currentPoints - horses[e].baseData.startingPoints;
                    int applied = Mathf.Clamp(hinder, 0, maxBack);
                    if (applied <= 0) continue;

                    horses[e].currentPoints -= applied;

                    HorseResultSpawner spE = horseViews[e].GetComponent<HorseResultSpawner>();
                    if (spE != null)
                        spE.ShowCustomText("-" + applied, Color.red);

                    AudioManager.Instance?.Play(uiSfx, ConstantManager.Sfx.Race.Negative);

                    // para que se note el retroceso
                    yield return new WaitForSeconds(0.25f);
                    horseViews[e].UpdatePositionSmooth();
                }

                // un respiro y seguimos con el turno del jugador normal (pero sin avance)
                yield return new WaitForSeconds(0.25f);
            }
            
            // APLICAR ZANAHORIA (BONO GARANTIZADO)
            if (isPlayer && guaranteedBonusNextRoll > 0)
            {
                baseGain += guaranteedBonusNextRoll;

                HorseResultSpawner spawnerBonus =
                    horseViews[i].GetComponent<HorseResultSpawner>();

                if (spawnerBonus != null)
                {
                    spawnerBonus.ShowBonusResult(guaranteedBonusNextRoll);
                }

                guaranteedBonusNextRoll = 0; 
                yield return new WaitForSeconds(0.8f);
            }
            
            if (isPlayer && playerExtraRolls > 0)
            {
                usedExtraRoll = true;
                
                HorseResultSpawner spawnerExtra =
                    horseViews[i].GetComponent<HorseResultSpawner>();

                // mostrar PRIMER resultado
                if (spawnerExtra != null)
                {
                    spawnerExtra.ShowResult(baseGain);
                    yield return new WaitForSeconds(0.8f);
                }

                // segunda tirada REAL
                int secondGain = ShotResolver.ResolvePureLuck(shotToUse);
                baseGain += secondGain;
                playerExtraRolls--;

                if (spawnerExtra != null)
                {
                    spawnerExtra.ShowResult(secondGain);
                    yield return new WaitForSeconds(0.8f);
                }
            }
            
            
            bool enemyAdvancedThisTurn = !isPlayer && baseGain > 0;

            // PENALIZAR AL LÍDER EN SU TURNO (CAMERA)
            if (leaderPenaltyNextTurn > 0 && i == GetLeaderIndex())
            {
                horses[i].currentPoints =
                    Mathf.Max(
                        horses[i].baseData.startingPoints,
                        horses[i].currentPoints - leaderPenaltyNextTurn
                    );

                HorseResultSpawner spawnerPenalty =
                    horseViews[i].GetComponent<HorseResultSpawner>();

                if (spawnerPenalty != null)
                {
                    spawnerPenalty.ShowCustomText(
                        "-" + leaderPenaltyNextTurn,
                        Color.red
                    );
                }

                AudioManager.Instance?.Play(
                    uiSfx,
                    ConstantManager.Sfx.Race.Negative
                );

                leaderPenaltyNextTurn = 0;

                yield return new WaitForSeconds(0.35f);
                horseViews[i].UpdatePositionSmooth();
            }
            
            // BONUS (SOLO JUGADOR)
            int bonusGain = 0;

            if (isPlayer)
            {
                bonusGain += permanentAllShotsBonus;
                bonusGain += GetAllShotsBonusForCurrentRound();

                if (shotToUse == ShotType.Low)
                {
                    bonusGain += GetLowRiskBonusForCurrentRound();
                }
            }

            // OTAL REAL (SE SUMA SOLO UNA VEZ)
            int totalGain = baseGain + bonusGain;
            horses[i].currentPoints += totalGain;

            // MOSTRAR RESULTADO PRIMERO
            HorseResultSpawner spawner =
                horseViews[i].GetComponent<HorseResultSpawner>();
            if (AudioManager.Instance != null && uiSfx != null)
            {
                AudioManager.Instance.Play(uiSfx, ConstantManager.Sfx.Race.PopScore);
            }
            
            bool swapMidSuccess = (isPlayer && swapMidActive && shotToUse == ShotType.Medium && rollGain > 0);
            
            if (spawner != null)
            {
                if (!usedExtraRoll && !swapMidSuccess) 
                {
                    spawner.ShowResult(baseGain);
                }
                yield return new WaitForSeconds(0.8f);
                
                if (isPlayer && bonusGain > 0)
                {
                    spawner.ShowBonusResult(bonusGain);
                    AudioManager.Instance.Play(uiSfx, ConstantManager.Sfx.Race.BonusPoints);
                    yield return new WaitForSeconds(0.4f);
                }
            }

            // ESPERAR A QUE SE LEA EL RESULTADO
            yield return new WaitForSeconds(1f);
            
            horseViews[i].UpdatePositionSmooth();

            // espera pequeña para que se vea el avance
            yield return new WaitForSeconds(0.35f);

            // APLICAR DEBUFF DESPUÉS DEL AVANCE (ENEMIGOS)
            if (enemyAdvancedThisTurn)
            {
                int enemyPenalty = GetEnemyDebuffForCurrentRound();

                if (enemyPenalty > 0)
                {
                    int maxBack =
                        horses[i].currentPoints - horses[i].baseData.startingPoints;

                    int penaltyApplied =
                        Mathf.Clamp(enemyPenalty, 0, maxBack);

                    if (penaltyApplied > 0)
                    {
                        horses[i].currentPoints -= penaltyApplied;

                        HorseResultSpawner spawnerDebuff =
                            horseViews[i].GetComponent<HorseResultSpawner>();

                        if (spawnerDebuff != null)
                        {
                            spawnerDebuff.ShowCustomText(
                                "-" + penaltyApplied,
                                Color.red
                            );

                            if (AudioManager.Instance != null && uiSfx != null)
                            {
                                AudioManager.Instance.Play(
                                    uiSfx,
                                    ConstantManager.Sfx.Race.Negative
                                );
                            }
                        }

                        yield return new WaitForSeconds(0.35f);
                        horseViews[i].UpdatePositionSmooth();
                    }
                }
            }

            // respiro antes del siguiente caballo
            yield return new WaitForSeconds(0.25f);
            
        }

        // CHECAR GANADOR
        for (int i = 0; i < horses.Length; i++)
        {
            if (horses[i].currentPoints >= horses[i].baseData.pointsToWin)
            {
                bool playerWon = (i == playerIndex);

                if (playerWon)
                {
                    int baseReward = 10; 
                    int bonusReward = GetRewardBonusForCurrentRound();

                    int totalReward = baseReward;

                    Debug.Log($"PLAYER WINS! Reward: {totalReward}");

                    // Aquí sumas el dinero al jugador
                    int reward = GetMoneyRewardForRound(currentRound);
                    EconomyManager.Instance.AddMoney(reward);
                }
                else
                {
                    Debug.Log($"{horses[i].baseData.horseName} WINS");
                    
                    if (currentRound == lossRoundForPermanentAllShots)
                    {
                        permanentAllShotsBonus += 1;
                        lossRoundForPermanentAllShots = -1;

                        Debug.Log("Permanent All-Shots +1 unlocked FOREVER");
                    }
                    
                    CheckLossConditions();
                }
                ConsumeRound();
                Debug.Log($"Round {currentRound}");

                yield return new WaitForSeconds(1f);
                CleanupRoundEffects();
                BlockLeaderNextTurn = false;
                ResetRace();
                yield break;
            }
        }

        resolvingTurn = false;
        //currentRound++;
        
        StartNewRound();
    }
    */
    void ResetRace()
    {
        isBossRace = false;
        
        resolvingTurn = false;
        startingAdvantageThisRace = 0;
        lowDisabledThisRace = false;


        for (int i = 0; i < horses.Length; i++)
        {
            horses[i].currentPoints = horses[i].baseData.startingPoints;
            horseViews[i].UpdatePositionSmooth();
        }

        flowManager.ReturnToMainMenu();
    }

    #region Extra functions for events

    void StartNewRound()
    {
        if (currentRound == bossRound)
        {
            ActivateBossRace(); 
            Debug.Log($"Starting Round  {currentRound}");
        }
        
        Debug.Log($"Starting Round {currentRound}");
        updateIndicators.UpdateText();
        ApplyStartingBonuses();

        turnUI.Show();
    }
    void ApplyStartingBonuses()
    {
        // bonus de eventos programados
        int bonus = GetStartingPointsBonusForCurrentRound();

        // 🥤 SODA: bonus de inicio de carrera
        if (startingBonusNextRace > 0)
        {
            bonus += startingBonusNextRace;
            startingBonusNextRace = 0; // ⚠️ se consume aquí
        }

        // si el bonus total es grande, bloquear LOW
        if (bonus >= 5)
        {
            lowDisabledThisRace = true;
        }

        if (bonus > 0)
        {
            horses[playerIndex].currentPoints += bonus;

            HorseResultSpawner spawner =
                horseViews[playerIndex].GetComponent<HorseResultSpawner>();

            if (spawner != null)
            {
                spawner.ShowBonusResult(bonus);
            }

            StartCoroutine(MoveAfterDelay(playerIndex, 0.6f));
        }
    }
    void CheckLossConditions()
    {
        for (int i = scheduledLossConditions.Count - 1; i >= 0; i--)
        {
            var condition = scheduledLossConditions[i];

            if (condition.triggerRound == currentRound)
            {
                ScheduleLowRiskBonus(
                    condition.bonusTargetRound,
                    condition.bonusAmount
                );

                Debug.Log(
                    $"Loss condition met. Bonus scheduled for round {condition.bonusTargetRound}"
                );

                scheduledLossConditions.RemoveAt(i); 
            }
        }
    }
    
    public void SchedulePermanentAllShotsOnLoss(int targetRound)
    {
        lossRoundForPermanentAllShots = targetRound;
        Debug.Log($"☠️ Must lose round {targetRound} to unlock permanent +1");
    }

    
    #endregion
    
    #region Program Event
    public void ScheduleLowRiskBonus(int targetRound, int bonusAmount)
    {
        scheduledLowRiskBonuses.Add(new ScheduledLowRiskBonus
        {
            targetRound = targetRound,
            bonusAmount = bonusAmount
        });

        Debug.Log($"Low Risk bonus scheduled for round {targetRound} (+{bonusAmount})");
    }
    public void ScheduleAllShotsBonus(int targetRound, int bonusAmount)
    {
        scheduledAllShotsBonuses.Add(new ScheduledAllShotsBonus
        {
            targetRound = targetRound,
            bonusAmount = bonusAmount
        });

        Debug.Log($"All shots bonus scheduled for round {targetRound} (+{bonusAmount})");
    }
    public void ScheduleStartingPointsBonus(int targetRound, int bonusPoints)
    {
        scheduledStartingPointsBonuses.Add(new ScheduledStartingPointsBonus
        {
            targetRound = targetRound,
            bonusPoints = bonusPoints
        });

        Debug.Log($"tarting points bonus scheduled for round {targetRound} (+{bonusPoints})");
    }
    public void ScheduleEnemyDebuff(int targetRound, int failPenalty)
    {
        scheduledEnemyDebuffs.Add(new ScheduledEnemyDebuff
        {
            targetRound = targetRound,
            failPenalty = failPenalty
        });

        Debug.Log($"Enemy debuff scheduled for round {targetRound} (fail +{failPenalty})");
    }
    public void ScheduleRewardBonus(int targetRound, int extraReward)
    {
        scheduledRewardBonuses.Add(new ScheduledRewardBonus
        {
            targetRound = targetRound,
            extraReward = extraReward
        });

        Debug.Log($"📅 Reward bonus scheduled for round {targetRound} (+{extraReward})");
    }
    
    public void ScheduleLossCondition(int triggerRound, int roundsAhead, int bonusAmount)
    {
        scheduledLossConditions.Add(new ScheduledLossCondition
        {
            triggerRound = triggerRound,
            bonusTargetRound = triggerRound + roundsAhead,
            bonusAmount = bonusAmount
        });

        Debug.Log(
            $"📜 Loss condition set: lose round {triggerRound}, gain +{bonusAmount} at round {triggerRound + roundsAhead}"
        );
    }
    
    
    #endregion

    #region Get Events

    int GetLowRiskBonusForCurrentRound()
    {
        int totalBonus = 0;

        for (int i = 0; i < scheduledLowRiskBonuses.Count; i++)
        {
            if (scheduledLowRiskBonuses[i].targetRound == currentRound)
            {
                totalBonus += scheduledLowRiskBonuses[i].bonusAmount;
            }
        }

        return totalBonus;
    }
    int GetAllShotsBonusForCurrentRound()
    {
        int totalBonus = 0;

        for (int i = 0; i < scheduledAllShotsBonuses.Count; i++)
        {
            if (scheduledAllShotsBonuses[i].targetRound == currentRound)
            {
                totalBonus += scheduledAllShotsBonuses[i].bonusAmount;
            }
        }

        return totalBonus;
    }
    int GetStartingPointsBonusForCurrentRound()
    {
        int bonus = 0;

        for (int i = scheduledStartingPointsBonuses.Count - 1; i >= 0; i--)
        {
            if (scheduledStartingPointsBonuses[i].targetRound == currentRound)
            {
                bonus += scheduledStartingPointsBonuses[i].bonusPoints;
                scheduledStartingPointsBonuses.RemoveAt(i);
            }
        }

        return bonus;
    }
    int GetEnemyDebuffForCurrentRound()
    {
        int penalty = 0;

        for (int i = 0; i < scheduledEnemyDebuffs.Count; i++)
        {
            if (scheduledEnemyDebuffs[i].targetRound == currentRound)
            {
                penalty += scheduledEnemyDebuffs[i].failPenalty;
            }
        }

        return penalty;
    }
    int GetRewardBonusForCurrentRound()
    {
        int bonus = 0;

        for (int i = scheduledRewardBonuses.Count - 1; i >= 0; i--)
        {
            if (scheduledRewardBonuses[i].targetRound == currentRound)
            {
                bonus += scheduledRewardBonuses[i].extraReward;
                scheduledRewardBonuses.RemoveAt(i);
            }
        }

        return bonus;
    }
    int GetLeaderIndex()
    {
        int leader = 0;
        int maxPoints = horses[0].currentPoints;

        for (int i = 1; i < horses.Length; i++)
        {
            if (horses[i].currentPoints > maxPoints)
            {
                maxPoints = horses[i].currentPoints;
                leader = i;
            }
        }

        return leader;
    }

    #endregion
    
    void CleanupRoundEffects()
    {
        scheduledLowRiskBonuses.RemoveAll(b => b.targetRound == currentRound);
        scheduledAllShotsBonuses.RemoveAll(b => b.targetRound == currentRound);
        scheduledEnemyDebuffs.RemoveAll(b => b.targetRound == currentRound);
        scheduledStartingPointsBonuses.RemoveAll(b => b.targetRound == currentRound);
        scheduledRewardBonuses.RemoveAll(b => b.targetRound == currentRound);
    }
    
    IEnumerator MoveAfterDelay(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        horseViews[index].UpdatePositionSmooth();
    }
    
    public void BlockAllEnemiesNextTurn()
    {
        enemiesBlockedThisTurn = horseViews.Length - 1;
    }
    
    int GetMoneyRewardForRound(int round)
    {
        // Normalizamos la ronda (0 → 1)
        float t = Mathf.Clamp01(round / 15f);

        // Curva suave de progresión
        float curve = Mathf.Pow(t, 1.5f);

        int baseReward = 5;
        int scaledReward = Mathf.RoundToInt(curve * 25);

        int reward = baseReward + scaledReward;

        // Spike cada 5 rondas
        if (round % 5 == 0)
            reward += 5;

        return reward;
    }
    
    void ActivateBossRace()
    {
        isBossRace = true;

        // ampliar pista
        foreach (var horse in horses)
        {
            horse.baseData.pointsToWin = 30;
        }

        Debug.Log("BOSS RACE ACTIVATED");
    }

    ShotType ChooseBossShot()
    {
        int r = UnityEngine.Random.Range(0, 100);
        if (r < 50) return ShotType.High;
        if (r < 80) return ShotType.Medium;
        return ShotType.Low;
    }

    int ResolveBossRoll(ShotType shot)
    {
        switch (shot)
        {
            case ShotType.High:   return UnityEngine.Random.Range(3, 6); 
            case ShotType.Medium: return UnityEngine.Random.Range(2, 4); 
            case ShotType.Low:    return UnityEngine.Random.Range(1, 3);
            default: return 0;
        }
    }
}
