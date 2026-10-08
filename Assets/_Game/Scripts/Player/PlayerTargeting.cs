using System.Collections.Generic;
using LootboundIsles.Combat;
using LootboundIsles.Enemies;
using UnityEngine;

namespace LootboundIsles.Player
{
    public class PlayerTargeting : MonoBehaviour
    {
        public Enemy FindNearestEnemy(float range)
        {
            Enemy nearestEnemy = null;
            float nearestDistanceSqr = range * range;

            foreach (Enemy enemy in Enemy.ActiveEnemies)
            {
                if (enemy == null)
                    continue;

                if (enemy.Health == null || enemy.Health.IsDead)
                    continue;

                Vector3 difference =
                    enemy.transform.position - transform.position;

                difference.y = 0f;

                float distanceSqr = difference.sqrMagnitude;

                if (distanceSqr > nearestDistanceSqr)
                    continue;

                nearestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }

            return nearestEnemy;
        }

        public void FindEnemiesInFront(
            float range,
            Vector3 attackDirection,
            float angle,
            int maxTargets,
            List<Enemy> results)
        {
            results.Clear();

            if (maxTargets <= 0)
                return;

            attackDirection.y = 0f;

            if (attackDirection.sqrMagnitude <= 0f)
                return;

            attackDirection.Normalize();

            float rangeSqr = range * range;
            float halfAngle = Mathf.Clamp(angle, 0f, 360f) * 0.5f;

            foreach (Enemy enemy in Enemy.ActiveEnemies)
            {
                if (enemy == null ||
                    enemy.Health == null ||
                    enemy.Health.IsDead)
                {
                    continue;
                }

                Vector3 difference =
                    enemy.transform.position - transform.position;

                difference.y = 0f;

                float distanceSqr = difference.sqrMagnitude;

                if (distanceSqr > rangeSqr ||
                    distanceSqr <= 0f)
                {
                    continue;
                }

                if (Vector3.Angle(attackDirection, difference) > halfAngle)
                    continue;

                InsertByDistance(
                    enemy,
                    distanceSqr,
                    maxTargets,
                    results
                );
            }
        }

        private void InsertByDistance(
            Enemy candidate,
            float candidateDistanceSqr,
            int maxTargets,
            List<Enemy> results)
        {
            int insertIndex = 0;

            while (insertIndex < results.Count)
            {
                Vector3 difference =
                    results[insertIndex].transform.position -
                    transform.position;

                difference.y = 0f;

                if (candidateDistanceSqr < difference.sqrMagnitude)
                    break;

                insertIndex++;
            }

            if (insertIndex >= maxTargets)
                return;

            results.Insert(insertIndex, candidate);

            if (results.Count > maxTargets)
            {
                results.RemoveAt(results.Count - 1);
            }
        }
    }
}
