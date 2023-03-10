using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace Polyjam2023
{
    public class Field : ICardLocation
    {
        private List<UnitInstance> enemyUnitsPresent  = new ();
        private List<UnitInstance> playerUnitsPresent  = new ();

        public event System.Action OnWrathOfTheForest;
        public event System.Action<UnitInstance> OnUnitAdded;
        public event System.Action<UnitInstance> OnUnitWounded;
        public event System.Action<UnitInstance> OnUnitKilled;

        public bool WrathOfTheForestEnabled = true;
        public IReadOnlyList<UnitInstance> EnemyUnitsPresent => enemyUnitsPresent;
        public IReadOnlyList<UnitInstance> PlayerUnitsPresent => playerUnitsPresent;
        
        public void AddUnit(UnitInstance newUnitInstance)
        {
            if (newUnitInstance.UnitCardTemplate.Ownership == Ownership.Enemy)
            {
                enemyUnitsPresent.Add(newUnitInstance);
            }
            else
            {
                playerUnitsPresent.Add(newUnitInstance);
            }
            OnUnitAdded?.Invoke(newUnitInstance);
        }

        public void ResolveFieldCombat()
        {
            var phases = new List<(int number, HashSet<UnitInstance> participants)>();

            #region EnemyUnitsDataGathering
            foreach (var enemyUnit in enemyUnitsPresent)
            {
                bool phaseNotFound = true;
                foreach (var phase in phases)
                {
                    if (phase.number == enemyUnit.currentInitiative)
                    {
                        phase.participants.Add(enemyUnit);
                        phaseNotFound = false;
                        break;
                    }
                }

                if (phaseNotFound)
                {
                    phases.Add((enemyUnit.currentInitiative, new HashSet<UnitInstance>{enemyUnit}));
                }
            }
            #endregion
            
            #region PlayerUnitsDataGathering
            foreach (var playerUnit in playerUnitsPresent)
            {
                bool phaseNotFound = true;
                foreach (var phase in phases)
                {
                    if (phase.number == playerUnit.currentInitiative)
                    {
                        phase.participants.Add(playerUnit);
                        phaseNotFound = false;
                        break;
                    }
                }

                if (phaseNotFound)
                {
                    phases.Add((playerUnit.currentInitiative, new HashSet<UnitInstance>{playerUnit}));
                }
            }
            #endregion

            phases = phases.OrderByDescending(phase => phase.number).ToList();

            var unitsKilled = new HashSet<UnitInstance>();
            foreach (var phase in phases)
            {
                int enemyAttackPotential = phase.participants.Where(participant =>
                    participant.UnitCardTemplate.Ownership == Ownership.Enemy && !unitsKilled.Contains(participant)).
                    Sum(enemyParticipant => enemyParticipant.currentAttack);
                int playerAttackPotential = phase.participants.Where(participant =>
                    participant.UnitCardTemplate.Ownership == Ownership.Player && !unitsKilled.Contains(participant)).
                    Sum(playerParticipant => playerParticipant.currentAttack);

                #region WoundingPlayerUnits
                while (enemyAttackPotential > 0 && playerUnitsPresent.Count > 0)
                {
                    var targetPlayerUnit = playerUnitsPresent.First();
                    if (targetPlayerUnit.currentHealth <= enemyAttackPotential)
                    {
                        enemyAttackPotential -= targetPlayerUnit.currentHealth;
                        targetPlayerUnit.currentHealth = 0;
                        playerUnitsPresent.Remove(targetPlayerUnit);
                        unitsKilled.Add(targetPlayerUnit);
                        OnUnitKilled?.Invoke(targetPlayerUnit);
                    }
                    else
                    {
                        targetPlayerUnit.currentHealth -= enemyAttackPotential;
                        enemyAttackPotential = 0;
                        OnUnitWounded?.Invoke(targetPlayerUnit);
                    }
                }
                #endregion

                #region WoundingEnemyUnits
                while (playerAttackPotential > 0 && enemyUnitsPresent.Count > 0)
                {
                    var targetEnemyUnit = enemyUnitsPresent.FirstOrDefault(enemy => enemy.UnitCardTemplate.CardName != EnemyManager.RootOfEvilName) ??
                                          enemyUnitsPresent.First();

                    if (targetEnemyUnit.currentHealth <= playerAttackPotential)
                    {
                        playerAttackPotential -= targetEnemyUnit.currentHealth;
                        targetEnemyUnit.currentHealth = 0;
                        enemyUnitsPresent.Remove(targetEnemyUnit);
                        unitsKilled.Add(targetEnemyUnit);
                        OnUnitKilled?.Invoke(targetEnemyUnit);
                    }
                    else
                    {
                        targetEnemyUnit.currentHealth -= playerAttackPotential;
                        playerAttackPotential = 0;
                        OnUnitWounded?.Invoke(targetEnemyUnit);
                    }
                }
                #endregion

                if (enemyUnitsPresent.Count == 0 || playerUnitsPresent.Count == 0)
                {
                    break;
                }
            }
            
            if (WrathOfTheForestEnabled && 
                enemyUnitsPresent.Count == 0 && 
                playerUnitsPresent.Count > 0)
            {
                playerUnitsPresent.Clear();
                OnWrathOfTheForest?.Invoke();
            }
        }
    }
}