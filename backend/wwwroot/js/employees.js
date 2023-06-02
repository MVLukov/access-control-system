"use-strict";
var connection = new signalR.HubConnectionBuilder().withUrl("/tagHub").build();

let form = document.getElementById("form");
let formBtn = document.getElementById("formBtn");
let tableData = document.getElementById("tableData");

let devices = document.getElementById("devices");
let employeeName = document.getElementById("employeeName");
let tagId = document.getElementById("tagId");

let btnPrevious = document.getElementById("btnPrevious");
let btnHome = document.getElementById("btnHome");
let btnNext = document.getElementById("btnNext");

let toastElm = document.getElementById("toast");
let toastImg = document.getElementById("toastImg");
let toastText = document.getElementById("toastText");

let empErr = document.getElementById("empErr");
let tagErr = document.getElementById("tagErr");

let foundTag = document.getElementById("foundTag");
var myModal = new bootstrap.Modal(document.getElementById("modal"), {});
let modalOpened = false;

let currentUserId = 0;
let page = 1;
let flag = false;
let employeeNameRegex = /^[A-Za-z0-9 ]{2,24}$/gm;
let tagIdRegex = /^[A-Fa-f0-9]{8}$/gm;

getEmployees(page);
getAllDevices();

btnPrevious.addEventListener("click", () => {
  if (page > 1) page--;
  if (page == 1) {
    document.getElementById("previous").classList.add("disabled");
  }

  document.getElementById("pageText").innerText = page;
  getEmployees(page);
});

btnHome.addEventListener("click", () => {
  page = 1;
  document.getElementById("pageText").innerText = page;
  document.getElementById("previous").classList.add("disabled");

  getEmployees(page);
});

btnNext.addEventListener("click", () => {
  page++;

  document.getElementById("pageText").innerText = page;
  document.getElementById("previous").classList.remove("disabled");
  getEmployees(page);
});

form.addEventListener("submit", async (e) => {
  e.preventDefault();
  clearErr();
  let devicesList = [];
  flag = false;

  for (const key in devices.options) {
    if (Object.hasOwnProperty.call(devices.options, key)) {
      const element = devices.options[key];

      if (element.selected) {
        devicesList.push({
          deviceId: element.value,
          deviceName: element.text,
          selected: true,
        });
      } else {
        devicesList.push({
          deviceId: element.value,
          deviceName: element.text,
          selected: false,
        });
      }
    }
  }

  if (!employeeName.value.match(employeeNameRegex)) {
    flag = true;
    employeeName.classList.add("is-invalid");
    empErr.innerText =
      "Employee name must be up to 24 symbols long and contain only letters, numbers and single whitespace!";
  }

  if (!tagId.value.match(tagIdRegex) && tagId.value != "") {
    flag = true;
    tagId.classList.add("is-invalid");
    tagErr.innerText =
      "Tag ID must be 8 symbols long and contain only letters and numbers!";
  }

  if (!flag) {
    if (currentUserId != 0) {
      let dataToSend = {
        id: currentUserId,
        employeeName: employeeName.value,
        tagId: tagId.value == "" ? null : tagId.value,
        devices: devicesList,
      };

      let res = await fetch(`/Home/UpdateEmployee`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(dataToSend),
      });

      if (res.redirected) {
        window.location.href = res.url;
        return;
      }

      let data = await res.text();

      if (res.status == 200) {
        if (data == "TAG_ERR") {
          tagId.classList.add("is-invalid");
          tagErr.innerText = "Tag already exists!";
        } else if (data == "EMP_ERR") {
          employeeName.classList.add("is-invalid");
          empErr.innerText = "Employee already exists";
        } else {
          const toast = new bootstrap.Toast(toastElm);
          toastText.innerText = "Employee updated successfully!";
          toastImg.src = "../success.svg";

          toast.show();
          clearForm();
        }
      } else {
        const toast = new bootstrap.Toast(toastElm);
        toastText.innerText = "Something went wrong!";
        toastImg.src = "../error.svg";

        toast.show();
      }
    } else {
      let dataToSend = {
        employeeName: employeeName.value,
        tagId: tagId.value == "" ? null : tagId.value,
        devices: devicesList,
      };

      let res = await fetch(`/Home/AddEmployee`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(dataToSend),
      });

      if (res.redirected) {
        window.location.href = res.url;
        return;
      }

      let data = await res.text();

      if (res.status == 200) {
        if (data == "TAG_ERR") {
          tagId.classList.add("is-invalid");
          tagErr.innerText = "Tag already exists!";
        } else if (data == "EMP_ERR") {
          employeeName.classList.add("is-invalid");
          empErr.innerText = "Employee already exists!";
        } else {
          const toast = new bootstrap.Toast(toastElm);
          toastText.innerText = "Employee added successfully!";
          toastImg.src = "../success.svg";

          toast.show();
          clearForm();
        }
      } else {
        const toast = new bootstrap.Toast(toastElm);
        toastText.innerText = "Something went wrong!";
        toastImg.src = "../error.svg";

        toast.show();
      }
    }
  }

  page = 1;
  getEmployees(page);
});

