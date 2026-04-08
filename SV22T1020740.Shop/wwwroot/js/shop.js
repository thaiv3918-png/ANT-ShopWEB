// cart.js - ĐẢM BẢO CÁC HÀM NÀY ĐƯỢC ĐỊNH NGHĨA ĐÚNG

// Hàm thêm sản phẩm vào giỏ hàng
function addCartItem(productId) {
    console.log("ADD:", productId);

    $.post("/Order/AddCartItem",
        { productId: productId, quantity: 1 },
        function (res) {
            console.log(res);

            if (res.code == 1) {
                alert("Đã thêm vào giỏ hàng");

                if (typeof loadCartCount === "function") {
                    loadCartCount();
                }

                if ($("#cartContainer").length) {
                    $("#cartContainer").load("/Order/ShowCart");
                }
            } else {
                if (res.message === "Vui lòng đăng nhập") {
                    showLoginAlert("Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng");
                } else {
                    alert(res.message);
                }
            }
        }).fail(function (xhr, status, error) {
            if (xhr.status === 401) {
                showLoginAlert("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại");
            } else {
                alert("Có lỗi xảy ra. Vui lòng thử lại sau");
            }
        });
}

// Hàm thêm sản phẩm vào giỏ hàng và chuyển thẳng đến checkout
function buyNow(productId) {
    window.location.href = "/Order/BuyNow?productId=" + productId;
}
// Hàm hiển thị thông báo và chuyển hướng đến trang login
function showLoginAlert(message, redirectAfterLogin = false) {
    if (redirectAfterLogin) {
        localStorage.setItem("redirectAfterLogin", window.location.href);
    }

    if (confirm(message + "\n\nBạn có muốn đăng nhập ngay không?")) {
        window.location.href = "/Account/Login?returnUrl=" + encodeURIComponent(window.location.href);
    }
}

// Hàm cập nhật giỏ hàng khi thay đổi số lượng
function updateCart(productId, qty) {
    $.post("/Order/UpdateCartItem",
        { productId: productId, quantity: qty },
        function (res) {
            if (res.code == 1) {
                let price = parseFloat($("#total-" + productId).attr("data-price")) || 0;
                let total = price * qty;
                $("#total-" + productId).text(total.toLocaleString("vi-VN") + " đ");

                if (typeof loadCartCount === "function") {
                    loadCartCount();
                }
            }
        }).fail(function (xhr, status, error) {
            if (xhr.status === 401) {
                showLoginAlert("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập để cập nhật giỏ hàng");
            }
        });
}
function updateCartTotal() {
    console.log("updateCartTotal called");
    let sum = 0;

    // Duyệt qua từng dòng sản phẩm
    $("tr[id^='row-']").each(function () {
        let row = $(this);
        let productId = this.id.replace("row-", "");

        // Lấy giá từ data-price của element .item-total
        let priceElement = $("#total-" + productId);
        let price = parseFloat(priceElement.attr("data-price")) || 0;

        // Lấy số lượng
        let qtyElement = $("#qty-" + productId);
        let qty = parseInt(qtyElement.val()) || 0;

        sum += price * qty;
    });

    // Cập nhật hiển thị tổng tiền
    let totalElement = $("#cart-total");
    if (totalElement.length) {
        totalElement.text(sum.toLocaleString("vi-VN") + " đ");
    } else {
        // Nếu không có #cart-total, tạo mới hoặc cập nhật element khác
        console.log("Không tìm thấy #cart-total, tổng tiền:", sum);
        // Bạn có thể thêm code để hiển thị tổng tiền ở đây
    }

    return sum;
}
// Hàm xóa toàn bộ giỏ hàng
function clearCart() {
    $.post("/Order/ClearCart", function (res) {
        if (res.code == 1) {
            $("#cartContainer").html(`
        <div class="text-center">
            <p>Giỏ hàng trống</p>
        </div>
    `);
            $("#cart-summary").hide();

            updateCartTotal(); // ✅ FIX

            loadCartCount();
        } else {
            if (res.message === "Vui lòng đăng nhập") {
                showLoginAlert("Vui lòng đăng nhập để quản lý giỏ hàng");
            } else {
                alert(res.message);
            }
        }
    }).fail(function (xhr, status, error) {
        if (xhr.status === 401) {
            showLoginAlert("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập để quản lý giỏ hàng");
        }
    });
}

