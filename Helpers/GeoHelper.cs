namespace TourGuideApp.Helpers
{
    public static class GeoHelper
    {
        // Tính khoảng cách giữa 2 điểm GPS theo công thức Haversine, trả về mét
        public static double CalculateDistanceMeters(double lat1, double lng1, double lat2, double lng2)
        {
            const double earthRadiusKm = 6371.0;

            double dLat = ToRadians(lat2 - lat1);
            double dLng = ToRadians(lng2 - lng1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double distanceKm = earthRadiusKm * c;
            return distanceKm * 1000; // đổi ra mét
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
