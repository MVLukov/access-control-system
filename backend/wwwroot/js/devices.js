let form = document.getElementById("deviceForm");
let tableData = document.getElementById("tableData");
let formBtn = document.getElementById("formBtn");

let btnPrevious = document.getElementById("btnPrevious");
let btnHome = document.getElementById("btnHome");
let btnNext = document.getElementById("btnNext");

let toastElm = document.getElementById("toast");
let toastImg = document.getElementById("toastImg");
let toastText = document.getElementById("toastText");

let deviceIdErr = document.getElementById("deviceIdErr");
let deviceNameErr = document.getElementById("deviceNameErr");

let deviceId = document.getElementById("deviceId");
let deviceName = document.getElementById("deviceName");

let selectedDevice = 0;
let page = 1;
let deviceIdRegex = /^[A-Za-z0-9]{4}$/gm;
let deviceNameRegex = /^[A-Za-zА-Яа-я0-9_]{2,24}$/gm;
let flag = false;

getDevices(page);

btnPrevious.addEventListener("click", () => {
  if (page > 1) page--;
  if (page == 1) {
    document.getElementById("previous").classList.add("disabled");
  }

  document.getElementById("pageText").innerText = page;
  getDevices(page);
});

btnHome.addEventListener("click", () => {
  page = 1;
  document.getElementById("pageText").innerText = page;
  document.getElementById("previous").classList.add("disabled");

  getDevices(page);
});

btnNext.addEventListener("click", () => {
  page++;

  document.getElementById("pageText").innerText = page;
  document.getElementById("previous").classList.remove("disabled");
  getDevices(page);
});

form.addEventListener("submit", async (event) => {
  event.preventDefault();

  clearErr();
  flag = false;

  if (!deviceId.value.match(deviceIdRegex)) {
    flag = true;
    deviceId.classList.add("is-invalid");
    deviceIdErr.innerText =
      "Device ID must be 4 symbols long and contain only numbers and letters!";
  }

  if (!deviceName.value.match(deviceNameRegex)) {
    flag = true;
    deviceName.classList.add("is-invalid");
    deviceNameErr.innerText =
      "Device name must be up to 24 symbols long and contain numbers, letters and _!";
  }

  if (!flag) {
    if (selectedDevice != 0) {
      let res = await fetch("/Home/UpdateDevice", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          deviceId: deviceId.value,
          deviceName: deviceName.value,
        }),
      });

      if (res.redirected) {
        window.location.href = res.url;
        return;
      }

      let data = await res.text();

      if (res.status == 200) {
        if (data == "DEV_NAME_ERR") {
          deviceName.classList.add("is-invalid");
          deviceNameErr.innerText = "Device with this name already exists!";
        } else {
          const toast = new bootstrap.Toast(toastElm);
          toastText.innerText = "Device updated successfully!";
          toastImg.src = "../success.svg";

          toast.show();
          clearForm();
        }
      } else {
        if (res.data == "DEV_ERR") {
          const toast = new bootstrap.Toast(toastElm);
          toastText.innerText = "Something went wrong!";
          toastImg.src = "../error.svg";

          toast.show();
        }
      }
    } else {
      let res = await fetch("/Home/AddDevice", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          deviceId: deviceId.value,
          deviceName: deviceName.value,
        }),
      });

      if (res.redirected) {
        window.location.href = res.url;
        return;
      }

      let data = await res.text();

      if (res.status == 200) {
        if (data == "DEV_ID_ERR") {
          deviceId.classList.add("is-invalid");
          deviceIdErr.innerText = "Device with this ID already exists!";
        } else if (data == "DEV_NAME_ERR") {
          deviceName.classList.add("is-invalid");
          deviceNameErr.innerText = "Device with this name already exists!";
        } else {
          const toast = new bootstrap.Toast(toastElm);
          toastText.innerText = "Device added successfully!";
          toastImg.src = "../success.svg";

          toast.show();
          clearForm();
        }
      } else {
        if (res.data == "DEV_ERR") {
          const toast = new bootstrap.Toast(toastElm);
          toastText.innerText = "Something went wrong!";
          toastImg.src = "../error.svg";

          toast.show();
        }
      }
    }

    page = 1;
    getDevices(page);

    Array.from(document.querySelectorAll(".active")).forEach((row) => {
      row.classList.remove("active");
    });
  }
});

async function getDevices(page) {
  let res = await fetch(`/Home/GetDevices?page=${page}`);

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

  deleteRows();

  if (data.length == 0) {
    let row = document.createElement("tr");
    for (let i = 0; i < 3; i++) {
      row.innerHTML += "<td>Empty</td>";
    }
    tableData.append(row);
  }

  for (let i = 0; i < dataLength; i++) {
    let device = data[i];
    let row = document.createElement("tr");

    row.id = device.deviceId;
    row.classList.add("tr-hover");

    row.addEventListener("click", () => {
      rowClick(device);
    });

    row.innerHTML = `<td>${device.deviceId}</td>`;
    row.innerHTML += `<td>${device.deviceName}</td>`;
    row.innerHTML += `<td><button class="btn btnColor" onclick="event.stopPropagation(); deleteDevice('${device.deviceId}')"><img class="trashIco" src="../trash.svg"></button></td>`;

    tableData.append(row);
  }
}

async function deleteDevice(deviceId) {
  let res = await fetch(`/Home/deleteDevice?deviceId=${deviceId}`);

  if (res.redirected) {
    window.location.href = res.url;
    return;
  }

  let data = await res.text();

  if (res.status == 200) {
    if (data == "DEV_ERR") {
      const toast = new bootstrap.Toast(toastElm);
      toastText.innerText = "Device wasn't deleted!";
      toastImg.src = "../error.svg";

      toast.show();
    } else {
      const toast = new bootstrap.Toast(toastElm);
      toastText.innerText = "Device added successfully!";
      toastImg.src = "../success.svg";

      toast.show();
      getDevices(page);
    }

    console.log(res.status);
  } else {
    const toast = new bootstrap.Toast(toastElm);
    toastText.innerText = "Something went wrong!";
    toastImg.src = "../error.svg";
    toast.show();
  }

  clearForm();
  clearErr();
}

function rowClick(device) {
  let row = document.getElementById(device.deviceId);
  clearErr();

  if (row.classList.contains("active")) {
    row.classList.remove("active");
    clearForm();
  } else {
    row.classList.add("active");
    formBtn.innerText = "Update device";
    selectedDevice = device.deviceId;

    document.getElementById("deviceId").disabled = true;
    document.getElementById("deviceId").value = device.deviceId;
    document.getElementById("deviceName").value = device.deviceName;
  }

  Array.from(document.querySelectorAll(".active")).forEach((row) => {
    if (row.id != device.deviceId) {
      row.classList.remove("active");
    }
  });
}

function deleteRows() {
  var child = tableData.lastElementChild;

  while (child) {
    tableData.removeChild(child);
    child = tableData.lastElementChild;
  }
}

function clearForm() {
  formBtn.innerText = "Add device";
  document.getElementById("deviceId").disabled = false;
  document.getElementById("deviceId").value = "";
  document.getElementById("deviceName").value = "";
  selectedDevice = 0;
}

function clearErr() {
  deviceId.classList.remove("is-invalid");
  deviceName.classList.remove("is-invalid");

  deviceIdErr.innerText = "";
  deviceNameErr.innerText = "";
}
