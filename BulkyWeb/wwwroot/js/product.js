var dataTable;
$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/product/getall' },
        "columns": [
            { data: 'title' },
            { data: 'isbn' },
            { data: 'price' },
            { data: 'author' },
            { data: 'category.name' },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                        <a class="btn btn-primary mx-2" href="/admin/product/upsert?id=${data}">
                            <i class="bi bi-pencil-square"></i>
                        </a>
                        <a class="btn btn-danger mx-2" onClick=Delete('/admin/product/DeleteById/${data}')>
                            <i class="bi bi-trash3"></i>
                        </a>
                    </div>`
                }
            }
        ]
    });
};

function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message);
                }
            })
        }
    });
}