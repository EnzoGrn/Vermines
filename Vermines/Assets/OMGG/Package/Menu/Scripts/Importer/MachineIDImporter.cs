#if UNITY_EDITOR

using UnityEditor.AssetImporters;
using UnityEngine;

namespace OMGG.Menu.Importer {

    using OMGG.Menu.Configuration;
    using OMGG.Menu.Tools;

    /// <summary>
    /// All asset ending with .id will be tried for a <see cref="MachineID"/> script.
    /// A local id is created for it that will never go into version control.
    /// </summary>
    [ScriptedImporter(1, "id")]
    public class MachineIDImporter : ScriptedImporter {

        public override void OnImportAsset(AssetImportContext context)
        {
            MachineID asset = ScriptableObject.CreateInstance<MachineID>();

            if (asset != null) {
                asset.ID = CodeGenerator.Create(8, "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789");

                context.AddObjectToAsset("root", asset);
            }
        }
    }
}

#endif
