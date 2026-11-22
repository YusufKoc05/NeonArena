using System.Collections.Generic;
using UnityEngine;

namespace NeonArena
{
    /// <summary>
    /// Glowing cyberpunk XP crystal dropped by defeated robots.
    /// Magnetizes smoothly into the player when within PickupRadius or when SuperMagnet triggers.
    /// </summary>
    public class XpGem : MonoBehaviour
    {
        public static readonly List<XpGem> ActiveGems = new List<XpGem>();

        public int XpValue = 10;
        private bool _magnetized;
        private float _magnetSpeed = 12f;
        private float _pulseOffset;

        public static void MagnetizeAll()
        {
            for (int i = 0; i < ActiveGems.Count; i++)
            {
                if (ActiveGems[i] != null)
                {
                    ActiveGems[i]._magnetized = true;
                    ActiveGems[i]._magnetSpeed = 20f;
                }
            }
        }

        private void OnEnable() => ActiveGems.Add(this);
        private void OnDisable() => ActiveGems.Remove(this);

        private void Start()
        {
            _pulseOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            var player = PlayerController.Instance;
            if (player == null || !player.IsAlive) return;

            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (!_magnetized && dist <= player.PickupRadius)
            {
                _magnetized = true;
            }

            if (_magnetized)
            {
                _magnetSpeed += 28f * Time.deltaTime;
                transform.position = Vector2.MoveTowards(transform.position, player.transform.position, _magnetSpeed * Time.deltaTime);
                if (dist < 0.45f)
                {
                    player.AddXp(XpValue);
                    NeonBurstEffect.Spawn(transform.position, new Color(0.15f, 1f, 0.7f), 4, 3.2f, 0.14f);
                    Destroy(gameObject);
                }
            }
            else
            {
                float s = 0.5f + Mathf.Sin(Time.time * 5f + _pulseOffset) * 0.08f;
                transform.localScale = Vector3.one * s;
            }
        }
    }
}
