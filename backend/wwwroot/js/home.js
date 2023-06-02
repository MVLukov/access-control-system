"use_strict";
var connection = new signalR.HubConnectionBuilder().withUrl("/tagHub").build();

let tableData = document.getElementById("historyTable");
let btnPrevious = document.getElementById("btnPrevious");
let btnHome = document.getElementById("btnHome");
let btnNext = document.getElementById("btnNext");
let datePicker = document.getElementById("datePicker");

let page = 1;
let date = "";

datePicker.addEventListener("change", () => {
  date = new Date(datePicker.value).toISOString().substring(0, 10);
  page = 1;
  document.getElementById("pageText").innerText = page;
  document.getElementById("previous").classList.add("disabled");
  document.getElementById("rstDate").classList.remove("d-none");
  fetchData();
});

fetchData();

btnPrevious.addEventListener("click", () => {
  if (page > 1) page--;
  if (page == 1) {
    document.getElementById("previous").classList.add("disabled");
  }

  document.getElementById("pageText").innerText = page;
  fetchData();
});

btnHome.addEventListener("click", () => {
  page = 1;
  document.getElementById("pageText").innerText = page;
  document.getElementById("previous").classList.add("disabled");

  fetchData();
});

btnNext.addEventListener("click", () => {
  page++;
  document.getElementById("pageText").innerText = page;
  document.getElementById("previous").classList.remove("disabled");
  fetchData();
});

async function fetchData() {
  deleteRows();
  let res;

  if (date == "") {
    res = await fetch(`/Home/GetHistory?page=${page}`);
  } else {
    res = await fetch(`/Home/GetHistory?page=${page}&date=${date}`);
  }

  if (res.redirected) {
    window.location.href = res.url;
    return;
  }

  let data = await res.json();
  let dataLength = data.length;

  if (dataLength == 0) {
    let row = document.createElement("tr");
    for (let i = 0; i < 4; i++) {
      row.innerHTML += "<td>Empty</td>";
    }
    tableData.append(row);
  }

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

  for (let i = 0; i < dataLength; i++) {
    let el = data[i];
    let row = document.createElement("tr");

    row.innerHTML = `<td>${
      el.employeeName == null ? "unknown" : el.employeeName
    }</td>`;

    if (el.declined) {
      row.innerHTML += `<td class="text text-danger">Access declined</td>`;
    } else {
      row.innerHTML += `<td class="text text-success">Access allowed</td>`;
    }

    row.innerHTML += `<td>${
      el.deviceName == null ? "unknown" : el.deviceName
    }</td>`;

    row.innerHTML += `<td>${new Date(
      el.timestamp + "Z"
    ).toLocaleString()}</td>`;

    tableData.append(row);
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
  fetchData();
}

connection.on("tagRead", async (uid) => {
  page = 1;
  date = "";
  fetchData();
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
