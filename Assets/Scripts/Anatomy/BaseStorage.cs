using System;
using System.Collections.Generic;
using UnityEngine;

namespace OffWorld.Anatomy
{
    /// <summary>
    /// Wraps a stored graft part with quality metadata.
    /// </summary>
    [Serializable]
    public struct StoredPart
    {
        public GraftPartSO part;
        public bool isDegraded;
    }

    /// <summary>
    /// Singleton storage for graft parts and DNA samples collected in the field.
    /// Items are added via drone pickup and consumed at base stations
    /// (SurgeryTable and ExtractionChamber). Not accessible from the field.
    /// </summary>
    public class BaseStorage : MonoBehaviour
    {
        public static BaseStorage Instance { get; private set; }

        [SerializeField] private List<StoredPart> storedParts = new List<StoredPart>();
        [SerializeField] private List<DNASampleSO> storedSamples = new List<DNASampleSO>();

        public event Action OnStorageChanged;

        public IReadOnlyList<StoredPart> StoredParts => storedParts;
        public IReadOnlyList<DNASampleSO> StoredSamples => storedSamples;
        public int PartCount => storedParts.Count;
        public int SampleCount => storedSamples.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AddPart(GraftPartSO part, bool degraded = false)
        {
            if (part == null) return;
            storedParts.Add(new StoredPart { part = part, isDegraded = degraded });
            OnStorageChanged?.Invoke();
        }

        public void AddSample(DNASampleSO sample)
        {
            if (sample == null) return;
            storedSamples.Add(sample);
            OnStorageChanged?.Invoke();
        }

        public bool RemovePart(StoredPart entry)
        {
            bool removed = storedParts.Remove(entry);
            if (removed) OnStorageChanged?.Invoke();
            return removed;
        }

        public bool RemoveSample(DNASampleSO sample)
        {
            bool removed = storedSamples.Remove(sample);
            if (removed) OnStorageChanged?.Invoke();
            return removed;
        }

        /// <summary>Get all stored part entries that fit a specific body slot.</summary>
        public List<StoredPart> GetPartsBySlot(BodySlot slot)
        {
            return storedParts.FindAll(p => p.part != null && p.part.slot == slot);
        }
    }
}
