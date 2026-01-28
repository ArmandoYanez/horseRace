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
    
    [Header("Boss Setup")]
    [SerializeField] private int bossIndex = 4;
    
    [Header("Mandatory Boss Battle UI")]
    [SerializeField] private GameObject shopButton;
    [SerializeField] private GameObject talkButton;
    [SerializeField] private GameObject RaceButton;
    [SerializeField] private MMFeedback BossMMF;

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
    
    [Header("Boss Swap Setup")]
    [SerializeField] private HorseView bossHorseView;   // el caballo boss (GameObject desactivado al inicio)
    [SerializeField] private int bossReplaceIndex = 2;  // caballo #3 => index 2 (0,1,2)
    [SerializeField] private bool hideOtherEnemiesDuringBoss = true;

    private HorseView originalHorseAtBossIndex;
    
    bool cameraPending = false;
    
    [Header("Enemy Progressive Behavior")]
    [Range(0f, 1f)] public float enemyFailChance = 0.2f;

    [Range(0f, 1f)] public float enemyPunishChanceRound10 = 0.08f;
    [Range(0f, 1f)] public float enemyPunishChanceRound15 = 0.15f;

    [Range(0f, 1f)] public float enemyDoubleRollChanceRound15 = 0.1f;
    
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
    
    void LockMandatoryBossUI()
    {
        if (RaceButton != null)
            RaceButton.SetActive(false);
        
        if (shopButton != null)
            shopButton.SetActive(false);

        if (talkButton != null)
            talkButton.SetActive(false);

        Debug.Log("⚠️ Boss Round: Shop y Talk desactivados");
    }
    
    public void ConsumeRound()
    {
        currentRound++;

        if (currentRound == bossRound)
        {
            LockMandatoryBossUI();
        }
        
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
        if (!horseViews[i].gameObject.activeInHierarchy)
            continue;
        
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
        if (!isPlayer && enemiesBlockedThisTurn > 0)
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
        if (BlockLeaderNextTurn && i == GetLeaderIndex())
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
                if (!horseViews[i].gameObject.activeInHierarchy)
                    continue;
                
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
        if (isBoss && UnityEngine.Random.value < 0.15f)
        {
            int extra = ResolveBossRoll(shotToUse);
            baseGain += extra;

            horseViews[i].GetComponent<HorseResultSpawner>()
                ?.ShowResult(extra);

            yield return new WaitForSeconds(0.6f);
        }
        
        // ================= ENEMY DOUBLE ROLL =================
        if (!isPlayer && !isBoss && EnemyCanDoubleRoll())
        {
            int extra = ShotResolver.ResolvePureLuck(shotToUse);
            baseGain += extra;

            horseViews[i].GetComponent<HorseResultSpawner>()
                ?.ShowResult(extra);

            yield return new WaitForSeconds(0.5f);
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
        if (!isPlayer && baseGain > 0)
        {
            int enemyPenalty = GetEnemyDebuffForCurrentRound();

            int maxBack =
                horses[i].currentPoints - horses[i].baseData.startingPoints;

            int applied = Mathf.Clamp(enemyPenalty, 0, maxBack);

            if (applied > 0)
            {
                horses[i].currentPoints -= applied;

                horseViews[i].GetComponent<HorseResultSpawner>()
                    ?.ShowCustomText("-" + applied, Color.red);

                AudioManager.Instance?.Play(
                    uiSfx, ConstantManager.Sfx.Race.Negative
                );

                yield return new WaitForSeconds(0.35f);
                horseViews[i].UpdatePositionSmooth();
            }
        }
        
        // ================= ENEMY PUNISH PLAYER =================
        if (
            !isPlayer &&
            !isBoss &&
            EnemyCanPunishPlayer() &&
            horses[playerIndex].currentPoints >
            horses[playerIndex].baseData.startingPoints
        )
        {
            int punish = 1;

            horses[playerIndex].currentPoints = Mathf.Max(
                horses[playerIndex].baseData.startingPoints,
                horses[playerIndex].currentPoints - punish
            );

            var playerSpawner =
                horseViews[playerIndex].GetComponent<HorseResultSpawner>();

            // Texto de intención (el enemigo anuncia la trampa)
            playerSpawner?.ShowCustomText("TRAP", new Color(1f, 0.6f, 0.2f));

            // Número de castigo
            playerSpawner?.ShowCustomText("-" + punish, Color.red);

            AudioManager.Instance?.Play(
                uiSfx, ConstantManager.Sfx.Race.Negative
            );

            yield return new WaitForSeconds(0.35f);
            horseViews[playerIndex].UpdatePositionSmooth();
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
    
    void ResetRace()
    {
        isBossRace = false;
        
        resolvingTurn = false;
        startingAdvantageThisRace = 0;
        lowDisabledThisRace = false;


        for (int i = 0; i < horses.Length; i++)
        {
            if (!horseViews[i].gameObject.activeInHierarchy)
                continue;
            
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
    
    void UpdateMandatoryBattleUI()
    {
        bool isMandatoryBossRound = currentRound == bossRound;
        
        if (isMandatoryBossRound)
        {
               RaceButton.GetComponent<TextMeshProUGUI>().text = "BOSS RACE";
               RaceButton.GetComponent<TextMeshProUGUI>().color = Color.red;
        }
        
        if (shopButton != null)
            shopButton.SetActive(!isMandatoryBossRound);

        if (talkButton != null)
            talkButton.SetActive(!isMandatoryBossRound);
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
            if (!horseViews[i].gameObject.activeInHierarchy)
                continue;
            
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
        if (r < 30) return ShotType.High;
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


    #region Helpers

    bool EnemyCanPunishPlayer()
    {
        if (currentRound < 10) return false;

        float chance =
            currentRound < 15
                ? enemyPunishChanceRound10
                : enemyPunishChanceRound15;

        return UnityEngine.Random.value < chance;
    }

    bool EnemyCanDoubleRoll()
    {
        if (currentRound < 15) return false;

        return UnityEngine.Random.value < enemyDoubleRollChanceRound15;
    }

    bool EnemyFails()
    {
        // Los enemigos normales SIEMPRE pueden fallar
        return UnityEngine.Random.value < enemyFailChance;
    }


    #endregion
    
    public void PrepareBossIntroScene()
    {
        // Solo hacer esto si realmente estamos en boss round
        if (currentRound < bossRound) return;

        // 1) Guardar el horse view original (caballo #3 normal)
        if (originalHorseAtBossIndex == null)
            originalHorseAtBossIndex = horseViews[bossReplaceIndex];

        // 2) Colocar el boss en la misma posición del caballo #3
        if (bossHorseView != null && originalHorseAtBossIndex != null)
        {
            bossHorseView.transform.position = originalHorseAtBossIndex.transform.position;
            bossHorseView.transform.rotation = originalHorseAtBossIndex.transform.rotation;
        }

        // 3) Apagar caballo #3 normal y prender boss
        if (originalHorseAtBossIndex != null)
            originalHorseAtBossIndex.gameObject.SetActive(false);

        if (bossHorseView != null)
            bossHorseView.gameObject.SetActive(true);

        // 4) Opcional: ocultar otros enemigos (pero NO el player)
        if (hideOtherEnemiesDuringBoss)
        {
            for (int i = 0; i < horseViews.Length; i++)
            {
                if (i == playerIndex) continue;
                if (i == bossReplaceIndex) continue;

                horseViews[i].gameObject.SetActive(false);
            }
        }

        // 5) Reemplazar referencia del array para que el sistema use al boss como "caballo 3"
        horseViews[bossReplaceIndex] = bossHorseView;

        // 6) Re-inicializar runtime del índice reemplazado para que tenga el SO del boss
        if (horses != null && bossHorseView != null)
        {
            horses[bossReplaceIndex] = new HorseRuntime(bossHorseView.horseData);
            bossHorseView.Initialize(horses[bossReplaceIndex]);
        }

        // 7) Marcar que es boss race y activar reglas
        ActivateBossRace();
    }
    
    public void ActivateBossOnlyRace()
    {
        if (!HasRaceInitialized)
        {
            Debug.Log("⚠️ Race not initialized yet, initializing now");
            InitRaceData();
        }

        if (bossIndex < 0 || bossIndex >= horseViews.Length)
        {
            Debug.LogError($"❌ BossIndex fuera de rango: {bossIndex}");
            return;
        }

        if (horses == null || horses.Length <= bossIndex)
        {
            Debug.LogError("❌ Horses array no inicializado o muy corto");
            return;
        }

        isBossRace = true;

        for (int i = 0; i < horseViews.Length; i++)
        {
            bool isPlayer = (i == playerIndex);
            bool isBoss = (i == bossIndex);

            horseViews[i].gameObject.SetActive(isPlayer || isBoss);
        }

        // Asegurar que el boss esté activo
        horseViews[bossIndex].gameObject.SetActive(true);

        // Re-inicializar runtime SOLO para los que corren
        horses[playerIndex] = new HorseRuntime(horseViews[playerIndex].horseData);
        horses[bossIndex]   = new HorseRuntime(horseViews[bossIndex].horseData);

        horseViews[playerIndex].Initialize(horses[playerIndex]);
        horseViews[bossIndex].Initialize(horses[bossIndex]);

        Debug.Log("🔥 Boss Only Race Activated (Player vs Boss)");
    }


}
