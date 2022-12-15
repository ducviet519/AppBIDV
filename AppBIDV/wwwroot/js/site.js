// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$.fn.callModal = function (url) {

    var ReportPopupElement = $('#myPopup');
    $.ajax({
        url: url,
        dataType: 'html',
        success: function (data) {
            $("body").find(".modal-backdrop").remove();
            ReportPopupElement.html(data);
            ReportPopupElement.find('.modal').modal('show');
        }, error: function (xhr, status) {
            switch (status) {
                case 404:
                    $(this).callToast("error", 'Lỗi!', 'Đường dẫn không đúng hoặc tính năng không tồn tại!');
                    break;
                case 500:
                    $(this).callToast("error", 'Lỗi!', 'Không kết nối được tới Server!');
                    break;
                case 0:
                    $(this).callToast("error", 'Lỗi!', 'Hệ thống không phản hồi!');
                    break;
                default:
                    $(this).callToast("error", 'Lỗi!', 'Sự cố không xác định! Lỗi: ' + status);
            }
        },
    });
}

$.fn.callToast = function (status, title, msg) {
    toastr.options = {
        "closeButton": false,
        "debug": true,
        "newestOnTop": false,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }
    if (status == "success") {
        toastr.success(msg, title)
    }
    else if (status == "info") {
        toastr.info(msg, title)
    }
    else if (status == "warning") {
        toastr.warning(msg, title)
    }
    else if (status == "error") {
        toastr.error(msg, title)
    }
}

$.fn.callTreeView = function (id, data, level, showBorder) {
    if (showBorder === "" || showBorder === null || showBorder === "1" || showBorder === 1) { showBorder = true; }
    else if (showBorder === "0" || showBorder === 0) { showBorder = false; }
    $(id).treeview({
        data: data,
        enableLinks: true,
        showBorder: showBorder,
        showTags: false,
        levels: level,
        collapseIcon: 'fas fa-minus',
        expandIcon: 'fas fa-plus'
    });
}

$.fn.callDataTable = function (id, columnData, url, pageLength, disableColumn) {
    var array = [];
    $.each(disableColumn.split(','), function (idx, val) {
        array.push(parseInt(val));
    });
    if (disableColumn == '') { disableColumn = 0; }
    var table = $(id).DataTable();
    if ($.fn.dataTable.isDataTable(id)) {
        table.destroy();
        $(id).find('tbody').empty();
    }
    var table = $(id).DataTable({
        "responsive": true,
        "paging": true,
        "lengthChange": false,
        "pageLength": pageLength,
        "searching": true,
        "processing": true,
        "ordering": true,
        "info": true,
        "autoWidth": true,
        "order": [[0, 'asc']],
        "columnDefs": [
            { orderable: false, targets: array },
            { className: "text-wrap", targets: "_all" },
            { defaultContent: '', targets: "_all" },
        ],
        "ajax": {
            "url": url,
            "type": "GET",
            "datatype": "json"
        },
        "columns": columnData,
        "language": {
            "sProcessing": "Đang tải dữ liệu...",
            "sLengthMenu": "Xem _MENU_ mục",
            "sZeroRecords": "Không tìm thấy dòng nào phù hợp",
            "sInfo": "Đang xem _START_ đến _END_ trong tổng số _TOTAL_ mục",
            "sInfoEmpty": "Đang xem 0 đến 0 trong tổng số 0 mục",
            "sInfoFiltered": "(được lọc từ _MAX_ mục)",
            "sInfoPostFix": "",
            "sSearch": "Tìm:",
            "sUrl": "",
            "oPaginate": {
                "sFirst": "Đầu",
                "sPrevious": "Trước",
                "sNext": "Tiếp",
                "sLast": "Cuối"
            }
        },
    });
    return table;
}