using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication_List.Models;
using static MiniExcelLibs.MiniExcel;

public class EmployeeController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public EmployeeController(AppDbContext context, IWebHostEnvironment webHostEnvironment)

    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<IActionResult> FilterEmployees(string? Empname, Guid? EmpDesignation, int? EmpStatus)
    {
        try
        {
            var query = from e in _context.TbEmp_Employee
                        join d in _context.TbEmp_Designation
                            on e.Emp_DesignationId equals d.Designation_Id into desGroup
                        from d in desGroup.DefaultIfEmpty()
                        join u in _context.Users
                            on e.Created_By equals u.User_ID into createdGroup
                        from u in createdGroup.DefaultIfEmpty()
                        join m in _context.Users
                            on e.Modified_By equals m.User_ID into modifiedGroup
                        from m in modifiedGroup.DefaultIfEmpty()
                        select new
                        {
                            e.EmployeeID,
                            e.EmployeeName,
                            e.Emp_DesignationId,
                            DesignationName = d.Designation_Name,
                            e.Emp_Address,
                            e.DateofJoining,
                            CreatedByUsername = u.Username,
                            ModifiedByUsername = m != null ? m.Username : "",
                            Created_Time = e.Created_Time,
                            Modified_Time = e.Modified_Time,
                            e.Status
                        };

            // Optional filters
            if (!string.IsNullOrWhiteSpace(Empname))
                query = query.Where(x => x.EmployeeName.Contains(Empname));

            if (EmpDesignation.HasValue)
                query = query.Where(x => x.Emp_DesignationId == EmpDesignation.Value);

            if (EmpStatus.HasValue)
                query = query.Where(x => x.Status == EmpStatus.Value);

            var employees = await query.OrderByDescending(x => x.Created_Time).ToListAsync();

            return Json(employees);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: Employee/Create
    //[HttpPost]
    //public async Task<IActionResult> AddEmployee(TbEmp_Employee model)
    //{
    //    try
    //    {
    //        if (string.IsNullOrWhiteSpace(model.EmployeeName))
    //            return BadRequest(new { success = false, message = "Employee name is required" });

    //        var employee = new TbEmp_Employee
    //        {
    //            EmployeeID = Guid.NewGuid(),
    //            EmployeeName = model.EmployeeName,
    //            Emp_DesignationId = model.Emp_DesignationId,
    //            Emp_Address = model.Emp_Address,
    //            GenderId = model.GenderId,
    //            DateofJoining = model.DateofJoining,
    //            Created_By = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
    //            Created_Time = DateTime.UtcNow
    //        };

    //        _context.TbEmp_Employee.Add(employee);
    //        await _context.SaveChangesAsync();

    //        // Return JSON with success message
    //        return Json(new { success = true, message = "Employee added successfully" });
    //    }
    //    catch(Exception ex) {
    //        return BadRequest(ex.InnerException.Message);
    //    }
    //}

    [HttpPost]
    public async Task<IActionResult> AddEmployee(EmployeeVM model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Employee.EmployeeName))
                return BadRequest(new { success = false, message = "Employee name is required" });

            // 1️⃣ Save Employee
            var employee = new TbEmp_Employee
            {
                EmployeeID = Guid.NewGuid(),
                EmployeeName = model.Employee.EmployeeName,
                Emp_DesignationId = model.Employee.Emp_DesignationId,
                Emp_Address = model.Employee.Emp_Address,
                GenderId = model.Employee.GenderId,
                DateofJoining = model.Employee.DateofJoining,
                Status = model.Employee.Status,
                Created_By = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                Created_Time = DateTime.UtcNow
            };

            _context.TbEmp_Employee.Add(employee);
            await _context.SaveChangesAsync(); // ✔ Required for FK

            // 2️⃣ Save Document
            if (model.DocumentFile != null && model.DocumentFile.Length > 0)
            {
                var folderPath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "documents/employees");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid() +
                               Path.GetExtension(model.DocumentFile.FileName);

                var filePath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.DocumentFile.CopyToAsync(stream);

                var document = new TbEmp_Document
                {
                    EmployeeId = employee.EmployeeID,
                    DocumentName = model.DocumentName ?? model.DocumentFile.FileName,
                    DocumentPath = "/documents/employees/" + fileName,
                    CreatedDate = DateTime.UtcNow
                };

                _context.TbEmp_Document.Add(document);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = "Employee & document saved successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployeebyId(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Employee ID is required.");

        var employee = await _context.TbEmp_Employee
            .Where(e => e.EmployeeID == id)
            .Select(e => new
            {
                e.EmployeeID,
                e.EmployeeName,
                e.Emp_DesignationId,
                e.Emp_Address,
                e.Created_Time,
                e.Modified_Time,
                e.GenderId,
                e.DateofJoining,
                e.Status,

                // 👇 Fetch document
                Document = _context.TbEmp_Document
                    .Where(d => d.EmployeeId == e.EmployeeID)
                    .Select(d => new
                    {
                        d.DocumentName,
                        d.DocumentPath
                    })
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (employee == null)
            return NotFound();

        return Json(employee);
    }


    //[HttpPost]
    //public async Task<IActionResult> UpdateEmployee([FromForm] TbEmp_Employee model)
    //{
    //    if (model == null || model.EmployeeID == Guid.Empty)
    //    {
    //        return BadRequest(new { message = "Invalid employee data." });
    //    }

    //    // Find existing employee in database
    //    var existingEmployee = await _context.TbEmp_Employee.FindAsync(model.EmployeeID);
    //    if (existingEmployee == null)
    //    {
    //        return NotFound(new { message = "Employee not found." });
    //    }

    //    // Update fields
    //    existingEmployee.EmployeeName = model.EmployeeName;
    //    existingEmployee.Emp_DesignationId = model.Emp_DesignationId;
    //    existingEmployee.Emp_Address = model.Emp_Address;
    //    existingEmployee.Modified_By = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    //    existingEmployee.Modified_Time = DateTime.UtcNow;

    //    try
    //    {
    //        _context.TbEmp_Employee.Update(existingEmployee);
    //        await _context.SaveChangesAsync();

    //        return Json(new { success = true, message = "Employee updated successfully." });
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new { success = false, message = ex.Message });
    //    }
    //}

    [HttpPost]
    public async Task<IActionResult> UpdateEmployee([FromForm] EmployeeVM model)
    {
        if (model == null || model.Employee.EmployeeID == Guid.Empty)
            return BadRequest(new { success = false, message = "Invalid employee data." });

        // Find existing employee
        var existingEmployee = await _context.TbEmp_Employee.FindAsync(model.Employee.EmployeeID);
        if (existingEmployee == null)
            return NotFound(new { success = false, message = "Employee not found." });

        // Update employee fields
        existingEmployee.EmployeeName = model.Employee.EmployeeName;
        existingEmployee.Emp_DesignationId = model.Employee.Emp_DesignationId;
        existingEmployee.Emp_Address = model.Employee.Emp_Address;
        existingEmployee.GenderId = model.Employee.GenderId;
        existingEmployee.DateofJoining = model.Employee.DateofJoining;
        existingEmployee.Status = model.Employee.Status;
        existingEmployee.Modified_By = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        existingEmployee.Modified_Time = DateTime.UtcNow;

        try
        {
            // Update employee
            _context.TbEmp_Employee.Update(existingEmployee);
            await _context.SaveChangesAsync();

            // Handle document upload (replace or add new)
            if (model.DocumentFile != null && model.DocumentFile.Length > 0)
            {
                var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "documents/employees");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.DocumentFile.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.DocumentFile.CopyToAsync(stream);

                // Check if employee already has a document
                var existingDocument = await _context.TbEmp_Document
                    .FirstOrDefaultAsync(d => d.EmployeeId == existingEmployee.EmployeeID);

                if (existingDocument != null)
                {
                    // Optional: Delete old file from server
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingDocument.DocumentPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);

                    // Update existing document
                    existingDocument.DocumentName = model.DocumentName ?? model.DocumentFile.FileName;
                    existingDocument.DocumentPath = "/documents/employees/" + fileName;
                    existingDocument.CreatedDate = DateTime.UtcNow; // or ModifiedDate if you have
                    _context.TbEmp_Document.Update(existingDocument);
                }
                else
                {
                    // Add new document
                    var newDocument = new TbEmp_Document
                    {
                        EmployeeId = existingEmployee.EmployeeID,
                        DocumentName = model.DocumentName ?? model.DocumentFile.FileName,
                        DocumentPath = "/documents/employees/" + fileName,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.TbEmp_Document.Add(newDocument);
                }

                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = "Employee updated successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost] // or [HttpDelete]
    public async Task<IActionResult> DeleteEmployee(Guid id)
    {

        var employee = await _context.TbEmp_Employee.FindAsync(id);
        if (employee == null)
            return NotFound(new { success = false, message = "Employee not found" });

        try
        {
            _context.TbEmp_Employee.Remove(employee);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Employee deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // POST: Employee/Delete/5
    //[HttpPost, ActionName("Delete")]
    //[ValidateAntiForgeryToken]

    //public IActionResult Create()
    //{
    //    var vm = new EmployeeVM
    //    {
    //        Employee = new TbEmp_Employee(),
    //        Designations = _context.TbEmp_Designation
    //            .Select(d => new SelectListItem
    //            {
    //                Value = d.Designation_Id.ToString(),
    //                Text = d.Designation_Name
    //            })
    //            .ToList()
    //    };

    //    return View(vm);
    //}

    [HttpPost]
    public IActionResult Create(EmployeeVM vm)
    {
        if (!ModelState.IsValid)
        {
            // 🔁 Rebind dropdown
            vm.Designations = _context.TbEmp_Designation
                .Select(d => new SelectListItem
                {
                    Value = d.Designation_Id.ToString(),
                    Text = d.Designation_Name
                })
                .ToList();

            return View(vm);
        }


        // save employee
        //vm.Employee.Designation_Id = vm.SelectedDesignationId.Value;

        //_context.TbEmp_Employees.Add(vm.Employee);
        //_context.SaveChanges(); 

        return RedirectToAction("List");
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDesignations()
    {
        var list = await _context.TbEmp_Designation
                         .OrderBy(d => d.Designation_Name)
                         .Select(d => new
                         {
                             d.Designation_Id,
                             d.Designation_Name
                         })
                         .ToListAsync();

        return Json(list);
    }


    [HttpPost]
    public async Task<IActionResult> SaveDesignation(TbEmp_Designation model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Designation_Name))
                return BadRequest(new { success = false, message = "Designation name is required" });

            var designation = new TbEmp_Designation
            {
                Designation_Id = Guid.NewGuid(),
                Designation_Name = model.Designation_Name,
                Priority = model.Priority,
                Inactive = model.Inactive,
            };

            _context.TbEmp_Designation.Add(designation);
            await _context.SaveChangesAsync();

            // Return JSON with success message
            return Json(new { success = true, message = "Designation added successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.InnerException.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> FilterDesignation()
    {
        try
        {
            var designation = await (from d in _context.TbEmp_Designation                                   
                                   orderby d.Priority ascending
                                   select new
                                   {
                                       d.Designation_Id,
                                       d.Designation_Name,
                                       d.Priority,
                                       Status = d.Inactive == 0 ? "Active" : "Inactive"
                                   }).ToListAsync();


            return Json(designation);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDesignationbyId(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Designation ID is required.");

        var des = await _context.TbEmp_Designation.FindAsync(id);

        if (des == null)
            return NotFound();

        return Json(new
        {
            des.Designation_Id,
            des.Designation_Name,
            des.Priority,
            des.Inactive,

        });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateDesignation([FromForm] TbEmp_Designation model)
    {
        if (model == null || model.Designation_Id == Guid.Empty)
        {
            return BadRequest(new { message = "Invalid Designation data." });
        }

        // Find existing employee in database
        var existingDes = await _context.TbEmp_Designation.FindAsync(model.Designation_Id);
        if (existingDes == null)
        {
            return NotFound(new { message = "Designation not found." });
        }

        // Update fields
        existingDes.Designation_Name = model.Designation_Name;
        existingDes.Priority = model.Priority;
        existingDes.Inactive = model.Inactive;

        try
        {
            _context.TbEmp_Designation.Update(existingDes);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Designation updated successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost] // or [HttpDelete]
    public async Task<IActionResult> DeleteDesignation(Guid id)
    {

        var designation = await _context.TbEmp_Designation.FindAsync(id);
        if (designation == null)
            return NotFound(new { success = false, message = "Designation not found" });

        try
        {
            _context.TbEmp_Designation.Remove(designation);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Designation deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }


[HttpGet]
public async Task<IActionResult> ExportEmployeesToExcel(string? Empname, Guid? Emp_Designation, int? Status)
{
    try
    {
        // 1️⃣ Build query (without formatting DateTime yet)
        var query = from e in _context.TbEmp_Employee
                    join d in _context.TbEmp_Designation
                        on e.Emp_DesignationId equals d.Designation_Id into desGroup
                    from d in desGroup.DefaultIfEmpty()
                    join u in _context.Users
                        on e.Created_By equals u.User_ID into createdGroup
                    from u in createdGroup.DefaultIfEmpty()
                    join m in _context.Users
                        on e.Modified_By equals m.User_ID into modifiedGroup
                    from m in modifiedGroup.DefaultIfEmpty()
                    select new
                    {
                        e.EmployeeID,
                        e.EmployeeName,
                        e.Emp_DesignationId,
                        DesignationName = d.Designation_Name,
                        e.Emp_Address,
                        DateofJoining = e.DateofJoining,
                        CreatedByUsername = u.Username,
                        ModifiedByUsername = m != null ? m.Username : "",
                        Created_Time = e.Created_Time,
                        Modified_Time = e.Modified_Time,
                        e.Status
                    };

        // 2️⃣ Apply optional filters
        if (!string.IsNullOrWhiteSpace(Empname))
            query = query.Where(x => x.EmployeeName.Contains(Empname));

        if (Emp_Designation.HasValue)
            query = query.Where(x => x.Emp_DesignationId == Emp_Designation.Value);

        if (Status.HasValue)
            query = query.Where(x => x.Status == Status.Value);

        // 3️⃣ Fetch data to memory
        var employees = await query
            .OrderByDescending(x => x.Created_Time)
            .AsNoTracking()
            .ToListAsync();

        // 4️⃣ Convert DateTime to string for export
        var exportData = employees.Select(x => new
        {
            x.EmployeeID,
            x.EmployeeName,
            x.Emp_DesignationId,
            x.DesignationName,
            x.Emp_Address,
            DateofJoining = x.DateofJoining?.ToString("yyyy-MM-dd") ?? "",
            x.CreatedByUsername,
            x.ModifiedByUsername,
            Created_Time = x.Created_Time.ToString("yyyy-MM-dd HH:mm:ss"),
            Modified_Time = x.Modified_Time.HasValue ? x.Modified_Time.Value.ToString("yyyy-MM-dd HH:mm:ss") : ""
        }).ToList();

        // 5️⃣ Generate Excel file in memory
        using var stream = new MemoryStream();
        await stream.SaveAsAsync(exportData); // MiniExcel method
        stream.Position = 0;

        var fileName = $"Employees_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

        return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}


}
