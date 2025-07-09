#region

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

#endregion

namespace nadena.dev.modular_avatar.core
{
    [Serializable]
    public struct ParameterConfig
    {
        internal const float VALUE_EPSILON = 0.000001f;
        
        public string nameOrPrefix;
        public string remapTo;
        public bool internalParameter, isPrefix;
        public ParameterSyncType syncType;
        public bool localOnly;

        public float defaultValue;
        public bool saved;

        public bool hasExplicitDefaultValue;

        /// <summary>
        /// Indicates that the default value for this parameter should be applied to any animators attached to the
        /// avatar as well, rather than just the expressions menu configuration.
        ///
        /// Note: Private API for now; will be exposed in 1.10. This is always considered to be true if the parameter
        /// is unsynced and has a default value override.
        /// </summary>
        [SerializeField]
        internal bool m_overrideAnimatorDefaults;

        internal bool OverrideAnimatorDefaults
        {
            get => m_overrideAnimatorDefaults || syncType == ParameterSyncType.NotSynced && HasDefaultValue;
            set => m_overrideAnimatorDefaults = value;
        }

        public bool HasDefaultValue => hasExplicitDefaultValue || Mathf.Abs(defaultValue) > VALUE_EPSILON;
    }

    /**
     * This enum is a bit poorly named, having been introduced before local-only parameters were a thing. In actuality,
     * this is the parameter type - NotSynced indicates the parameter should not be registered in Expression Parameters.
     */
    public enum ParameterSyncType
    {
        NotSynced,
        Int,
        Float,
        Bool,
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("Modular Avatar/MA Parameters")]
    [HelpURL("https://modular-avatar.nadena.dev/docs/reference/parameters?lang=auto")]
    public class ModularAvatarParameters : AvatarTagComponent
    {
        public List<ParameterConfig> parameters = new List<ParameterConfig>();
        [CanBeNull] public VRCExpressionParameters vrcParameters = null;
        
        private new void OnValidate()
        {
            base.OnValidate();
            
            if (vrcParameters != null)
            {
                this.ImportValues();
            }
        }

        public void ImportValues()
        {
#if UNITY_EDITOR
            var source = this.vrcParameters;
            if (source == null)
            {
                return;
            }
            
            RuntimeUtil.InvalidateMenu();

            var newParameters = new List<ParameterConfig>();
            
            foreach (var parameter in source.parameters)
            {
                ParameterSyncType pst;

                switch (parameter.valueType)
                {
                    case VRCExpressionParameters.ValueType.Bool: pst = ParameterSyncType.Bool; break;
                    case VRCExpressionParameters.ValueType.Float: pst = ParameterSyncType.Float; break;
                    case VRCExpressionParameters.ValueType.Int: pst = ParameterSyncType.Int; break;
                    default: pst = ParameterSyncType.Float; break;
                }

                newParameters.Add(new ParameterConfig()
                {
                    internalParameter = false,
                    nameOrPrefix = parameter.name,
                    isPrefix = false,
                    remapTo = "",
                    syncType = pst,
                    localOnly = !parameter.networkSynced,
                    defaultValue = parameter.defaultValue,
                    saved = parameter.saved,
                });
            }
            
            this.parameters = newParameters;
#endif
        }
        
        public override void ResolveReferences()
        {
            // no-op
        }
    }
}