$(document).ready(function () {


    FilterEmployees();
    loadDesignations();

    $('#DateofJoining').datepicker({
        format: 'dd-mm-yyyy',
        autoclose: true,
        todayHighlight: true,  
    });

    $("#AddEmployeeDetail").submit(function (e) {
        e.preventDefault();
        SaveEmployee();
        //ClearSubmit();
    });

    $("#AddEmpDesignation").submit(function (e) {
        e.preventDefault();
        SaveDesignation();
    });

    $(document).on('click', '.btn-edit', function (e) {
        e.preventDefault();
        GetEmployee($(this).data('id'));
    });

    $(document).on('click', '.btn-delete', function (e) {
        e.preventDefault();
        DeleteEmployee($(this).data('id'));
    });

    $("#addEmployeeBtn").click(function () {
        $('#Emplbl').text('Add Employee');
        loadDesignations();
        ClearSubmit();               
    });


    //$('.btn-editdes').click(function () {
    //    GetDesignationByID($(this).data('id'))
    //});

    $(document).on('click', '.btn-editdes', function (e) {
        e.preventDefault();
        GetDesignationByID($(this).data('id'));
    });

    $("#addDesignationbtn").click(function () {
        ClearDesignation();
        FilterDesignation();
    });

    $(document).on('click', '.btn-deletedes', function (e) {
        e.preventDefault();
        DeleteDesignation($(this).data('id'));
    });

    $('#resetDesignationBtn').click(function () {
        ClearDesignation();
    });


    //$('#btnExport').click(function () {

    //    // Get filter values from your page (optional)
    //    var Empname = $('#txtEmpName').val();              // Example input
    //    var Emp_Designation = $('#ddlEmpDesignation').val(); // Example dropdown
    //    var Status = $('#ddlStatus').val();               // Example dropdown

    //    // Construct URL with query parameters
    //    var url = '/Employee/ExportEmployeesToExcel?';
    //    if (Empname) url += 'Empname=' + encodeURIComponent(Empname) + '&';
    //    if (Emp_Designation) url += 'Emp_Designation=' + encodeURIComponent(Emp_Designation) + '&';
    //    if (Status) url += 'Status=' + encodeURIComponent(Status);

    //    // Remove trailing '&' if exists
    //    if (url.endsWith('&') || url.endsWith('?')) {
    //        url = url.slice(0, -1);
    //    }

    //    // Trigger download
    //    window.location.href = url;
    //});

    $('#btnExport').click(function () {
        var Empname = $('#txtEmpName').val();
        var Emp_Designation = $('#ddlEmpDesignation').val();
        var Status = $('#ddlStatus').val();

        var url = '/Employee/ExportEmployeesToExcel?';
        if (Empname) url += 'Empname=' + encodeURIComponent(Empname) + '&';
        if (Emp_Designation) url += 'Emp_Designation=' + encodeURIComponent(Emp_Designation) + '&';
        if (Status) url += 'Status=' + encodeURIComponent(Status);
        if (url.endsWith('&') || url.endsWith('?')) url = url.slice(0, -1);

        // Trigger download
        window.location.href = url;
    });


    


});

function SaveEmployee() {

    var EmpId = $('#EmployeeID').val();
    var Joingdt = $('#DateofJoining').val(); 

    if (!Joingdt) {
        showToast("Date of Joining is required", false);
        return;
    }
    var parts = Joingdt.split('-');
    var formattedDate = parts[2] + '-' + parts[1] + '-' + parts[0]; 

    var formData = new FormData();

    formData.append("Employee.EmployeeID", EmpId);
    formData.append("Employee.EmployeeName", $('#Empname').val());
    formData.append("Employee.Emp_DesignationId", $('#EmpDesig').val());
    formData.append("Employee.Emp_Address", $('#EmpAddress').val());
    formData.append("Employee.GenderId", $('input[name="GenderId"]:checked').val());
    formData.append("Employee.DateofJoining", formattedDate);
    formData.append("Employee.Status", $('#EmpStatus').val());

    var fileInput = document.getElementById("EmpformFile");
    if (fileInput.files.length > 0) {
        formData.append("DocumentFile", fileInput.files[0]);
    }

    var url = EmpId ? '/Employee/UpdateEmployee' : '/Employee/AddEmployee';

    $.ajax({
        url: url,
        type: 'POST',
        data: formData,
        contentType: false,   
        processData: false,
        beforeSend: function () {
            showLoader();
        },
        success: function (response) {
            $('#employeeModal').modal('hide');
            $('#AddEmployeeDetail')[0].reset();
            $('#EmployeeID').val('');
            showToast(response.message, true);
            FilterEmployees();
        },
        error: function (err) {
            showToast(err.responseJSON?.message || "Server error", false);
        },
        complete: function () {
            hideLoader();
        }
    });
}

