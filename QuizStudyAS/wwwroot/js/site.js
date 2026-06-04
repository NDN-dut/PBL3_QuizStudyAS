// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {

    // ==========================================
    // 1. ĐIỀU KHIỂN SIDEBAR (GIỮ NGUYÊN)
    // ==========================================
    const btnToggle = document.getElementById('btn-toggle-sidebar');
    if (btnToggle) {
        btnToggle.addEventListener('click', function (e) {
            e.preventDefault();
            document.documentElement.classList.toggle('sidebar-expanded-state');

            const isExpanded = document.documentElement.classList.contains('sidebar-expanded-state');
            localStorage.setItem('quiz_study_sidebar_state', isExpanded);
        });
    }

    // ==========================================
    // 2. ĐIỀU KHIỂN GIAO DIỆN SÁNG / TỐI (ĐÃ FIX LỖI)
    // ==========================================
    const themeBtns = document.querySelectorAll('.theme-toggle-btn');
    const themeIcons = document.querySelectorAll('.theme-icon');
    const themeTexts = document.querySelectorAll('.theme-text');

    function syncThemeUI(isDark) {
        themeIcons.forEach(icon => {
            // Thêm class me-2/me-3 tùy vị trí để không mất khoảng cách
            const marginClass = icon.classList.contains('me-3') ? 'me-3' : (icon.classList.contains('me-2') ? 'me-2' : '');
            icon.className = isDark ? `bi bi-sun-fill fs-6 text-warning theme-icon ${marginClass}` : `bi bi-moon-stars-fill fs-6 text-secondary theme-icon ${marginClass}`;
        });
        themeTexts.forEach(text => {
            text.innerText = isDark ? 'Giao diện sáng' : 'Giao diện tối';
        });
    }

    const isCurrentlyDark = document.documentElement.classList.contains('dark-theme');
    syncThemeUI(isCurrentlyDark);

    themeBtns.forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            // Tránh menu dropdown bị đóng ngay lập tức nếu click vào trong
            e.stopPropagation();

            document.documentElement.classList.toggle('dark-theme');
            document.body.classList.toggle('dark-theme');

            const nowDark = document.documentElement.classList.contains('dark-theme');
            syncThemeUI(nowDark);
            localStorage.setItem('quiz_study_theme', nowDark ? 'dark' : 'light');
        });
    });

    // ==========================================
    // 3. TẢI SỐ LƯỢNG THÔNG BÁO (BADGE)
    // ==========================================
    function fetchRequestCount() {
        fetch('/Classroom/GetPendingRequestCount')
            .then(res => res.json())
            .then(data => {
                const badges = document.querySelectorAll('.request-badge-count');
                badges.forEach(badge => {
                    if (data.count > 0) {
                        badge.innerText = data.count > 99 ? '99+' : data.count;
                        badge.style.display = 'inline-block';
                    } else {
                        badge.style.display = 'none';
                    }
                });
            })
            .catch(err => console.error("Lỗi tải thông báo", err));
    }

    // HÀM MỚI: TẢI SỐ LƯỢNG BÀI KIỂM TRA CHƯA LÀM
    function fetchExamCount() {
        fetch('/Exam/GetPendingExamCount')
            .then(response => response.json())
            .then(data => {
                const badge = document.getElementById('exam-badge');
                if (badge) {
                    if (data.count > 0) {
                        // Nếu lớn hơn 9 thì hiện 9+ cho đẹp
                        badge.innerText = data.count > 9 ? '9+' : data.count;
                        badge.style.display = 'inline-block';

                        // Thêm một chút hiệu ứng nhịp tim nhẹ nhàng để thu hút sự chú ý
                        badge.style.animation = 'pulse 2s infinite';
                    } else {
                        badge.style.display = 'none';
                    }
                }
            })
            .catch(err => console.error("Lỗi tải thông báo bài kiểm tra", err));
    }

    // Gọi ngay khi load trang
    fetchRequestCount();
    fetchExamCount();
});