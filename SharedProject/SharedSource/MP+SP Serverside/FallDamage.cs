
using Barotrauma.LuaCs.Events;
using static Neurotrauma.HF;
using static Neurotrauma.NTC;

namespace Neurotrauma
{
    public static class NTFallDamage
    {
        public static void OnChangeFallDamage(float impactDamage, Character character, Vector2 impactPos, Vector2 velocity)
        {
            Print("Fall Damage");
        }

    }
}
