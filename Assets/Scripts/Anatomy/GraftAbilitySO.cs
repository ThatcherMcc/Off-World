using UnityEngine;

namespace OffWorld.Anatomy
{
    /// <summary>
    /// Data definition for an active ability granted by a grafted body part.
    /// The actual runtime logic lives in a MonoBehaviour component added to the player.
    /// </summary>
    [CreateAssetMenu(fileName = "NewGraftAbility", menuName = "Off-World/Graft Ability")]
    public class GraftAbilitySO : ScriptableObject
    {
        public string abilityName;
        [TextArea] public string description;
        public float cooldown;
        public Sprite icon;

        [Tooltip("Full class name of the MonoBehaviour that implements this ability's logic.")]
        public string abilityComponentType;
    }
}
