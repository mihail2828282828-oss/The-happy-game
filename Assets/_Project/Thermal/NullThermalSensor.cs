namespace Front21.Thermal
{
    public sealed class NullThermalSensor : IThermalSensor
    {
        public float ReadCelsius() => 0f;
    }
}
