using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(TestMod.TestMod), "TestMod", "1.0.0", "Test")]
[assembly: MelonGame("Unity", "ThienMenhLacHong")]

namespace TestMod
{
    public class TestMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Debug.Log("=== TEST MOD LOADED ===");
            LoggerInstance.Msg("TestMod initialized!");
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                LoggerInstance.Msg("F2 pressed!");
            }
        }
    }
}
