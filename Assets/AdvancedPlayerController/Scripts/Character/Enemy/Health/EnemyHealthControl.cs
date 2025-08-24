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
            // 1.���жϽ�ɫ������ֵ�Ƿ����0������0��Ӧ���Ǹ񵲶�����ֱ������

            if (damage < 30f)
            {
                // �����Ʒ����������и񵲻������ܡ�

                // ���Ʒ��������������˶���
                _animator.Play(hitName, layer: 0, normalizedTime: 0f);

                // ������Ч
                GamePoolManager.Instance.TryGetPoolItem(
                    name: "HitSound",
                    transform.position,
                    Quaternion.identity
                );
            }
            else
            {
                //// ���Ʒ��������������˶���
                //_animator.Play(hitName, layer: 0, normalizedTime: 0f);

                //// ������Ч
                //GamePoolManager.MainInstance.TryGetPoolItem(
                //    name: "HitSound",
                //    transform.position,
                //    Quaternion.identity
                //);
            }
        }

    }

}