// Hàm xóa một sản phẩm khỏi giỏ hàng
function deleteCartItem(productId) {
    $.post("/Order/DeleteCartItem",
        { productId: productId },
        function (res) {
            if (res.code == 1) {
                $("#row-" + productId).fadeOut(200, function () {
                    $(this).remove();
                    if ($("tr[id^='row-']").length == 0) {
                        $("#cartContainer").html(`
                            <div class="text-center">
                                <p>Giỏ hàng trống</p>
                            </div>
                        `);
                        $("#cart-summary").remove();
                    } else {
                        updateCartTotal();
                    }
                });
                loadCartCount();
            } else {
                if (res.message === "Vui lòng đăng nhập") {
                    showLoginAlert("Vui lòng đăng nhập để xóa sản phẩm");
                } else {
                    alert(res.message);
                }
            }
        }).fail(function (xhr, status, error) {
            if (xhr.status === 401) {
                showLoginAlert("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập để xóa sản phẩm");
            }
        });
}
function showDeleteModal(productId) {
    $("#deleteModalContent").load("/Order/DeleteCartItem?productId=" + productId, function () {
        let modal = new bootstrap.Modal(document.getElementById('deleteModal'));
        modal.show();
    });
}
function changeQty(productId, delta) {
    let qtyInput = document.getElementById(`qty-${productId}`);
    let newQty = parseInt(qtyInput.value) + delta;
    if (newQty < 1) newQty = 1;

    qtyInput.value = newQty;

    // update UI ngay
    let price = parseFloat($("#total-" + productId).attr("data-price")) || 0;
    let total = price * newQty;
    $("#total-" + productId).text(total.toLocaleString("vi-VN") + " đ");

    updateCartTotal();

    // gọi server sau
    updateCart(productId, newQty);
}
// Khởi tạo khi trang load xong
$(document).ready(function () {
    console.log("cart.js loaded - checking functions:");
    console.log("changeQty defined:", typeof changeQty !== "undefined");
    console.log("updateCartTotal defined:", typeof updateCartTotal !== "undefined");

    // Cập nhật tổng tiền ban đầu
    updateCartTotal();
});
$(document).on("click", ".open-modal", function (e) {
    e.preventDefault();

    let url = $(this).attr("href");

    $("#modalContent").load(url, function () {
        let modal = new bootstrap.Modal(document.getElementById('modalContainer'));
        modal.show();
    });
});

function reloadOrderList() {

    fetch("/Order/Search?page=1")
        .then(res => res.text())
        .then(html => {
            document.getElementById("orderContainer").innerHTML = html; // ✅ FIX
        });
}
function cancelOrder(id) {
    if (!confirm("Bạn chắc chắn muốn hủy đơn?")) return;

    $.post("/Order/Cancel", { id: id }, function (res) {
        if (res.code > 0) {
            alert(res.message || "Đã hủy đơn hàng");

            // Reload lại danh sách
            let form = document.getElementById("formSearch");
            let url = form.action;
            let data = $(form).serialize() + "&Page=1";

            $("#orderContainer").load(url + "?" + data, function () {
                // Optional: Scroll lên đầu danh sách
                $('html, body').animate({
                    scrollTop: $("#orderContainer").offset().top - 100
                }, 300);
            });

        } else {
            alert(res.message || "Không thể hủy đơn hàng");
        }
    }).fail(function (xhr, status, error) {
        console.error("Error:", error);
        alert("Có lỗi xảy ra khi hủy đơn hàng. Vui lòng thử lại sau.");
    });
}