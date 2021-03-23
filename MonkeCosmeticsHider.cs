using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using GorillaLocomotion;
using System.Linq;

namespace MonkeCosmeticsHider
{
    [BepInPlugin("org.auralius.monkeytag.cosmeticshider", "Monke Cosmetics Hider", "1.0.0.0")]
    [BepInProcess("Gorilla Tag.exe")]
    public class MonkePlugin : BaseUnityPlugin
    {
        private void Awake() => new Harmony("com.auralius.monkeytag.cosmeticshider").PatchAll(Assembly.GetExecutingAssembly());

        [HarmonyPatch(typeof(Player))]
        [HarmonyPatch("Update")]
        private class MonkeCosmeticsHider_Patch
        {
            private static readonly string[] lastCosmetics = new string[3];
            private static bool firstHide = false;

            private static void RehideCosmetics()
            {
                foreach (GameObject obj in FindObjectsOfType<GameObject>()) // Cycle through all game objects.
                {
                    if (obj.activeInHierarchy)
                    {
                        try
                        {
                            if (obj.transform.parent.name == "Cosmetics" || 
                            obj.transform.parent.transform.parent.name == "Cosmetics") // Getting the parent name as well because some cosmetics have a bow and they do weird stuff.
                            { 
                                if (!(obj.name.Contains("Button") || obj.name.Contains("Badge") || obj.name.Contains("Text"))) // Filter the cosmetic buttons and stuff.
                                {
                                    obj.layer = 4; // Set layer to 4.
                                    firstHide = true; // If a cosmetic has actually been hidden, then stop checking.
                                }
                            }
                        }
                        catch {
                            // Getting the name on some game objects will throw an error. Unavoidable to my knowledge, as testing if it will error causes the error. So just skip that object and keep going.
                        }
                    }
                }

                Camera.allCameras[0].cullingMask = ~(1 << 4); // Hide layer 4 from the main camera. I'm pretty sure that the main cameras index in allCameras won't change, so I'm not checking if index 0 is the right camera.
            }
            private static void Postfix()
            {
                if (!firstHide) RehideCosmetics(); // Cosmetics take a bit to load in, so wait until a cosmetic is actually hidden.

                string[] cosmetics = {
                    PlayerPrefs.GetString("hatCosmetic", "none"),
                    PlayerPrefs.GetString("faceCosmetic", "none"),
                    PlayerPrefs.GetString("badgeCosmetic", "none")
                };

                if(cosmetics.All(s => string.Equals("none", s))) // if the player isn't wearing any hats then set firstHide.
                {
                    firstHide = true;
                }

                for(int i = 0; i < 3; i++)
                {
                    if (lastCosmetics[i] != cosmetics[i]) // If a cosmetic changed, rehide all cosmetics.
                    {
                        RehideCosmetics();
                        lastCosmetics[i] = cosmetics[i];
                    }
                }
            }
        }
    }
}