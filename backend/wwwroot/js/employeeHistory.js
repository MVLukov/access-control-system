"use_strict";

let tableData = document.getElementById("historyTable");
let btnPrevious = document.getElementById("btnPrevious");
let btnHome = document.getElementById("btnHome");
let btnNext = document.getElementById("btnNext");

let datePicker = document.getElementById("datePicker");
let page = 1;
let date = "";

const urlSearchParams = new URLSearchParams(window.location.search);
const params = Object.fromEntries(urlSearchParams.entries());

datePicker.addEventListener("change", () => {
  date = new Date(datePicker.value).toISOString().substring(0, 10);
  page = 1;
  document.getElementById("pageText").innerText = page;
  document.getElementById("previous").classList.add("disabled");
  document.getElementById("rstDate").classList.remove("d-none");
  getData();
});

btnPrevious.addEventListener("click", () => {
  if (page > 1) page--;
  if (page == 1) {
    document.getElementById("previous").classList.add("disabled");
  }

  document.getElementById("pageText").innerText = page;
  getData();
});

btnHome.addEventListener("click", () => {
  page = 1;
  document.getElementById("pageText").innerText = page;
  document.getElementById("previous").classList.add("disabled");

  getData();
});

btnNext.addEventListener("click", () => {
  page++;
  document.getElementById("pageText").innerText = page;
  document.getElementById("previous").classList.remove("disabled");
  getData();
});

getData();

async function getData() {
  deleteRows();
  let employeeId = params?.employeeId;

  if (employeeId != null) {
    let res;

    if (date == "") {
      res = await fetch(
        `/Home/GetEmployeeHistory?employeeId=${employeeId}&page=${page}`
      );
    } else {
      res = await fetch(
        `/Home/GetEmployeeHistory?employeeId=${employeeId}&page=${page}&date=${date}`
      );
    }

    let data = await res.json();
    let dataLength = data.length;

    if (dataLength == 16) {
      document.getElementById("next").classList.remove("disabled");
      document.getElementById("pagination").classList.remove("d-none");
      dataLength--;
    } else {
      document.getElementById("next").classList.add("disabled");
      if (page == 1) {
        document.getElementById("pagination").classList.add("d-none");
      }
    }

    if (dataLength == 0) {
      let row = document.createElement("tr");
      for (let i = 0; i < 4; i++) {
        row.innerHTML += `<td>Empty</td>`;
      }
      tableData.append(row);

      return;
    }

    for (let i = 0; i < dataLength; i++) {
      let el = data[i];
      let row = document.createElement("tr");

      row.innerHTML = `<td>${
        el.employeeName == null ? "not specified" : el.employeeName
      }</td>`;
      // row.innerHTML += `<td>${el.tagId}</td>`;

      if (el.declined) {
        row.innerHTML += `<td class="text text-danger">Access denied</td>`;
      } else {
        row.innerHTML += `<td class="text text-success">Access allowed</td>`;
      }

      row.innerHTML += `<td>${
        el.deviceName == null ? "unknown device" : el.deviceName
      }</td>`;

      row.innerHTML += `<td>${new Date(
        el.timestamp + "Z"
      ).toLocaleString()}</td>`;

      tableData.append(row);
    }
  }
}

function deleteRows() {
  var child = tableData.lastElementChild;

  while (child) {
    tableData.removeChild(child);
    child = tableData.lastElementChild;
  }
}

function resetDate() {
  date = "";
  datePicker.value = "";
  page = 1;
  document.getElementById("rstDate").classList.add("d-none");
  getData();
}
