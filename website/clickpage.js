import { send } from "./_utils";

let title1 = document.getElementsByTagName("title");
let image1 = document.getElementsByTagName("img")[0];
let h1 = document.getElementsByTagName("h1")[0];
let descrip = document.getElementsByClassName("description")[0];

let query = new URLSearchParams(window.location.search);

async function getDish() {
    /**
     * @type {{
    *  Description: string,
    *  Image: string,
    *  Name: string,
    * }}
    */
    let dishData = await send("/getDish", query.get("dish"));

    console.log(dishData);

    image1.src = dishData.Image;
    h1.innerText = dishData.Name;
    descrip.innerText = dishData.Description;

}

getDish();