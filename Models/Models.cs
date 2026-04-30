using System;

namespace BILCAM.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public bool IsAdmin => Role == "admin";
    }

    public class Resource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public bool IsAvailable { get; set; }

        public string CategoryDisplay =>
            Category == "classroom" ? "강의실" :
            Category == "laptop" ? "공용 노트북" : "우산";

        public string StatusDisplay => IsAvailable ? "예약 가능" : "사용 중";
    }

    public class Reservation
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ResourceId { get; set; }
        public string ResourceName { get; set; }
        public DateTime ReservationDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public string StatusDisplay =>
            Status == "pending" ? "승인 대기" :
            Status == "approved" ? "승인됨" : "반려됨";

        public string DateDisplay =>
            ReservationDate.ToString("yyyy-MM-dd");

        public string TimeDisplay => $"{StartTime} ~ {EndTime}";
    }
}
