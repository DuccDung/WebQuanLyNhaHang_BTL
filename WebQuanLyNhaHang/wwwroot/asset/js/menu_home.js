document.querySelectorAll('.menu_scroll_link').forEach(item => {
    item.addEventListener('click', function() {
        // Tìm menu cấp 2 liên quan
        const submenu = this.nextElementSibling;
        
        // Kiểm tra nếu menu cấp 2 tồn tại và là danh sách con
        if (submenu && submenu.classList.contains('sidebar_menu_lv2')) {
            // Toggle lớp 'open' để thả xuống hoặc ẩn đi
            submenu.classList.toggle('open');
        }
    });
});
document.getElementById('menuSelect').addEventListener('change', function() {
    var selectedValue = this.value;
    if (selectedValue) {
        // Điều hướng tới phần tử với id tương ứng
        window.location.href = selectedValue;
    }
});
