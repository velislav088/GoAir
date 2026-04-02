namespace GoAir.Data.Common
{
    public static class EntityValidation
    {
        public static class Aircraft
        {
            public const int ModelMaxLength = 50;
            public const int ManufacturerMaxLength = 100;

            public const int MinCapacity = 1;
            public const int MaxCapacity = 800;
        }

        public static class Airport
        {
            public const int NameMaxLength = 100;
            public const int CityMaxLength = 100;

            public const int IataCodeLength = 3;
        }

        public static class Flight
        {
            public const int FlightNumberMinLength = 2;
            public const int FlightNumberMaxLength = 8;
        }

        public static class Ticket
        {
            public const int SeatNumberMaxLength = 5;

            public const double MinPrice = 0.01d;
            public const double MaxPrice = 100000d;
        }

        public static class Review
        {
            public const int MinRating = 1;
            public const int MaxRating = 5;

            public const int CommentMaxLength = 1000;
        }
    }
}