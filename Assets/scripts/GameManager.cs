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
    
    #endregion
    
    public SoundLibrary uiSfx;
    public GameFlowManager flowManager;
    private HorseRuntime[] horses;
    private int playerIndex = -1;
    private bool resolvingTurn = false;
    public bool HasRaceInitialized { get; private set; }
    private int lossRoundForPermanentAllShots = -1;

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
        Debug.Log($"🔄 Round {currentRound}");
        round_effect.PlayFeedbacks();
    }

    public void UpdateTextRound()
    {
        roundtxt.text = currentRound.ToString() + "/15";
    }

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
    public void PlayerChooseShot(ShotType shot)
    {
        if (resolvingTurn) return;

        turnUI.Hide();
        StartCoroutine(ResolveTurn(shot));
    }
    IEnumerator ResolveTurn(ShotType playerShot)
    {
        resolvingTurn = true;

        for (int i = 0; i < horses.Length; i++)
        {
            ShotType shotToUse =
                (i == playerIndex)
                    ? playerShot
                    : AIShotChooser.ChooseShot();

            bool isPlayer = (i == playerIndex);

// BASE (resultado puro)
            int baseGain = ShotResolver.ResolvePureLuck(shotToUse);

// DEBUFF A ENEMIGOS
            if (!isPlayer)
            {
                int enemyPenalty = GetEnemyDebuffForCurrentRound();
                if (enemyPenalty > 0 && baseGain > 0)
                {
                    baseGain = Mathf.Max(0, baseGain - enemyPenalty);
                }
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
            if (spawner != null)
            {
                spawner.ShowResult(baseGain);
                yield return new WaitForSeconds(0.8f);

// MOSTRAR BONUS SEPARADO (AZUL)
                if (isPlayer && bonusGain > 0)
                {
                    spawner.ShowBonusResult(bonusGain);
                    AudioManager.Instance.Play(uiSfx, ConstantManager.Sfx.Race.BonusPoints);
                    yield return new WaitForSeconds(0.4f);
                }
            }

            // ESPERAR A QUE SE LEA EL RESULTADO
            yield return new WaitForSeconds(1f);

            // AHORA MOVER EL CABALLO
            horseViews[i].UpdatePositionSmooth();

            // PEQUEÑO RESPIRO ANTES DEL SIGUIENTE
            yield return new WaitForSeconds(0.3f);
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
                    // EconomyManager.Instance.AddMoney(totalReward);
                }
                else
                {
                    Debug.Log($"{horses[i].baseData.horseName} WINS");
                    
                    if (currentRound == lossRoundForPermanentAllShots)
                    {
                        permanentAllShotsBonus += 1;
                        lossRoundForPermanentAllShots = -1;

                        Debug.Log("🔥 Permanent All-Shots +1 unlocked FOREVER");
                    }
                    
                    CheckLossConditions();
                }
                ConsumeRound();
                Debug.Log($"Round {currentRound}");

                yield return new WaitForSeconds(1f);
                CleanupRoundEffects();
                ResetRace();
                yield break;
            }
        }

        resolvingTurn = false;
        //currentRound++;
        StartNewRound();
    }
    void ResetRace()
    {
        resolvingTurn = false;

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
        Debug.Log($"Starting Round {currentRound}");

        ApplyStartingBonuses();

        turnUI.Show();
    }
    void ApplyStartingBonuses()
    {
        int bonus = GetStartingPointsBonusForCurrentRound();

        if (bonus > 0)
        {
            horses[playerIndex].currentPoints += bonus;
            horseViews[playerIndex].UpdatePositionSmooth();
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

        for (int i = scheduledEnemyDebuffs.Count - 1; i >= 0; i--)
        {
            if (scheduledEnemyDebuffs[i].targetRound == currentRound)
            {
                penalty += scheduledEnemyDebuffs[i].failPenalty;
                scheduledEnemyDebuffs.RemoveAt(i);
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

    #endregion
    
    void CleanupRoundEffects()
    {
        scheduledLowRiskBonuses.RemoveAll(b => b.targetRound == currentRound);
        scheduledAllShotsBonuses.RemoveAll(b => b.targetRound == currentRound);
        scheduledEnemyDebuffs.RemoveAll(b => b.targetRound == currentRound);
        scheduledStartingPointsBonuses.RemoveAll(b => b.targetRound == currentRound);
        scheduledRewardBonuses.RemoveAll(b => b.targetRound == currentRound);
    }






}
