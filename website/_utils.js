/**
 * @param {any} object
 */
function objectToQuery(object) {
  let segments = [];
  for (let field in object) {
    segments.push(
      encodeURIComponent(field) +
      "=" +
      encodeURIComponent(object[field])
    );
  }

  return segments.join("&");
}

/**
 * @param {string} path
 * @param {any} queryObject
 * @returns {Promise<any>}
 */
export async function get(path, queryObject) {
  let response = await fetch(
    path + "?" + objectToQuery(queryObject),
    { method: "GET" }
  );

  let text = await response.text();
  try { return JSON.parse(text); }
  catch {
    if (text.length > 0) { return text; }
    else { return null; }
  }
}

/** 
 * @param {string} path
 * @param {any} bodyObject
 * @returns {Promise<any | null>}
*/
export async function send(path, bodyObject) {
  let response = await fetch(path, {
    method: "POST",
    body: JSON.stringify(bodyObject),
  });

  let text = await response.text();
  try { return JSON.parse(text); }
  catch {
    if (text.length > 0) { return text; }
    else { return null; }
  }
}

/**
 * @returns {any}
 */
export function getQuery() {
  return Object.fromEntries(new URLSearchParams(location.search).entries());
}