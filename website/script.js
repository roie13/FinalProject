

import Cookies from "./_cookies";
import { send } from "./_utils";

let container = document.getElementsByClassName("container")[0];
let img = document.getElementsByTagName("img")[3];
let New = document.getElementsByClassName("New")[0];
let img2 = document.getElementsByTagName("img")[4];

let input0 = document.getElementsByTagName("input")[0];
let input1 = document.getElementsByTagName("input")[1];
let input2 = document.getElementsByTagName("input")[2];

/**@type {HTMLButtonElement} */
let addButton = document.getElementById("addButton");

addButton.onclick = function () {
    let dish = {
        name: input0.value,
        img: input1.value,
        description: input2.value,
    };
    send("/addDish", dish);

    input0.value = "";
    input1.value = "";
    input2.value = "";

    getAll();
};

async function getAll() {
    let dishes = await send("/getalldishes");

    console.log(dishes);

    container.innerHTML = null;
    for (let i = 0; i < dishes.length; i++) {
        createDish(dishes[i].Name, dishes[i].Image, dishes[i].Id);
    }
}


async function createDish(name, src, dishId) {
    let firstName = name.split(" ")[0];

    // תבנית
    let dishDiv = document.createElement("div");
    dishDiv.classList.add("dish");
    container.appendChild(dishDiv);
    // קישור
    let a = document.createElement("a");
    a.href = "clickpage.html?dish=" + firstName;
    dishDiv.appendChild(a);
    // תמונה
    let image = document.createElement("img");
    image.width = 150;
    image.height = 150;
    a.appendChild(image);
    image.src = src;
    // שם ותיבת סימון
    let nameAndCheckboxDiv = document.createElement("div");
    nameAndCheckboxDiv.classList.add("name-and-checkbox");
    dishDiv.appendChild(nameAndCheckboxDiv);

    //קופסת בדיקה
    const Checkbox = document.createElement("input");
    Checkbox.type = "checkbox";
    nameAndCheckboxDiv.appendChild(Checkbox);
    var isChecked = await send("/isChecked", {
        userId: Cookies.get("id"),
        dishId: dishId,
    });
    console.log(isChecked);
    Checkbox.checked = isChecked;

    Checkbox.onclick = function() {
        send("/check", {
            userId: Cookies.get("id"),
            dishId: dishId,
            isChecked: Checkbox.checked,
        });
    };

    // שם
    let nameDiv = document.createElement("div");
    nameDiv.innerText = name;
    nameAndCheckboxDiv.appendChild(nameDiv);
}



getAll();

