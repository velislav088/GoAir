namespace GoAir.Common
{
    public class EntityValidation
    {
        public static class Aircraft
        {
            public const int ModelMaxLength = 50;
            public const int ManufacturerMaxLength = 100;

            public const int MinCapacity = 1;
            public const int MaxCapacity = 600;
        }

        public static class Airport
        {
            public const int NameMaxLength = 100;
            public const int CityMaxLength = 100;

            public const int MinTerminals = 1;
            public const int MaxTerminals = 10;
        }

        public static class Flight
        {
            public const int FlightNumberMaxLength = 10;

            public const double MinPrice = 0;
            public const double MaxPrice = 100000;
        }
    }
}