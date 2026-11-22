using UnityEngine;

namespace NeonArena
{
    /// <summary>
    /// Spawns level-scaled sector robots and summons each Sector's high-level Final Boss
    /// once the sector kill quota is reached.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        public Vector2 ArenaHalfBounds = new Vector2(24f, 16f);
        public int MaxActiveEnemies = 75;

        private float _spawnTimer;

        private void Update()
        {
            var gm = GameManager.Instance;
            var player = PlayerController.Instance;
            if (gm == null || !gm.IsPlaying || player == null || !player.IsAlive) return;

            // Check if Sector quota reached -> Spawn Final Boss!
            if (gm.ShouldSpawnBoss)
            {
                Vector3 bossPos = GetSpawnPositionOutsideView(player.transform.position, 10.5f, 12.5f);
                var boss = BossAI.SpawnBoss(gm.CurrentSector, bossPos, player.Level);
                gm.OnBossSpawned(boss);
                return;
            }

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f) return;

            int sector = gm.CurrentSector;
            bool bossFight = gm.IsBossActive;

            // Slower minion trickle during Boss fight so focus stays on the Boss
            float interval = bossFight
                ? Mathf.Max(2.2f, 3.8f - sector * 0.35f)
                : Mathf.Max(0.35f, 1.3f - (sector - 1) * 0.18f);
            _spawnTimer = interval;

            int cap = bossFight ? 14 : MaxActiveEnemies;
            if (EnemyAI.ActiveEnemies.Count >= cap) return;

            int batchCount = bossFight ? 1 : (1 + sector / 2);
            for (int i = 0; i < batchCount; i++)
            {
                if (EnemyAI.ActiveEnemies.Count >= cap) break;
                Vector3 spawnPos = GetSpawnPositionOutsideView(player.transform.position, 11f, 14.5f);
                RobotType type = PickRobotType(sector, gm.SectorProgressRatio);
                int enemyLevel = CalculateEnemyLevel(sector, gm.SectorProgressRatio);
                EnemyAI.Spawn(type, spawnPos, enemyLevel);
            }
        }

        private static int CalculateEnemyLevel(int sector, float progress)
        {
            int minLv = 1 + (sector - 1) * 4;
            int maxLv = minLv + 2 + Mathf.RoundToInt(progress * 2f);
            return Random.Range(minLv, maxLv + 1);
        }

        private static RobotType PickRobotType(int sector, float progress)
        {
            float roll = Random.value;

            if (sector == 1)
            {
                if (progress > 0.45f && roll < 0.35f) return RobotType.Swarmer;
                if (progress > 0.72f && roll < 0.50f) return RobotType.Tank;
                return RobotType.Drone;
            }

            if (sector == 2)
            {
                if (roll < 0.24f) return RobotType.Sniper;
                if (roll < 0.42f) return RobotType.Tank;
                if (roll < 0.70f) return RobotType.Swarmer;
                return RobotType.Drone;
            }

            // Sector 3+: All 5 robot classes active!
            if (roll < 0.22f) return RobotType.Splitter;
            if (roll < 0.44f) return RobotType.Sniper;
            if (roll < 0.62f) return RobotType.Tank;
            if (roll < 0.82f) return RobotType.Swarmer;
            return RobotType.Drone;
        }

        private Vector3 GetSpawnPositionOutsideView(Vector3 center, float minDist, float maxDist)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(minDist, maxDist);
            float x = Mathf.Clamp(center.x + Mathf.Cos(angle) * dist, -ArenaHalfBounds.x + 1.5f, ArenaHalfBounds.x - 1.5f);
            float y = Mathf.Clamp(center.y + Mathf.Sin(angle) * dist, -ArenaHalfBounds.y + 1.5f, ArenaHalfBounds.y - 1.5f);
            return new Vector3(x, y, 0f);
        }
    }
}