async function getEmployees(page) {
  let res = await fetch(`/Home/GetAllEmployees?page=${page}`);

  if (res.redirected) {
    window.location.href = res.url;
    return;
  }

  let data = await res.json();
  let dataLength = data.length;

  if (dataLength == 11) {
    document.getElementById("next").classList.remove("disabled");
    document.getElementById("pagination").classList.remove("d-none");
    dataLength--;
  } else {
    document.getElementById("next").classList.add("disabled");
    if (page == 1) {
      document.getElementById("pagination").classList.add("d-none");
    }
  }

  loadData(data, dataLength);
}

async function deleteEmployee(id) {
  let res = await fetch(`/Home/DeleteEmployee?employeeId=${id}`);

  if (res.redirected) {
    window.location.href = res.url;
    return;
  }

  clearErr();
  clearForm();

  if (res.status == 200) {
    formBtn.innerText = "Add employee";

    for (const key in devices.options) {
      if (Object.hasOwnProperty.call(devices.options, key)) {
        const element = devices.options[key];
        element.selected = false;
      }
    }

    devices.loadOptions();

    const toast = new bootstrap.Toast(toastElm);
    toastText.innerText = "Employee deleted successfully!";
    toastImg.src = "../success.svg";

    toast.show();
    getEmployees(page);
  } else {
    const toast = new bootstrap.Toast(toastElm);
    toastText.innerText = "Something went wrong!";
    toastImg.src = "../error.svg";

    toast.show();
  }
}

function getEmployeeHistory(employeeId) {
  window.location.href = `/Home/EmployeeHistory?employeeId=${employeeId}`;
}

async function getAllDevices() {
  let res = await fetch("/Home/getDevices");

  if (res.redirected) {
    window.location.href = res.url;
    return;
  }

  let data = await res.json();

  devices.innerHTML = "";

  data.forEach((el) => {
    if (el.selected) {
      devices.innerHTML += `<option selected value=${el.deviceId}>${el.deviceName}</option>`;
    } else {
      devices.innerHTML += `<option value=${el.deviceId}>${el.deviceName}</option>`;
    }
  });

  devices.loadOptions();
}

async function getAllDevicesOfEmployee(id) {
  let res = await fetch(`/Home/getDevicesOfEmployee?employeeId=${id}`);

  if (res.redirected) {
    window.location.href = res.url;
    return;
  }

  let data = await res.json();

  devices.innerHTML = "";

  data.forEach((el) => {
    if (el.selected) {
      devices.innerHTML += `<option selected value=${el.deviceId}>${el.deviceName}</option>`;
    } else {
      devices.innerHTML += `<option value=${el.deviceId}>${el.deviceName}</option>`;
    }
  });

  devices.loadOptions();
}

async function rowClick(employee) {
  let row = document.getElementById(employee.id);
  clearErr();

  if (row.classList.contains("active")) {
    row.classList.remove("active");
    formBtn.innerText = "Add employee";

    clearForm();
  } else {
    getAllDevicesOfEmployee(employee.id);

    row.classList.add("active");
    formBtn.innerText = "Update employee";
    currentUserId = employee.id;

    employeeName.value = employee.employeeName;
    tagId.value = employee.tagId;
  }

  Array.from(document.querySelectorAll(".active")).forEach((row) => {
    if (row.id != employee.id) {
      row.classList.remove("active");
    }
  });
}