function FilterEmployees() {

    var sEmpname = "";
    if ($("#txtEmpname").val() != null) {
        sEmpname = $("#txtEmpname").val().toString();
    }

    var sEmpDesignation = "";
    if ($("#ddlEmpdesig").val() != null) {
        sEmpDesignation = $("#ddlEmpdesig").val();
    }

    var sEmpStatus = "";
    if ($("#ddlEmpStatus").val() != null) {
        sEmpStatus = $("#ddlEmpStatus").val();
    }

    $.ajax({
        url: '/Employee/FilterEmployees',
        type: 'GET',
        data: { "Empname": sEmpname, "EmpDesignation": sEmpDesignation, "EmpStatus": sEmpStatus },
        beforeSend: function () {
            showLoader();
        },
        success: function (employees) {

            if ($.fn.DataTable.isDataTable('#employeeTable')) {
                $('#employeeTable').DataTable().destroy();
            }

            var tbody = $("#employeeTable tbody");
            tbody.empty();

            $.each(employees, function (index, emp) {
                var row = `<tr data-id="${emp.employeeID}">
                    <td>${emp.employeeName}</td>
                    <td>${emp.designationName}</td>
                    <td>${emp.emp_Address}</td>
                    <td>${formatDateYYMMDD(emp.dateofJoining)}</td>
                    <td>${emp.createdByUsername}</td>
                    <td>${emp.modifiedByUsername}</td>
                    <td>${emp.created_Time ? new Date(emp.created_Time).toLocaleString() : ''}</td>
                    <td>${emp.modified_Time ? new Date(emp.modified_Time).toLocaleString() : ''}</td>
                    <td class="text-center">
                        <a href="#" class="btn btn-warning btn-sm btn-edit"  data-id="${emp.employeeID}">Edit</a>
                        <a href="#"  class="btn btn-danger btn-sm btn-delete" data-id="${emp.employeeID}">Delete</a>
                    </td>
                </tr>`;
                tbody.append(row);
            });

            $('#employeeTable').DataTable({
                "paging": true,
                "searching": false,
                "ordering": true,
                "lengthChange": false
            });
        },
        error: function (xhr) {
            console.log("Error loading employees:", xhr.responseText);
        },
        complete: function () {
            hideLoader();
        }
    });
}

function formatDateYYMMDD(dateStr) {
    if (!dateStr) return ''; // return empty string if null or undefined

    const d = new Date(dateStr);
    if (isNaN(d.getTime())) return ''; // invalid date check

    const yyyy = String(d.getFullYear()).slice(-4);
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');

    return `${dd}/${mm}/${yyyy}`;
}

function GetEmployee(empId) {
    if (!empId) {
        console.error("Employee ID is required for editing.");
        return;
    }

    $.ajax({
        url: '/Employee/GetEmployeebyId', 
        type: 'GET',
        data: { id: empId },
        beforeSend: function () {
            showLoader();
        },
        success: function (emp) {
            if (!emp) {
                alert("Employee not found!");
                return;
            }

            $('#EmployeeID').val(emp.employeeID);        
            $('#Empname').val(emp.employeeName);
            loadDesignations(emp.emp_DesignationId);
            $('#EmpAddress').val(emp.emp_Address);
            $('input[name="GenderId"][value="' + emp.genderId + '"]').prop('checked', true);
            if (emp.dateofJoining) {
                var dateParts = emp.dateofJoining.split('-'); // ["2026","02","17"]
                var jsDate = new Date(dateParts[0], dateParts[1] - 1, dateParts[2]);
            }
            $('#DateofJoining').datepicker('setDate', jsDate);
            $('#EmpStatus').val(emp.status);

            if (emp.document && emp.document.documentPath) {
                $('#docPreview').html(`
                <a href="${emp.document.documentPath}" target="_blank" class="btn btn-sm btn-primary">
                   ${emp.document.documentName}
                </a>
            `);
            } else {
                $('#docPreview').html('<span class="text-muted">No document uploaded</span>');
            }

            $('#Emplbl').text('Edit Employee');
            $('#employeeModal').modal('show');
        },
        error: function (xhr, status, error) {
            console.error("Error fetching employee:", error);
        },
        complete: function () {
            hideLoader();
        }
    });
}

