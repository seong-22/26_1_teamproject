using System.Collections.Generic;

namespace BILCAM
{
    public static class AppLanguage
    {
        public static string Current = "ko";

        private static readonly Dictionary<string, Dictionary<string, string>> _texts = new Dictionary<string, Dictionary<string, string>>
        {
            ["login_title"] = new Dictionary<string, string> { ["ko"] = "BILCAM — 로그인", ["en"] = "BILCAM — Login" },
            ["login_sub"] = new Dictionary<string, string> { ["ko"] = "빌려 쓰는 캠퍼스 — 새빛관", ["en"] = "Borrow-Share — Saebit Hall" },
            ["login_id"] = new Dictionary<string, string> { ["ko"] = "아이디", ["en"] = "ID" },
            ["login_pw"] = new Dictionary<string, string> { ["ko"] = "비밀번호", ["en"] = "Password" },
            ["login_btn"] = new Dictionary<string, string> { ["ko"] = "로그인", ["en"] = "Login" },
            ["login_register"] = new Dictionary<string, string> { ["ko"] = "회원가입", ["en"] = "Sign Up" },
            ["login_error_empty"] = new Dictionary<string, string> { ["ko"] = "아이디와 비밀번호를 입력하세요.", ["en"] = "Please enter your ID and password." },
            ["login_error_fail"] = new Dictionary<string, string> { ["ko"] = "아이디 또는 비밀번호가 올바르지 않습니다.", ["en"] = "Incorrect ID or password." },

            ["register_title"] = new Dictionary<string, string> { ["ko"] = "BILCAM — 회원가입", ["en"] = "BILCAM — Sign Up" },
            ["register_header"] = new Dictionary<string, string> { ["ko"] = "회원가입", ["en"] = "Sign Up" },
            ["register_id"] = new Dictionary<string, string> { ["ko"] = "아이디", ["en"] = "ID" },
            ["register_pw"] = new Dictionary<string, string> { ["ko"] = "비밀번호", ["en"] = "Password" },
            ["register_pw2"] = new Dictionary<string, string> { ["ko"] = "비밀번호 확인", ["en"] = "Confirm Password" },
            ["register_name"] = new Dictionary<string, string> { ["ko"] = "이름", ["en"] = "Name" },
            ["register_studentid"] = new Dictionary<string, string> { ["ko"] = "학번", ["en"] = "Student ID" },
            ["register_submit"] = new Dictionary<string, string> { ["ko"] = "가입 완료", ["en"] = "Register" },
            ["register_cancel"] = new Dictionary<string, string> { ["ko"] = "취소", ["en"] = "Cancel" },
            ["register_err_empty"] = new Dictionary<string, string> { ["ko"] = "모든 항목을 입력하세요.", ["en"] = "Please fill in all fields." },
            ["register_err_pw"] = new Dictionary<string, string> { ["ko"] = "비밀번호가 일치하지 않습니다.", ["en"] = "Passwords do not match." },
            ["register_err_pwlen"] = new Dictionary<string, string> { ["ko"] = "비밀번호는 8자 이상 입력하세요.", ["en"] = "Password must be at least 8 characters." },
            ["register_err_studentid"] = new Dictionary<string, string> { ["ko"] = "학번은 숫자만 입력하세요.", ["en"] = "Student ID must be numeric." },
            ["register_err_dup"] = new Dictionary<string, string> { ["ko"] = "이미 사용 중인 아이디입니다.", ["en"] = "This ID is already taken." },
            ["register_success"] = new Dictionary<string, string> { ["ko"] = "회원가입이 완료되었습니다!", ["en"] = "Registration complete!" },

            ["student_tab_resources"] = new Dictionary<string, string> { ["ko"] = "  자원 조회  ", ["en"] = "  Resources  " },
            ["student_tab_myres"] = new Dictionary<string, string> { ["ko"] = "  내 예약  ", ["en"] = "  My Reservations  " },
            ["student_role"] = new Dictionary<string, string> { ["ko"] = "학생", ["en"] = "Student" },
            ["student_logout"] = new Dictionary<string, string> { ["ko"] = "로그아웃", ["en"] = "Logout" },
            ["student_cat_classroom"] = new Dictionary<string, string> { ["ko"] = "강의실", ["en"] = "Classroom" },
            ["student_cat_laptop"] = new Dictionary<string, string> { ["ko"] = "공용 노트북", ["en"] = "Laptop" },
            ["student_cat_umbrella"] = new Dictionary<string, string> { ["ko"] = "우산", ["en"] = "Umbrella" },
            ["student_available"] = new Dictionary<string, string> { ["ko"] = "예약 가능", ["en"] = "Available" },
            ["student_inuse"] = new Dictionary<string, string> { ["ko"] = "사용 중", ["en"] = "In Use" },
            ["student_no_resources"] = new Dictionary<string, string> { ["ko"] = "예약 내역이 없습니다.", ["en"] = "No reservations found." },
            ["student_tab_pending"] = new Dictionary<string, string> { ["ko"] = "  승인 대기  ", ["en"] = "  Pending  " },
            ["student_tab_approved"] = new Dictionary<string, string> { ["ko"] = "  승인됨  ", ["en"] = "  Approved  " },
            ["student_tab_rejected"] = new Dictionary<string, string> { ["ko"] = "  반려됨  ", ["en"] = "  Rejected  " },
            ["student_empty_pending"] = new Dictionary<string, string> { ["ko"] = "승인 대기 중인 예약이 없습니다.", ["en"] = "No pending reservations." },
            ["student_empty_approved"] = new Dictionary<string, string> { ["ko"] = "승인된 예약이 없습니다.", ["en"] = "No approved reservations." },
            ["student_empty_rejected"] = new Dictionary<string, string> { ["ko"] = "반려된 예약이 없습니다.", ["en"] = "No rejected reservations." },
            ["student_status_pending"] = new Dictionary<string, string> { ["ko"] = "승인 대기", ["en"] = "Pending" },
            ["student_status_approved"] = new Dictionary<string, string> { ["ko"] = "승인됨", ["en"] = "Approved" },
            ["student_status_rejected"] = new Dictionary<string, string> { ["ko"] = "반려됨", ["en"] = "Rejected" },
            ["student_cancel"] = new Dictionary<string, string> { ["ko"] = "예약 취소", ["en"] = "Cancel" },
            ["student_cancel_confirm"] = new Dictionary<string, string> { ["ko"] = "예약을 취소하시겠습니까?", ["en"] = "Cancel this reservation?" },
            ["student_memo"] = new Dictionary<string, string> { ["ko"] = "메모: ", ["en"] = "Memo: " },
            ["student_reject_reason"] = new Dictionary<string, string> { ["ko"] = "반려 사유: ", ["en"] = "Reason: " },
            ["student_daily"] = new Dictionary<string, string> { ["ko"] = "(하루 대여)", ["en"] = "(Full day)" },

            ["res_date"] = new Dictionary<string, string> { ["ko"] = "날짜 선택", ["en"] = "Select Date" },
            ["res_time"] = new Dictionary<string, string> { ["ko"] = "시간대 선택 (1시간 단위)", ["en"] = "Select Time Slot (1 hour)" },
            ["res_memo"] = new Dictionary<string, string> { ["ko"] = "사용 목적 / 메모 (선택)", ["en"] = "Purpose / Memo (optional)" },
            ["res_submit"] = new Dictionary<string, string> { ["ko"] = "예약 신청", ["en"] = "Reserve" },
            ["res_legend_avail"] = new Dictionary<string, string> { ["ko"] = "예약 가능", ["en"] = "Available" },
            ["res_legend_taken"] = new Dictionary<string, string> { ["ko"] = "이미 예약됨", ["en"] = "Reserved" },
            ["res_legend_selected"] = new Dictionary<string, string> { ["ko"] = "선택됨", ["en"] = "Selected" },
            ["res_legend_unavail"] = new Dictionary<string, string> { ["ko"] = "예약 불가", ["en"] = "Unavailable" },
            ["res_warn_taken_date"] = new Dictionary<string, string> { ["ko"] = "이미 예약된 날짜입니다. 다른 날짜를 선택해주세요.", ["en"] = "This date is already reserved. Please choose another." },
            ["res_warn_no_slot"] = new Dictionary<string, string> { ["ko"] = "시간대를 선택하세요.", ["en"] = "Please select a time slot." },
            ["res_warn_slot_taken"] = new Dictionary<string, string> { ["ko"] = "해당 시간대는 이미 예약되어 있습니다.", ["en"] = "This time slot is already reserved." },
            ["res_success"] = new Dictionary<string, string> { ["ko"] = "예약 신청이 완료되었습니다!\n관리자 승인 후 확정됩니다.", ["en"] = "Reservation submitted!\nAwaiting admin approval." },
            ["res_date_ok"] = new Dictionary<string, string> { ["ko"] = "예약 가능한 날짜입니다.", ["en"] = "This date is available." },
            ["res_date_no"] = new Dictionary<string, string> { ["ko"] = "이미 예약된 날짜입니다.", ["en"] = "This date is already reserved." },
            ["res_date_select"] = new Dictionary<string, string> { ["ko"] = "날짜를 선택해주세요.", ["en"] = "Please select a date." },
            ["res_selected_time"] = new Dictionary<string, string> { ["ko"] = "선택된 시간: ", ["en"] = "Selected: " },
            ["res_no_time"] = new Dictionary<string, string> { ["ko"] = "선택된 시간: 없음", ["en"] = "Selected: None" },

            ["admin_title"] = new Dictionary<string, string> { ["ko"] = "BILCAM  관리자 패널", ["en"] = "BILCAM  Admin Panel" },
            ["admin_logout"] = new Dictionary<string, string> { ["ko"] = "로그아웃", ["en"] = "Logout" },
            ["admin_tab_pending"] = new Dictionary<string, string> { ["ko"] = "  승인 대기  ", ["en"] = "  Pending  " },
            ["admin_tab_all"] = new Dictionary<string, string> { ["ko"] = "  전체 예약  ", ["en"] = "  All Reservations  " },
            ["admin_tab_items"] = new Dictionary<string, string> { ["ko"] = "  자원 관리  ", ["en"] = "  Resources  " },
            ["admin_empty_pending"] = new Dictionary<string, string> { ["ko"] = "승인 대기 중인 예약이 없습니다.", ["en"] = "No pending reservations." },
            ["admin_empty_all"] = new Dictionary<string, string> { ["ko"] = "예약 내역이 없습니다.", ["en"] = "No reservations found." },
            ["admin_approve"] = new Dictionary<string, string> { ["ko"] = "승인", ["en"] = "Approve" },
            ["admin_reject"] = new Dictionary<string, string> { ["ko"] = "반려", ["en"] = "Reject" },
            ["admin_reject_title"] = new Dictionary<string, string> { ["ko"] = "반려 사유 입력", ["en"] = "Rejection Reason" },
            ["admin_reject_label"] = new Dictionary<string, string> { ["ko"] = "반려 사유를 입력해주세요. (선택)", ["en"] = "Enter reason for rejection. (optional)" },
            ["admin_reject_ok"] = new Dictionary<string, string> { ["ko"] = "반려 처리", ["en"] = "Reject" },
            ["admin_reject_cancel"] = new Dictionary<string, string> { ["ko"] = "취소", ["en"] = "Cancel" },
            ["admin_status_pending"] = new Dictionary<string, string> { ["ko"] = "  대기  ", ["en"] = "  Pending  " },
            ["admin_status_approved"] = new Dictionary<string, string> { ["ko"] = "  승인  ", ["en"] = "  Approved  " },
            ["admin_status_rejected"] = new Dictionary<string, string> { ["ko"] = "  반려  ", ["en"] = "  Rejected  " },
            ["admin_applicant"] = new Dictionary<string, string> { ["ko"] = "신청자: ", ["en"] = "Applicant: " },
            ["admin_studentid"] = new Dictionary<string, string> { ["ko"] = "학번: ", ["en"] = "Student ID: " },
            ["admin_memo"] = new Dictionary<string, string> { ["ko"] = "메모: ", ["en"] = "Memo: " },
            ["admin_reject_reason"] = new Dictionary<string, string> { ["ko"] = "반려 사유: ", ["en"] = "Reason: " },
            ["admin_add_resource"] = new Dictionary<string, string> { ["ko"] = "+ 새 자원 추가", ["en"] = "+ Add Resource" },
            ["admin_available"] = new Dictionary<string, string> { ["ko"] = " 사용 가능 ", ["en"] = " Available " },
            ["admin_unavailable"] = new Dictionary<string, string> { ["ko"] = "  사용 불가  ", ["en"] = "  Unavailable  " },
            ["admin_set_unavailable"] = new Dictionary<string, string> { ["ko"] = "사용 불가로 변경", ["en"] = "Set Unavailable" },
            ["admin_set_available"] = new Dictionary<string, string> { ["ko"] = "사용 가능으로 변경", ["en"] = "Set Available" },
        };

        public static string Get(string key)
        {
            if (_texts.TryGetValue(key, out var dict))
                return dict.TryGetValue(Current, out var val) ? val : key;
            return key;
        }
    }
}