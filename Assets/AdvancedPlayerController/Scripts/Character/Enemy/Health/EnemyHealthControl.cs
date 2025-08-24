using Spiderman.Health;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spiderman.Health
{
    public class EnemyHealthControl : CharacterHealthBase
    {
        protected override void CharacterHitAction(float damage, string hitName, string parrName)
        {
            // 1.先判断角色的耐力值是否大于0，大于0就应该是格挡而不是直接受伤

            if (damage < 30f)
            {
                // 不是破防动作：进行格挡或者闪避。

                // 是破防动作：播放受伤动画
                _animator.Play(hitName, layer: 0, normalizedTime: 0f);

                // 播放音效
                GamePoolManager.Instance.TryGetPoolItem(
                    name: "HitSound",
                    transform.position,
                    Quaternion.identity
                );
            }
            else
            {
                //// 是破防动作：播放受伤动画
                //_animator.Play(hitName, layer: 0, normalizedTime: 0f);

                //// 播放音效
                //GamePoolManager.MainInstance.TryGetPoolItem(
                //    name: "HitSound",
                //    transform.position,
                //    Quaternion.identity
                //);
            }
        }

    }

}