//function GetEmployee(empId) {
//    $.get('/Employee/GetEmployeebyId', { id: id }, function (res) {

//        // Employee fields
//        $('#EmployeeID').val(res.employeeID);
//        $('#Empname').val(res.employeeName);
//        $('#EmpDesig').val(res.emp_DesignationId);
//        $('#EmpAddress').val(res.emp_Address);
//        $('input[name="GenderId"][value="' + res.genderId + '"]').prop('checked', true);
//        $('#DateofJoining').val(formatDate(res.dateofJoining));

//        // 📄 Show document if exists
//        if (res.document && res.document.documentPath) {
//            $('#docPreview').html(`
//                <a href="${res.document.documentPath}" target="_blank" class="btn btn-sm btn-outline-primary">
//                    View Document: ${res.document.documentName}
//                </a>
//            `);
//        } else {
//            $('#docPreview').html('<span class="text-muted">No document uploaded</span>');
//        }

//        $('#employeeModal').modal('show');
//    });
//}
//function formatDate(date) {
//    if (!date) return '';
//    var d = new Date(date);
//    var day = String(d.getDate()).padStart(2, '0');
//    var month = String(d.getMonth() + 1).padStart(2, '0');
//    var year = d.getFullYear();
//    return `${day}-${month}-${year}`;
//}

function DeleteEmployee(empId) {
    if (!empId) return;
    swal({
        title: "Are you sure?",
        text: "Do you want to delete the selected record?",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55",
        confirmButtonText: "Yes, delete it!",
        cancelButtonText: "No, cancel!",
        closeOnConfirm: false,
        closeOnCancel: true
    }, function (isConfirm) {  
        if (isConfirm) {
            $.ajax({
                url: '/Employee/DeleteEmployee',
                type: 'POST',
                data: { id: empId },
                beforeSend: function () {
                    showLoader();
                },
                success: function (res) {
                    if (res.success) {
                        swal("Deleted!", res.message, "success");
                        FilterEmployees();
                    } else {
                        swal("Error!", res.message, "error");
                    }
                },
                error: function (err) {
                    console.error("Error deleting employee:", err);
                    swal("Error!", "Something went wrong.", "error");
                },
                complete: function () {
                    hideLoader();
                }
            });
        }
    });
}

function confirmLogout() {
    swal({
        title: "Are you sure?",
        text: "Do you really want to logout?",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55",
        confirmButtonText: "Yes, logout",
        cancelButtonText: "No, stay here",
        closeOnConfirm: false
    }, function (isConfirm) {
        if (isConfirm) {
            window.location.href = "/User/Logout";
        }
    });
}

function ClearSubmit() {
    $("#AddEmployeeDetail ")[0].reset();
    $("#EmployeeID").val("");
    $('#docPreview').html('');
    $('#EmpformFile').val('');
}

function loadDesignations(selectedId = null) {
    $.get('/Employee/GetAllDesignations', function (data) {
        var $dropdown = $('#EmpDesig, #ddlEmpdesig');
        $dropdown.empty();
        $dropdown.append('<option value="">--Select Designation--</option>');

        data.forEach(function (desig) {
            var selected = desig.designation_Id === selectedId ? 'selected' : '';
            $dropdown.append(
                `<option value="${desig.designation_Id}" ${selected}>${desig.designation_Name}</option>`
            );
        });
    });
}

