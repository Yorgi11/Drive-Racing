using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace QF_Tools.QF_SmartOBJs
{
    public enum DamageType
    {
        Health
    }
    public class QF_DamageableObject : MonoBehaviour
    {
        [SerializeField] private bool m_spawnHpBar;
        [SerializeField] private float m_maxHp;
        [SerializeField] private Slider m_hpSlider;
        [SerializeField] private GameObject m_hpBarPrefab;
        public DamageType m_damageType;
        public float CurrentHp;
        protected virtual void Start()
        {
            CurrentHp = m_maxHp;
            if (m_spawnHpBar) m_hpSlider = Instantiate(m_hpBarPrefab, transform).GetComponentInChildren<Slider>();
            if (m_hpSlider)
            {
                m_hpSlider.maxValue = m_maxHp;
                m_hpSlider.value = CurrentHp;
            }
            if (m_spawnHpBar) m_hpSlider.gameObject.SetActive(false);
        }
        public IEnumerator RegenAmount(float amount, float time)
        {
            float t = 0f;
            float a = amount / time;
            while (t < time)
            {
                t += Time.deltaTime;
                CurrentHp += a;
                if (m_hpSlider) m_hpSlider.value = CurrentHp;
                if (CurrentHp >= m_maxHp)
                {
                    CurrentHp = m_maxHp;
                    if (m_hpSlider)
                    {
                        m_hpSlider.value = CurrentHp;
                        m_hpSlider.gameObject.SetActive(false);
                    }
                    break;
                }
            }
            yield return null;
        }
        public void IncreaseHpByAmount(float amount)
        {
            CurrentHp += amount;
            if (CurrentHp >= m_maxHp)
            {
                CurrentHp = m_maxHp;
                if (m_hpSlider) m_hpSlider.gameObject.SetActive(false);
            }
            if (m_hpSlider) m_hpSlider.value = CurrentHp;
        }
        public bool TakeDamage(float damage)
        {
            CurrentHp -= damage;
            if (m_hpSlider)
            {
                m_hpSlider.value = CurrentHp;
                m_hpSlider.gameObject.SetActive(true);
            }
            if (CurrentHp <= 0f)
            {
                CurrentHp = 0f;
                if (m_hpSlider) m_hpSlider.value = CurrentHp;
                return true;
            }
            return false;
        }
    }
}