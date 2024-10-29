using UnityEngine;
using System.Globalization;

namespace InfimaGames.LowPolyShooterPack.Interface
{
    public class TextAmmunitionCurrent : ElementText
    {
        [Header("Colors")]
        [SerializeField]
        private bool updateColor = true;
        [SerializeField]
        private float emptySpeed = 1.5f;
        [SerializeField]
        private Color emptyColor = Color.red;

        protected override void Tick()
        {
            // Assuming equippedWeapon is a reference to Weapon script.
            int current = equippedWeapon.GetAmmunitionCurrent();
            int total = equippedWeapon.GetAmmunitionTotal();

            textMesh.text = current.ToString(CultureInfo.InvariantCulture);

            if (updateColor)
            {
                float colorAlpha = (float)current / total * emptySpeed;
                textMesh.color = Color.Lerp(emptyColor, Color.white, colorAlpha);   
            }
        }
    }
}
