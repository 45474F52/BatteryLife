namespace BatteryLife.Serializer
{
    public abstract class Serializer<T>
    {
        public abstract void Serialize(T value);
        public abstract T Deserialize();
    }
}