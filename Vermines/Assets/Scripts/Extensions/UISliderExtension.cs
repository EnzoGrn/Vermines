using UnityEngine.UI;

namespace Vermines.Extension {

    using Vermines.Core.Settings;
    using Vermines.UI;

    public static class UISliderExtension {

        public static void SetOptionsValueFloat(this UISlider slider, OptionsValue value)
        {
            if (value.Type != OptionsValueType.Float) {
                slider.value = 0f;

                return;
            }

            if (slider.minValue != value.FloatValue.MinValue || slider.maxValue != value.FloatValue.MaxValue) {

                // Setting min and max value will unfortunately always trigger onValueChanged
                // so we need to removed this event data and reassign it again agterwards
                Slider.SliderEvent onValueChanged = slider.onValueChanged;

                slider.onValueChanged = new Slider.SliderEvent();

                slider.minValue = value.FloatValue.MinValue;
                slider.maxValue = value.FloatValue.MaxValue;

                slider.onValueChanged = onValueChanged;
            }

            slider.SetValue(value.FloatValue.Value);
        }

        public static void SetOptionsValueInt(this UISlider slider, OptionsValue value)
        {
            if (value.Type != OptionsValueType.Int) {
                slider.value = 0f;

                return;
            }

            if (slider.minValue != value.IntValue.MinValue || slider.maxValue != value.IntValue.MaxValue) {

                // Setting min and max value will unfortunately always trigger onValueChanged
                // so we need to removed this event data and reassign it again agterwards
                Slider.SliderEvent onValueChanged = slider.onValueChanged;

                slider.onValueChanged = new Slider.SliderEvent();

                slider.minValue = value.IntValue.MinValue;
                slider.maxValue = value.IntValue.MaxValue;

                slider.onValueChanged = onValueChanged;
            }

            slider.SetValue(value.IntValue.Value);
        }

        public static void SetOptionsValueFloat(this SliderModule @this, OptionsValue value)
        {
            UISlider slider = @this.Slider;

            if (value.Type != OptionsValueType.Float) {
                @this.SetValue(0f);

                return;
            }

            if (slider.minValue != value.FloatValue.MinValue || slider.maxValue != value.FloatValue.MaxValue) {

                // Setting min and max value will unfortunately always trigger onValueChanged
                // so we need to removed this event data and reassign it again agterwards
                Slider.SliderEvent onValueChanged = slider.onValueChanged;

                slider.onValueChanged = new Slider.SliderEvent();

                slider.minValue = value.FloatValue.MinValue;
                slider.maxValue = value.FloatValue.MaxValue;

                slider.onValueChanged = onValueChanged;
            }

            @this.SetValue(value.FloatValue.Value);
        }

        public static void SetOptionsValueInt(this SliderModule @this, OptionsValue value)
        {
            UISlider slider = @this.Slider;

            if (value.Type != OptionsValueType.Int) {
                @this.SetValue(0f);

                return;
            }

            if (slider.minValue != value.IntValue.MinValue || slider.maxValue != value.IntValue.MaxValue) {

                // Setting min and max value will unfortunately always trigger onValueChanged
                // so we need to removed this event data and reassign it again agterwards
                Slider.SliderEvent onValueChanged = slider.onValueChanged;

                slider.onValueChanged = new Slider.SliderEvent();

                slider.minValue = value.IntValue.MinValue;
                slider.maxValue = value.IntValue.MaxValue;

                slider.onValueChanged = onValueChanged;
            }

            @this.SetValue((float)value.IntValue.Value);
        }
    }
}
