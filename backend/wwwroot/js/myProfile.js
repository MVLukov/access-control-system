"use_strict";

let form = document.getElementById("form");
let currPassword = document.getElementById("currPassword");
let newPassword = document.getElementById("newPassword");
let currPassErr = document.getElementById("currPassErr");
let newPassErr = document.getElementById("newPassErr");
let flag = false;

form.addEventListener("submit", async (e) => {
  e.preventDefault();

  currPassErr.innerText = "";
  newPassErr.innerText = "";
  currPassword.classList.remove("is-invalid");
  newPassword.classList.remove("is-invalid");
  flag = false;

  let passRegex =
    /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,24}$/;

  if (!currPassword.value.match(passRegex)) {
    currPassword.classList.add("is-invalid");
    currPassErr.innerText =
      "Password must contain at least one number, one letter and one special character! (8-24)";
    flag = true;
  }

  if (!newPassword.value.match(passRegex)) {
    newPassword.classList.add("is-invalid");
    newPassErr.innerText =
      "Password must contain at least one number, one letter and one special character! (8-24)";
    flag = true;
  }

  if (!flag) {
    let res = await fetch("/Home/ChangePassword", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        CurrentPassword: currPassword.value,
        NewPassword: newPassword.value,
      }),
    });

    if (res.redirected) {
      window.location.href = res.url;
      return;
    }

    let text = await res.text();

    if (res.status == 200) {
      if (text == "INVALID_CREDENTIALS") {
        currPassword.classList.add("is-invalid");
        currPassErr.innerText = "Invalid credentials!";
      }
    }
  } else {
    if (currPassword.value == "") {
      currPassword.classList.add("is-invalid");
      currPassErr.innerText = "Password is required!";
    }

    if (newPassword.value == "") {
      newPassword.classList.add("is-invalid");
      newPassErr.innerText = "Password is required!";
    }
  }
});
