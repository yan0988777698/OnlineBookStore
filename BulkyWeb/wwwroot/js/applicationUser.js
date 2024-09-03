var dataTable;
$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Admin/ApplicationUser/GetAll' },
        "columns": [
            { data: 'name' },
            { data: 'email' },
            { data: 'phoneNumber' },
            { data: 'company.name' },
            { data: 'role' },
            {
                data: { id: "id", lockoutEnd: "lockoutEnd" },
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();
                    if (lockout > today) {
                        return `<div class="">
                                    <a onclick = LockUnlock('${data.id}') class="btn btn-danger mx-2">
                                        <i class="bi bi-lock-fill"></i>
                                    </a>
                                    <a class="btn btn-danger mx-2" href="ApplicationUser/RoleManagement?id=${data.id}">
                                        <i class="bi bi-pencil-square"></i>
                                    </a>
                                </div>`
                    } else {
                        return `<div class="">
                                    <a onclick = LockUnlock('${data.id}') class="btn btn-success mx-2">
                                        <i class="bi bi-unlock-fill"></i>
                                    </a>
                                    <a class="btn btn-danger mx-2" href="ApplicationUser/RoleManagement?id=${data.id}">
                                        <i class="bi bi-pencil-square"></i>
                                    </a>
                                </div>`
                    }
                }
            }
        ]
    });
};

function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: "/Admin/ApplicationUser/LockUnlock",
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
        }
    })
}