function SaveDesignation() {
    var DesignationId = $('#DesignationID').val();

    var DesignationData = {
        Designation_Id: DesignationId,
        Designation_Name: $('#DesignationName').val(),
        Priority: parseInt($('#DPriority').val()) || 0,
        Inactive: $('#inactive').is(':checked') ? 1 : 0
    };

    var url = DesignationId ? '/Employee/UpdateDesignation' : '/Employee/SaveDesignation';

    $.ajax({
        url: url,
        type: 'POST',
        data: DesignationData,
        beforeSend: function () {
            showLoader();
        },
        success: function (response) {
           // $('#employeeModal').modal('hide');
            $('#AddEmpDesignation')[0].reset();
            $('#DesignationID').val('');
            showToast(response.message, true);
            FilterDesignation();
            loadDesignations();
        },
        error: function (err) {
            showToast(err.responseJSON?.message || "Server error", false);
        },
        complete: function () {
            hideLoader();
        }
    });
}

function FilterDesignation() {

    showLoader();

    $.ajax({
        url: '/Employee/FilterDesignation',
        type: 'GET',
        beforeSend: function () {
            showLoader();
        },
        success: function (designation) {

            if ($.fn.DataTable.isDataTable('#tbldesignation')) {
                $('#tbldesignation').DataTable().clear().destroy();
            }

            var tbody = $("#tbldesignation tbody");
            tbody.empty();

            $.each(designation, function (index, des) {
                var row = `<tr data-id="${des.designation_Id}">
                    <td>${des.designation_Name}</td>
                    <td>${des.priority}</td>
                    <td>${des.status}</td>
                    <td class="text-center">
                        <a href="#" class="btn btn-warning btn-sm btn-editdes"  data-id="${des.designation_Id}">Edit</a>
                        <a href="#"  class="btn btn-danger btn-sm btn-deletedes" data-id="${des.designation_Id}">Delete</a>
                    </td>
                </tr>`;
                tbody.append(row);
            });

            // Initialize DataTable
            $('#tbldesignation').DataTable({
                "paging": true,
                "searching": false,
                "ordering": true,
                "destroy": true,
                "pagelength": 10,
                lengthChange: false,

            });
        },
        error: function (xhr) {
            console.log("Error loading employees:", xhr.responseText);
        },
        complete: function () {
            hideLoader();
        }
    });
}

function GetDesignationByID(desId) {
    if (!desId) {
        console.error("Designation ID is required for editing.");
        return;
    }
    $('#designationlbl').text('Edit Designation');

    showLoader();

    $.ajax({
        url: '/Employee/GetDesignationbyId',
        type: 'GET',
        data: { id: desId },
        beforeSend: function () {
            showLoader();
        },
        success: function (des) {
            if (!des) {
                alert("Designation not found!");
                return;
            }

            $('#DesignationID').val(des.designation_Id);
            $('#DesignationName').val(des.designation_Name);
            $('#DPriority').val(des.priority);
            $('#inactive').prop('checked', des.inactive === 1);

        },
        error: function (xhr, status, error) {
            console.error("Error fetching employee:", error);
        },
        complete: function () {
            hideLoader();
        }
    });
}

function ClearDesignation() {
    $('#designationlbl').text('Designation Master');
    $("#AddEmpDesignation ")[0].reset();
    $("#DesignationId").val("");
}

function DeleteDesignation(desId) {

    if (!desId) return;

    swal({
        title: "Are you sure?",
        text: "Do you sure to delete selected record !",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: '#DD6B55',
        confirmButtonText: 'Yes, I am sure!',
        cancelButtonText: "No, cancel it!",
        closeOnConfirm: false,
        closeOnCancel: false
    }, function (isConfirm) {
        if (isConfirm) {
            $.ajax({
                url: '/Employee/DeleteDesignation',
                type: 'POST',
                data: { id: desId },
                beforeSend: function () {
                    showLoader();
                },
                success: function (res) {
                    if (res.success) {
                        showToast(res.message);
                        FilterDesignation();
                    } else {
                        showToast(res.message);
                    }
                },
                error: function (err) {
                    console.error("Error deleting employee:", err);
                },
                complete: function () {
                    hideLoader();
                }
            });
        }
    });
}



