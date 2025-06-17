using UnityEngine;

namespace Vermines.Configuration.Network {

    public class NetworkedIntSettingBehaviour : NetworkSettingBehaviour<int> {

        [SerializeField]
        private int _Min = 0;

        [SerializeField]
        private int _Max = 100;

        protected override bool TryParse(string input, out int parsedValue)
        {
            bool success = int.TryParse(input, out parsedValue);

            if (success)
                parsedValue = Mathf.Clamp(parsedValue, _Min, _Max);
            return success;
        }

        public IntSetting CreateSetting()
        {
            return new IntSetting(_FieldName, _Min, _Max);
        }
    }
}
