var TRANSFER_URL = "https://api.pocketfulofquarters.com";
var SERVER_TOKEN = "xk9q9fdxhbu65lyplqhwqe29klduvtoj9izi0g4wgrdy56b1ddle"; //ENTER SERVER TOKEN HERE
var APP_ADDRESS = "0x1e2aa0184dcde9502568593d6c5da68966a3275a"; //ENTER APP ADDRESS HERE


//PlayFab Cloud script handler
handlers.AwardQuarters = function (args, context) {
    var result = AwardQuarters(args);
    return result;
};



function AwardQuarters (args) {

    var amount = args["amount"];
    var user = args["userId"];

    if (SERVER_TOKEN == "") {
        log.error("Missing SERVER_TOKEN parameter");
        return;
    }
    if (APP_ADDRESS == "") {
        log.error("Missing APP_ADDRESS parameter");
        return;
    }
    if (amount == undefined || amount == null) {
        log.error("Missing amount parameter");
        return;
    }
    if (user == undefined || user == null) {
        log.error("Missing user parameter");
        return;
    }

    log.info("Award " + amount + " quarters to user: " + user);


    var requestAuthorized = true;

    /*
       ######## ENTER YOUR CUSTOM LOGIC HERE,
       BY DEFAULT EVERY AWARD REQUEST IS ACCEPTED

    */


    if (!requestAuthorized) return { error: "Award Quarters request not authorized by game server" };

    var postData = {
        "amount": amount,
        "user": user
    }

    var url = TRANSFER_URL + "/v1/accounts/" + APP_ADDRESS + "/transfer";

    var headers = {
        'Authorization': 'Bearer ' + SERVER_TOKEN,
        'Content-Type': 'application/json;charset=UTF-8'
    };

    var contentType = "application/json";
    var contentBody = JSON.stringify(postData);
    var response = http.request(url, "post", contentBody, contentType, headers);

    return response;
}





handlers.VerifyApplePurchase = function (args, context) {

    log.info(args);

    var result = VerifyApplePurchase(args);
    return result;

};





function VerifyApplePurchase (args) {

    var result = {};

    var user = args["UserId"];
    var useSandbox = args["UseSandbox"];
    var productId = args["ProductId"];
    var transactionId = args["TransactionId"];
    var receipt = JSON.parse(args["Receipt"])["Payload"];

    log.info("VerifyApplePurchase");
    log.info("Product id:" + productId);
    log.info("Transaction id:" + transactionId);
    log.info("Receipt data: " + receipt);


    var url = "";
    if (useSandbox) {
        url = "https://sandbox.itunes.apple.com/verifyReceipt";
    }
    else {
        url = "https://buy.itunes.apple.com/verifyReceipt";
    }

    log.info(url);
    var postData = {
        "receipt-data": receipt
    }

    var contentType = "application/json";
    var contentBody = JSON.stringify(postData);
    var response = http.request(url, "post", contentBody, contentType, null);

    log.info(response);

    var status = JSON.parse(response)["status"];

    log.info("Status " + status);

    if (status == 0) {

        var responseReceipt = JSON.parse(response)["receipt"];
        log.info(responseReceipt);
        var receiptPurchases = responseReceipt["in_app"];
        log.info("in_app " + receiptPurchases);
        log.info("receipt purchases count: " + receiptPurchases.length);

        //parse amount from productId
        var receiptProductId = receiptPurchases[0]["product_id"];
        log.info(receiptProductId);


        var quartersAmount = ParseQuartersAmount(receiptProductId);
        log.info("quartersAmount " + quartersAmount);

        //transaction is valid, award quarters
        var transferPostData = {
            "amount": quartersAmount,
            "user": user
        }

        var transferUrl = TRANSFER_URL + "/v1/accounts/" + APP_ADDRESS + "/transfer";

        var headers = {
            'Authorization': 'Bearer ' + SERVER_TOKEN,
            'Content-Type': 'application/json;charset=UTF-8'
        };

        var transferResponse = http.request(transferUrl, "post", JSON.stringify(transferPostData), "application/json", headers);


        log.info(transferResponse);

        result = {
            Status : "Success"
        };
    }
    else {
        result = {
            Status : "Error",
            ErrorCode : status,
            ErrorMessage : ParseAppleError(status)
        };
    }

    return result;
}



function ParseQuartersAmount (productId) {
    var quartersStripped = productId.replace("Quarters","");
    log.info(quartersStripped);
    return parseInt(quartersStripped);
}



function ParseAppleError (errorCode) {

    if (errorCode == 21000) return "The App Store could not read the JSON object provided.";
    else if (errorCode == 21002) return "The data in the receipt-data property was malformed or missing.";
    else if (errorCode == 21003) return "The receipt could not be authenticated.";
    else if (errorCode == 21004) return "The shared secret you provided does not match the shared secret on file for your account.";
    else if (errorCode == 21005) return "The receipt server is not currently available.";
    else if (errorCode == 21006) return "This receipt is valid but the subscription has expired. When this status code is returned to your server, the receipt data is also decoded and returned as part of the response.";
    else if (errorCode == 21007) return "This receipt is from the test environment, but it was sent to the production environment for verification. Send it to the test environment instead.";
    else if (errorCode == 21008) return "This receipt is from the production environment, but it was sent to the test environment for verification. Send it to the production environment instead.";
    else if (errorCode == 21010) return "This receipt could not be authorized. Treat this the same as if a purchase was never made.";
    else return "Internal data access error."


}
