async function toggleAccess(id) {
  let res = await fetch(`/Home/ToggleEmployeeAccess?employeeId=${id}`);

  if (res.redirected) {
    window.location.href = res.url;
    return;
  }

  if ((await res.status) == 200) {
    getEmployees(page);
  }
}

function deleteRows() {
  var child = tableData.lastElementChild;

  while (child) {
    tableData.removeChild(child);
    child = tableData.lastElementChild;
  }
}

function clearForm() {
  formBtn.innerText = "Add employee";
  employeeName.value = "";
  tagId.value = "";
  currentUserId = 0;

  for (const key in devices.options) {
    if (Object.hasOwnProperty.call(devices.options, key)) {
      const element = devices.options[key];
      element.selected = false;
    }
  }
  devices.loadOptions();
}

function loadData(data, dataLength) {
  deleteRows();

  if (dataLength == 0) {
    let row = document.createElement("tr");

    for (let i = 0; i < 4; i++) {
      row.innerHTML += "<td>Empty </td>";
    }
    tableData.append(row);

    return;
  }

  for (let i = 0; i < dataLength; i++) {
    const employee = data[i];
    let row = document.createElement("tr");

    row.classList.add("tr-hover");
    row.id = employee.id;

    row.innerHTML += `<td>${employee.employeeName}</td>`;
    row.innerHTML += `<td>
        ${employee.tagId == null ? "unknown tag" : employee.tagId}
        </td>`;

    if (employee.isAllowed) {
      let html = `<td><div class="form-check form-switch ">
        <input class="form-check-input" type="checkbox" role="switch" style="transform: scale(1.2);" onclick="event.stopPropagation(); toggleAccess(${employee.id})" checked>
      </div></td>`;
      row.innerHTML += html;
    } else {
      let html = `<td><div class="form-check form-switch ">
        <input class="form-check-input" type="checkbox" role="switch" style="transform: scale(1.2);" onclick="event.stopPropagation(); toggleAccess(${employee.id})">
      </div></td>`;
      row.innerHTML += html;
    }

    let buttons = `<td>
    <button class="btn btnColor" onclick="event.stopPropagation(); getEmployeeHistory('${employee.id}')"><img class="trashIco" src="../info.svg"></button>
    <button class="btn btnColor" onclick="event.stopPropagation(); deleteEmployee('${employee.id}')"><img class="trashIco" src="../trash.svg"></button>
    </td>`;
    row.innerHTML += buttons;

    row.addEventListener("click", (e) => {
      e.stopPropagation();
      rowClick(employee);
    });

    tableData.append(row);
  }
}

function clearErr() {
  employeeName.classList.remove("is-invalid");
  tagId.classList.remove("is-invalid");
  empErr.innerText = "";
  tagErr.innerText = "";
}

function scanForTags() {
  modalOpened = true;
  myModal.show();
}

function selectTag() {
  clearModal();
  myModal.hide();
  tagId.value = foundTag.innerText;
  modalOpened = false;
}

function clearModal() {
  document.getElementById("hideScanHeader").classList.remove("d-none");
  foundTag.classList.add("d-none");
  foundTag.classList.remove("tagHover");
  modalOpened = false;
}

async function searchEmployee() {
  clearForm();

  let empName = document.getElementById("searchEmployee").value;
  page = 1;
  document.getElementById("pageText").innerText = page;
  document.getElementById("previous").classList.add("disabled");
  document.getElementById("pagination").classList.add("d-none");

  if (empName == "") {
    getEmployees(page);
    return;
  }

  let res = await fetch(`/Home/SearchEmployee?employeeName=${empName}`);

  if (res.redirected) {
    window.location.href = res.url;
    return;
  }

  let data = await res.json();

  loadData(data, data.length);
}

connection.on("tagRead", async (uid) => {
  if (modalOpened) {
    document.getElementById("hideScanHeader").classList.add("d-none");
    foundTag.classList.remove("d-none");
    foundTag.classList.add("tagHover");
    foundTag.innerText = uid;
  }
});

connection
  .start()
  .then(function () {
    console.log("Start hub");
    //document.getElementById("sendButton").disabled = false;
  })
  .catch(function (err) {
    return console.error(err.toString());
  });
