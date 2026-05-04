(function (window, document) {
    "use strict";

    const storageKey = "cloudyServiceLanguage";
    const dictionaries = {
        vi: {
            pageTitle: "Kính chào quý khách",
            languageButton: "Chọn ngôn ngữ",
            greeting: "Xin chào",
            tableServe: "Chúng tôi sẽ trả đồ cho bạn tại bàn:",
            pointLogin: "Nhập số điện thoại để tích điểm",
            callPayment: "Gọi thanh toán",
            callStaff: "Gọi nhân viên",
            review: "Đánh giá",
            menuButton: "Xem Menu - Gọi món",
            submittedTitle: "Các món đã gọi",
            submittedSummary: "{count} món - {total}đ",
            submittedEmpty: "Bạn chưa gửi món nào. Hãy vào menu để chọn món và gửi yêu cầu gọi món.",
            paymentQuestion: "Bạn muốn thanh toán bằng hình thức nào?",
            cash: "Tiền mặt",
            card: "Thẻ ngân hàng",
            wallet: "Ứng dụng điện thoại",
            sendRequest: "Gửi yêu cầu",
            sentRequest: "Đã gửi yêu cầu",
            sent: "Đã gửi",
            paymentRequestSent: "Đã gửi yêu cầu thanh toán. Nhân viên sẽ đến hỗ trợ bạn.",
            requestSent: "Đã gửi yêu cầu. Nhân viên sẽ đến hỗ trợ bạn.",
            staffQuestion: "Bạn muốn yêu cầu nhân viên làm gì?",
            requestPlaceholder: "Nhập yêu cầu của bạn...",
            reviewQuestion: "Trải nghiệm của bạn ở nhà hàng hôm nay thế nào?",
            reviewIssueQuestion: "Bạn có điều gì chưa hài lòng phải không?",
            tagClean: "Vệ sinh không sạch sẽ",
            tagStaff: "Nhân viên không nhiệt tình",
            tagTaste: "Món ăn không ngon",
            tagSlow: "Món ăn phục vụ lâu",
            tagPrice: "Giá không phù hợp với chất lượng",
            tagSpace: "Không gian bất tiện",
            tagNoise: "Không gian ồn",
            reviewPlaceholder: "Viết góp ý cho nhà hàng...",
            reviewPhoneText: "Nhà hàng rất trân trọng và mong muốn phản hồi lại đánh giá trên, bạn vui lòng để lại số điện thoại nhé",
            phonePlaceholder: "Số điện thoại của bạn",
            sendReview: "Gửi đánh giá",
            reviewThanks: "Cảm ơn bạn đã gửi đánh giá.",
            paymentCompleteTitle: "Thanh toán thành công",
            paymentCompleteMessage: "Cảm ơn bạn đã dùng bữa tại nhà hàng.",
            paymentCompleteUpdating: "Đang cập nhật lại đơn tại bàn...",
            close: "Đóng",
            searchPlaceholder: "Bạn muốn tìm món gì?",
            orderedBadge: "Đã gọi {count}",
            cartFooter: "Xem giỏ hàng({count} món)",
            cartTitle: "Các món đang gọi",
            clearCart: "Xóa hết",
            cartEmptyTitle: "Giỏ hàng đang trống",
            cartEmptyText: "Bạn chưa chọn món nào. Hãy quay lại menu để thêm đồ uống hoặc bánh yêu thích.",
            viewMenu: "Xem menu",
            edit: "Sửa",
            totalPrice: "Tổng tiền",
            submitOrder: "Gửi yêu cầu gọi món",
            removeConfirmDefault: "Bạn có chắc muốn loại bỏ sản phẩm này khỏi giỏ hàng của mình không?",
            removeConfirmProduct: "Bạn có chắc muốn loại bỏ sản phẩm {name} khỏi giỏ hàng của mình không?",
            clearConfirm: "Bạn có chắc muốn xóa tất cả sản phẩm trong giỏ hàng của mình không?",
            orderConfirm: "Bạn có chắc muốn đặt những món đã chọn không?",
            no: "Không",
            agree: "Đồng ý",
            orderSuccessTitle: "Gọi món thành công, vui lòng chờ nhân viên ra xác nhận!",
            orderSuccessNoteBefore: "Lưu ý: Nếu đợi quá lâu hoặc có thay đổi về món đã gọi bạn có thể dùng chức năng",
            orderSuccessNoteAction: "Gọi nhân viên",
            orderSuccessNoteAfter: "ở màn hình chính",
            productBack: "Quay lại",
            productNotePlaceholder: "Bạn có ghi chú gì cho nhà hàng không?",
            addToOrder: "Thêm vào đơn",
            updateOrder: "Cập nhật món",
            addingOrder: "Đang thêm...",
            updatingOrder: "Đang cập nhật...",
            quantity: "Số lượng",
            searchEmptyTitle: "Không tìm thấy món phù hợp",
            searchEmptyText: "Bạn thử nhập tên món khác, loại món hoặc mức giá nhé.",
            searchErrorTitle: "Chưa tìm được món",
            searchErrorText: "Bạn thử nhập lại sau một chút nhé.",
            paymentMethodCash: "Tiền mặt",
            paymentMethodCard: "Thẻ ngân hàng",
            paymentMethodWallet: "Ứng dụng điện thoại"
        },
        en: {
            pageTitle: "Welcome",
            languageButton: "Choose language",
            greeting: "Hello",
            tableServe: "We will serve your order at table:",
            pointLogin: "Enter phone number to earn points",
            callPayment: "Request payment",
            callStaff: "Call staff",
            review: "Review",
            menuButton: "View Menu - Order",
            submittedTitle: "Ordered items",
            submittedSummary: "{count} items - {total}đ",
            submittedEmpty: "You have not sent any items yet. Open the menu to choose items and submit your order.",
            paymentQuestion: "How would you like to pay?",
            cash: "Cash",
            card: "Bank card",
            wallet: "Mobile wallet",
            sendRequest: "Send request",
            sentRequest: "Request sent",
            sent: "Sent",
            paymentRequestSent: "Payment request sent. Staff will come to assist you.",
            requestSent: "Request sent. Staff will come to assist you.",
            staffQuestion: "What would you like staff to help with?",
            requestPlaceholder: "Type your request...",
            reviewQuestion: "How was your experience at the restaurant today?",
            reviewIssueQuestion: "Was there anything you were not satisfied with?",
            tagClean: "Cleanliness issue",
            tagStaff: "Staff not helpful",
            tagTaste: "Food did not taste good",
            tagSlow: "Food was served slowly",
            tagPrice: "Price did not match quality",
            tagSpace: "Space was inconvenient",
            tagNoise: "Space was noisy",
            reviewPlaceholder: "Write feedback for the restaurant...",
            reviewPhoneText: "We appreciate your feedback and may contact you about it. Please leave your phone number.",
            phonePlaceholder: "Your phone number",
            sendReview: "Send review",
            reviewThanks: "Thank you for your review.",
            paymentCompleteTitle: "Payment successful",
            paymentCompleteMessage: "Thank you for dining with us.",
            paymentCompleteUpdating: "Updating your table order...",
            close: "Close",
            searchPlaceholder: "What would you like to order?",
            orderedBadge: "Ordered {count}",
            cartFooter: "View cart ({count} items)",
            cartTitle: "Current order",
            clearCart: "Clear all",
            cartEmptyTitle: "Your cart is empty",
            cartEmptyText: "You have not selected any items yet. Go back to the menu to add your favorite drinks or cakes.",
            viewMenu: "View menu",
            edit: "Edit",
            totalPrice: "Total",
            submitOrder: "Send order request",
            removeConfirmDefault: "Are you sure you want to remove this item from your cart?",
            removeConfirmProduct: "Are you sure you want to remove {name} from your cart?",
            clearConfirm: "Are you sure you want to remove all items from your cart?",
            orderConfirm: "Are you sure you want to place the selected items?",
            no: "No",
            agree: "Confirm",
            orderSuccessTitle: "Order sent successfully. Please wait for staff confirmation.",
            orderSuccessNoteBefore: "Note: If you wait too long or want to change your order, you can use",
            orderSuccessNoteAction: "Call staff",
            orderSuccessNoteAfter: "on the home screen",
            productBack: "Back",
            productNotePlaceholder: "Do you have a note for the restaurant?",
            addToOrder: "Add to order",
            updateOrder: "Update order",
            addingOrder: "Adding...",
            updatingOrder: "Updating...",
            quantity: "Quantity",
            searchEmptyTitle: "No matching items found",
            searchEmptyText: "Try another item name, category, or price.",
            searchErrorTitle: "Could not find items",
            searchErrorText: "Please try again in a moment.",
            paymentMethodCash: "Cash",
            paymentMethodCard: "Bank card",
            paymentMethodWallet: "Mobile wallet"
        }
    };

    function getLanguage() {
        return window.localStorage.getItem(storageKey) === "en" ? "en" : "vi";
    }

    function normalizeLanguage(language) {
        return language === "en" ? "en" : "vi";
    }

    function t(key, replacements) {
        const language = getLanguage();
        let value = (dictionaries[language] && dictionaries[language][key])
            || (dictionaries.vi && dictionaries.vi[key])
            || key;

        if (replacements && typeof value === "string") {
            Object.keys(replacements).forEach(function (name) {
                value = value.split("{" + name + "}").join(replacements[name] == null ? "" : String(replacements[name]));
            });
        }

        return value;
    }

    function readTemplateData(element) {
        const data = {};

        Object.keys(element.dataset).forEach(function (name) {
            if (name !== "i18nTemplate") {
                data[name] = element.dataset[name];
            }
        });

        return data;
    }

    function apply(root) {
        const scope = root || document;
        document.documentElement.lang = getLanguage();

        scope.querySelectorAll("[data-i18n]").forEach(function (element) {
            element.textContent = t(element.dataset.i18n);
        });

        scope.querySelectorAll("[data-i18n-template]").forEach(function (element) {
            element.textContent = t(element.dataset.i18nTemplate, readTemplateData(element));
        });

        scope.querySelectorAll("[data-i18n-attr]").forEach(function (element) {
            element.dataset.i18nAttr.split(",").forEach(function (entry) {
                const parts = entry.split(":");
                if (parts.length === 2) {
                    element.setAttribute(parts[0].trim(), t(parts[1].trim()));
                }
            });
        });
    }

    function setLanguage(language) {
        window.localStorage.setItem(storageKey, normalizeLanguage(language));
        apply(document);
        document.dispatchEvent(new CustomEvent("cloudy:language-changed", {
            detail: { language: getLanguage() }
        }));
    }

    function paymentMethodLabel(method, fallback) {
        const normalized = (method || "").trim().toLowerCase();
        if (normalized === "cash") {
            return t("paymentMethodCash");
        }
        if (normalized === "card") {
            return t("paymentMethodCard");
        }
        if (normalized === "wallet") {
            return t("paymentMethodWallet");
        }

        const fallbackText = (fallback || "").trim().toLowerCase();
        if (fallbackText === "tiền mặt" || fallbackText === "cash") {
            return t("paymentMethodCash");
        }
        if (fallbackText === "thẻ ngân hàng" || fallbackText === "bank card") {
            return t("paymentMethodCard");
        }
        if (fallbackText === "ứng dụng điện thoại" || fallbackText === "mobile wallet") {
            return t("paymentMethodWallet");
        }

        return fallback || "";
    }

    function cartFooterText(count) {
        return t("cartFooter", { count: count || 0 });
    }

    window.CloudyDineInLanguage = {
        storageKey: storageKey,
        getLanguage: getLanguage,
        setLanguage: setLanguage,
        t: t,
        apply: apply,
        paymentMethodLabel: paymentMethodLabel,
        cartFooterText: cartFooterText
    };

    document.addEventListener("DOMContentLoaded", function () {
        apply(document);
    });
})(window, document);
