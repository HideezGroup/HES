async function createCredentials(credentialsOptionsJson) {
    let credentialsOptions = JSON.parse(credentialsOptionsJson);
    credentialsOptions.challenge = coerceToArrayBuffer(credentialsOptions.challenge);
    credentialsOptions.user.id = coerceToArrayBuffer(credentialsOptions.user.id);
    credentialsOptions.excludeCredentials = credentialsOptions.excludeCredentials.map((c) => {
        c.id = coerceToArrayBuffer(c.id);
        return c;
    });

    let credentials = await navigator.credentials.create({
        publicKey: credentialsOptions
    });

    let data = {
        id: credentials.id,
        rawId: coerceToBase64Url(new Uint8Array(credentials.rawId)),
        type: credentials.type,
        extensions: credentials.getClientExtensionResults(),
        response: {
            AttestationObject: coerceToBase64Url(new Uint8Array(credentials.response.attestationObject)),
            clientDataJson: coerceToBase64Url(new Uint8Array(credentials.response.clientDataJSON))
        }
    };

    return JSON.stringify(data);
}

async function getCredentials(assertionOptionsJson) {
    let assertionOptions = JSON.parse(assertionOptionsJson);
    // todo: switch this to coercebase64
    const challenge = assertionOptions.challenge.replace(/-/g, "+").replace(/_/g, "/");
    assertionOptions.challenge = Uint8Array.from(atob(challenge), c => c.charCodeAt(0));
    // fix escaping. Change this to coerce
    assertionOptions.allowCredentials.forEach(function (listItem) {
        var fixedId = listItem.id.replace(/\_/g, "/").replace(/\-/g, "+");
        listItem.id = Uint8Array.from(atob(fixedId), c => c.charCodeAt(0));
    });

    let credential = await navigator.credentials.get({ publicKey: assertionOptions })

    // Move data into Arrays incase it is super long
    let authData = new Uint8Array(credential.response.authenticatorData);
    let clientDataJSON = new Uint8Array(credential.response.clientDataJSON);
    let rawId = new Uint8Array(credential.rawId);
    let sig = new Uint8Array(credential.response.signature);
    const data = {
        id: credential.id,
        rawId: coerceToBase64Url(rawId),
        type: credential.type,
        extensions: credential.getClientExtensionResults(),
        response: {
            authenticatorData: coerceToBase64Url(authData),
            clientDataJson: coerceToBase64Url(clientDataJSON),
            signature: coerceToBase64Url(sig)
        }
    };

    return JSON.stringify(data);
}

coerceToArrayBuffer = function (thing, name) {
    if (typeof thing === "string") {
        // base64url to base64
        thing = thing.replace(/-/g, "+").replace(/_/g, "/");

        // base64 to Uint8Array
        var str = window.atob(thing);
        var bytes = new Uint8Array(str.length);
        for (var i = 0; i < str.length; i++) {
            bytes[i] = str.charCodeAt(i);
        }
        thing = bytes;
    }

    // Array to Uint8Array
    if (Array.isArray(thing)) {
        thing = new Uint8Array(thing);
    }

    // Uint8Array to ArrayBuffer
    if (thing instanceof Uint8Array) {
        thing = thing.buffer;
    }

    // error if none of the above worked
    if (!(thing instanceof ArrayBuffer)) {
        throw new TypeError("could not coerce '" + name + "' to ArrayBuffer");
    }

    return thing;
};

coerceToBase64Url = function (thing) {
    // Array or ArrayBuffer to Uint8Array
    if (Array.isArray(thing)) {
        thing = Uint8Array.from(thing);
    }

    if (thing instanceof ArrayBuffer) {
        thing = new Uint8Array(thing);
    }

    // Uint8Array to base64
    if (thing instanceof Uint8Array) {
        var str = "";
        var len = thing.byteLength;

        for (var i = 0; i < len; i++) {
            str += String.fromCharCode(thing[i]);
        }
        thing = window.btoa(str);
    }

    if (typeof thing !== "string") {
        throw new Error("could not coerce to string");
    }

    // base64 to base64url
    // NOTE: "=" at the end of challenge is optional, strip it off here
    thing = thing.replace(/\+/g, "-").replace(/\//g, "_").replace(/=*$/g, "");

    return thing;
};