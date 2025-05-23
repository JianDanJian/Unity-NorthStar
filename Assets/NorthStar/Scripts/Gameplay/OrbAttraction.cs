// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using UnityEngine;

namespace NorthStar
{
    public class OrbAttraction : MonoBehaviour
    {
        //This could be simplified to having fewer variables to manage by using exact values in the animation curves, rather than using them as a multiplier
        //However scaling of animation curves in Unity Editor can be quite frustrating, so individual defined values for the min and max range has been found to
        //allow easier iteration on the effects without needing to change the whole curve.
        public AudioSource AudioSource;
        [Tooltip("Volume of audiosource, sampling curve using the orbAttraction value (0 to 1 range)")]
        public AnimationCurve AudioVolumeCurve;
        public Light Light;
        [Range(0f, 10f)]
        public float LightMinIntensity;
        [Range(0f, 100f)]
        public float LightMaxIntensity;
        [Tooltip("How the light should lerp between min and max intensity, sampling curve using the orbAttraction value")]
        public AnimationCurve LightIntensityCurve;
        [Range(0f, 20f)]
        public float LightMinRange;
        [Range(0f, 50f)]
        public float LightMaxRange;
        [Tooltip("How the light should lerp between min and max range, sampling curve using the orbAttraction value")]
        public AnimationCurve LightRangeCurve;
        public ParticleSystemForceField OrbForceField;
        [Range(0f, 0.1f)]
        public float OrbForceFieldMinForce = 0.025f;
        [Range(0f, 0.1f)]
        public float OrbForceFieldMaxForce = 0.06f;
        public AnimationCurve OrbForceFieldForceCurve;
        [Tooltip("Maximum speed the orb attraction can go up from 0 to 1 (per second value)")]
        public float AttractionWarmUpRate = 1f;
        private float m_attractionWarmUpRateScaled;
        [Tooltip("The maximum change value will be multiplied by this based on what the value currently is (so we can have quick warmup at start, and then gradually creep up as you hold the pose")]
        [SerializeField] private AnimationCurve m_attractionWarmUpCurve;
        [Tooltip("Maximum speed the orb attraction can go down from 1 to 0 (per second value)")]
        public float AttractionCooldownRate = 0.5f;
        //We might not need the curve control on cooldown rate, but if we decide to add it will go here
        private float m_orbAttraction;
        private float m_targetOrbAttraction;
        [SerializeField] private GameObject m_orbGlowMesh;
        [Tooltip("Smallest size of the glow surrounding the orb.  Should be just big enough to surround it")]
        [Range(1f, 30f)]
        [SerializeField] private float m_orbGlowMinSize = 1.1f;
        [Tooltip("Largest size of the glow surrounding the orb.  Should be just big enough to surround it")]
        [Range(1f, 30f)]
        [SerializeField] private float m_orbGlowMaxSize = 20f;
        [Tooltip("How the size of the orb glow will change in response to the attraction power")]
        [SerializeField] private AnimationCurve m_orbGlowSizeCurve;
        //to-do, variables to reduce the scale of the orb glow based on how far it is from the player (since the effect will fall apart if they go inside the particle mesh)

        private List<HandOrbEffects> m_handOrbEffectsList = new();

        public void AddHandToList(HandOrbEffects handOrbEffect)
        {
            m_handOrbEffectsList.Add(handOrbEffect);
        }

        public void RemoveHandFromList(HandOrbEffects handOrbEffect)
        {

            _ = m_handOrbEffectsList.Remove(handOrbEffect);
        }

        private void Update()
        {
            //For now doing all of this in update to avoid preemptive optimisation
            //If for whatever reason it has any significant cost can move to a coroutine that updates less regularly
            CalculateOrbAttraction();
            UpdateOrbEffects();
        }

        private void CalculateOrbAttraction()
        {
            //Calculate a scaled warmup speed based on the current value
            m_attractionWarmUpRateScaled = Mathf.Lerp(0, AttractionWarmUpRate, m_attractionWarmUpCurve.Evaluate(m_orbAttraction));
            //Get the hand power value from all active hands in the scene, then divide by the number to give us a 0 to 1 range
            m_targetOrbAttraction = 0f;
            foreach (var hand in m_handOrbEffectsList)
            {
                m_targetOrbAttraction += hand.HandPower;
            }
            m_targetOrbAttraction /= m_handOrbEffectsList.Count;

            if (m_orbAttraction < m_targetOrbAttraction)
            {
                m_orbAttraction += m_attractionWarmUpRateScaled * Time.deltaTime;
                if (m_orbAttraction > m_targetOrbAttraction)
                {
                    m_orbAttraction = m_targetOrbAttraction;
                }
            }
            else if (m_orbAttraction > m_targetOrbAttraction)
            {
                m_orbAttraction -= AttractionCooldownRate * Time.deltaTime;
                if (m_orbAttraction < m_targetOrbAttraction)
                {
                    m_orbAttraction = m_targetOrbAttraction;
                }
            }
        }
        private void UpdateOrbEffects()
        {
            //null checks every frame could be bad, need to check performance
            if (AudioSource != null)
            {
                AudioSource.volume = AudioVolumeCurve.Evaluate(m_orbAttraction);
            }
            if (Light != null)
            {
                Light.intensity = Mathf.Lerp(LightMinIntensity, LightMaxIntensity, LightIntensityCurve.Evaluate(m_orbAttraction));
                Light.range = Mathf.Lerp(LightMinRange, LightMaxRange, LightRangeCurve.Evaluate(m_orbAttraction));
            }
            if (OrbForceField != null)
            {
                OrbForceField.gravity = Mathf.Lerp(OrbForceFieldMinForce, OrbForceFieldMaxForce, OrbForceFieldForceCurve.Evaluate(m_orbAttraction));
            }
            if (m_orbGlowMesh != null)
            {
                m_orbGlowMesh.transform.localScale = Vector3.one * Mathf.Lerp(m_orbGlowMinSize, m_orbGlowMaxSize, m_orbGlowSizeCurve.Evaluate(m_orbAttraction));
            }
        }
    }
}
