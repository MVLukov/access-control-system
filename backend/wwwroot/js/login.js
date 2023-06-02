"use_strict";

let form = document.getElementById("loginForm");
let username = document.getElementById("username");
let password = document.getElementById("password");
let usernameErr = document.getElementById("usernameErr");
let passErr = document.getElementById("passwordErr");
let flag = false;

form.addEventListener("submit", async (e) => {
  e.preventDefault();

  usernameErr.innerText = "";
  passErr.innerText = "";
  username.classList.remove("is-invalid");
  password.classList.remove("is-invalid");
  flag = false;

  let passRegex =
    /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,24}$/;
  let nameRegex = /^[a-zA-Z]{2,}$/;

  if (!password.value.match(passRegex)) {
    password.classList.add("is-invalid");
    passErr.innerText =
      "Password must contain at least one number, one letter and one special character!";
    flag = true;
  }

  if (!username.value.match(nameRegex)) {
    username.classList.add("is-invalid");
    usernameErr.innerText = "User name must contain only letters!";
    flag = true;
  }

  if (!flag) {
    try {
      let req = await fetch("/Home/Login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          Username: username.value,
          Password: password.value,
        }),
        redirect: "follow",
      });

      if (req.redirected) {
        window.location.href = req.url;
      }

      let statusText = await req.text();

      if (statusText == "InvalidCredentials") {
        usernameErr.innerText = "Invalid credentials!";
        username.classList.add("is-invalid");

        passErr.innerText = "Invalid credentials!";
        password.classList.add("is-invalid");
      }
    } catch (error) {
      console.log(error);
    }
  } else {
    if (username.value == "") {
      usernameErr.innerText = "Username is required!";
      username.classList.add("is-invalid");
    }

    if (password.value == "") {
      passErr.innerText = "Password is required!";
      password.classList.add("is-invalid");
    }
  }
});
