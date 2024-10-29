using UnityEngine;
using System.Globalization;

namespace InfimaGames.LowPolyShooterPack.Interface
{
    public class TextAmmunitionTotal : ElementText
    {
        protected override void Tick()
        {
            // Access total ammunition directly from equippedWeapon.
            int ammunitionTotal = equippedWeapon.GetAmmunitionTotal();
            
            textMesh.text = ammunitionTotal.ToString(CultureInfo.InvariantCulture);
        }
    }
}
